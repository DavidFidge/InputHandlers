namespace InputHandlers.Mouse;

public interface IMouseHandler
{
    /// <summary>
    /// Handle mouse wheel movement
    /// </summary>
    /// <param name="mouseState">state of mouse when handler was called</param>
    /// <param name="difference">
    /// Direction and magnitude the user moved the wheel since last update.  Positive is down, negative is up
    /// </param>
    void HandleMouseScrollWheelMove(MouseState mouseState, int difference);

    /// <summary>
    /// Handle mouse movement when neither left or right mouse buttons are down.  This event is continuously sent every
    /// update while the mouse moves.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    /// <param name="origin">Previous state of mouse contining position from last poll</param>
    void HandleMouseMoving(MouseState mouseState, MouseState origin);

    /// <summary>
    /// Handle left mouse click.  A mouse up event is sent just prior to this and is followed up by this event.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    /// <param name="origin">State of mouse when button went down.  The position in here is generally the one you want to use for determining click location.</param>
    void HandleLeftMouseClick(MouseState mouseState, MouseState origin);

    /// <summary>
    /// Handle left mouse double click.  Unlike a left mouse click, no mouse up event is sent for this action - this is
    /// normal as unlike a single click which is processed on mouse up, a double click is processed immediately on the
    /// mouse down.  Windows works like this too.  The mouse up done after releasing from double click is
    /// suppressed but the mouse state will remain in a mouse down state but will do absolutely nothing until the mouse
    /// button is released.  Note, all actions as described in HandleLeftMouseClick WILL be performed for the first mouse
    /// click in the double click sequence, so your code may have to consider this if handling both single click and double
    /// click events.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    /// <param name="origin">State of mouse when button went down.  The position in here is generally the one you want to use for determining click location.</param>
    void HandleLeftMouseDoubleClick(MouseState mouseState, MouseState origin);

    /// <summary>
    /// Handle left mouse down.  If the user holds down the mouse button and moves the mouse past the threshold for
    /// dragging then HandleLeftMouseDragging events will be sent afterwards.  If the user eventually releases the mouse in
    /// the same place within the threshold then a mouse up and mouse click event will be sent.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    void HandleLeftMouseDown(MouseState mouseState);

    /// <summary>
    /// Handle left mouse up.  This event is only called at the end of a single click or dragging is done.  It is not
    /// called at the end of a double click.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    /// <param name="origin">State of mouse when button went down.  The position in here is generally the one you want to use for determining click location.</param>
    void HandleLeftMouseUp(MouseState mouseState, MouseState origin);

    /// <summary>
    /// Handle the situation where the mouse is being held down while the mouse is moving.  This event is continuously sent
    /// every update while the mouse moves.
    /// </summary>
    /// <param name="mouseState">state of mouse when handler was called</param>
    /// <param name="originalMouseState">
    /// State of the mouse when drag was initiated.  This can be used to retrieve the position where the
    /// drag was initiated
    /// </param>
    void HandleLeftMouseDragging(MouseState mouseState, MouseState originalMouseState);

    /// <summary>
    /// Handle left mouse drag completion.  A mouse up event is sent just prior to this event.
    /// </summary>
    /// <param name="mouseState">state of mouse when handler was called</param>
    /// <param name="originalMouseState">
    /// State of the mouse when drag was initiated.  This can be used to retrieve the position where the
    /// drag was initiated
    /// </param>
    void HandleLeftMouseDragDone(MouseState mouseState, MouseState originalMouseState);

    /// <summary>
    /// Handle left mouse click.  A mouse up event is sent just prior to this and is followed up by this event.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    /// <param name="origin">State of mouse when button went down.  The position in here is generally the one you want to use for determining click location.</param>
    void HandleRightMouseClick(MouseState mouseState, MouseState origin);

    /// <summary>
    /// Handle right mouse double click.  See left mouse double click description for in depth info.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    /// <param name="origin">State of mouse when button went down.  The position in here is generally the one you want to use for determining click location.</param>
    void HandleRightMouseDoubleClick(MouseState mouseState, MouseState origin);

    /// <summary>
    /// Handle right mouse down.  If the user holds down the mouse button and moves the mouse past the threshold for
    /// dragging then HandleLeftMouseDragging events will be sent afterwards.  If the user eventually releases the mouse in
    /// the same place within the threshold then a mouse up and mouse click event will be sent.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    void HandleRightMouseDown(MouseState mouseState);

    /// <summary>
    /// Handle right mouse up.  This event is only called at the end of a single click or dragging is done.  It is not
    /// called at the end of a double click.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    /// <param name="origin">State of mouse when button went down.  The position in here is generally the one you want to use for determining click location.</param>
    void HandleRightMouseUp(MouseState mouseState, MouseState origin);

    /// <summary>
    /// Handle the situation where the mouse is being held down while the mouse is moving.  This event is continuously sent
    /// every update while the mouse moves.
    /// </summary>
    /// <param name="mouseState">state of mouse when handler was called</param>
    /// <param name="originalMouseState">
    /// State of the mouse when drag was initiated.  This can be used to retrieve the position where the
    /// drag was initiated
    /// </param>
    void HandleRightMouseDragging(MouseState mouseState, MouseState originalMouseState);

    /// <summary>
    /// Handle right mouse drag completion.  A mouse up event is sent just prior to this event.
    /// </summary>
    /// <param name="mouseState">state of mouse when handler was called</param>
    /// <param name="originalMouseState">
    /// State of the mouse when drag was initiated.  This can be used to retrieve the position where the
    /// drag was initiated
    /// </param>
    void HandleRightMouseDragDone(MouseState mouseState, MouseState originalMouseState);

    /// <summary>
    /// Handle left mouse click.  A mouse up event is sent just prior to this and is followed up by this event.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    /// <param name="origin">State of mouse when button went down.  The position in here is generally the one you want to use for determining click location.</param>
    void HandleMiddleMouseClick(MouseState mouseState, MouseState origin);

    /// <summary>
    /// Handle middle mouse double click.  See left mouse double click description for in depth info.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    /// <param name="origin">State of mouse when button went down.  The position in here is generally the one you want to use for determining click location.</param>
    void HandleMiddleMouseDoubleClick(MouseState mouseState, MouseState origin);

    /// <summary>
    /// Handle middle mouse down.  If the user holds down the mouse button and moves the mouse past the threshold for
    /// dragging then HandleLeftMouseDragging events will be sent afterwards.  If the user eventually releases the mouse in
    /// the same place within the threshold then a mouse up and mouse click event will be sent.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    void HandleMiddleMouseDown(MouseState mouseState);

    /// <summary>
    /// Handle middle mouse up.  This event is only called at the end of a single click or dragging is done.  It is not
    /// called at the end of a double click.
    /// </summary>
    /// <param name="mouseState">State of mouse when handler was called</param>
    /// <param name="origin">State of mouse when button went down.  The position in here is generally the one you want to use for determining click location.</param>
    void HandleMiddleMouseUp(MouseState mouseState, MouseState origin);

    /// <summary>
    /// Handle the situation where the mouse is being held down while the mouse is moving.  This event is continuously sent
    /// every update while the mouse moves.
    /// </summary>
    /// <param name="mouseState">state of mouse when handler was called</param>
    /// <param name="originalMouseState">
    /// State of the mouse when drag was initiated.  This can be used to retrieve the position where the
    /// drag was initiated
    /// </param>
    void HandleMiddleMouseDragging(MouseState mouseState, MouseState originalMouseState);

    /// <summary>
    /// Handle middle mouse drag completion.  A mouse up event is sent just prior to this event.
    /// </summary>
    /// <param name="mouseState">state of mouse when handler was called</param>
    /// <param name="originalMouseState">
    /// State of the mouse when drag was initiated.  This can be used to retrieve the position where the
    /// drag was initiated
    /// </param>
    void HandleMiddleMouseDragDone(MouseState mouseState, MouseState originalMouseState);
}