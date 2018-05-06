using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using InputHandlers.State;
using InputHandlers.StateMachine;

using Microsoft.Xna.Framework.Input;

namespace InputHandlers.Mouse
{
    public class MouseInput
    {
        private readonly StateMachine<MouseInput> _mouseStateMachine;
        private readonly MouseStationaryState _mouseStationaryState;
        private readonly MouseMovingState _mouseMovingState;
        private readonly MouseLeftDownState _mouseLeftDownState;
        private readonly MouseLeftDraggingState _mouseLeftDraggingState;
        private readonly MouseRightDownState _mouseRightDownState;
        private readonly MouseRightDraggingState _mouseRightDraggingState;
        private readonly MouseMiddleDownState _mouseMiddleDownState;
        private readonly MouseMiddleDraggingState _mouseMiddleDraggingState;
        private readonly List<IMouseHandler> _mouseHandlers;

        public MouseState OldMouseState { get; private set; }
        public MouseState CurrentMouseState { get; private set; }
        public MouseState DragOriginPosition { get; private set; }
        public IStopwatchProvider StopwatchProvider { get; private set; }

        /// <summary>
        /// DragVariance is a fudging factor for detecting the difference between mouse clicks and mouse drags.
        /// This is because a fast user may do a mouse click while slightly moving the mouse between mouse down and mouse up.
        /// If it wasnt for this fudging factor then it would go into drag mode which isnt what the user probably wanted.
        /// </summary>
        public int DragVariance { get; set; }

        /// <summary>
        /// If time between clicks in milliseconds is less than this value then it is considered a double click
        /// </summary>
        public int DoubleClickDetectionTimeDelay { get; set; }

        /// <summary>
        /// This is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time. 
        /// </summary>
        public int UpdateNumber { get; private set; }

        public MouseInput() : this(new StopwatchProvider())
        {
        }

        public MouseInput(IStopwatchProvider stopwatchProvider)
        {
            _mouseHandlers = new List<IMouseHandler>();
            _mouseStationaryState = new MouseStationaryState();
            _mouseMovingState = new MouseMovingState();
            _mouseLeftDownState = new MouseLeftDownState();
            _mouseLeftDraggingState = new MouseLeftDraggingState();
            _mouseRightDownState = new MouseRightDownState();
            _mouseRightDraggingState = new MouseRightDraggingState();
            _mouseMiddleDownState = new MouseMiddleDownState();
            _mouseMiddleDraggingState = new MouseMiddleDraggingState();

            UpdateNumber = 0;
            DragVariance = 10;
            DoubleClickDetectionTimeDelay = 400;

            StopwatchProvider = stopwatchProvider;
            StopwatchProvider.Start();

            _mouseStateMachine = new StateMachine<MouseInput>(this);
            _mouseStateMachine.SetCurrentState(_mouseStationaryState);
            _mouseStateMachine.SetPreviousState(_mouseStationaryState);
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

        private void CallHandleMouseMoving(MouseState m, MouseState origin)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleMouseMoving(m, origin);
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

        private void CallHandleMiddleMouseClick(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleMiddleMouseClick(m);
            }
        }

        private void CallHandleMiddleMouseDoubleClick(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleMiddleMouseDoubleClick(m);
            }
        }

        private void CallHandleMiddleMouseDown(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleMiddleMouseDown(m);
            }
        }

        private void CallHandleMiddleMouseUp(MouseState m)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleMiddleMouseUp(m);
            }
        }

        private void CallHandleMiddleMouseDragging(MouseState m, MouseState origin)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleMiddleMouseDragging(m, origin);
            }
        }

        private void CallHandleMiddleMouseDragDone(MouseState m, MouseState origin)
        {
            foreach (var mouseHandler in _mouseHandlers)
            {
                mouseHandler.HandleMiddleMouseDragDone(m, origin);
            }
        }

        /// <summary>
        /// Poll the mouse for updates.
        /// </summary>
        /// <param name="mouseState">a mouse state.  You should use the XNA input function, Mouse.GetState(), as this parameter.</param>
        public void Poll(MouseState mouseState)
        {
            UpdateNumber++;

            if (UpdateNumber == int.MaxValue)
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
            StopwatchProvider.Stop();
            StopwatchProvider.Reset();
            StopwatchProvider.Start();
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

        private bool IsStartingDragDrop()
        {
            return (Math.Abs(DragOriginPosition.X - CurrentMouseState.X) > DragVariance) ||
                   (Math.Abs(DragOriginPosition.Y - CurrentMouseState.Y) > DragVariance);
        }

        private abstract class MouseUnpressedButtonState : State<MouseInput>
        {
            protected bool TryChangeStateForButtons(MouseInput mouseInput)
            {
                if (mouseInput.CurrentMouseState.LeftButton == ButtonState.Pressed)
                {
                    mouseInput._mouseStateMachine.ChangeState(mouseInput._mouseLeftDownState);
                    return true;
                }

                if (mouseInput.CurrentMouseState.RightButton == ButtonState.Pressed)
                {
                    mouseInput._mouseStateMachine.ChangeState(mouseInput._mouseRightDownState);
                    return true;
                }

                if (mouseInput.CurrentMouseState.MiddleButton == ButtonState.Pressed)
                {
                    mouseInput._mouseStateMachine.ChangeState(mouseInput._mouseMiddleDownState);
                    return true;
                }

                return false;
            }
        }

        private class MouseStationaryState : MouseUnpressedButtonState
        {
            public override void Enter(MouseInput mouseInput)
            {
            }

            public override void Execute(MouseInput mouseInput)
            {
                if (TryChangeStateForButtons(mouseInput))
                    return;

                if (mouseInput.CurrentMouseState.X != mouseInput.OldMouseState.X || mouseInput.CurrentMouseState.Y != mouseInput.OldMouseState.Y)
                    mouseInput._mouseStateMachine.ChangeState(mouseInput._mouseMovingState);
            }

            public override void Exit(MouseInput mouseInput)
            {
            }

            public override void Reset(MouseInput mouseInput)
            {
            }
        }

        private class MouseMovingState : MouseUnpressedButtonState
        {
            public override void Enter(MouseInput mouseInput)
            {
                mouseInput.CallHandleMouseMoving(mouseInput.CurrentMouseState, mouseInput.OldMouseState);
            }

            public override void Execute(MouseInput mouseInput)
            {
                if (TryChangeStateForButtons(mouseInput))
                    return;
                if (mouseInput.CurrentMouseState.X == mouseInput.OldMouseState.X && mouseInput.CurrentMouseState.Y == mouseInput.OldMouseState.Y)
                    mouseInput._mouseStateMachine.ChangeState(mouseInput._mouseStationaryState);
                else
                    mouseInput.CallHandleMouseMoving(mouseInput.CurrentMouseState, mouseInput.OldMouseState);
            }

            public override void Exit(MouseInput mouseInput)
            {
            }

            public override void Reset(MouseInput mouseInput)
            {
            }
        }

        private abstract class MouseButtonDownState : State<MouseInput>
        {
            private double _detectDoubleClickTime;
            private bool _wasDoubleClickDone;

            public MouseButtonDownState()
            {
                _detectDoubleClickTime = double.NegativeInfinity;
                _wasDoubleClickDone = false;
            }

            protected void EnterInternal(
                MouseInput mouseInput,
                Action<MouseState> mouseDownAction,
                Action<MouseState> mouseDoubleClickAction)
            {
                mouseInput.DragOriginPosition = mouseInput.CurrentMouseState;

                if (double.IsNegativeInfinity(_detectDoubleClickTime))
                {
                    _detectDoubleClickTime = mouseInput.StopwatchProvider.Elapsed.TotalMilliseconds;
                    mouseDownAction(mouseInput.CurrentMouseState);
                }
                else
                {
                    _detectDoubleClickTime -= mouseInput.StopwatchProvider.Elapsed.TotalMilliseconds;

                    if (_detectDoubleClickTime >= -mouseInput.DoubleClickDetectionTimeDelay)
                    {
                        mouseDoubleClickAction(mouseInput.DragOriginPosition);

                        _detectDoubleClickTime = double.NegativeInfinity;

                        _wasDoubleClickDone = true;
                    }
                    else
                    {
                        _detectDoubleClickTime = mouseInput.StopwatchProvider.Elapsed.TotalMilliseconds;
                        mouseDownAction(mouseInput.CurrentMouseState);
                    }
                }
            }

            protected void ExecuteInternal(
                MouseInput mouseInput,
                Action<MouseState> mouseUpAction,
                Action<MouseState> mouseClickAction,
                State<MouseInput> draggingState,
                ButtonState buttonState
                )
            {
                if (_wasDoubleClickDone)
                {
                    if (buttonState == ButtonState.Released)
                    {
                        _wasDoubleClickDone = false;
                        mouseInput._mouseStateMachine.ChangeState(mouseInput._mouseStationaryState);
                    }
                }
                else if (buttonState == ButtonState.Released)
                {
                    mouseUpAction(mouseInput.DragOriginPosition);
                    mouseClickAction(mouseInput.DragOriginPosition);
                    mouseInput._mouseStateMachine.ChangeState(mouseInput._mouseStationaryState);
                }
                else if (mouseInput.IsStartingDragDrop())
                {
                    mouseInput._mouseStateMachine.ChangeState(draggingState);
                }
            }

            public override void Exit(MouseInput mouseInput)
            {
            }

            public override void Reset(MouseInput mouseInput)
            {
                _detectDoubleClickTime = double.NegativeInfinity;
                _wasDoubleClickDone = false;
            }
        }

        private abstract class MouseDraggingState : State<MouseInput>
        {
            protected void EnterInternal(
                MouseInput mouseInput,
                Action<MouseState, MouseState> mouseDraggingAction
                )
            {
                mouseDraggingAction(mouseInput.CurrentMouseState, mouseInput.DragOriginPosition);
            }

            protected void ExecuteInternal(
                MouseInput mouseInput,
                Action<MouseState> mouseUpAction,
                Action<MouseState, MouseState> mouseDragging,
                Action<MouseState, MouseState> mouseDragDone,
                ButtonState buttonState
                )
            {
                if (buttonState == ButtonState.Released)
                {
                    mouseUpAction(mouseInput.CurrentMouseState);
                    mouseDragDone(mouseInput.CurrentMouseState, mouseInput.DragOriginPosition);
                    mouseInput._mouseStateMachine.ChangeState(mouseInput._mouseStationaryState);
                }
                else if ((mouseInput.CurrentMouseState.X != mouseInput.OldMouseState.X) || (mouseInput.CurrentMouseState.Y != mouseInput.OldMouseState.Y))
                {
                    mouseDragging(mouseInput.CurrentMouseState, mouseInput.DragOriginPosition);
                }
            }

            public override void Exit(MouseInput mouseInput)
            {
            }

            public override void Reset(MouseInput mouseInput)
            {
            }
        }

        private class MouseLeftDownState : MouseButtonDownState
        {
            public override void Enter(MouseInput mouseInput)
            {
                EnterInternal(mouseInput, mouseInput.CallHandleLeftMouseDown, mouseInput.CallHandleLeftMouseDoubleClick);
            }

            public override void Execute(MouseInput mouseInput)
            {
                ExecuteInternal(
                    mouseInput,
                    mouseInput.CallHandleLeftMouseUp,
                    mouseInput.CallHandleLeftMouseClick,
                    mouseInput._mouseLeftDraggingState,
                    mouseInput.CurrentMouseState.LeftButton);
            }
        }

        private class MouseLeftDraggingState : MouseDraggingState
        {
            public override void Enter(MouseInput mouseInput)
            {
                EnterInternal(mouseInput, mouseInput.CallHandleLeftMouseDragging);
            }

            public override void Execute(MouseInput mouseInput)
            {
                ExecuteInternal(
                    mouseInput,
                    mouseInput.CallHandleLeftMouseUp,
                    mouseInput.CallHandleLeftMouseDragging,
                    mouseInput.CallHandleLeftMouseDragDone,
                    mouseInput.CurrentMouseState.LeftButton);
            }
        }

        private class MouseRightDownState : MouseButtonDownState
        {
            public override void Enter(MouseInput mouseInput)
            {
                EnterInternal(mouseInput, mouseInput.CallHandleRightMouseDown, mouseInput.CallHandleRightMouseDoubleClick);
            }

            public override void Execute(MouseInput mouseInput)
            {
                ExecuteInternal(
                    mouseInput,
                    mouseInput.CallHandleRightMouseUp,
                    mouseInput.CallHandleRightMouseClick,
                    mouseInput._mouseRightDraggingState,
                    mouseInput.CurrentMouseState.RightButton);
            }
        }

        private class MouseRightDraggingState : MouseDraggingState
        {
            public override void Enter(MouseInput mouseInput)
            {
                EnterInternal(mouseInput, mouseInput.CallHandleRightMouseDragging);
            }

            public override void Execute(MouseInput mouseInput)
            {
                ExecuteInternal(
                    mouseInput,
                    mouseInput.CallHandleRightMouseUp,
                    mouseInput.CallHandleRightMouseDragging,
                    mouseInput.CallHandleRightMouseDragDone,
                    mouseInput.CurrentMouseState.RightButton);
            }
        }

        private class MouseMiddleDownState : MouseButtonDownState
        {
            public override void Enter(MouseInput mouseInput)
            {
                EnterInternal(mouseInput, mouseInput.CallHandleMiddleMouseDown, mouseInput.CallHandleMiddleMouseDoubleClick);
            }

            public override void Execute(MouseInput mouseInput)
            {
                ExecuteInternal(
                    mouseInput,
                    mouseInput.CallHandleMiddleMouseUp,
                    mouseInput.CallHandleMiddleMouseClick,
                    mouseInput._mouseMiddleDraggingState,
                    mouseInput.CurrentMouseState.MiddleButton);
            }
        }

        private class MouseMiddleDraggingState : MouseDraggingState
        {
            public override void Enter(MouseInput mouseInput)
            {
                EnterInternal(mouseInput, mouseInput.CallHandleMiddleMouseDragging);
            }

            public override void Execute(MouseInput mouseInput)
            {
                ExecuteInternal(
                    mouseInput,
                    mouseInput.CallHandleMiddleMouseUp,
                    mouseInput.CallHandleMiddleMouseDragging,
                    mouseInput.CallHandleMiddleMouseDragDone,
                    mouseInput.CurrentMouseState.MiddleButton
                    );
            }
        }
    }
}
