using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Muni
{
    internal sealed class MessageProducer : IEquatable<MessageProducer>
    {
        private readonly object target;
        private readonly MethodInfo method;
        private bool valid = true;

        // hashcode is computed once on object creation and cached as an optimization.
        // this helps lookup and dispatch be fast.
        private readonly int hashcode;

        public Type ProducerType
        {
            get { return target.GetType(); }
        }

        public bool IsValid
        {
            get { return valid; }
        }

        public MessageProducer(object target, MethodInfo method)
        {
            this.target = target;
            this.method = method;

            unchecked
            {
                hashcode = (3 + target.GetHashCode())*3 + method.GetHashCode();
            }
        }

        public void Invalidate()
        {
            valid = false;
        }

        public object Produce()
        {
            if (!valid)
            {
                throw new InvalidOperationException("Producer " + method.Name + " has already been invalidated");
            }

            return method.Invoke(target, new object[0]);
        }

        public override string ToString()
        {
            return "[MessageProducer " + method.Name + "]";
        }

        public bool Equals(MessageProducer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return target.Equals(other.target) && method.Equals(other.method);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MessageProducer && Equals((MessageProducer) obj);
        }

        public override int GetHashCode()
        {
            return hashcode;
        }

        public static bool operator ==(MessageProducer left, MessageProducer right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MessageProducer left, MessageProducer right)
        {
            return !Equals(left, right);
        }
    }
}
