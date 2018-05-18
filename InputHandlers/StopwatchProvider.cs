using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace InputHandlers
{
    public class StopwatchProvider : IStopwatchProvider
    {
        private readonly Stopwatch _stopwatch;

        public StopwatchProvider()
        {
            _stopwatch = new Stopwatch();
        }

        public void Start()
        {
            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }

        public void Reset()
        {
            _stopwatch.Reset();
        }

        public TimeSpan Elapsed
        {
            get { return _stopwatch.Elapsed; }
        }
    }
}