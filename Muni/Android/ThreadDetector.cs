using System.Threading;

namespace Muni
{
    class ThreadDetector
    {
        public bool IsOnMainThread
        {
            // SynchronizationContext.Current is null everywhere except the main thread... handy!
            get { return SynchronizationContext.Current != null; }
        }
    }
}