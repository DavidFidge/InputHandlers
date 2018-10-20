using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using InputHandlers.Keyboard;
using InputHandlers.Mouse;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace InputHandlers.Sample
{
    public class InputHandler : IMouseHandler, IKeyboardHandler
    {
        private enum KeyboardLabelTypes
        {
            CurrentState,
            KeyDown,
            KeyLost,
            KeyRepeat,
            KeysReleased,
            RunningKeys,
            Example
        };

        private enum MouseLabelTypes
        {
            CurrentState,
            ScrollWheel,
            Moving,
            LeftClick,
            LeftDoubleClick,
            LeftDown,
            LeftUp,
            LeftDragging,
            LeftDraggingDone,
            RightClick,
            RightDoubleClick,
            RightDown,
            RightUp,
            RightDragging,
            RightDraggingDone,
            MiddleClick,
            MiddleDoubleClick,
            MiddleDown,
            MiddleUp,
            MiddleDragging,
            MiddleDraggingDone
        }

        private Dictionary<KeyboardLabelTypes, SimpleLabel> _keyboardLabels;
        private Dictionary<MouseLabelTypes, SimpleLabel> _mouseLabels;
        private Stopwatch _realTimer;

        public void HandleKeyboardKeyDown(Keys[] keysDown, Keys keyInFocus, KeyboardModifier keyboardModifier)
        {
            _keyboardLabels[KeyboardLabelTypes.KeyDown].HighlightRed(_realTimer);
            WriteCurrentKeysToTextbox(keysDown, keyboardModifier);
            WriteTextToTextbox(keyInFocus, keyboardModifier);
        }

        public void HandleKeyboardKeyLost(Keys[] keysDown, KeyboardModifier keyboardModifier)
        {
            _keyboardLabels[KeyboardLabelTypes.KeyLost].HighlightRed(_realTimer);
            WriteCurrentKeysToTextbox(keysDown, keyboardModifier);
        }

        public void HandleKeyboardKeyRepeat(Keys repeatingKey, KeyboardModifier keyboardModifier)
        {
            _keyboardLabels[KeyboardLabelTypes.KeyRepeat].Activate();
            WriteTextToTextbox(repeatingKey, keyboardModifier);
        }

        public void HandleKeyboardKeysReleased()
        {
            _keyboardLabels[KeyboardLabelTypes.KeysReleased].HighlightRed(_realTimer);
            _keyboardLabels[KeyboardLabelTypes.RunningKeys].Text = "Running Keys: ";
            _keyboardLabels[KeyboardLabelTypes.Example].Text = "No Keys Down";
        }

        public void HandleMouseScrollWheelMove(MouseState mouseState, int difference)
        {
            _mouseLabels[MouseLabelTypes.ScrollWheel].HighlightRed(_realTimer);
        }

        public void HandleMouseMoving(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.Moving].Activate();
        }

        public void HandleLeftMouseClick(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.LeftClick].HighlightRed(_realTimer);
        }

        public void HandleLeftMouseDoubleClick(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.LeftDoubleClick].HighlightRed(_realTimer);
        }

        public void HandleLeftMouseDown(MouseState mouseState)
        {
            _mouseLabels[MouseLabelTypes.LeftDown].Activate();
        }

        public void HandleLeftMouseUp(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.LeftDown].Deactivate();
            _mouseLabels[MouseLabelTypes.LeftUp].HighlightRed(_realTimer);
        }

        public void HandleLeftMouseDragging(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.LeftDown].Deactivate();
            _mouseLabels[MouseLabelTypes.LeftDragging].Activate();
        }

        public void HandleLeftMouseDragDone(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.LeftDragging].Deactivate();
            _mouseLabels[MouseLabelTypes.LeftDraggingDone].HighlightRed(_realTimer);
        }

        public void HandleRightMouseClick(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.RightClick].HighlightRed(_realTimer);
        }

        public void HandleRightMouseDoubleClick(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.RightDoubleClick].HighlightRed(_realTimer);
        }

        public void HandleRightMouseDown(MouseState mouseState)
        {
            _mouseLabels[MouseLabelTypes.RightDown].Activate();
        }

        public void HandleRightMouseUp(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.RightDown].Deactivate();
            _mouseLabels[MouseLabelTypes.RightUp].HighlightRed(_realTimer);
        }

        public void HandleRightMouseDragging(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.RightDown].Deactivate();
            _mouseLabels[MouseLabelTypes.RightDragging].Activate();
        }

        public void HandleRightMouseDragDone(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.RightDragging].Deactivate();
            _mouseLabels[MouseLabelTypes.RightDraggingDone].HighlightRed(_realTimer);
        }

        public void HandleMiddleMouseClick(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.MiddleClick].HighlightRed(_realTimer);
        }

        public void HandleMiddleMouseDoubleClick(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.MiddleDoubleClick].HighlightRed(_realTimer);
        }

        public void HandleMiddleMouseDown(MouseState mouseState)
        {
            _mouseLabels[MouseLabelTypes.MiddleDown].Activate();
        }

        public void HandleMiddleMouseUp(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.MiddleDown].Deactivate();
            _mouseLabels[MouseLabelTypes.MiddleUp].HighlightRed(_realTimer);
        }

        public void HandleMiddleMouseDragging(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.MiddleDown].Deactivate();
            _mouseLabels[MouseLabelTypes.MiddleDragging].Activate();
        }

        public void HandleMiddleMouseDragDone(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[MouseLabelTypes.MiddleDragging].Deactivate();
            _mouseLabels[MouseLabelTypes.MiddleDraggingDone].HighlightRed(_realTimer);
        }

        public void Initialise()
        {
            _realTimer = new Stopwatch();
            _realTimer.Start();

            _mouseLabels = new Dictionary<MouseLabelTypes, SimpleLabel>();
            _keyboardLabels = new Dictionary<KeyboardLabelTypes, SimpleLabel>();

            var currentPosition = new Vector2(400.0f, 30.0f);
            var gap = new Vector2(0.0f, 30.0f);

            _keyboardLabels.Add(KeyboardLabelTypes.CurrentState, new SimpleLabel(currentPosition += gap, "Current State: "));
            _keyboardLabels.Add(KeyboardLabelTypes.KeyDown, new SimpleLabel(currentPosition += gap, "HandleKeyboardKeyDown Fired"));
            _keyboardLabels.Add(KeyboardLabelTypes.KeyLost, new SimpleLabel(currentPosition += gap, "HandleKeyboardKeyLost Fired"));
            _keyboardLabels.Add(KeyboardLabelTypes.KeyRepeat, new SimpleLabel(currentPosition += gap, "HandleKeyboardKeyRepeat Fired"));
            _keyboardLabels.Add(KeyboardLabelTypes.KeysReleased, new SimpleLabel(currentPosition += gap, "HandleKeyboardKeysReleased Fired"));
            _keyboardLabels.Add(KeyboardLabelTypes.RunningKeys, new SimpleLabel(currentPosition += gap, "Running Keys: "));
            _keyboardLabels.Add(KeyboardLabelTypes.Example, new SimpleLabel(currentPosition += gap, "Textbox Text Example: "));

            currentPosition = new Vector2(30.0f, 30.0f);

            _mouseLabels.Add(MouseLabelTypes.CurrentState, new SimpleLabel(currentPosition += gap, "Current State: "));
            _mouseLabels.Add(MouseLabelTypes.ScrollWheel, new SimpleLabel(currentPosition += gap, "HandleMouseScrollWheelMove Fired"));
            _mouseLabels.Add(MouseLabelTypes.Moving, new SimpleLabel(currentPosition += gap, "HandleMouseMoving Fired"));
            _mouseLabels.Add(MouseLabelTypes.LeftClick, new SimpleLabel(currentPosition += gap, "HandleLeftMouseClick Fired"));
            _mouseLabels.Add(MouseLabelTypes.LeftDoubleClick, new SimpleLabel(currentPosition += gap, "HandleLeftMouseDoubleClick Fired"));
            _mouseLabels.Add(MouseLabelTypes.LeftDown, new SimpleLabel(currentPosition += gap, "HandleLeftMouseDown Fired"));
            _mouseLabels.Add(MouseLabelTypes.LeftUp, new SimpleLabel(currentPosition += gap, "HandleLeftMouseUp Fired"));
            _mouseLabels.Add(MouseLabelTypes.LeftDragging, new SimpleLabel(currentPosition += gap, "HandleLeftMouseDragging Fired"));
            _mouseLabels.Add(MouseLabelTypes.LeftDraggingDone, new SimpleLabel(currentPosition += gap, "HandleLeftMouseDragDone Fired"));
            _mouseLabels.Add(MouseLabelTypes.RightClick, new SimpleLabel(currentPosition += gap, "HandleRightMouseClick Fired"));
            _mouseLabels.Add(MouseLabelTypes.RightDoubleClick, new SimpleLabel(currentPosition += gap, "HandleRightMouseDoubleClick Fired"));
            _mouseLabels.Add(MouseLabelTypes.RightDown, new SimpleLabel(currentPosition += gap, "HandleRightMouseDown Fired"));
            _mouseLabels.Add(MouseLabelTypes.RightUp, new SimpleLabel(currentPosition += gap, "HandleRightMouseUp Fired"));
            _mouseLabels.Add(MouseLabelTypes.RightDragging, new SimpleLabel(currentPosition += gap, "HandleRightMouseDragging Fired"));
            _mouseLabels.Add(MouseLabelTypes.RightDraggingDone, new SimpleLabel(currentPosition += gap, "HandleRightMouseDragDone Fired"));
            _mouseLabels.Add(MouseLabelTypes.MiddleClick, new SimpleLabel(currentPosition += gap, "HandleMiddleMouseClick Fired"));
            _mouseLabels.Add(MouseLabelTypes.MiddleDoubleClick, new SimpleLabel(currentPosition += gap, "HandleMiddleMouseDoubleClick Fired"));
            _mouseLabels.Add(MouseLabelTypes.MiddleDown, new SimpleLabel(currentPosition += gap, "HandleMiddleMouseDown Fired"));
            _mouseLabels.Add(MouseLabelTypes.MiddleUp, new SimpleLabel(currentPosition += gap, "HandleMiddleMouseUp Fired"));
            _mouseLabels.Add(MouseLabelTypes.MiddleDragging, new SimpleLabel(currentPosition += gap, "HandleMiddleMouseDragging Fired"));
            _mouseLabels.Add(MouseLabelTypes.MiddleDraggingDone, new SimpleLabel(currentPosition += gap, "HandleMiddleMouseDragDone Fired"));
        }

        public void UpdateLabelsBeforePoll()
        {
            _mouseLabels[MouseLabelTypes.Moving].Deactivate();
            _keyboardLabels[KeyboardLabelTypes.KeyRepeat].Deactivate();
        }

        public void UpdateLabelsAfterPoll(GameTime gameTime, MouseInput mouseInput, Keyboard.KeyboardInput keyboardInput)
        {
            _mouseLabels[MouseLabelTypes.CurrentState].Text = "Current State: " + mouseInput.CurrentStateAsString();
            _keyboardLabels[KeyboardLabelTypes.CurrentState].Text = "Current State: " + keyboardInput.GetCurrentStateTypeName();

            foreach (var label in _mouseLabels.Values)
                label.Update(gameTime, _realTimer);

            foreach (var label in _keyboardLabels.Values)
                label.Update(gameTime, _realTimer);
        }

        public void DrawLabels(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            foreach (var label in _mouseLabels.Values)
                label.Draw(spriteBatch, spriteFont);

            foreach (var label in _keyboardLabels.Values)
                label.Draw(spriteBatch, spriteFont);
        }

        public void WriteTextToTextbox(Keys focus, KeyboardModifier keyboardModifier)
        {
            _keyboardLabels[KeyboardLabelTypes.RunningKeys].Text += focus.Display(keyboardModifier);

            if ((focus == Keys.Back) && (_keyboardLabels[KeyboardLabelTypes.RunningKeys].Text.Length > 22))
                _keyboardLabels[KeyboardLabelTypes.RunningKeys].Text = _keyboardLabels[KeyboardLabelTypes.RunningKeys].Text.Remove(_keyboardLabels[KeyboardLabelTypes.RunningKeys].Text.Length - 1, 1);
        }

        public void WriteCurrentKeysToTextbox(Keys[] keyList, KeyboardModifier keyboardModifier)
        {
            // Note - sometimes more than 2 keys wont register.  See this for explanation of keyboard hardware limitations:
            // http://blogs.msdn.com/shawnhar/archive/2007/03/28/keyboards-suck.aspx
            _keyboardLabels[KeyboardLabelTypes.Example].Text = "Current Keys: ";
            foreach (var k in keyList)
                _keyboardLabels[KeyboardLabelTypes.Example].Text += k.ToString();
            if ((KeyboardModifier.Alt & keyboardModifier) == KeyboardModifier.Alt)
                _keyboardLabels[KeyboardLabelTypes.Example].Text += " +Alt ";
            if ((KeyboardModifier.Shift & keyboardModifier) == KeyboardModifier.Shift)
                _keyboardLabels[KeyboardLabelTypes.Example].Text += " +Shift ";
            if ((KeyboardModifier.Ctrl & keyboardModifier) == KeyboardModifier.Ctrl)
                _keyboardLabels[KeyboardLabelTypes.Example].Text += " +Ctrl ";
        }
    }
}