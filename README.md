# InputHandlers
A library for handling keyboard and mouse input in MonoGame.

## Overview
This library processes mouse and keyboard updates from XNA and broadcasts changes in state to subscribers.  You subscribe to changes by implementing IMouseHandler and IKeyboardHandler and passing them to MouseInput and KeyboardInput.  Upon each Poll call, your subscription(s) will receive calls if any change of state occurred.

A sample project is provided which shows how the events work and how to use the library.

The library has 80% test code coverage.


## Example
To set up a keyboard handler:

Create a class that inherits from IKeyboardHandler:

    public class KeyboardHandler : IKeyboardHandler
    {
        public void HandleKeyboardKeyDown(Keys[] keysDown, Keys keyInFocus, KeyboardModifier keyboardModifier)
        {
            if (keyInFocus == Keys.A)
            {
                // Do Stuff
            }
        }

        // Rest of interface implementation goes here
    }

In your application, create a new KeyboardInput object.  You will want to do this in the initialization phase of your game.

Create your KeyboardHandler object and call KeyboardInput.Subscribe.  Your KeyboardHandler will now receive a callback every time a keyboard event occurs when polling the keyboard during a game update.  You can subscribe as many keyboard handlers as you want and they will all receive a callback.  You can even subscribe and unsubscribe within a callback of a handler.

In the game's Update method, call KeyboardInput.Poll, passing in MonoGame's Keyboard.GetState().

    public class MyGame
    {
        KeyboardInput _keyboardInput;
        KeyboardHandler _keyboardHandler;
        
        public MyGame()
        {
            _keyboardInput = new KeyboardInput();
            _keyboardHandler = new KeyboardHandler();
            
            _keyboardInput.Subscribe(_keyboardHandler);
        }
            
        protected override void Update(GameTime gameTime)
        {      
            _keyboardInput.Poll(Microsoft.Xna.Framework.Input.Keyboard.GetState());     
             base.Update(gameTime);
        }
        
The process for setting up a mouse handler is exactly the same - use the MouseInput class and IMouseHandler interface.

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
