using System.Windows;

namespace Muni
{
    class ThreadDetector
    {
        public bool IsOnMainThread
        {
            get { return Deployment.Current.Dispatcher.CheckAccess(); }
        }
    }
}