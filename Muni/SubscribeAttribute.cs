using System;

namespace Muni
{
    /// <summary>
    /// Marks a method as a message subscriber.
    /// </summary>
    /// <remarks>
    /// The target method must have one and only one argument, whose type is the
    /// type of the message to be subscribed on.  The following restrictions apply:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The method must be public.</description>
    ///   </item>
    ///   <item>
    ///     <description>The method must have one and only one argument.</description>
    ///   </item>
    ///   <item>
    ///     <description>The argument may not be a generic parameter.</description>
    ///   </item>
    ///   <item>
    ///     <description>The argument must be a concrete type, not an interface.</description>
    ///   </item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class SubscribeAttribute : Attribute
    {
    }
}
