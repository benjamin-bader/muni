using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Muni
{
    public class Bus
    {
        public const string DefaultIdentifier = "default";

        public event EventHandler<DeadMessageEventArgs> DeadMessageReceived;

        private readonly object padlock = new object();

        private readonly IDictionary<Type, ISet<MessageHandler>> handlersByType =
            new Dictionary<Type, ISet<MessageHandler>>();

        private readonly IDictionary<Type, MessageProducer> producersByType =
            new Dictionary<Type, MessageProducer>();

        private readonly string identifier;
        private readonly ThreadEnforcer enforcer;
        private readonly ThreadLocal<Queue<MessageWithHandler>> eventsToDispatch =
            new ThreadLocal<Queue<MessageWithHandler>>(() => new Queue<MessageWithHandler>());

        private readonly ThreadLocal<bool> isDispatching = new ThreadLocal<bool>(() => false);

        public Bus(string identifier = DefaultIdentifier, ThreadEnforcer enforcer = null)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            if (enforcer == null)
            {
                enforcer = ThreadEnforcer.AnyThread;
            }

            this.identifier = identifier;
            this.enforcer = enforcer;
        }

        public override string ToString()
        {
            return "[Bus " + identifier + "]";
        }

        public void Register(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target", "Cannot register a null object.");
            }

            var producerMethods = HandlerFinder.FindAllProducers(target);
            foreach (var kvp in producerMethods)
            {
                var messageType = kvp.Key;
                var foundProducer = kvp.Value;

                lock (padlock)
                {
                    MessageProducer existingProducer;
                    if (producersByType.TryGetValue(messageType, out existingProducer))
                    {
                        throw new ArgumentException("Producer method for type " + messageType +
                        " found on type " + target.GetType() + ", but already registered by type" +
                        " " + existingProducer.ProducerType);
                    }

                    producersByType[messageType] = foundProducer;
                }

                ISet<MessageHandler> handlers;
                if (handlersByType.TryGetValue(messageType, out handlers))
                {
                    foreach (var handler in handlers)
                    {
                        DispatchProducerMessageToHandler(foundProducer, handler);
                    }
                }
            }

            var subscriberMethods = HandlerFinder.FindAllSubscribers(target);
            foreach (var kvp in subscriberMethods)
            {
                var messageType = kvp.Key;
                var foundHandlers = kvp.Value;

                ISet<MessageHandler> handlers;
                lock (padlock)
                {
                    if (!handlersByType.TryGetValue(messageType, out handlers))
                    {
                        handlersByType[messageType] = handlers = new HashSet<MessageHandler>();
                    }
                }

                handlers.UnionWith(foundHandlers);
            }

            foreach (var kvp in subscriberMethods)
            {
                var messageType = kvp.Key;
                var foundHandlers = kvp.Value;

                if (foundHandlers.Count > 0)
                {
                    lock (padlock)
                    {
                        MessageProducer producer;
                        if (!producersByType.TryGetValue(messageType, out producer))
                        {
                            continue;
                        }

                        foreach (var handler in foundHandlers)
                        {
                            if (!producer.IsValid)
                            {
                                break;
                            }

                            DispatchProducerMessageToHandler(producer, handler);
                        }
                    }
                }
            }
        }

        private void DispatchProducerMessageToHandler(MessageProducer producer, MessageHandler handler)
        {
            object message;
            try
            {
                message = producer.Produce();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }

            if (message == null)
            {
                return;
            }

            try
            {
                handler.HandleMessage(message);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public void Unregister(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target", "Cannot unregister a null object");
            }

            enforcer.EnforceThreadAffinity();

            var subscriberMethods = HandlerFinder.FindAllSubscribers(target);
            foreach (var kvp in subscriberMethods)
            {
                var messageType = kvp.Key;
                var foundHandlers = kvp.Value;

                ISet<MessageHandler> handlers;
                lock (padlock)
                {
                    handlersByType.TryGetValue(messageType, out handlers);
                }

                if (handlers == null || !handlers.IsSupersetOf(foundHandlers))
                {
                    throw new ArgumentException("Missing message handler for [Subscribe] method.  Is " + target.GetType().FullName + " registered?");
                }

                foreach (var h in handlers)
                {
                    if (foundHandlers.Contains(h))
                    {
                        h.Invalidate();
                    }
                }

                handlers.ExceptWith(foundHandlers);
            }

            var producerMethods = HandlerFinder.FindAllProducers(target);
            foreach (var messageType in producerMethods.Keys)
            {
                MessageProducer producer;
                lock (padlock)
                {
                    producersByType.TryGetValue(messageType, out producer);
                }

                if (producer == null)
                {
                    throw new ArgumentException("Missing message producer for [Produce] method.  Is " + target.GetType().FullName + " registered?");
                }

                producer.Invalidate();

                lock (padlock)
                {
                    producersByType.Remove(messageType);
                }
            }
        }

        public void Post(object message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "Cannot post a null message");
            }

            enforcer.EnforceThreadAffinity();

            var dispatchTypes = FlattenHeirarchy(message.GetType());
            var dispatched = false;

            lock (padlock)
            {
                foreach (var t in dispatchTypes)
                {
                    ISet<MessageHandler> handlers;
                    if (handlersByType.TryGetValue(t, out handlers) && handlers.Count > 0)
                    {
                        dispatched = true;
                        foreach (var handler in handlers)
                        {
                            EnqueueEvent(handler, message);
                        }
                    }
                }
            }

            if (!dispatched)
            {
                var deadMessage = message as DeadMessage;
                if (deadMessage != null)
                {
                    OnDeadMessage(deadMessage);
                }
                else
                {
                    Post(new DeadMessage(this, message));
                }
            }

            DispatchQueuedEvents();
        }

        private void EnqueueEvent(MessageHandler handler, object message)
        {
            eventsToDispatch.Value.Enqueue(new MessageWithHandler(message, handler));
        }

        private void DispatchQueuedEvents()
        {
            if (isDispatching.Value)
            {
                return;
            }

            isDispatching.Value = true;

            try
            {
                while (eventsToDispatch.Value.Count > 0)
                {
                    var tup = eventsToDispatch.Value.Dequeue();

                    var ev = tup.Message;
                    var handler = tup.Handler;

                    if (handler.IsValid)
                    {
                        Dispatch(handler, ev);
                    }
                }
            }
            finally
            {
                isDispatching.Value = false;
            }
        }

        private void Dispatch(MessageHandler handler, object ev)
        {
            try
            {
                handler.HandleMessage(ev);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        private static readonly IDictionary<Type, ISet<Type>> HierarchyCache = new Dictionary<Type, ISet<Type>>();
        private static ISet<Type> FlattenHeirarchy(Type t)
        {
            ISet<Type> hierarchy;
            lock (HierarchyCache)
            {
                if (!HierarchyCache.TryGetValue(t, out hierarchy))
                {
                    HierarchyCache[t] = hierarchy = CalculateFlattenedHierarchy(t);
                }
            }

            return hierarchy;
        }

        private static ISet<Type> CalculateFlattenedHierarchy(Type t)
        {
            ISet<Type> hierarchy = new HashSet<Type>();

            while (t != null)
            {
                hierarchy.Add(t);
                t = t.GetTypeInfo().BaseType;
            }

            return hierarchy;
        }

        protected void OnDeadMessage(DeadMessage message)
        {
            var handler = DeadMessageReceived;
            if (handler != null)
            {
                handler(this, new DeadMessageEventArgs(message));
            }
        }

        // Stick with this instead of Tuple to avoid unnecessary generics
        private sealed class MessageWithHandler
        {
            public readonly object Message;
            public readonly MessageHandler Handler;

            public MessageWithHandler(object message, MessageHandler handler)
            {
                Message = message;
                Handler = handler;
            }
        }
    }
}
