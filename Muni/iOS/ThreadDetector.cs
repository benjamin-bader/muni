using MonoTouch.Foundation;

namespace Muni
{
    internal sealed class ThreadDetector
    {
        public static bool IsOnMainThread
        {
            get { return NSThread.IsMain; }
        }
    }
}