using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace InputHandlers.Mouse
{
    /// <summary>
    ///     IMouseHandler provides the functions to implement handlers for mouse events.
    ///     <para>
    ///         Give this interface to any class that you want to directly or indirectly deal with events.  Write the
    ///         implementation for each event (for those events you dont want to implement, write a function with does nothing
    ///         - because this is an interface, you must provide an implementation for all functions).  In the addevents
    ///         function, add the handlers you want MouseHandler to call to the corresponding MouseHandler event object using
    ///         the appropriate delegate provided.
    ///     </para>
    /// </summary>
    /// <example>
    ///     If you wanted a class to handle a left mouse click do the following:
    ///     <code>
    /// Class MyClass : IMouseHandler
    /// {
    ///     MouseHandler mh;
    ///     void HandleLeftMouseClick(MouseState m)
    ///     {
    ///         ...your handling code here...
    ///     }
    ///     
    ///     void addmouseevents()
    ///     {
    ///         //add your handler by adding a new delegate (with your interface function as a parameter) to the event
    ///         mh.HandleLeftMouseClick += new MouseHandler.DelHandleLeftMouseClick(this.HandleLeftMouseClick);
    ///     }
    /// }
    /// </code>
    ///     Then whenever the user clicks the left mouse your HandleLeftMouseClick function will be called.
    /// </example>
    public interface IMouseHandler
    {
        /// <summary>
        ///     handle mouse wheel movement
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        /// <param name="diff">
        ///     direction and magnitude the user moved the wheel since last update.  Positive is down, negative is
        ///     up.
        /// </param>
        void HandleMouseScrollWheelMove(MouseState m, int diff);

        /// <summary>
        ///     handle mouse movement when neither left or right mouse buttons are down.  This event is continuously sent every
        ///     update while the mouse moves.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleMouseMoving(MouseState m);

        /// <summary>
        ///     handle left mouse click.  A mouse up event is sent just prior to this and is followed up by this event.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleLeftMouseClick(MouseState m);

        /// <summary>
        ///     handle left mouse double click.  Unlike a left mouse click, no mouse up event is sent for this action - this is
        ///     normal as unlike a single click which is processed on mouse up, a double click is processed immediately on the
        ///     mouse down.  Windows desktop works like this too.  The mouse up done after releasing from double click is
        ///     suppressed but the mouse state will remain in a mouse down state but will do absolutely nothing until the mouse
        ///     button is released.  Note, all actions as described in HandleLeftMouseClick WILL be performed for the first mouse
        ///     click in the double click sequence, so your code may have to consider this if handling both single click and double
        ///     click events.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleLeftMouseDoubleClick(MouseState m);

        /// <summary>
        ///     handle left mouse down.  If the user holds down the mouse button and moves the mouse past the threshold for
        ///     dragging then HandleLeftMouseDragging events will be sent afterwards.  If the user eventually releases the mouse in
        ///     the same place within the threshold then a mouse up and mouse click event will be sent.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleLeftMouseDown(MouseState m);

        /// <summary>
        ///     handle left mouse up.  This event is only called at the end of a single click or dragging is done.  It is not
        ///     called at the end of a double click.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleLeftMouseUp(MouseState m);

        /// <summary>
        ///     handle the situation where the mouse is being held down while the mouse is moving.  This event is continuously sent
        ///     every update while the mouse moves.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        /// <param name="origin">
        ///     state of the mouse when drag was initiated.  This can be used to retrieve the position where the
        ///     drag was initiated
        /// </param>
        void HandleLeftMouseDragging(MouseState m, MouseState origin);

        /// <summary>
        ///     handle left mouse drag completion.  A mouse up event is sent just prior to this event.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        /// <param name="origin">
        ///     state of the mouse when drag was initiated.  This can be used to retrieve the position where the
        ///     drag was initiated
        /// </param>
        void HandleLeftMouseDragDone(MouseState m, MouseState origin);

        /// <summary>
        ///     handle left mouse click.  A mouse up event is sent just prior to this and is followed up by this event.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleRightMouseClick(MouseState m);

        /// <summary>
        ///     handle right mouse double click.  See left mouse double click description for in depth info.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleRightMouseDoubleClick(MouseState m);

        /// <summary>
        ///     handle right mouse down.  If the user holds down the mouse button and moves the mouse past the threshold for
        ///     dragging then HandleLeftMouseDragging events will be sent afterwards.  If the user eventually releases the mouse in
        ///     the same place within the threshold then a mouse up and mouse click event will be sent.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleRightMouseDown(MouseState m);

        /// <summary>
        ///     handle right mouse up.  This event is only called at the end of a single click or dragging is done.  It is not
        ///     called at the end of a double click.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleRightMouseUp(MouseState m);

        /// <summary>
        ///     handle the situation where the mouse is being held down while the mouse is moving.  This event is continuously sent
        ///     every update while the mouse moves.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        /// <param name="origin">
        ///     state of the mouse when drag was initiated.  This can be used to retrieve the position where the
        ///     drag was initiated
        /// </param>
        void HandleRightMouseDragging(MouseState m, MouseState origin);

        /// <summary>
        ///     handle right mouse drag completion.  A mouse up event is sent just prior to this event.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        /// <param name="origin">
        ///     state of the mouse when drag was initiated.  This can be used to retrieve the position where the
        ///     drag was initiated
        /// </param>
        void HandleRightMouseDragDone(MouseState m, MouseState origin);
    }
}