using System;

namespace Muni
{
    public class DeadMessageEventArgs : EventArgs
    {
        public DeadMessage DeadMessage { get; private set; }

        public DeadMessageEventArgs(DeadMessage message)
        {
            DeadMessage = message;
        }
    }
}
