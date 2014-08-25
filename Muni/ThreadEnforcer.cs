using System;

namespace Muni
{
    /// <summary>
    /// Ensures that messages are only posted on the right threads.
    /// </summary>
    public abstract class ThreadEnforcer
    {
        /// <summary>
        /// The most permissive possible <see cref="ThreadEnforcer"/>, which permits
        /// messages to be posted from any thread.
        /// </summary>
        public static readonly ThreadEnforcer AnyThread = new AnyThreadEnforcer();

        /// <summary>
        /// A <see cref="ThreadEnforcer"/> that requires all messages to be posted from
        /// the UI thread.
        /// </summary>
        public static readonly ThreadEnforcer MainThread = new MainThreadEnforcer();

        /// <summary>
        /// When implemented in a derived class, ensures that the current operation
        /// is on an allowed thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the current operation is not on an allowed thread.
        /// </exception>
        public abstract void EnforceThreadAffinity();

        private class MainThreadEnforcer : ThreadEnforcer
        {
            public override void EnforceThreadAffinity()
            {
                if (!ThreadDetector.IsOnMainThread)
                {
                    throw new InvalidOperationException("Messages can only be posted on the main thread!");
                }
            }
        }

        private class AnyThreadEnforcer : ThreadEnforcer
        {
            public override void EnforceThreadAffinity()
            {
                // it's all good here
            }
        }
    }
}
