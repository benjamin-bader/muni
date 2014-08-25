using System;

namespace Muni
{
    /// <summary>
    /// Represents a message posted for which no listeners were subscribed.
    /// </summary>
    public class DeadMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message itself.
        /// </summary>
        /// <value>
        /// The message itself.
        /// </value>
        public object DeadMessage { get; private set; }

        /// <summary>
        /// Creates a new <see cref="DeadMessageEventArgs"/> instance.
        /// </summary>
        /// <param name="message">
        /// The message itself.
        /// </param>
        public DeadMessageEventArgs(object message)
        {
            DeadMessage = message;
        }
    }
}
