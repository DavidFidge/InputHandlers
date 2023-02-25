namespace InputHandlers.Mouse;

public interface IMouseInput
{
    MouseState OldMouseState { get; }
    MouseState CurrentMouseState { get; }
    MouseState DragOriginPosition { get; }
    IStopwatchProvider StopwatchProvider { get; }
    bool IsLeftButtonEnabled { get; set; }
    bool IsMiddleButtonEnabled { get; set; }
    bool IsRightButtonEnabled { get; set; }

    /// <summary>
    /// DragVariance is a fudging factor for detecting the difference between mouse clicks and mouse drags.
    /// This is because a fast user may do a mouse click while slightly moving the mouse between mouse down and mouse up.
    /// If it wasnt for this fudging factor then it would go into drag mode which isnt what the user probably wanted.
    /// </summary>
    int DragVariance { get; set; }

    /// <summary>
    /// If time between clicks in milliseconds is less than this value then it is considered a double click
    /// </summary>
    int DoubleClickDetectionTimeDelay { get; set; }

    /// <summary>
    /// This is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time. 
    /// </summary>
    int UpdateNumber { get; }

    /// <summary>
    /// Whether to wait for a neutral mouse state before applying pending subscriptions. Old subscriptions are still removed immediately. Defaults to False.
    /// </summary>
    bool WaitForNeutralStateBeforeApplyingNewSubscriptions { get; set; }

    void Subscribe(IMouseHandler mouseHandler);
    void Unsubscribe(IMouseHandler mouseHandler);

    /// <summary>
    /// Poll the mouse for updates.
    /// </summary>
    /// <param name="mouseState">a mouse state.  You should use the XNA input function, Mouse.GetState(), as this parameter.</param>
    void Poll(MouseState mouseState);

    /// <summary>
    /// Reset to stationary state.  You may wish to call this when, for example, switching interface screens.
    /// </summary>
    void Reset();

    /// <summary>
    /// Use this if you want to suppress double click detection (clears left, middle and right all at once).
    /// May be useful for scenarios where you want to suppress clicks under some circumstances e.g. if the user
    /// double clicks a button to close a window rather than single clicks and the click is subsequently
    /// detected afterwards incorrectly as a different action. You could call this after the window closes
    /// to suppress the double click. 
    /// </summary>
    void ResetDoubleClickDetection();
}