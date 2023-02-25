namespace InputHandlers.Mouse;

public class MouseHandlerSubscription
{
    protected bool Equals(MouseHandlerSubscription other)
    {
        return Equals(_mouseHandler, other._mouseHandler);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MouseHandlerSubscription)obj);
    }

    public override int GetHashCode()
    {
        return (_mouseHandler != null ? _mouseHandler.GetHashCode() : 0);
    }

    public IMouseHandler MouseHandler => _mouseHandler;
    private readonly IMouseHandler _mouseHandler;

    public TimeSpan SubscribedTime => _subscribedTime;
    private TimeSpan _subscribedTime;

    public MouseHandlerSubscription(IMouseHandler mouseHandler, IStopwatchProvider stopwatchProvider)
    {
        _mouseHandler = mouseHandler;
        _subscribedTime = stopwatchProvider.Elapsed;
    }

    public void UpdateSubscribedTime(IStopwatchProvider stopwatchProvider)
    {
        _subscribedTime = stopwatchProvider.Elapsed;
    }
}