using System;

namespace Muni
{
    /// <summary>
    /// Designates a method as a message producer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sometimes one is interested not only in subscribing to events as they
    /// happen, but also in receiving the most recent value of specific events.
    /// TODO fill me in plox
    /// </para>
    /// <para>
    /// The following restrictions apply to producer methods:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The method must be public.</description>
    ///   </item>
    ///   <item>
    ///     <description>The method must have no parameters.</description>
    ///   </item>
    ///   <item>
    ///     <description>The method must have a non-void return type.</description>
    ///   </item>
    ///   <item>
    ///     <description>The return type may not be a generic parameter.</description>
    ///   </item>
    ///   <item>
    ///     <description>The return type must be concrete, and not an interface.</description>
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ProduceAttribute : Attribute
    {
    }
}
