namespace InputHandlers.Keyboard;

/// <summary>
/// This is a bit field for "modifier" keys, i.e. control, shift and alt keys.
/// Bit 1 = either control key down
/// Bit 2 = either shift key down
/// Bit 3 = either alt key down
/// </summary>
[Flags]
public enum KeyboardModifier
{
    None = 0,
    Ctrl = 1,
    Shift = 2,
    Alt = 4
}