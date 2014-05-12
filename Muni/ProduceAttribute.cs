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
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ProduceAttribute : Attribute
    {
    }
}
