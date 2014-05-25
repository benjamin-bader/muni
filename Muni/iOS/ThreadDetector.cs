using MonoTouch.Foundation;

namespace Muni
{
    internal sealed class ThreadDetector
    {
        public bool IsOnMainThread
        {
            get { return NSThread.IsMain; }
        }
    }
}