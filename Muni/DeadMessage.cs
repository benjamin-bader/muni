namespace Muni
{
    /// <summary>
    /// Represents a message that has no subscribers.  Useful for debugging!
    /// </summary>
    public class DeadMessage
    {
        /// <summary>
        /// The bus that originated this message.
        /// </summary>
        public Bus Bus { get; private set; }

        /// <summary>
        /// The message object itself.
        /// </summary>
        public object Message { get; private set; }

        public DeadMessage(Bus bus, object message)
        {
            Bus = bus;
            Message = message;
        }
    }
}
