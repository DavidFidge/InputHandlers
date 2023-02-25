using InputHandlers.State;
using InputHandlers.StateMachine;

namespace InputHandlers.Mouse;

public class MouseInput : IMouseInput
{
    private readonly StateMachine<MouseInput> _mouseStateMachine;
    private readonly MouseStationaryState _mouseStationaryState = new MouseStationaryState();
    private readonly MouseMovingState _mouseMovingState = new MouseMovingState();
    private readonly MouseLeftDownState _mouseLeftDownState = new MouseLeftDownState();
    private readonly MouseLeftDraggingState _mouseLeftDraggingState = new MouseLeftDraggingState();
    private readonly MouseRightDownState _mouseRightDownState = new MouseRightDownState();
    private readonly MouseRightDraggingState _mouseRightDraggingState = new MouseRightDraggingState();
    private readonly MouseMiddleDownState _mouseMiddleDownState = new MouseMiddleDownState();
    private readonly MouseMiddleDraggingState _mouseMiddleDraggingState = new MouseMiddleDraggingState();
    private readonly HashSet<IMouseHandler> _mouseHandlerSubscriptions = new HashSet<IMouseHandler>();
    private readonly HashSet<MouseHandlerSubscription> _pendingAddedSubscriptions = new HashSet<MouseHandlerSubscription>();
    private readonly HashSet<MouseHandlerSubscription> _pendingRemovedSubscriptions = new HashSet<MouseHandlerSubscription>();

    public MouseState OldMouseState { get; private set; }
    public MouseState CurrentMouseState { get; private set; }
    public MouseState DragOriginPosition { get; private set; }
    public IStopwatchProvider StopwatchProvider { get; private set; }
    public bool IsLeftButtonEnabled { get; set; } = true;
    public bool IsMiddleButtonEnabled { get; set; } = true;
    public bool IsRightButtonEnabled { get; set; } = true;

    /// <summary>
    /// DragVariance is a fudging factor for detecting the difference between mouse clicks and mouse drags.
    /// This is because a fast user may do a mouse click while slightly moving the mouse between mouse down and mouse up.
    /// If it wasnt for this fudging factor then it would go into drag mode which isnt what the user probably wanted.
    /// </summary>
    public int DragVariance { get; set; } = 10;

    /// <summary>
    /// If time between clicks in milliseconds is less than this value then it is considered a double click
    /// </summary>
    public int DoubleClickDetectionTimeDelay { get; set; } = 400;

    /// <summary>
    /// This is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time. 
    /// </summary>
    public int UpdateNumber { get; private set; }
        
    /// <summary>
    /// Whether to wait for a neutral mouse state before applying pending subscriptions. Old subscriptions are still removed immediately. Defaults to False.
    /// </summary>
    public bool WaitForNeutralStateBeforeApplyingNewSubscriptions { get; set; }

    public MouseInput() : this(new StopwatchProvider())
    {
    }

    public MouseInput(IStopwatchProvider stopwatchProvider)
    {
        StopwatchProvider = stopwatchProvider;
        StopwatchProvider.Start();

        _mouseStateMachine = new StateMachine<MouseInput>(this);
        _mouseStateMachine.SetCurrentState(_mouseStationaryState);
        _mouseStateMachine.SetPreviousState(_mouseStationaryState);
    }

    public void Subscribe(IMouseHandler mouseHandler)
    {
        var existingSubscription = _pendingAddedSubscriptions.SingleOrDefault(s => s.MouseHandler.Equals(mouseHandler));
            
        if (existingSubscription != null)
            existingSubscription.UpdateSubscribedTime(StopwatchProvider);
        else
            _pendingAddedSubscriptions.Add(new MouseHandlerSubscription(mouseHandler, StopwatchProvider));
    }

    public void Unsubscribe(IMouseHandler mouseHandler)
    {
        var existingSubscription = _pendingRemovedSubscriptions.SingleOrDefault(s => s.MouseHandler.Equals(mouseHandler));
            
        if (existingSubscription != null)
            existingSubscription.UpdateSubscribedTime(StopwatchProvider);
        else
            _pendingRemovedSubscriptions.Add(new MouseHandlerSubscription(mouseHandler, StopwatchProvider));
    }

    private void CallHandleMouseScrollWheelMove(MouseState m, int diff)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleMouseScrollWheelMove(m, diff);
        }
    }

    private void CallHandleMouseMoving(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleMouseMoving(m, origin);
        }
    }

    private void CallHandleLeftMouseClick(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleLeftMouseClick(m, origin);
        }
    }

    private void CallHandleLeftMouseDoubleClick(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleLeftMouseDoubleClick(m, origin);
        }
    }

    private void CallHandleLeftMouseDown(MouseState m)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleLeftMouseDown(m);
        }
    }

    private void CallHandleLeftMouseUp(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleLeftMouseUp(m, origin);
        }
    }

    private void CallHandleLeftMouseDragging(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleLeftMouseDragging(m, origin);
        }
    }

    private void CallHandleLeftMouseDragDone(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleLeftMouseDragDone(m, origin);
        }
    }

    private void CallHandleRightMouseClick(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleRightMouseClick(m, origin);
        }
    }

    private void CallHandleRightMouseDoubleClick(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleRightMouseDoubleClick(m, origin);
        }
    }

    private void CallHandleRightMouseDown(MouseState m)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleRightMouseDown(m);
        }
    }

    private void CallHandleRightMouseUp(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleRightMouseUp(m, origin);
        }
    }

    private void CallHandleRightMouseDragging(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleRightMouseDragging(m, origin);
        }
    }

    private void CallHandleRightMouseDragDone(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleRightMouseDragDone(m, origin);
        }
    }

    private void CallHandleMiddleMouseClick(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleMiddleMouseClick(m, origin);
        }
    }

    private void CallHandleMiddleMouseDoubleClick(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleMiddleMouseDoubleClick(m, origin);
        }
    }

    private void CallHandleMiddleMouseDown(MouseState m)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleMiddleMouseDown(m);
        }
    }

    private void CallHandleMiddleMouseUp(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleMiddleMouseUp(m, origin);
        }
    }

    private void CallHandleMiddleMouseDragging(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
        {
            mouseHandler.HandleMiddleMouseDragging(m, origin);
        }
    }

    private void CallHandleMiddleMouseDragDone(MouseState m, MouseState origin)
    {
        foreach (var mouseHandler in _mouseHandlerSubscriptions)
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

        UpdateSubscriptions();

        _mouseStateMachine.Update();
    }

    private void UpdateSubscriptions()
    {
        foreach (var pendingRemovedSubscription in _pendingRemovedSubscriptions)
        {
            _mouseHandlerSubscriptions.Remove(pendingRemovedSubscription.MouseHandler);

            var addedSubscription =
                _pendingAddedSubscriptions.SingleOrDefault(s => s.Equals(pendingRemovedSubscription));

            if (addedSubscription != null && pendingRemovedSubscription.SubscribedTime >= addedSubscription.SubscribedTime)
                _pendingAddedSubscriptions.Remove(addedSubscription);
        }
            
        _pendingRemovedSubscriptions.Clear();

        if (WaitForNeutralStateBeforeApplyingNewSubscriptions && _mouseStateMachine.CurrentState is not MouseStationaryState)
            return;
            
        foreach (var pendingAddedSubscription in _pendingAddedSubscriptions)
            _mouseHandlerSubscriptions.Add(pendingAddedSubscription.MouseHandler);

        _pendingAddedSubscriptions.Clear();
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

    /// <summary>
    /// Use this if you want to suppress double click detection (clears left, middle and right all at once).
    /// May be useful for scenarios where you want to suppress clicks under some circumstances e.g. if the user
    /// double clicks a button to close a window rather than single clicks and the click is subsequently
    /// detected afterwards incorrectly as a different action. You could call this after the window closes
    /// to suppress the double click. 
    /// </summary>
    public void ResetDoubleClickDetection()
    {
        _mouseLeftDownState.ResetDoubleClickDetection();
        _mouseRightDownState.ResetDoubleClickDetection();
        _mouseMiddleDownState.ResetDoubleClickDetection();
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
            if (mouseInput.IsLeftButtonEnabled && mouseInput.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                mouseInput._mouseStateMachine.ChangeState(mouseInput._mouseLeftDownState);
                return true;
            }

            if (mouseInput.IsRightButtonEnabled && mouseInput.CurrentMouseState.RightButton == ButtonState.Pressed)
            {
                mouseInput._mouseStateMachine.ChangeState(mouseInput._mouseRightDownState);
                return true;
            }

            if (mouseInput.IsMiddleButtonEnabled && mouseInput.CurrentMouseState.MiddleButton == ButtonState.Pressed)
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
            Action<MouseState, MouseState> mouseDoubleClickAction)
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
                    mouseDoubleClickAction(mouseInput.CurrentMouseState, mouseInput.DragOriginPosition);

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
            Action<MouseState, MouseState> mouseUpAction,
            Action<MouseState, MouseState> mouseClickAction,
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
                mouseUpAction(mouseInput.CurrentMouseState, mouseInput.DragOriginPosition);
                mouseClickAction(mouseInput.CurrentMouseState, mouseInput.DragOriginPosition);
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
            ResetDoubleClickDetection();
        }

        public void ResetDoubleClickDetection()
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
            Action<MouseState, MouseState> mouseUpAction,
            Action<MouseState, MouseState> mouseDragging,
            Action<MouseState, MouseState> mouseDragDone,
            ButtonState buttonState
        )
        {
            if (buttonState == ButtonState.Released)
            {
                mouseUpAction(mouseInput.CurrentMouseState, mouseInput.DragOriginPosition);
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