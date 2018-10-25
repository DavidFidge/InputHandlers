# InputHandlers
A library for handling keyboard and mouse input in MonoGame.

## Overview
This library processes mouse and keyboard updates from XNA and broadcasts changes in state to subscribers.  You subscribe to changes by implementing IMouseHandler and IKeyboardHandler and passing them to MouseInput and KeyboardInput.  Upon each Poll call, your subscription(s) will receive calls if any change of state occurred.

A sample project is provided which shows how the events work and how to use the library.

The library has 80% test code coverage.

## Mouse Events
HandleMouseScrollWheelMove

HandleMouseMoving

HandleLeftMouseClick

HandleLeftMouseDoubleClick

HandleLeftMouseDown

HandleLeftMouseUp

HandleLeftMouseDragging

HandleLeftMouseDragDone

HandleRightMouseClick

HandleRightMouseDoubleClick

HandleRightMouseDown

HandleRightMouseUp

HandleRightMouseDragging

HandleRightMouseDragDone

HandleMiddleMouseClick

HandleMiddleMouseDoubleClick

HandleMiddleMouseDown

HandleMiddleMouseUp

HandleMiddleMouseDragging

HandleMiddleMouseDragDone

## Keyboard Events
HandleKeyboardKeyDown

HandleKeyboardKeyLost

HandleKeyboardKeyRepeat

HandleKeyboardKeysReleased

## Version History
### 1.1.0
Changed mouse handler parameters for mouse left, middle and right click events.  The first parameter is now the current mouse state.  The second parameter, 'origin', is the mouse state when the left, middle or right mouse button was first pressed.   This is the parameter you will typically want to use when processing the position of a click or double click.

Fixed exception when subscribing to handlers within a handle event of a handler.

### 1.0.1
Added interfaces for mouse handler and keyboard handler to make testing and dependency injection easier.

### 1.0.0
Initial release
