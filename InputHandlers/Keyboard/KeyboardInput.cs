using InputHandlers.State;
using InputHandlers.StateMachine;

namespace InputHandlers.Keyboard;

public class KeyboardInput : IKeyboardInput
{
    private readonly StateMachine<KeyboardInput> _keyboardStateMachine;
    private readonly KeyboardKeyDownState _keyboardKeyDownState = new KeyboardKeyDownState();
    private readonly KeyboardKeyLostState _keyboardKeyLostState = new KeyboardKeyLostState();
    private readonly KeyboardKeyRepeatState _keyboardKeyRepeatState = new KeyboardKeyRepeatState();
    private readonly KeyboardUnpressedState _keyboardUnpressedState = new KeyboardUnpressedState();

    private int _repeatDelay = 1000;
    private int _repeatFrequency = 50;

    private readonly HashSet<IKeyboardHandler> _keyboardHandlerSubscriptions = new HashSet<IKeyboardHandler>();
    private readonly HashSet<KeyboardHandlerSubscription> _pendingAddedSubscriptions = new HashSet<KeyboardHandlerSubscription>();
    private readonly HashSet<KeyboardHandlerSubscription> _pendingRemovedSubscriptions = new HashSet<KeyboardHandlerSubscription>();

    public KeyboardState OldKeyboardState { get; private set; }
    public KeyboardState CurrentKeyboardState { get; private set; }

    public IList<Keys> UnmanagedKeys { get; private set; } = new List<Keys>();

    public IStopwatchProvider StopwatchProvider { get; private set; }

    private readonly KeyDelta _keyDelta;

    /// <summary>
    /// This is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time. 
    /// </summary>
    public int UpdateNumber { get; private set; }

    /// <summary>
    /// Sets time delay in milliseconds to wait for a key being held down until it repeats
    /// </summary>
    public int RepeatDelay
    {
        get => _repeatDelay;
        set
        {
            if (value > 0)
                _repeatDelay = value;
        }
    }

    /// <summary>
    /// Set time in milliseconds between key repeats once it has started to repeat
    /// </summary>
    public int RepeatFrequency
    {
        get { return _repeatFrequency; }
        set
        {
            if (value > 0)
                _repeatFrequency = value;
        }
    }

    /// <summary>
    /// Whether to treat modifier keys (ctrl/alt/shift) as actual keys
    /// </summary>
    public bool TreatModifiersAsKeys
    {
        get { return _keyDelta.TreatModifiersAsKeys; }
        set { _keyDelta.TreatModifiersAsKeys = value; }
    }

    /// <summary>
    /// Whether to wait for a neutral keyboard state before applying new pending subscriptions. Old subscriptions are still removed immediately. Defaults to False.
    /// </summary>
    public bool WaitForNeutralStateBeforeApplyingNewSubscriptions { get; set; }

    public bool IsRepeatEnabled { get; set; } = true;

    public KeyboardInput() : this(new StopwatchProvider())
    {
    }

    public KeyboardInput(IStopwatchProvider stopwatchProvider)
    {
        _keyDelta = new KeyDelta(UnmanagedKeys);
            
        StopwatchProvider = stopwatchProvider;
        StopwatchProvider.Start();

        _keyboardStateMachine = new StateMachine<KeyboardInput>(this);
        _keyboardStateMachine.SetCurrentState(_keyboardUnpressedState);
        _keyboardStateMachine.SetPreviousState(_keyboardUnpressedState);
    }

    public void Subscribe(IKeyboardHandler keyboardHandler)
    {
        if (keyboardHandler != null)
        {
            var existingSubscription = _pendingAddedSubscriptions.SingleOrDefault(s => s.KeyboardHandler.Equals(keyboardHandler));
            
            if (existingSubscription != null)
                existingSubscription.UpdateSubscribedTime(StopwatchProvider);
            else
                _pendingAddedSubscriptions.Add(new KeyboardHandlerSubscription(keyboardHandler, StopwatchProvider));
        }
    }

    public void Unsubscribe(IKeyboardHandler keyboardHandler)
    {
        if (keyboardHandler != null)
        {
            var existingSubscription = _pendingRemovedSubscriptions.SingleOrDefault(s => s.KeyboardHandler.Equals(keyboardHandler));
            
            if (existingSubscription != null)
                existingSubscription.UpdateSubscribedTime(StopwatchProvider);
            else
                _pendingRemovedSubscriptions.Add(new KeyboardHandlerSubscription(keyboardHandler, StopwatchProvider));
        }
    }

    private void CallHandleKeyboardKeyDown(Keys[] keysDown, Keys focus, KeyboardModifier keyboardModifier)
    {
        foreach (var keyboardHandler in _keyboardHandlerSubscriptions)
        {
            keyboardHandler.HandleKeyboardKeyDown(keysDown, focus, keyboardModifier);
        }
    }

    private void CallHandleKeyboardKeyLost(Keys[] keysDown, KeyboardModifier keyboardModifier)
    {
        foreach (var keyboardHandler in _keyboardHandlerSubscriptions)
        {
            keyboardHandler.HandleKeyboardKeyLost(keysDown, keyboardModifier);
        }
    }

    private void CallHandleKeyboardKeyRepeat(Keys repeatingKey, KeyboardModifier keyboardModifier)
    {
        foreach (var keyboardHandler in _keyboardHandlerSubscriptions)
        {
            keyboardHandler.HandleKeyboardKeyRepeat(repeatingKey, keyboardModifier);
        }
    }

    private void CallHandleKeyboardKeysReleased()
    {
        foreach (var keyboardHandler in _keyboardHandlerSubscriptions)
        {
            keyboardHandler.HandleKeyboardKeysReleased();
        }
    }

    /// <summary>
    /// Poll the keyboard for updates.
    /// </summary>
    /// <param name="keyboardState">a keyboard state.  You should use the XNA input function, Keyboard.GetState(), as this parameter.</param>
    public void Poll(KeyboardState keyboardState)
    {
        UpdateNumber++;

        if (UpdateNumber == int.MaxValue)
            UpdateNumber = 0;

        OldKeyboardState = CurrentKeyboardState;
        CurrentKeyboardState = keyboardState;

        _keyDelta.Update(CurrentKeyboardState);

        UpdateSubscriptions();

        _keyboardStateMachine.Update();
    }

    private void UpdateSubscriptions()
    {
        foreach (var pendingRemovedSubscription in _pendingRemovedSubscriptions)
        {
            _keyboardHandlerSubscriptions.Remove(pendingRemovedSubscription.KeyboardHandler);

            var addedSubscription =
                _pendingAddedSubscriptions.SingleOrDefault(s => s.Equals(pendingRemovedSubscription));

            if (addedSubscription != null && pendingRemovedSubscription.SubscribedTime >= addedSubscription.SubscribedTime)
                _pendingAddedSubscriptions.Remove(addedSubscription);
        }
            
        _pendingRemovedSubscriptions.Clear();

        if (WaitForNeutralStateBeforeApplyingNewSubscriptions && _keyboardStateMachine.CurrentState is not KeyboardUnpressedState)
            return;
            
        foreach (var pendingAddedSubscription in _pendingAddedSubscriptions)
            _keyboardHandlerSubscriptions.Add(pendingAddedSubscription.KeyboardHandler);

        _pendingAddedSubscriptions.Clear();
    }

    /// <summary>
    /// Reset to unpressed state.  You may wish to call this when, for example, switching interface screens.
    /// </summary>
    public void Reset()
    {
        StopwatchProvider.Stop();
        StopwatchProvider.Reset();
        StopwatchProvider.Start();

        UpdateNumber = 0;
        _keyDelta.Stop();

        _keyboardStateMachine.CurrentState.Reset(this);
        _keyboardStateMachine.SetCurrentState(_keyboardUnpressedState);
        _keyboardStateMachine.SetPreviousState(_keyboardUnpressedState);
    }

    public string GetCurrentStateTypeName()
    {
        return _keyboardStateMachine.GetCurrentStateTypeName();
    }

    private sealed class KeyboardUnpressedState : State<KeyboardInput>
    {
        public override void Enter(KeyboardInput keyboardInput)
        {
            keyboardInput.CallHandleKeyboardKeysReleased();
            keyboardInput._keyDelta.Stop();
        }

        public override void Execute(KeyboardInput keyboardInput)
        {
            var pressedKeys = keyboardInput.CurrentKeyboardState.GetPressedKeys();

            foreach (var key in pressedKeys)
            {
                if (!keyboardInput.UnmanagedKeys.Contains(key))
                {
                    keyboardInput._keyDelta.Start(keyboardInput.CurrentKeyboardState);
                    keyboardInput._keyDelta.Update(keyboardInput.CurrentKeyboardState);

                    if (keyboardInput._keyDelta.HasKeysPressed)
                    {
                        keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                    }
                    else
                    {
                        keyboardInput._keyDelta.Stop();
                    }

                    break;
                }
            }
        }

        public override void Exit(KeyboardInput keyboardInput)
        {
        }

        public override void Reset(KeyboardInput keyboardInput)
        {
        }
    }

    /// <summary>
    /// Key down state.  A key down event is sent for EVERY new key found.  If one or more modifier keys only then only one
    /// kbkeydown is sent.
    /// </summary>
    private sealed class KeyboardKeyDownState : State<KeyboardInput>
    {
        private TimeSpan _elapsedTimeSinceKeysChanged;

        public override void Enter(KeyboardInput keyboardInput)
        {
            var keyDelta = keyboardInput._keyDelta;

            _elapsedTimeSinceKeysChanged = _elapsedTimeSinceKeysChanged = keyboardInput.StopwatchProvider.Elapsed;

            foreach (var newkey in keyDelta.NewKeyDelta)
            {
                keyboardInput.CallHandleKeyboardKeyDown(keyDelta.NewKeyList, newkey, keyDelta.NewModifiers);
            }
        }

        public override void Execute(KeyboardInput keyboardInput)
        {
            var keyDelta = keyboardInput._keyDelta;

            if (keyDelta.HasNoKeysPressed)
            {
                keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardUnpressedState);
                return;
            }

            if (keyDelta.AreKeysLost && keyDelta.HasNoAddedKeys)
            {
                keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyLostState);
            }
            else
            {
                if (keyDelta.HasNoAddedKeys)
                {
                    if (keyDelta.NewModifiers != keyDelta.LastModifiers)
                    {
                        _elapsedTimeSinceKeysChanged = keyboardInput.StopwatchProvider.Elapsed;

                        var modifierDiff = keyDelta.NewModifiers & keyDelta.LastModifiers;

                        if (modifierDiff == KeyboardModifier.None
                            || (modifierDiff & keyDelta.NewModifiers) == (modifierDiff & keyDelta.LastModifiers))
                        {
                            // had one key the same but other two were different, send keyboard down
                            keyboardInput.CallHandleKeyboardKeyDown(keyDelta.NewKeyList, Keys.None, keyDelta.NewModifiers);
                        }
                        else if ((modifierDiff & keyDelta.NewModifiers) == modifierDiff)
                        {
                            // new mod bits only had 1, which means it lost one, change to lost state
                            keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyLostState);
                        }
                        else if ((modifierDiff & keyDelta.LastModifiers) == modifierDiff)
                        {
                            // old mod bits is less, send down event
                            keyboardInput.CallHandleKeyboardKeyDown(keyDelta.NewKeyList, Keys.None, keyDelta.NewModifiers);
                        }
                        else
                            throw new Exception("code error, unhandled mod key state");
                    }
                    else if (keyboardInput.IsRepeatEnabled
                             && keyDelta.FocusKey != Keys.None
                             && keyboardInput.StopwatchProvider.Elapsed.TotalMilliseconds - _elapsedTimeSinceKeysChanged.TotalMilliseconds >=
                             keyboardInput._repeatDelay
                            )
                    {
                        keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyRepeatState);
                    }
                }
                else
                {
                    foreach (var newkey in keyDelta.NewKeyDelta)
                    {
                        keyboardInput.CallHandleKeyboardKeyDown(keyDelta.NewKeyList, newkey, keyDelta.NewModifiers);

                        _elapsedTimeSinceKeysChanged = keyboardInput.StopwatchProvider.Elapsed;
                    }
                }
            }
        }

        public override void Exit(KeyboardInput keyboardInput)
        {
        }

        public override void Reset(KeyboardInput keyboardInput)
        {
        }
    }

    /// <summary>
    /// Key lost state happens when one or more keys are released but keys are still being held down.  Only one
    /// kbkeylost event is sent regardless of how many keys were lost.
    /// note - sometimes more than 2 keys wont register.  See this for explanation of keyboard hardware limitations:
    /// http://blogs.msdn.com/shawnhar/archive/2007/03/28/keyboards-suck.aspx
    /// 2nd note - GetPressedKeys has a few other issues too - for example, holding down shift and pressing numpad9 or
    /// numpad3 will register a pageup/pagedown key in XNA, then on releasing the shift key and then releasing the numpad
    /// key will cause unexpected behaviour.
    /// </summary>
    private sealed class KeyboardKeyLostState : State<KeyboardInput>
    {
        private TimeSpan _elapsedTimeSinceKeyLost;

        public override void Enter(KeyboardInput keyboardInput)
        {
            var keyDelta = keyboardInput._keyDelta;

            _elapsedTimeSinceKeyLost = _elapsedTimeSinceKeyLost = keyboardInput.StopwatchProvider.Elapsed;

            keyboardInput.CallHandleKeyboardKeyLost(keyDelta.NewKeyList, keyDelta.NewModifiers);
        }

        public override void Execute(KeyboardInput keyboardInput)
        {
            var keyDelta = keyboardInput._keyDelta;

            if (keyDelta.HasNoKeysPressed)
            {
                keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardUnpressedState);
                return;
            }

            if (keyDelta.AreKeysLost && keyDelta.HasNoAddedKeys)
            {
                keyboardInput.CallHandleKeyboardKeyLost(keyDelta.NewKeyList, keyDelta.NewModifiers);
            }
            else
            {
                if (keyDelta.HasNoAddedKeys)
                {
                    if (keyDelta.NewModifiers != keyDelta.LastModifiers)
                    {
                        _elapsedTimeSinceKeyLost = keyboardInput.StopwatchProvider.Elapsed;

                        var modifierDiff = keyDelta.NewModifiers & keyDelta.LastModifiers;

                        if ((modifierDiff == KeyboardModifier.None)
                            || ((modifierDiff & keyDelta.NewModifiers) == (modifierDiff & keyDelta.LastModifiers)))
                        {
                            // had one key the same but other two were different, send keyboard down
                            keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                        }
                        else if ((modifierDiff & keyDelta.NewModifiers) == modifierDiff)
                        {
                            // new mod bits only had 1, which means it lost one,
                            // send key lost 
                            keyboardInput.CallHandleKeyboardKeyLost(keyDelta.NewKeyList, keyDelta.NewModifiers);
                        }
                        else if ((modifierDiff & keyDelta.LastModifiers) == modifierDiff)
                        {
                            // old mod bits is less, send down event
                            keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                        }
                        else
                            throw new Exception("code error, unhandled mod key state");
                    }
                    else if (keyboardInput.IsRepeatEnabled
                             && keyDelta.LastFocusKey != Keys.None
                             && keyDelta.LastFocusKey == keyDelta.NewestKey
                             && keyboardInput.StopwatchProvider.Elapsed.TotalMilliseconds - _elapsedTimeSinceKeyLost.TotalMilliseconds >=
                             keyboardInput._repeatDelay
                            )
                    {
                        keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyRepeatState);
                    }
                }
                else if (keyDelta.HasAddedKeys)
                    keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
            }
        }

        public override void Exit(KeyboardInput keyboardInput)
        {
        }

        public override void Reset(KeyboardInput keyboardInput)
        {
        }
    }

    /// <summary>
    /// Key repeat state is entered when a key is held down for long enough and nothing else occurs.  A key repeat
    /// event happens every single time a poll occurs if the repeat delay time has been exceeded.
    /// </summary>
    private sealed class KeyboardKeyRepeatState : State<KeyboardInput>
    {
        private TimeSpan _lastTime;
        private double _repeatRunning;

        public override void Enter(KeyboardInput keyboardInput)
        {
            _repeatRunning = 0;
            Execute(keyboardInput);
        }

        public override void Execute(KeyboardInput keyboardInput)
        {
            var keyDelta = keyboardInput._keyDelta;

            if (keyDelta.NewKeyList.Length == 0)
            {
                keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardUnpressedState);
                return;
            }

            if (keyDelta.AreKeysLost && keyDelta.HasNoAddedKeys)
            {
                keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyLostState);
            }
            else
            {
                if (keyDelta.HasNoAddedKeys)
                {
                    if (keyDelta.NewModifiers != keyDelta.LastModifiers)
                    {
                        var modifierDiff = keyDelta.NewModifiers & keyDelta.LastModifiers;

                        if (modifierDiff == KeyboardModifier.None || (modifierDiff & keyDelta.NewModifiers) ==
                            (modifierDiff & keyDelta.LastModifiers))
                        {
                            // had one key the same but other two were different, send keyboard down
                            keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                        }
                        else if ((modifierDiff & keyDelta.NewModifiers) == modifierDiff)
                        {
                            // new mod bits only had 1, which means it lost one, change to lost state
                            keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyLostState);
                        }
                        else if ((modifierDiff & keyDelta.LastModifiers) == modifierDiff)
                        {
                            // old mod bits is less, send down event
                            keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                        }
                        else
                            throw new Exception("code error, unhandled mod key state");
                    }
                    else
                    {
                        _repeatRunning -= (keyboardInput.StopwatchProvider.Elapsed - _lastTime).TotalMilliseconds;
                        _lastTime = keyboardInput.StopwatchProvider.Elapsed;

                        if (_repeatRunning <= 0)
                        {
                            keyboardInput.CallHandleKeyboardKeyRepeat(
                                keyDelta.FocusKey,
                                keyDelta.LastModifiers);
                            _repeatRunning = keyboardInput._repeatFrequency;
                        }
                    }
                }
                else
                    keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
            }

        }

        public override void Exit(KeyboardInput keyboardInput)
        {
        }

        public override void Reset(KeyboardInput keyboardInput)
        {
        }
    }
}