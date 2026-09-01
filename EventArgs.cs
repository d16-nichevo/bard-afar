using System;

namespace BardAfar
{
    /// <summary>
    /// Generic EventArgs.
    /// </summary>
    public class EventArgs<T> : EventArgs
    {
        public T Value { get; }
        public EventArgs(T value) => Value = value;
    }
}
