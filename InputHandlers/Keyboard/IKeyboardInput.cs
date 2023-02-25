namespace InputHandlers.Keyboard;

public interface IKeyboardInput
{
    KeyboardState OldKeyboardState { get; }
    KeyboardState CurrentKeyboardState { get; }
    IList<Keys> UnmanagedKeys { get; }
    IStopwatchProvider StopwatchProvider { get; }

    /// <summary>
    /// This is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time. 
    /// </summary>
    int UpdateNumber { get; }

    /// <summary>
    /// Sets time delay in milliseconds to wait for a key being held down until it repeats
    /// </summary>
    int RepeatDelay { get; set; }

    /// <summary>
    /// Set time in milliseconds between key repeats once it has started to repeat
    /// </summary>
    int RepeatFrequency { get; set; }

    /// <summary>
    /// Whether to treat modifier keys (ctrl/alt/shift) as actual keys
    /// </summary>
    bool TreatModifiersAsKeys { get; set; }
        
    /// <summary>
    /// Whether to wait for a neutral keyboard state before applying new pending subscriptions. Old subscriptions are still removed immediately. Defaults to False.
    /// </summary>
    bool WaitForNeutralStateBeforeApplyingNewSubscriptions { get; set; }

    bool IsRepeatEnabled { get; set; }
    void Subscribe(IKeyboardHandler keyboardHandler);
    void Unsubscribe(IKeyboardHandler keyboardHandler);

    /// <summary>
    /// Poll the keyboard for updates.
    /// </summary>
    /// <param name="keyboardState">a keyboard state.  You should use the XNA input function, Keyboard.GetState(), as this parameter.</param>
    void Poll(KeyboardState keyboardState);

    /// <summary>
    /// Reset to unpressed state.  You may wish to call this when, for example, switching interface screens.
    /// </summary>
    void Reset();
}