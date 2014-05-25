using System.Windows;

namespace Muni
{
    internal static class ThreadDetector
    {
        public static bool IsOnMainThread
        {
            get { return Deployment.Current.Dispatcher.CheckAccess(); }
        }
    }
}