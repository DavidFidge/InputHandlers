using System;

namespace InputHandlers.Tests;

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

    public bool IsRunning { get; set; }

    public TimeSpan Elapsed { get; set; }

    public void AdvanceByMilliseconds(int milliseconds)
    {
        Elapsed = Elapsed.Add(new TimeSpan(0, 0, 0, 0, milliseconds));
    }
}