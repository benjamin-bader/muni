using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Muni
{
    sealed class MessageProducer
    {
        private readonly object target;
        private readonly MethodInfo method;
        private readonly int hashcode;
        private bool valid = true;

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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            var that = (MessageProducer) obj;
            return target.Equals(that.target) && method.Equals(that.method);
        }

        public override int GetHashCode()
        {
            return hashcode;
        }
    }
}
