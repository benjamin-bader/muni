using System;

namespace Muni
{
    public abstract class ThreadEnforcer
    {
        public static readonly ThreadEnforcer AnyThread = new AnyThreadEnforcer();
        public static readonly ThreadEnforcer MainThread = new MainThreadEnforcer();

        public abstract void EnforceThreadAffinity();

        private class MainThreadEnforcer : ThreadEnforcer
        {
            public override void EnforceThreadAffinity()
            {
                if (!ThreadDetector.IsOnMainThread)
                {
                    throw new InvalidOperationException("");
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
