# InputHandlers
A library for handling keyboard and mouse input in MonoGame. The master branch is compiled against DesktopGL. A WindowsDX branch is also maintained. NuGet packages are provided for both. If using a project reference, git checkout master for DesktopGL and git checkout WindowsDX for WindowsDX.

## Overview
This library processes mouse and keyboard updates from MonoGame and broadcasts them out as common changes of state that user interfaces typically need. For the mouse, it detects mouse single clicks, double clicks, dragging etc. For the keyboard, it detects key down, key up, key repeating etc.

You can change various aspects of the behaviour, for example, how long to wait between clicks for a double click to be detected, how long a key is held down before a key starts repeating, whether shift/alt/ctrl are treated as keys or modifiers etc.

The library is subscriber-based. You implement IMouseHandler and IKeyboardHandler, then call Subscribe, passing in your handler. Upon each Poll call, your subscription(s) will receive calls if any change of state occurred.

You can have multiple handlers subscribed at once and remove or add subscribed handlers at any time. This works great for screen transitions - for example, in a game the player may press 'i' to open their inventory. At that point you can swap out a keyboard handler that manages the main game for a keyboard handler that manages the inventory screen.

A sample project is provided which shows how the events work and how to use the library.

The library has 83% test coverage.

This library vastly reduces the amount of boilerplate, repetitive code that you would have to implement yourself in your own project.

## Example - Keyboard

1) In your application, create a new KeyboardInput object.  You will want to do this in the initialization phase of your game.

```
    public class MyGame
    {
        KeyboardInput _keyboardInput;
        
        public MyGame()
        {
            _keyboardInput = new KeyboardInput();
        }
    }
```

2) Create a new class that inherits from IKeyboardHandler.  Implement all the methods from the interface.

```
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
```

3)  Subscribe your new keyboard handler to KeyboardInput by calling KeyboardInput.Subscribe

```
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
    }
```
        
4) In your game's Update method, call KeyboardInput.Poll

```
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
    }
```

Your KeyboardHandler will now receive a callback every time a keyboard event occurs when polling the keyboard during a game update.  You can subscribe as many keyboard handlers as you want and they will all receive a callback.  You can even subscribe and unsubscribe within a callback of a handler.

The following properties can be changed on KeyboardInput to control how keyboard input works:

**IsRepeatEnabled** - defaults to true - when true enables keyboard key repeat events when keyboard keys are held down

**TreatModifiersAsKeys** - defaults to false - when true, shift alt and delete become keys in their own right and you'll receive key events for them.  If false, they are treated as modifiers only and will not send their own key events.

**RepeatDelay** - milliseconds - how long to hold down a key before it starts sending key repeat events

**RepeatFrequency** - milliseconds - how often repeat events occur once key repeat events start occurring

**UnmanagedKeys** - list of keys which you do not want the keyboard handler to handle

**WaitForNeutralStateBeforeApplyingNewSubscriptions** - Whether to wait for a neutral keyboard state before applying new pending subscriptions. Removal of subscriptions is still performed immediately. Defaults to False.

## Example - Mouse

Setting up a mouse is almost identical to a keyboard.  You create a MouseInput object, create a new mouse handler class that inherits from IMouseHandler, implementing the methods, subscribe the handler to MouseInput and call MouseInput.Poll in your game's Update method

```
    public class MyGame
    {
        MouseInput _mouseInput;
        MouseHandler _mouseHandler;

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
    }
        
    public class MouseHandler : IMouseHandler
    {
        // Your IMouseHandler implementation goes here.
    }
```

Your MouseHandler will now receive a callback every time a mouse event occurs when polling the mouse during a game update.  You can subscribe as many mouse handlers as you want and they will all receive a callback.  You can even subscribe and unsubscribe within a callback of a handler.
 
The following properties can be changed on MouseInput to control how mouse input works:

**DragVariance** - defaults to 10 pixels - tolerance for when a left/right/middle button is held and moved is treated as a drag event rather than a left click.

**DoubleClickDetectionTimeDelay** - milliseconds - time where two clicks must occur for it to be treated as a double click rather than 2 single clicks.

**IsLeftButtonEnabled, IsRightButtonEnabled, IsMiddleButtonEnabled** - defaults to true - enables / disables the relevant button

**WaitForNeutralStateBeforeApplyingNewSubscriptions** - Whether to wait for a neutral mouse state before applying new pending subscriptions. Removal of subscriptions is still performed immediately. Defaults to False. You may want to use this in the event where you detected a mouse down event to close a window and changed the mouse handler subscriptions. It will stop the subsequent mouse up and click event going to the new handler on the next poll. Note it will not suppress a double click event - you would need to call ResetDoubleClickDetection().

**ResetDoubleClickDetection()** - Resets double click detection (clears left, middle and right all at once). Useful for certain scenarios where you don't want the possibility of a double click being performed after a single click (or other action) has been handled and you don't want a double click to happen.

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

### 1.7.0
Pending subscriptions are now internally time stamped. Prior to this, if you called Remove subscription and immediately performed Add with one of the added objects being one of those in the set being removed then the add would not happen. Now the order of remove and add calls are known and this scenario will now work as expected.

Added WaitForNeutralStateBeforeApplyingNewSubscriptions flag so that you can suppress events being fired to a new subscription until a neutral state has been achieved (i.e. for mouse, mouse is not moving and no buttons pressed, and for keyboard, no keys are down).

Added ResetDoubleClickDetection() method so that you can manually reset the double click detection state. As an example, this can be useful if you want to suppress a double click from happening after a single click has been handled. 

### 1.6.0
Updated to .NET 6 and MonoGame 3.8.1

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
