using Windows.ApplicationModel.Core;

namespace Muni
{
    internal static class ThreadDetector
    {
        public static bool IsOnMainThread
        {
            get { return CoreApplication.MainView.Dispatcher.HasThreadAccess; }
        }
    }
}