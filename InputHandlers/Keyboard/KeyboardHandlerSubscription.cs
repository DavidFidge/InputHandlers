namespace InputHandlers.Keyboard;

public class KeyboardHandlerSubscription
{
    protected bool Equals(KeyboardHandlerSubscription other)
    {
        return Equals(_keyboardHandler, other._keyboardHandler);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((KeyboardHandlerSubscription)obj);
    }

    public override int GetHashCode()
    {
        return (_keyboardHandler != null ? _keyboardHandler.GetHashCode() : 0);
    }

    public IKeyboardHandler KeyboardHandler => _keyboardHandler;
    private readonly IKeyboardHandler _keyboardHandler;
        
    public TimeSpan SubscribedTime => _subscribedTime;
    private TimeSpan _subscribedTime;
        
    public KeyboardHandlerSubscription(IKeyboardHandler keyboardHandler, IStopwatchProvider stopwatchProvider)
    {
        _keyboardHandler = keyboardHandler;
        _subscribedTime = stopwatchProvider.Elapsed;
    }
    
    public void UpdateSubscribedTime(IStopwatchProvider stopwatchProvider)
    {
        _subscribedTime = stopwatchProvider.Elapsed;
    }
}