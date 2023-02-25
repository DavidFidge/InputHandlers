namespace InputHandlers.Keyboard;

public static class KeysExtensions
{
    public static bool IsKeyAlpha(this Keys key)
    {
        return key >= Keys.A && key <= Keys.Z;
    }

    public static bool IsKeyNumber(this Keys key)
    {
        return key >= Keys.D0 && key <= Keys.D9;
    }

    public static bool IsKeyNumberpad(this Keys key)
    {
        return key >= Keys.NumPad0 && key <= Keys.NumPad9;
    }

    public static bool IsKeyNumeric(this Keys key, bool treatNumpadAsNumeric = true)
    {
        if (IsKeyNumber(key))
            return true;

        return treatNumpadAsNumeric && IsKeyNumberpad(key);
    }

    public static bool IsKeyAlphanumeric(this Keys key)
    {
        return IsKeyAlpha(key) || IsKeyNumeric(key);
    }

    public static bool IsFunctionKey(this Keys key)
    {
        return key >= Keys.F1 && key <= Keys.F12;
    }

    public static bool IsKeySpace(this Keys key)
    {
        return key == Keys.Space;
    }

    public static bool IsShift(this Keys key)
    {
        return key == Keys.LeftShift || key == Keys.RightShift;
    }

    public static bool IsCtrl(this Keys key)
    {
        return key == Keys.LeftControl || key == Keys.RightControl;
    }

    public static bool IsAlt(this Keys key)
    {
        return key == Keys.LeftAlt || key == Keys.RightAlt;
    }

    public static bool IsShiftDown(this KeyboardModifier keyboardModifier)
    {
        if ((KeyboardModifier.Shift & keyboardModifier) == KeyboardModifier.Shift)
            return true;

        return false;
    }

    public static bool IsCtrlDown(this KeyboardModifier keyboardModifier)
    {
        return (KeyboardModifier.Ctrl & keyboardModifier) == KeyboardModifier.Ctrl;
    }

    public static bool IsAltDown(this KeyboardModifier keyboardModifier)
    {
        return (KeyboardModifier.Alt & keyboardModifier) == KeyboardModifier.Alt;
    }

    /// <summary>
    /// Returns whether this key is a "modifier" key i.e. control, shift or alt
    /// </summary>
    public static bool IsModifierKey(this Keys key)
    {
        return IsShift(key) || IsAlt(key) || IsCtrl(key);
    }

    /// <summary>
    /// Returns KeyboardModifier object which has shift bit flagged if a shift key was pressed
    /// </summary>
    /// <param name="key"></param>
    /// <returns>KeyboardModifier object which has shift bit flagged if a shift key was pressed</returns>
    public static KeyboardModifier IsShiftModifier(this Keys key)
    {
        if (key == Keys.LeftShift || key == Keys.RightShift)
            return KeyboardModifier.Shift;

        return KeyboardModifier.None;
    }

    /// <summary>
    /// Returns KeyboardModifier object with control key flagged if a control key was pressed
    /// </summary>
    /// <param name="key"></param>
    /// <returns>KeyboardModifier object with control key flagged if a control key was pressed</returns>
    public static KeyboardModifier IsCtrlModifier(this Keys key)
    {
        if (key == Keys.LeftControl || key == Keys.RightControl)
            return KeyboardModifier.Ctrl;

        return KeyboardModifier.None;
    }

    /// <summary>
    /// Returns KeyboardModifier object with alt key flagged if an alt key is pressed
    /// </summary>
    /// <param name="key"></param>
    /// <returns>KeyboardModifier object with alt key flagged if an alt key is pressed</returns>
    public static KeyboardModifier IsAltModifier(this Keys key)
    {
        if (key == Keys.LeftAlt || key == Keys.RightAlt)
            return KeyboardModifier.Alt;

        return KeyboardModifier.None;
    }

    /// <summary>
    ///     convert the current key into a printable string.  Defaults to any kind of printable character and accounts for
    ///     shift key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="keyboardModifier"></param>
    /// <returns></returns>
    public static string Display(this Keys key, KeyboardModifier keyboardModifier)
    {
        return Display(key, keyboardModifier, true, true, true, false);
    }

    /// <summary>
    ///     convert the current key into a printable string.  Defaults to alphanumerics on and and accounts for shift key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="keyboardModifier"></param>
    /// <param name="selectspecials"></param>
    /// <returns></returns>
    public static string Display(this Keys key, KeyboardModifier keyboardModifier, bool selectspecials)
    {
        return Display(key, keyboardModifier, selectspecials, true, true, false);
    }

    /// <summary>
    ///     convert the current key into a printable string given filtering options. Defaults to spaces allowed
    /// </summary>
    /// <param name="key"></param>
    /// <param name="keyboardModifier"></param>
    /// <param name="selectspecials">filters so that specials are included in the string if true</param>
    /// <param name="selectAlphas">filters so that alphas are selected if true</param>
    /// <param name="selectNumerics">filters so that numerics are selected if true </param>
    /// <returns></returns>
    public static string Display(this Keys key, KeyboardModifier keyboardModifier, bool selectspecials, bool selectAlphas,
        bool selectNumerics)
    {
        return Display(key, keyboardModifier, selectspecials, selectAlphas, selectNumerics, false);
    }

    /// <summary>
    ///     convert the current key into a printable string.  If users want to force upper case or lower case they can change
    ///     modifiers m (for example, to enable use of the caps lock key, test it outside the function, if its on pass in a
    ///     kbmodifiers with shift flag, if off pass KeyboardModifier.None)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="keyboardModifier"></param>
    /// <param name="selectspecials">filters so that specials are included in the string if true</param>
    /// <param name="selectAlphas">filters so that alphas are selected if true</param>
    /// <param name="selectNumerics">filters so that numerics are selected if true </param>
    /// <param name="suppressSpace">no spaces are output</param>
    /// <param name="treatNumpadAsNumeric">treat numpad as numeric keys</param>
    /// <returns></returns>
    public static string Display(this Keys key, KeyboardModifier keyboardModifier, bool selectspecials, bool selectAlphas,
        bool selectNumerics, bool suppressSpace, bool treatNumpadAsNumeric = true)
    {
        if (IsKeySpace(key) && !suppressSpace)
            return " ";

        if ((IsKeyAlpha(key) && selectAlphas)
            || (IsKeyNumber(key) && selectNumerics)
            || (treatNumpadAsNumeric && IsKeyNumberpad(key) && selectNumerics)
            || (selectspecials 
                && ((!IsKeyAlpha(key) && !IsKeyNumeric(key))
                    || (IsKeyNumber(key) && IsShiftDown(keyboardModifier)))))
            if (IsShiftDown(keyboardModifier))
            {
                if (!(!selectspecials && IsKeyNumber(key)))
                    return shiftedKeys[key.GetHashCode()];
            }
            else
            {
                return unshiftedKeys[key.GetHashCode()];
            }

        return "";
    }

    /// <summary>
    /// Gets KeyboardModifier flags indicating which "modifier" keys are down (control, shift, alt)
    /// </summary>
    /// <param name="keys">list of keys to test</param>
    /// <returns>bit field with modifier keys flagged</returns>
    public static KeyboardModifier GetModifiers(this Keys[] keys)
    {
        var keyboardModifier = KeyboardModifier.None;

        foreach (var key in keys)
        {
            keyboardModifier = keyboardModifier | IsShiftModifier(key);
            keyboardModifier = keyboardModifier | IsAltModifier(key);
            keyboardModifier = keyboardModifier | IsCtrlModifier(key);
        }

        return keyboardModifier;
    }

    private static readonly string[] unshiftedKeys =
    {
        "", "", "", "", "", "", "", "", "", "",
        "", "", "", "", "", "", "", "", "", "",
        "", "", "", "", "", "", "", "", "", "",
        "", "", " ", "", "", "", "", "", "", "", //30-39
        "", "", "", "", "", "", "", "", "0", "1", //40-49
        "2", "3", "4", "5", "6", "7", "8", "9", "", "", //50-59
        "", "", "", "", "", "a", "b", "c", "d", "e", //60-69
        "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", //70-79
        "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", //80-89
        "z", "", "", "", "", "", "0", "1", "2", "3", //90-99
        "4", "5", "6", "7", "8", "9", "*", "+", "", "-", //100-109
        ".", "/", "", "", "", "", "", "", "", "", //110-119
        "", "", "", "", "", "", "", "", "", "", //120-129
        "", "", "", "", "", "", "", "", "", "", //130-139
        "", "", "", "", "", "", "", "", "", "", //140-149
        "", "", "", "", "", "", "", "", "", "", //150-159
        "", "", "", "", "", "", "", "", "", "", //160-169
        "", "", "", "", "", "", "", "", "", "", //170-179
        "", "", "", "", "", "", ";", "=", ",", "-", //180-189
        ".", "/", "`", "", "", "", "", "", "", "", //190-199
        "", "", "", "", "", "", "", "", "", "", //200-209
        "", "", "", "", "", "", "", "", "", "[", //210-219
        "\\", "]", "'", "", "", "", "", "", "", "", //220-229
        "", "", "", "", "", "", "", "", "", "", //230-239
        "", "", "", "", "", "", "", "", "", "", //240-249
        "", "", "", "", "", ""
    }; //250-255

    private static readonly string[] shiftedKeys =
    {
        "", "", "", "", "", "", "", "", "", "",
        "", "", "", "", "", "", "", "", "", "",
        "", "", "", "", "", "", "", "", "", "",
        "", "", " ", "", "", "", "", "", "", "", //30-39
        "", "", "", "", "", "", "", "", ")", "!", //40-49
        "@", "#", "$", "%", "^", "&", "*", "(", "", "", //50-59
        "", "", "", "", "", "A", "B", "C", "D", "E", //60-69
        "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", //70-79
        "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", //80-89
        "Z", "", "", "", "", "", "", "", "", "", //90-99
        "", "", "", "", "", "", "*", "+", "", "-", //100-109
        ".", "/", "", "", "", "", "", "", "", "", //110-119
        "", "", "", "", "", "", "", "", "", "", //120-129
        "", "", "", "", "", "", "", "", "", "", //130-139
        "", "", "", "", "", "", "", "", "", "", //140-149
        "", "", "", "", "", "", "", "", "", "", //150-159
        "", "", "", "", "", "", "", "", "", "", //160-169
        "", "", "", "", "", "", "", "", "", "", //170-179
        "", "", "", "", "", "", ":", "+", "<", "_", //180-189
        ">", "?", "~", "", "", "", "", "", "", "", //190-199
        "", "", "", "", "", "", "", "", "", "", //200-209
        "", "", "", "", "", "", "", "", "", "{", //210-219
        "|", "}", "\"", "", "", "", "", "", "", "", //220-229
        "", "", "", "", "", "", "", "", "", "", //230-239
        "", "", "", "", "", "", "", "", "", "", //240-249
        "", "", "", "", "", ""
    }; //250-255
}