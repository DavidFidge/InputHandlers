using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Input;

namespace InputHandlers.Mouse
{
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
    }
}