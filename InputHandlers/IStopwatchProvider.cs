using System;

namespace InputHandlers
{
    public interface IStopwatchProvider
    {
        void Start();
        void Stop();
        void Reset();
        void Restart();
        bool IsRunning { get; }
        TimeSpan Elapsed { get; }
        long ElapsedMilliseconds { get; }
        long ElapsedTicks { get; }
    }
}