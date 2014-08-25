using System;
using System.Reflection;

namespace Muni
{
    /// <summary>
    /// Represents a message subscriber, defined as an object with a handler
    /// method.
    /// </summary>
    internal sealed class MessageHandler : IEquatable<MessageHandler>
    {
        private readonly object target;
        private readonly MethodInfo method;
        private bool valid = true;

        // hashcode is computed on instance creation and cached as an optimization.
        private readonly int hashCode;

        public bool IsValid
        {
            get { return valid; }
        }

        public MessageHandler(object target, MethodInfo method)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            this.target = target;
            this.method = method;

            unchecked
            {
                hashCode = (3 + target.GetHashCode())*3 + method.GetHashCode();
            }
        }

        /// <summary>
        /// Marks this handler as no longer valid.
        /// </summary>
        /// <remarks>
        /// After this method returns, any following calls to
        /// <see cref="HandleMessage"/> will fail with an
        /// <see cref="InvalidOperationException"/>.
        /// </remarks>
        public void Invalidate()
        {
            valid = false;
        }

        /// <summary>
        /// Invokes the handler method with the given message as its argument.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this handler has been invalidated.
        /// </exception>
        /// <exception cref="TargetInvocationException">
        /// Thrown when invoking the handler method fails with any exception.
        /// </exception>
        public void HandleMessage(object message)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException(ToString() + " has been invalidated and can no longer handle events.");
            }

            method.Invoke(target, new[] { message });
        }

        public override string ToString()
        {
            return "[MessageHandler " + method.Name + "]";
        }

        public bool Equals(MessageHandler other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return target.Equals(other.target) && method.Equals(other.method);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MessageHandler && Equals((MessageHandler) obj);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public static bool operator ==(MessageHandler left, MessageHandler right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MessageHandler left, MessageHandler right)
        {
            return !Equals(left, right);
        }
    }
}
