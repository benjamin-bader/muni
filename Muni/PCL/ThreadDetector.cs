using System;

namespace Muni
{
    class ThreadDetector
    {
        public ThreadDetector()
        {
            throw new InvalidOperationException("You're Doing It Wrong - install the nuget package in your platform-specific application!");
        }

        public bool IsOnMainThread
        {
            get { throw new InvalidOperationException("You're Doing It Wrong - install the nuget package in your platform-specific application!"); }
        }
    }
}