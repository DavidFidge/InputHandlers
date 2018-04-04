using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using InputHandlers.State;
using InputHandlers.StateMachine;

using Microsoft.Xna.Framework.Input;

namespace InputHandlers.Mouse
{
    public class MouseHandler
    {
        private readonly StateMachine<MouseHandler> _mouseStateMachine;
        private readonly MouseStationaryState _mouseStationaryState;
        private readonly MouseMovingState _mouseMovingState;
        private readonly MouseLeftDownState _mouseLeftDownState;
        private readonly MouseLeftDraggingState _mouseLeftDraggingState;
        private readonly MouseRightDownState _mouseRightDownState;
        private readonly MouseRightDraggingState _mouseRightDraggingState;
        private readonly List<IMouseHandler> _mouseHandlers;

        public MouseState OldMouseState { get; private set; }
        public MouseState CurrentMouseState { get; private set; }
        public MouseState DragOriginPosition { get; private set; }
        public Stopwatch LastPollTime { get; private set; }

        /// <summary>
        /// DragVariance is a fudging factor for detecting the difference between mouse clicks and mouse drags.
        /// This is because a fast user may do a mouse click while slightly moving the mouse between mouse down and mouse up.
        /// If it wasnt for this fudging factor then it would go into drag mode which isnt what the user probably wanted.
        /// </summary>
        public uint DragVariance { get; set; }

        /// <summary>
        /// If time between clicks in milliseconds is less than this value then it is considered a double click
        /// </summary>
        public uint DoubleClickTime { get; set; }

        /// <summary>
        /// This is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time. 
        /// </summary>
        public uint UpdateNumber { get; private set; }

        /// <summary>
        /// Create mouse handler object and initialise state to stationary
        /// </summary>
        public MouseHandler()
        {
            _mouseHandlers = new List<IMouseHandler>();
            _mouseStationaryState = new MouseStationaryState();
            _mouseMovingState = new MouseMovingState();
            _mouseLeftDownState = new MouseLeftDownState();
            _mouseLeftDraggingState = new MouseLeftDraggingState();
            _mouseRightDownState = new MouseRightDownState();
            _mouseRightDraggingState = new MouseRightDraggingState();
            LastPollTime = new Stopwatch();
            LastPollTime.Start();
            UpdateNumber = 0;
            _mouseStateMachine = new StateMachine<MouseHandler>(this);
            _mouseStateMachine.SetCurrentState(_mouseStationaryState);
            _mouseStateMachine.SetPreviousState(_mouseStationaryState);
            DragVariance = 10;
            DoubleClickTime = 400;
        }

        public void Subscribe(IMouseHandler mouseHandler)
        {
            if (mouseHandler != null)
                _mouseHandlers.Add(mouseHandler);
        }

        public void Unsubscribe(IMouseHandler mouseHandler)
        {
            if (mouseHandler != null)
                _mouseHandlers.Remove(mouseHandler);
        }

        private void CallHandleMouseScrollWheelMove(MouseState m, int diff)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleMouseScrollWheelMove(m, diff);
            }
        }

        private void CallHandleMouseMoving(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleMouseMoving(m);
            }
        }

        private void CallHandleLeftMouseClick(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleLeftMouseClick(m);
            }
        }

        private void CallHandleLeftMouseDoubleClick(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleLeftMouseDoubleClick(m);
            }
        }

        private void CallHandleLeftMouseDown(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleLeftMouseDown(m);
            }
        }

        private void CallHandleLeftMouseUp(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleLeftMouseUp(m);
            }
        }

        private void CallHandleLeftMouseDragging(MouseState m, MouseState origin)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleLeftMouseDragging(m, origin);
            }
        }

        private void CallHandleLeftMouseDragDone(MouseState m, MouseState origin)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleLeftMouseDragDone(m, origin);
            }
        }

        private void CallHandleRightMouseClick(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleRightMouseClick(m);
            }
        }

        private void CallHandleRightMouseDoubleClick(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleRightMouseDoubleClick(m);
            }
        }

        private void CallHandleRightMouseDown(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleRightMouseDown(m);
            }
        }

        private void CallHandleRightMouseUp(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleRightMouseUp(m);
            }
        }

        private void CallHandleRightMouseDragging(MouseState m, MouseState origin)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleRightMouseDragging(m, origin);
            }
        }

        private void CallHandleRightMouseDragDone(MouseState m, MouseState origin)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleRightMouseDragDone(m, origin);
            }
        }

        /// <summary>
        /// Poll the mouse for updates.
        /// </summary>
        /// <param name="mouseState">a mouse state.  You should use the XNA input function, Mouse.GetState(), as this parameter.</param>
        public void Poll(MouseState mouseState)
        {
            UpdateNumber++;

            if (UpdateNumber == uint.MaxValue)
                UpdateNumber = 0;

            OldMouseState = CurrentMouseState;
            CurrentMouseState = mouseState;

            CheckScrollWheel();

            _mouseStateMachine.Update();
        }

        /// <summary>
        /// Reset to stationary state.  You may wish to call this when, for example, switching interface screens.
        /// </summary>
        public void Reset()
        {
            LastPollTime = new Stopwatch();
            LastPollTime.Start();
            UpdateNumber = 0;
            _mouseStateMachine.CurrentState.Reset(this);
            _mouseStateMachine.SetCurrentState(_mouseStationaryState);
            _mouseStateMachine.SetPreviousState(_mouseStationaryState);
        }

        public string CurrentStateAsString()
        {
            return _mouseStateMachine.GetCurrentStateTypeName();
        }

        private void CheckScrollWheel()
        {
            var diff = CurrentMouseState.ScrollWheelValue - OldMouseState.ScrollWheelValue;

            if (diff != 0)
                CallHandleMouseScrollWheelMove(CurrentMouseState, diff);
        }

        private sealed class MouseStationaryState : State<MouseHandler>
        {
            public override void Enter(MouseHandler mouseHandler)
            {
            }

            public override void Execute(MouseHandler mouseHandler)
            {
                // change state if mouse performs an action (left/right down or mouse moves), give left preference
                if (mouseHandler.CurrentMouseState.LeftButton == ButtonState.Pressed)
                    mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseLeftDownState);
                else if (mouseHandler.CurrentMouseState.RightButton == ButtonState.Pressed)
                    mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseRightDownState);
                else if ((mouseHandler.CurrentMouseState.X != mouseHandler.OldMouseState.X) || (mouseHandler.CurrentMouseState.Y != mouseHandler.OldMouseState.Y))
                    mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseMovingState);
            }

            public override void Exit(MouseHandler mouseHandler)
            {
            }

            public override void Reset(MouseHandler mouseHandler)
            {
            }
        }

        private sealed class MouseMovingState : State<MouseHandler>
        {
            public override void Enter(MouseHandler mouseHandler)
            {
                mouseHandler.CallHandleMouseMoving(mouseHandler.CurrentMouseState);
            }

            public override void Execute(MouseHandler e)
            {
                // give left preference
                if (e.CurrentMouseState.LeftButton == ButtonState.Pressed)
                    e._mouseStateMachine.ChangeState(e._mouseLeftDownState);
                else if (e.CurrentMouseState.RightButton == ButtonState.Pressed)
                    e._mouseStateMachine.ChangeState(e._mouseRightDownState);
                else if ((e.CurrentMouseState.X == e.OldMouseState.X) && (e.CurrentMouseState.Y == e.OldMouseState.Y))
                    e._mouseStateMachine.ChangeState(e._mouseStationaryState);
                else
                    e.CallHandleMouseMoving(e.CurrentMouseState);
            }

            public override void Exit(MouseHandler mouseHandler)
            {
            }

            public override void Reset(MouseHandler mouseHandler)
            {
            }
        }

        private sealed class MouseLeftDownState : State<MouseHandler>
        {
            private double _detectDoubleClickTime;
            private bool _wasDoubleClickDone;

            public MouseLeftDownState()
            {
                _detectDoubleClickTime = double.NegativeInfinity;
                _wasDoubleClickDone = false;
            }

            public override void Enter(MouseHandler mouseHandler)
            {
                // store the point where the initial click has been made
                mouseHandler.DragOriginPosition = mouseHandler.CurrentMouseState;

                if (_detectDoubleClickTime == double.NegativeInfinity)
                {
                    // first mouse down for detection, set double click time
                    _detectDoubleClickTime = mouseHandler.LastPollTime.Elapsed.TotalMilliseconds;
                    mouseHandler.CallHandleLeftMouseDown(mouseHandler.CurrentMouseState);
                }
                else
                {
                    // double click time has been set, subtract elapsed time since first click
                    _detectDoubleClickTime -= mouseHandler.LastPollTime.Elapsed.TotalMilliseconds;

                    if (_detectDoubleClickTime >= -mouseHandler.DoubleClickTime)
                    {
                        // double click has happened in the allowed timeframe, do a double click event
                        mouseHandler.CallHandleLeftMouseDoubleClick(mouseHandler.DragOriginPosition);

                        _detectDoubleClickTime = double.NegativeInfinity;

                        // rather than just change back to a stationary state, keep it consistent and wait until we get a mouse up (see execute)
                        _wasDoubleClickDone = true;
                    }
                    else
                    {
                        // delay between first mouse click and this 2nd click was too long.  Stay in this state.  However this could be the first click of another double click, so set double click time 
                        _detectDoubleClickTime = mouseHandler.LastPollTime.Elapsed.TotalMilliseconds;
                        mouseHandler.CallHandleLeftMouseDown(mouseHandler.CurrentMouseState);
                    }
                }
            }

            public override void Execute(MouseHandler mouseHandler)
            {
                if (_wasDoubleClickDone)
                {
                    // a double click was just done on the entry code.  Dont want to send a mouse up or anything when this happens.  Just want to wait for the user to release the mouse button.
                    if (mouseHandler.CurrentMouseState.LeftButton == ButtonState.Released)
                    {
                        _wasDoubleClickDone = false;
                        mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseStationaryState);
                    }
                }
                // check the 2 exit conditions - released mouse or moved mouse sufficiently to warrant dragging
                else if (mouseHandler.CurrentMouseState.LeftButton == ButtonState.Released)
                {
                    // send a mouse up then a mouse click event to the interface, but use old position
                    mouseHandler.CallHandleLeftMouseUp(mouseHandler.DragOriginPosition);
                    mouseHandler.CallHandleLeftMouseClick(mouseHandler.DragOriginPosition);
                    mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseStationaryState);
                }
                else if ((Math.Abs(mouseHandler.DragOriginPosition.X - mouseHandler.CurrentMouseState.X) > mouseHandler.DragVariance) ||
                         (Math.Abs(mouseHandler.DragOriginPosition.Y - mouseHandler.CurrentMouseState.Y) > mouseHandler.DragVariance))
                {
                    // dont go into drag unless it breaks the fudging factor threshold
                    mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseLeftDraggingState);
                }
            }

            public override void Exit(MouseHandler mouseHandler)
            {
            }

            public override void Reset(MouseHandler mouseHandler)
            {
                _detectDoubleClickTime = double.NegativeInfinity;
                _wasDoubleClickDone = false;
            }
        }

        private sealed class MouseLeftDraggingState : State<MouseHandler>
        {
            public override void Enter(MouseHandler mouseHandler)
            {
                mouseHandler.CallHandleLeftMouseDragging(mouseHandler.CurrentMouseState, mouseHandler.DragOriginPosition);
            }

            public override void Execute(MouseHandler mouseHandler)
            {
                if (mouseHandler.CurrentMouseState.LeftButton == ButtonState.Released)
                {
                    mouseHandler.CallHandleLeftMouseUp(mouseHandler.CurrentMouseState);
                    mouseHandler.CallHandleLeftMouseDragDone(mouseHandler.CurrentMouseState, mouseHandler.DragOriginPosition);
                    mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseStationaryState);
                }
                else if ((mouseHandler.CurrentMouseState.X != mouseHandler.OldMouseState.X) || (mouseHandler.CurrentMouseState.Y != mouseHandler.OldMouseState.Y))
                {
                    mouseHandler.CallHandleLeftMouseDragging(mouseHandler.CurrentMouseState, mouseHandler.DragOriginPosition);
                }
            }

            public override void Exit(MouseHandler mouseHandler)
            {
            }

            public override void Reset(MouseHandler mouseHandler)
            {
            }
        }

        private sealed class MouseRightDownState : State<MouseHandler>
        {
            private double _detectDoubleClickTime;
            private bool _wasDoubleClickDone;

            public MouseRightDownState()
            {
                _detectDoubleClickTime = double.NegativeInfinity;
                _wasDoubleClickDone = false;
            }

            public override void Enter(MouseHandler mouseHandler)
            {
                mouseHandler.DragOriginPosition = mouseHandler.CurrentMouseState;

                if (_detectDoubleClickTime == double.NegativeInfinity)
                {
                    // first mouse down for detection, set double click time
                    _detectDoubleClickTime = mouseHandler.LastPollTime.Elapsed.TotalMilliseconds;
                    
                    mouseHandler.CallHandleRightMouseDown(mouseHandler.CurrentMouseState);
                }
                else
                {
                    _detectDoubleClickTime -= mouseHandler.LastPollTime.Elapsed.TotalMilliseconds;

                    if (_detectDoubleClickTime >= -mouseHandler.DoubleClickTime)
                    {
                        // double click has happened in the allowed timeframe, do a double click event
                        mouseHandler.CallHandleRightMouseDoubleClick(mouseHandler.DragOriginPosition);

                        _detectDoubleClickTime = double.NegativeInfinity;

                        // rather than just change back to a stationary state, keep it consistent and wait until we get a mouse up (see execute)
                        _wasDoubleClickDone = true;
                    }
                    else
                    {
                        // delay between first mouse click and this 2nd click was too long.  Stay in this state.  However this could be the first click of another double click, so set double click time 
                        _detectDoubleClickTime = mouseHandler.LastPollTime.Elapsed.TotalMilliseconds;
                        mouseHandler.CallHandleRightMouseDown(mouseHandler.CurrentMouseState);
                    }
                }
            }

            public override void Execute(MouseHandler mouseHandler)
            {
                if (_wasDoubleClickDone)
                {
                    // a double click was just done on the entry code.  Dont want to send a mouse up or anything when this happens.  Just want to wait for the user to release the mouse button.
                    if (mouseHandler.CurrentMouseState.RightButton == ButtonState.Released)
                    {
                        _wasDoubleClickDone = false;
                        mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseStationaryState);
                    }
                }
                // check the 2 exit conditions - released mouse or moved mouse sufficiently to warrant dragging
                else if (mouseHandler.CurrentMouseState.RightButton == ButtonState.Released)
                {
                    // send a mouse up then a mouse click event to the interface, but use old position
                    mouseHandler.CallHandleRightMouseUp(mouseHandler.DragOriginPosition);
                    mouseHandler.CallHandleRightMouseClick(mouseHandler.DragOriginPosition);
                    mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseStationaryState);
                }
                else if ((Math.Abs(mouseHandler.DragOriginPosition.X - mouseHandler.CurrentMouseState.X) > mouseHandler.DragVariance) ||
                         (Math.Abs(mouseHandler.DragOriginPosition.Y - mouseHandler.CurrentMouseState.Y) > mouseHandler.DragVariance))
                {
                    // dont go into drag unless it breaks the fudging factor threshold
                    mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseRightDraggingState);
                }
            }

            public override void Exit(MouseHandler mouseHandler)
            {
            }

            public override void Reset(MouseHandler mouseHandler)
            {
                _detectDoubleClickTime = double.NegativeInfinity;
                _wasDoubleClickDone = false;
            }
        }

        private sealed class MouseRightDraggingState : State<MouseHandler>
        {
            public override void Enter(MouseHandler mouseHandler)
            {
                mouseHandler.CallHandleRightMouseDragging(mouseHandler.CurrentMouseState, mouseHandler.DragOriginPosition);
            }

            public override void Execute(MouseHandler mouseHandler)
            {
                if (mouseHandler.CurrentMouseState.RightButton == ButtonState.Released)
                {
                    mouseHandler.CallHandleRightMouseUp(mouseHandler.CurrentMouseState);
                    mouseHandler.CallHandleRightMouseDragDone(mouseHandler.CurrentMouseState, mouseHandler.DragOriginPosition);
                    mouseHandler._mouseStateMachine.ChangeState(mouseHandler._mouseStationaryState);
                }
                else if (((mouseHandler.CurrentMouseState.X != mouseHandler.OldMouseState.X) || (mouseHandler.CurrentMouseState.Y != mouseHandler.OldMouseState.Y)))
                {
                    mouseHandler.CallHandleRightMouseDragging(mouseHandler.CurrentMouseState, mouseHandler.DragOriginPosition);
                }
            }

            public override void Exit(MouseHandler mouseHandler)
            {
            }

            public override void Reset(MouseHandler mouseHandler)
            {
            }
        }
    }
}
