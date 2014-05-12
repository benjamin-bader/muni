using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Muni
{
    internal static class HandlerFinder
    {
        private static readonly IDictionary<Type, IDictionary<Type, ISet<MethodInfo>>> SubscriberCache =
            new Dictionary<Type, IDictionary<Type, ISet<MethodInfo>>>();

        private static readonly IDictionary<Type, IDictionary<Type, MethodInfo>> ProducerCache =
            new Dictionary<Type, IDictionary<Type, MethodInfo>>();

        /// <summary>
        /// Attempts to determine if the given method's parameter is a generic argument.
        /// </summary>
        /// <remarks>
        /// This addresses the case where a type has a generic argument, and the method's
        /// parameter is that generic argument.  In this case, TypeInfo.IsGeneric
        /// </remarks>
        /// <param name="declaringType"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static bool IsConstructedGenericMethod(TypeInfo declaringType, MethodInfo method)
        {
            if (!declaringType.IsGenericType)
            {
                return false;
            }

            var t = declaringType.GetGenericTypeDefinition().GetTypeInfo();
            var genericMethods = t.DeclaredMethods.Where(m => m.Name == method.Name);

            foreach (var gm in genericMethods)
            {
                var parameters = gm.GetParameters();

                if (parameters.Length != 1)
                {
                    // Same name, but not the method in question.
                    continue;
                }

                if (parameters[0].ParameterType == method.GetParameters()[0].ParameterType)
                {
                    return false;
                }

                if (parameters[0].ParameterType.IsGenericParameter)
                {
                    return true;
                }
            }

            throw new Exception("ASSERT FALSE");
        }

        private static void FindSubscriberAndProducerMethods(Type t)
        {
            IDictionary<Type, ISet<MethodInfo>> subscribers = new Dictionary<Type, ISet<MethodInfo>>();
            IDictionary<Type, MethodInfo> producers = new Dictionary<Type, MethodInfo>();

            foreach (var method in from m in t.GetTypeInfo().DeclaredMethods
                                   where m.GetCustomAttribute<Subscribe>() != null
                                   select m)
            {
                var parameters = method.GetParameters().ToList();

                if (parameters.Count != 1)
                {
                    throw new ArgumentException("Method " + method.Name + " has a [Subscribe] attribute but " +
                                                "requires " + parameters.Count + "parameters.  Methods must have " +
                                                "a single argument.");
                }

                var paramType = parameters.Single().ParameterType;
                var paramTypeInfo = paramType.GetTypeInfo();
                if (paramTypeInfo.IsInterface)
                {
                    var msg = "Method " + method.Name + " has a [Subscribe] attribute but its argument is an interface.  " +
                              "Subscription must be on a concrete type.";
                    throw new ArgumentException(msg);
                }

                if (paramTypeInfo.IsGenericParameter || IsConstructedGenericMethod(t.GetTypeInfo(), method))
                {
                    var msg = "Method " + method.Name +
                              " has a [Subscribe] attribute but its argument is a generic argument.  " +
                              "Subscription must be on a non-generic type.";
                    throw new ArgumentException(msg);
                }

                if (!method.IsPublic)
                {
                    var msg = "Method " + method.Name + " has a [Subscribe] attribute but is not public.";
                    throw new ArgumentException(msg);
                }

                ISet<MethodInfo> subscriberSet;
                if (!subscribers.TryGetValue(paramType, out subscriberSet))
                {
                    subscriberSet = new HashSet<MethodInfo>();
                    subscribers[paramType] = subscriberSet;
                }

                subscriberSet.Add(method);
            }

            foreach (var method in from m in t.GetTypeInfo().DeclaredMethods
                                   where m.GetCustomAttribute<ProduceAttribute>() != null
                                   select m)
            {
                var parameters = method.GetParameters().ToList();

                if (parameters.Count != 0)
                {
                    throw new ArgumentException("Method " + method.Name + " has a [Produce] attribute but " +
                                                "requires " + parameters.Count + "parameters.  Producer methods must have " +
                                                "no arguments.");
                }

                var returnType = method.ReturnType;
                if (returnType == null || returnType == typeof (void))
                {
                    var msg = "Method " + method.Name +
                              " has a [Produce] attribute but it does not return any value.  Producer methods" +
                              " must have a non-void return type.";

                    throw new ArgumentException(msg);
                }

                if (returnType.GetTypeInfo().IsInterface)
                {
                    var msg = "Method " + method.Name +
                              " has a [Produce] attribute but its return type is an interface. Messages must" +
                              " be concrete types.";
                    throw new ArgumentException(msg);
                }

                if (!method.IsPublic)
                {
                    var msg = "Method " + method.Name + " has a [Produce] attribute but is not public.";
                    throw new ArgumentException(msg);
                }

                if (producers.ContainsKey(returnType))
                {
                    throw new ArgumentException("A producer for type " + returnType + " has already been registered.");
                }

                producers[returnType] = method;
            }

            SubscriberCache.Add(t, subscribers);
            ProducerCache.Add(t, producers);
        }

        internal static IDictionary<Type, ISet<MessageHandler>> FindAllSubscribers(object target)
        {
            var targetType = target.GetType();
            var handlers = new Dictionary<Type, ISet<MessageHandler>>();

            if (!SubscriberCache.ContainsKey(targetType))
            {
                FindSubscriberAndProducerMethods(targetType);
            }

            var subscriberMethods = SubscriberCache[targetType];
            if (subscriberMethods.Count > 0)
            {
                foreach (var kvp in subscriberMethods)
                {
                    var messageType = kvp.Key;
                    var methods = kvp.Value;

                    handlers[messageType] = new HashSet<MessageHandler>(methods.Select(m => new MessageHandler(target, m)));
                }
            }

            return handlers;
        }

        internal static IDictionary<Type, MessageProducer> FindAllProducers(object target)
        {
            var targetType = target.GetType();
            var producers = new Dictionary<Type, MessageProducer>();

            if (!ProducerCache.ContainsKey(targetType))
            {
                FindSubscriberAndProducerMethods(targetType);
            }

            var producerMethods = ProducerCache[targetType];
            if (producerMethods.Count > 0)
            {
                foreach (var kvp in producerMethods)
                {
                    var messageType = kvp.Key;
                    var method = kvp.Value;

                    producers[messageType] = new MessageProducer(target, method);
                }
            }

            return producers;
        }
    }
}
