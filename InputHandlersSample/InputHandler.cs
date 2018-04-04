using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using InputHandlers.Keyboard;
using InputHandlers.Mouse;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace InputHandlersSample
{
    public class InputHandler : IMouseHandler, IKeyboardHandler
    {
        private List<SimpleLabel> _keyboardLabels;
        private List<SimpleLabel> _mouseLabels;
        private Stopwatch _realTimer;

        public void HandleKeyboardKeyDown(Keys[] keysDown, Keys keyInFocus, KeyboardModifier keyboardModifier)
        {
            _keyboardLabels[1].HighlightRed(_realTimer);
            _keyboardLabels[5].Text += keyInFocus.ToString();
            WriteCurrentKeysToTextbox(keysDown, keyboardModifier);
            WriteTextToTextbox(keyInFocus, keyboardModifier);
        }

        public void HandleKeyboardKeyLost(Keys[] keysDown, KeyboardModifier keyboardModifier)
        {
            _keyboardLabels[2].HighlightRed(_realTimer);
            WriteCurrentKeysToTextbox(keysDown, keyboardModifier);
        }

        public void HandleKeyboardKeyRepeat(Keys repeatingKey, KeyboardModifier keyboardModifier)
        {
            _keyboardLabels[3].Activate();
            _keyboardLabels[5].Text += repeatingKey.ToString();
            WriteTextToTextbox(repeatingKey, keyboardModifier);
        }

        public void HandleKeyboardKeysReleased()
        {
            _keyboardLabels[4].HighlightRed(_realTimer);
            _keyboardLabels[5].Text = "running keys: ";
            _keyboardLabels[7].Text = "no keys down";
        }

        public void HandleMouseScrollWheelMove(MouseState mouseState, int difference)
        {
            _mouseLabels[1].HighlightRed(_realTimer);
        }

        public void HandleMouseMoving(MouseState mouseState)
        {
            _mouseLabels[2].Activate();
        }

        public void HandleLeftMouseClick(MouseState mouseState)
        {
            _mouseLabels[3].HighlightRed(_realTimer);
        }

        public void HandleLeftMouseDoubleClick(MouseState mouseState)
        {
            _mouseLabels[4].HighlightRed(_realTimer);
        }

        public void HandleLeftMouseDown(MouseState mouseState)
        {
            _mouseLabels[5].Activate();
        }

        public void HandleLeftMouseUp(MouseState mouseState)
        {
            _mouseLabels[5].Deactivate();
            _mouseLabels[6].HighlightRed(_realTimer);
        }

        public void HandleLeftMouseDragging(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[5].Deactivate();
            _mouseLabels[7].Activate();
        }

        public void HandleLeftMouseDragDone(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[7].Deactivate();
            _mouseLabels[8].HighlightRed(_realTimer);
        }

        public void HandleRightMouseClick(MouseState mouseState)
        {
            _mouseLabels[9].HighlightRed(_realTimer);
        }

        public void HandleRightMouseDoubleClick(MouseState mouseState)
        {
            _mouseLabels[10].HighlightRed(_realTimer);
        }

        public void HandleRightMouseDown(MouseState mouseState)
        {
            _mouseLabels[11].Activate();
        }

        public void HandleRightMouseUp(MouseState mouseState)
        {
            _mouseLabels[11].Deactivate();
            _mouseLabels[12].HighlightRed(_realTimer);
        }

        public void HandleRightMouseDragging(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[11].Deactivate();
            _mouseLabels[13].Activate();
        }

        public void HandleRightMouseDragDone(MouseState mouseState, MouseState originalMouseState)
        {
            _mouseLabels[13].Deactivate();
            _mouseLabels[14].HighlightRed(_realTimer);
        }

        public void Initialise()
        {
            _realTimer = new Stopwatch();
            _realTimer.Start();

            _mouseLabels = new List<SimpleLabel>();
            _keyboardLabels = new List<SimpleLabel>();

            var currentPosition = new Vector2(400.0f, 30.0f);
            var gap = new Vector2(0.0f, 30.0f);

            _keyboardLabels.Add(new SimpleLabel(currentPosition += gap, "currentstate: "));
            _keyboardLabels.Add(new SimpleLabel(currentPosition += gap, "kbkeydown fired"));
            _keyboardLabels.Add(new SimpleLabel(currentPosition += gap, "kbkeylost fired"));
            _keyboardLabels.Add(new SimpleLabel(currentPosition += gap, "kbkeyrepeat fired"));
            _keyboardLabels.Add(new SimpleLabel(currentPosition += gap, "kbkeysreleased fired"));
            _keyboardLabels.Add(new SimpleLabel(currentPosition += gap, "running keys: "));
            _keyboardLabels.Add(new SimpleLabel(currentPosition += gap, "textbox text example: "));
            _keyboardLabels.Add(new SimpleLabel(currentPosition += gap, "no keys down"));

            currentPosition = new Vector2(30.0f, 30.0f);

            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "currentstate: "));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleMouseScrollWheelMove fired"));

            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleMouseMoving fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleLeftMouseClick fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleLeftMouseDoubleClick fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleLeftMouseDown fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleLeftMouseUp fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleLeftMouseDragging fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleLeftMouseDragDone fired"));

            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleRightMouseClick fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleRightMouseDoubleClick fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleRightMouseDown fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleRightMouseUp fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleRightMouseDragging fired"));
            _mouseLabels.Add(new SimpleLabel(currentPosition += gap, "HandleRightMouseDragDone fired"));
        }

        public void UpdateLabelsBeforePoll()
        {
            _mouseLabels[2].Deactivate();
            _keyboardLabels[3].Deactivate();
        }

        public void UpdateLabelsAfterPoll(GameTime gameTime, MouseHandler mouseHandler, KeyboardHandler keyboardHandler)
        {
            _mouseLabels[0].Text = "current state: " + mouseHandler.CurrentStateAsString();
            _keyboardLabels[0].Text = "current state: " + keyboardHandler.GetCurrentStateTypeName();

            foreach (var sl in _mouseLabels)
                sl.Update(gameTime, _realTimer);
            foreach (var sl in _keyboardLabels)
                sl.Update(gameTime, _realTimer);
        }

        public void DrawLabels(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            foreach (var sl in _mouseLabels)
                sl.Draw(spriteBatch, spriteFont);
            foreach (var sl in _keyboardLabels)
                sl.Draw(spriteBatch, spriteFont);
        }

        public void WriteTextToTextbox(Keys focus, KeyboardModifier m)
        {
            _keyboardLabels[6].Text += focus.Display(m);

            if ((focus == Keys.Back) && (_keyboardLabels[6].Text.Length > 22))
                _keyboardLabels[6].Text = _keyboardLabels[6].Text.Remove(_keyboardLabels[6].Text.Length - 1, 1);
        }

        public void WriteCurrentKeysToTextbox(Keys[] klist, KeyboardModifier m)
        {
            // Note - sometimes more than 2 keys wont register.  See this for explanation of keyboard hardware limitations:
            // http://blogs.msdn.com/shawnhar/archive/2007/03/28/keyboards-suck.aspx
            _keyboardLabels[7].Text = "current keys: ";
            foreach (var k in klist)
                _keyboardLabels[7].Text += k.ToString();
            if ((KeyboardModifier.Alt & m) == KeyboardModifier.Alt)
                _keyboardLabels[7].Text += " +alt ";
            if ((KeyboardModifier.Shift & m) == KeyboardModifier.Shift)
                _keyboardLabels[7].Text += " +shift ";
            if ((KeyboardModifier.Ctrl & m) == KeyboardModifier.Ctrl)
                _keyboardLabels[7].Text += " +ctrl ";
        }
    }
}