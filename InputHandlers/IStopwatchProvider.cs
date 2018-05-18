using System;

namespace InputHandlers
{
    public interface IStopwatchProvider
    {
        void Start();
        void Stop();
        void Reset();
        TimeSpan Elapsed { get; }
    }
}