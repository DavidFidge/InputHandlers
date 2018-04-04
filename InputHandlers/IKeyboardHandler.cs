using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace InputHandlers.Keyboard
{
    /// <summary>
    ///     IKeyboardHandler provides the functions to implement handlers for mouse events.
    ///     <para>
    ///         Give this interface to any class that you want to directly or indirectly deal with events.  Write the
    ///         implementation for each event (for those events you dont want to implement, write a function with does nothing
    ///         - because this is an interface, you must provide an implementation for all functions).  In the addevents
    ///         function, add the handlers you want KeyboardHandler to call to the corresponding KeyboardHandler event object using the
    ///         appropriate delegate provided.
    ///     </para>
    /// </summary>
    /// <example>
    ///     If you wanted a class to handle a kbdown event do the following:
    ///     <code>
    /// Class MyClass : IKeyboardHandler
    /// {
    ///     KeyboardHandler kh;
    ///     HandleKeyboardKeyDown(Keys[] klist, Keys focus, KeyboardModifier m) 
    ///     {
    ///         ...your handling code here...
    ///     }
    ///     
    ///     void AddKeyboardEvents()
    ///     {
    ///         //add your handler by adding a new delegate (with your interface function as a parameter) to the event
    ///         kh.HandleKeyboardKeyDown += new KeyboardHandler.DelHandleKBKeyDown(this.HandleKeyboardKeyDown);
    ///     }
    /// }
    /// </code>
    ///     Then whenever the user presses a keyboard key your HandleKeyboardKeyDown function will be called.
    /// </example>
    public interface IKeyboardHandler
    {
        /// <summary>
        ///     handle a key down event.  A unique call is made to this event every single time an individual key is pressed.  The
        ///     'focus' key of each call is retrievable through the focus parameter.
        /// </summary>
        /// <param name="klist">
        ///     The entire set of keys currently held down on the keyboard (please note that number of keys
        ///     detectable is subject to hardware limitations on your keyboard)
        /// </param>
        /// <param name="focus">The 'focus' key of this event.</param>
        /// <param name="m">a bit field holding control, alt and shift key status</param>
        void HandleKeyboardKeyDown(Keys[] klist, Keys focus, KeyboardModifier m);

        /// <summary>
        ///     handle a key lost event.  This occurs when multiple keys are held down and then a key is released but soome keys
        ///     are still being held.
        /// </summary>
        /// <param name="klist">
        ///     The entire set of keys currently held down on the keyboard (please note that number of keys
        ///     detectable is subject to hardware limitations on your keyboard)
        /// </param>
        /// <param name="m">a bit field holding control, alt and shift key status</param>
        void HandleKeyboardKeyLost(Keys[] klist, KeyboardModifier m);

        /// <summary>
        ///     handle a key repeat event.  Once the repeat delay threshold is exceeded when the same key(s) are held down for long
        ///     enough then the program will start sending key repeat events on every update.
        /// </summary>
        /// <param name="repeatkey">the key that is to be repeated.  This is always the last key held down.</param>
        /// <param name="m">a bit field holding control, alt and shift key status</param>
        void HandleKeyboardKeyRepeat(Keys repeatkey, KeyboardModifier m);

        /// <summary>
        ///     handle the situation where all keys have been released.
        /// </summary>
        void HandleKeyboardKeysReleased();
    }
}