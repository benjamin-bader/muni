using Android.OS;

namespace Muni
{
    internal static class ThreadDetector
    {
        public static bool IsOnMainThread
        {
            get { return !ReferenceEquals(Looper.MainLooper, Looper.MyLooper()); }
        }
    }
}