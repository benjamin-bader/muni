using MonoTouch.Foundation;

namespace Muni
{
    class ThreadDetector
    {
        public bool IsOnMainThread
        {
            get { return NSThread.IsMain; }
        }
    }
}