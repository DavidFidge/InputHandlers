using System;

namespace InputHandlers.Tests
{
    public class TestStopwatchProvider : IStopwatchProvider
    {
        public TestStopwatchProvider()
        {
            Reset();
        }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Reset()
        {
            Stop();
            Elapsed = TimeSpan.Zero;
        }

        public void Restart()
        {
            Start();
        }

        public bool IsRunning { get; set; }
        public TimeSpan Elapsed { get; set; }
        public long ElapsedMilliseconds { get { return Elapsed.Milliseconds; } }

        public long ElapsedTicks
        {
            get { return Elapsed.Ticks; }
        }

        public void AdvanceBySeconds(int seconds)
        {
            Elapsed = Elapsed.Add(new TimeSpan(0, 0, seconds));
        }

        public void AdvanceByMilliseconds(int milliseconds)
        {
            Elapsed = Elapsed.Add(new TimeSpan(0, 0, 0, 0, milliseconds));
        }
    }
}
