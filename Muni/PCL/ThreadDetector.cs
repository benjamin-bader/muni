using System;

namespace Muni
{
    internal static class ThreadDetector
    {
        public static bool IsOnMainThread
        {
            get { throw new InvalidOperationException("You're Doing It Wrong - install the nuget package in your platform-specific application!"); }
        }
    }
}