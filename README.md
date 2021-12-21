# InputHandlers
A library for handling keyboard and mouse input in MonoGame.  Compiled against DesktopGL.  A WindowsDX branch is also maintained.  NuGet packages are provided for both.  If using the source directly rather than nuget packages, you can git checkout WindowsDX to use the WindowsDX version.


## Overview
This library processes mouse and keyboard updates from XNA and broadcasts changes in state to subscribers.  You subscribe to changes by implementing IMouseHandler and IKeyboardHandler and passing them to MouseInput and KeyboardInput.  Upon each Poll call, your subscription(s) will receive calls if any change of state occurred.

A sample project is provided which shows how the events work and how to use the library.

The library has 80% test code coverage.


## Example - Keyboard

1) In your application, create a new KeyboardInput object.  You will want to do this in the initialization phase of your game.

    public class MyGame
    {
        KeyboardInput _keyboardInput;
        
        public MyGame()
        {
            _keyboardInput = new KeyboardInput();
        }
    }


2) Create a new class that inherits from IKeyboardHandler.  Implement all the methods from the interface.

    public class KeyboardHandler : IKeyboardHandler
    {
        public void HandleKeyboardKeyDown(Keys[] keysDown, Keys keyInFocus, KeyboardModifier keyboardModifier)
        {
            if (keyInFocus == Keys.A)
            {
                // Do Stuff in your game when the A key is pressed!
            }
        }

        // Rest of interface implementation goes here
    }


3)  Subscribe your new keyboard handler to KeyboardInput by calling KeyboardInput.Subscribe

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
        
4) In your game's Update method, call KeyboardInput.Poll

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


## Example - Mouse

Setting up a mouse is almost identical to a keyboard.  You create a MouseInput object, create a new mouse handler class that inherits from IMouseHandler, implementing the methods, subscribe the handler to MouseInput and call MouseInput.Poll in your game's Update method

        public MyGame()
        {
            _mouseInput = new MouseInput();
            _mouseHandler = new MouseHandler();
            
            _mouseInput.Subscribe(_mouseHandler);
        }
            
        protected override void Update(GameTime gameTime)
        {      
            _mouseInput.Poll(Microsoft.Xna.Framework.Input.Mouse.GetState());     
             base.Update(gameTime);
        }
        
        public class MouseHandler : IMouseHandler
        {
            // Your IMouseHandler implementation goes here.
        }


Your KeyboardHandler will now receive a callback every time a keyboard event occurs when polling the keyboard during a game update.  You can subscribe as many keyboard handlers as you want and they will all receive a callback.  You can even subscribe and unsubscribe within a callback of a handler.
 
The following properties can be changed on MouseInput to control how mouse input works:

DragVariance - defaults to 10 pixels - tolerance for when a left/right/middle button is held and moved is treated as a drag event rather than a left click.

DoubleClickDetectionTimeDelay - milliseconds - time where two clicks must occur for it to be treated as a double click rather than 2 single clicks.

IsLeftButtonEnabled, IsRightButtonEnabled, IsMiddleButtonEnabled - defaults to true - enables / disables the relevant button


## Keyboard Events
HandleKeyboardKeyDown

HandleKeyboardKeyLost

HandleKeyboardKeyRepeat

HandleKeyboardKeysReleased
  

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


## Version History

### 1.5.0
Fixed keyboard state constantly firing key down and key lost events when TreatModifiersAsKeys is false and a modifier key is pressed along with another key.
   
Key repeats can now occur if a new key is pressed and an older key lost. This fixes an issue where there might be several milliseconds where a user begins pressing a new key before releasing the old one. This new behaviour is aligned to the way that windows key repeating behaves.

Change target framework project setting to support both .Net Core 3.1 and .Net 5.0.

### 1.4.0
Updated to .NET 5.0

### 1.3.0
Updated to MonoGame 3.8 and .NET Core 3.1

### 1.2.0
Updated to MonoGame 3.7

### 1.1.0
Changed mouse handler parameters for mouse left, middle and right click events.  The first parameter is now the current mouse state.  The second parameter, 'origin', is the mouse state when the left, middle or right mouse button was first pressed.   This is the parameter you will typically want to use when processing the position of a click or double click.

Fixed exception when subscribing to handlers within a handle event of a handler.

### 1.0.1
Added interfaces for mouse handler and keyboard handler to make testing and dependency injection easier.

### 1.0.0
Initial release
