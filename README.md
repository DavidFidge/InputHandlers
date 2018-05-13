# InputHandlers
A library for handling keyboard and mouse input in MonoGame.

## Overview
This library processes mouse and keyboard updates from XNA and broadcasts changes in state to subscribers.  You subscribe to changes by implementing IMouseHandler and IKeyboardHandler and passing them to the MouseInput and KeyboardInput classes.  Upon each Poll call, your implementations of IMouseHandler and IKeyboardHandler will receive calls if any change of state occurred.

A sample project is provided which shows how the events work and how to use the library.

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
