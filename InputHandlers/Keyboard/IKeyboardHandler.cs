namespace InputHandlers.Keyboard;

public interface IKeyboardHandler
{
    /// <summary>
    /// Handle a key down event.  A unique call is made to this event every single time an individual key is pressed.  The
    /// 'keyInFocus' key of each call is retrievable through the keyInFocus parameter.
    /// </summary>
    /// <param name="keysDown">
    /// The entire set of keys currently held down on the keyboard (please note that number of keys
    /// detectable is subject to hardware limitations on your keyboard)
    /// </param>
    /// <param name="keyInFocus">The key that was pressed to create this event.</param>
    /// <param name="keyboardModifier">a bit field holding control, alt and shift key status</param>
    void HandleKeyboardKeyDown(Keys[] keysDown, Keys keyInFocus, KeyboardModifier keyboardModifier);

    /// <summary>
    /// Handle a key lost event.  This occurs when multiple keys are held down and then a key is released but some keys
    /// are still being held.
    /// </summary>
    /// <param name="keysDown">
    /// The entire set of keys currently held down on the keyboard (please note that number of keys
    /// detectable is subject to hardware limitations on your keyboard)
    /// </param>
    /// <param name="keyboardModifier">a bit field holding control, alt and shift key status</param>
    void HandleKeyboardKeyLost(Keys[] keysDown, KeyboardModifier keyboardModifier);

    /// <summary>
    /// Handle a key repeat event.  Once the repeat delay threshold is exceeded when the same key(s) are held down for long
    /// enough then the program will start sending key repeat events on every update.
    /// </summary>
    /// <param name="repeatingKey">the key that is to be repeated.  This is always the last key held down.</param>
    /// <param name="keyboardModifier">a bit field holding control, alt and shift key status</param>
    void HandleKeyboardKeyRepeat(Keys repeatingKey, KeyboardModifier keyboardModifier);

    /// <summary>
    /// Handle the situation where all keys have been released.
    /// </summary>
    void HandleKeyboardKeysReleased();
}