using System.Collections.Generic;
using System;
using System.Linq;

using InputHandlers.State;
using InputHandlers.StateMachine;

using Microsoft.Xna.Framework.Input;

namespace InputHandlers.Keyboard
{
    public class KeyboardInput
    {
        private readonly StateMachine<KeyboardInput> _keyboardStateMachine;
        private List<Keys> _newlyFoundKeys;
        private Keys _focusKey;
        private readonly KeyboardKeyDownState _keyboardKeyDownState;
        private readonly KeyboardKeyLostState _keyboardKeyLostState;
        private readonly KeyboardKeyRepeatState _keyboardKeyRepeatState;
        private readonly KeyboardUnpressedState _keyboardUnpressedState;

        private Keys[] _lastKeyList;
        private KeyboardModifier _lastModifiers;
        private Keys[] _newKeyList;
        private KeyboardModifier _newModifiers;

        private double _repeatDelay = 1000.0;
        private double _repeatFrequency = 50.0;
        private readonly List<IKeyboardHandler> _keyboardHandlers;

        public KeyboardState OldKeyboardState { get; private set; }
        public KeyboardState CurrentKeyboardState { get; private set; }

        public List<Keys> UnmanagedKeys { get; }
        public IStopwatchProvider StopwatchProvider { get; private set; }

        /// <summary>
        /// This is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time. 
        /// </summary>
        public int UpdateNumber { get; private set; }

        /// <summary>
        /// Sets time delay in milliseconds to wait for a key being held down until it repeats
        /// </summary>
        public double RepeatDelay
        {
            get { return _repeatDelay; }
            set
            {
                if (value > 0)
                    _repeatDelay = value;
            }
        }

        /// <summary>
        /// Set time in milliseconds between key repeats once it has started to repeat
        /// </summary>
        public double RepeatFrequency
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
        public bool TreatModifiersAsKeys { get; set; }

        public KeyboardInput() : this(new StopwatchProvider())
        {
        }

        public KeyboardInput(IStopwatchProvider stopwatchProvider)
        {
            _keyboardHandlers = new List<IKeyboardHandler>();

            _keyboardUnpressedState = new KeyboardUnpressedState();
            _keyboardKeyDownState = new KeyboardKeyDownState();
            _keyboardKeyLostState = new KeyboardKeyLostState();
            _keyboardKeyRepeatState = new KeyboardKeyRepeatState();

            UpdateNumber = 0;
            _newlyFoundKeys = new List<Keys>(0);
            UnmanagedKeys = new List<Keys>(0);

            StopwatchProvider = stopwatchProvider;
            StopwatchProvider.Start();

            _keyboardStateMachine = new StateMachine<KeyboardInput>(this);
            _keyboardStateMachine.SetCurrentState(_keyboardUnpressedState);
            _keyboardStateMachine.SetPreviousState(_keyboardUnpressedState);
        }

        public void Subscribe(IKeyboardHandler keyboardHandler)
        {
            if (keyboardHandler != null)
                _keyboardHandlers.Add(keyboardHandler);
        }

        public void Unsubscribe(IKeyboardHandler keyboardHandler)
        {
            if (keyboardHandler != null)
                _keyboardHandlers.Remove(keyboardHandler);
        }

        private void CallHandleKeyboardKeyDown(Keys[] keysDown, Keys focus, KeyboardModifier keyboardModifier)
        {
            foreach (var keyboardHandler in _keyboardHandlers)
            {
                keyboardHandler.HandleKeyboardKeyDown(keysDown, focus, keyboardModifier);
            }
        }

        private void CallHandleKeyboardKeyLost(Keys[] keysDown, KeyboardModifier keyboardModifier)
        {
            foreach (var keyboardHandler in _keyboardHandlers)
            {
                keyboardHandler.HandleKeyboardKeyLost(keysDown, keyboardModifier);
            }
        }

        private void CallHandleKeyboardKeyRepeat(Keys repeatingKey, KeyboardModifier keyboardModifier)
        {
            foreach (var keyboardHandler in _keyboardHandlers)
            {
                keyboardHandler.HandleKeyboardKeyRepeat(repeatingKey, keyboardModifier);
            }
        }

        private void CallHandleKeyboardKeysReleased()
        {
            foreach (var keyboardHandler in _keyboardHandlers)
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
            _keyboardStateMachine.Update();
        }

        /// <summary>
        /// Reset to unpressed state.  You may wish to call this when, for example, switching interface screens.
        /// </summary>
        public void Reset()
        {
            StopwatchProvider.Stop();
            StopwatchProvider.Reset();
            StopwatchProvider.Start();

            _newlyFoundKeys.Clear();

            UpdateNumber = 0;

            _keyboardStateMachine.CurrentState.Reset(this);
            _keyboardStateMachine.SetCurrentState(_keyboardUnpressedState);
            _keyboardStateMachine.SetPreviousState(_keyboardUnpressedState);
        }

        public string GetCurrentStateTypeName()
        {
            return _keyboardStateMachine.GetCurrentStateTypeName();
        }

        private bool IsUnmanagedKey(Keys key)
        {
            return UnmanagedKeys.Contains(key);
        }

        private bool IsModifierKeyAndNotTreatingModifiersAsKey(Keys key)
        {
            return !TreatModifiersAsKeys && key.IsModifierKey();
        }

        private void StripUnmanagedKeysAndModifiers(ref Keys[] keyList)
        {
            if (UnmanagedKeys.Count == 0 && TreatModifiersAsKeys)
                return;

            int keyListIndex;
            var keyListUpdateIndex = 0;

            for (keyListIndex = 0; keyListIndex < keyList.Length; keyListIndex++)
                if (!(IsModifierKeyAndNotTreatingModifiersAsKey(keyList[keyListIndex]) || IsUnmanagedKey(keyList[keyListIndex])))
                {
                    keyList[keyListUpdateIndex] = keyList[keyListIndex];
                    keyListUpdateIndex++;
                }
            if (keyListUpdateIndex < keyList.Length)
            {
                keyList = keyList.Take(keyListUpdateIndex).ToArray();
            }
        }

        private void InitialiseLastKeyListFromKeyboardState()
        {
            _lastKeyList = CurrentKeyboardState.GetPressedKeys();
            _lastModifiers = CurrentKeyboardState.GetModifiers();
            StripUnmanagedKeysAndModifiers(ref _lastKeyList);
            _newlyFoundKeys.Clear();
        }

        private void GetNewKeyListFromKeyboardState()
        {
            _newKeyList = CurrentKeyboardState.GetPressedKeys();
            _newModifiers = CurrentKeyboardState.GetModifiers();
            StripUnmanagedKeysAndModifiers(ref _newKeyList);

            _newlyFoundKeys = _newKeyList.Except(_lastKeyList).ToList();
        }

        private void UpdateLastKeyListWithNewKeyList()
        {
            _lastKeyList = _newKeyList;
            _lastModifiers = _newModifiers;
            _newlyFoundKeys.Clear();
        }

        private bool AreKeysLost
        {
            get
            {
                return _newKeyList.Length < _lastKeyList.Length;
            }
        }

        private bool HasNoAddedKeys
        {
            get
            {
                return !_newlyFoundKeys.Any();
            }
        }

        private bool HasNoKeysPressed
        {
            get
            {
                return !_newKeyList.Any();
            }
        }

        private sealed class KeyboardUnpressedState : State<KeyboardInput>
        {
            public override void Enter(KeyboardInput keyboardInput)
            {
                keyboardInput.CallHandleKeyboardKeysReleased();
            }

            public override void Execute(KeyboardInput keyboardInput)
            {
                // get pressed keys list
                var pressedKeys = keyboardInput.CurrentKeyboardState.GetPressedKeys();

                foreach (var dwn in pressedKeys)
                    if ((keyboardInput.UnmanagedKeys.Count == 0) || !keyboardInput.UnmanagedKeys.Contains(dwn))
                    {
                        keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                        break;
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
                _elapsedTimeSinceKeysChanged = _elapsedTimeSinceKeysChanged = keyboardInput.StopwatchProvider.Elapsed;

                if (keyboardInput._keyboardStateMachine.PreviousState == keyboardInput._keyboardUnpressedState)
                {
                    keyboardInput._focusKey = Keys.None;

                    keyboardInput.InitialiseLastKeyListFromKeyboardState();

                    if (keyboardInput._lastKeyList.Length == 0)
                        keyboardInput.CallHandleKeyboardKeyDown(keyboardInput._lastKeyList, Keys.None, keyboardInput._lastModifiers);

                    foreach (var kevent in keyboardInput._lastKeyList)
                    {
                        keyboardInput.CallHandleKeyboardKeyDown(keyboardInput._lastKeyList, kevent, keyboardInput._lastModifiers);

                        keyboardInput._focusKey = kevent;
                    }
                }
                else
                {
                    foreach (var newkey in keyboardInput._newlyFoundKeys)
                    {
                        keyboardInput.CallHandleKeyboardKeyDown(keyboardInput._newKeyList, newkey, keyboardInput._newModifiers);
                        keyboardInput._focusKey = newkey;
                    }
                }
            }

            public override void Execute(KeyboardInput keyboardInput)
            {
                keyboardInput.GetNewKeyListFromKeyboardState();

                if (keyboardInput.HasNoKeysPressed)
                {
                    keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardUnpressedState);
                    return;
                }

                if (keyboardInput.AreKeysLost && keyboardInput.HasNoAddedKeys)
                {
                    keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyLostState);
                }
                else
                {
                    if (keyboardInput.HasNoAddedKeys)
                    {
                        if (keyboardInput._newModifiers != keyboardInput._lastModifiers)
                        {
                            var modifierDiff = keyboardInput._newModifiers & keyboardInput._lastModifiers;

                            keyboardInput._focusKey = Keys.None;

                            _elapsedTimeSinceKeysChanged = keyboardInput.StopwatchProvider.Elapsed;

                            if (modifierDiff == KeyboardModifier.None
                                || (modifierDiff & keyboardInput._newModifiers) == (modifierDiff & keyboardInput._lastModifiers))
                            {
                                // had one key the same but other two were different, send keyboard down
                                keyboardInput.CallHandleKeyboardKeyDown(keyboardInput._newKeyList, Keys.None, keyboardInput._newModifiers);
                            }
                            else if ((modifierDiff & keyboardInput._newModifiers) == modifierDiff)
                            {
                                // new mod bits only had 1, which means it lost one, change to lost state
                                keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyLostState);
                            }
                            else if ((modifierDiff & keyboardInput._lastModifiers) == modifierDiff)
                            {
                                // old mod bits is less, send down event
                                keyboardInput.CallHandleKeyboardKeyDown(keyboardInput._newKeyList, Keys.None, keyboardInput._newModifiers);
                            }
                            else
                                throw new Exception("code error, unhandled mod key state");
                        }
                        else if (keyboardInput._focusKey != Keys.None
                                 && (keyboardInput.StopwatchProvider.Elapsed.TotalMilliseconds - _elapsedTimeSinceKeysChanged.TotalMilliseconds >
                                     keyboardInput._repeatDelay))
                        {
                            keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyRepeatState);
                        }
                    }
                    else
                        foreach (var newkey in keyboardInput._newlyFoundKeys)
                        {
                            keyboardInput.CallHandleKeyboardKeyDown(keyboardInput._newKeyList, newkey, keyboardInput._newModifiers);
                            keyboardInput._focusKey = newkey;
                            _elapsedTimeSinceKeysChanged = keyboardInput.StopwatchProvider.Elapsed;
                        }
                }
                
                keyboardInput.UpdateLastKeyListWithNewKeyList();
            }

            public override void Exit(KeyboardInput keyboardInput)
            {
                keyboardInput.UpdateLastKeyListWithNewKeyList();
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
            public override void Enter(KeyboardInput keyboardInput)
            {
                keyboardInput.CallHandleKeyboardKeyLost(keyboardInput._newKeyList, keyboardInput._newModifiers);
            }

            public override void Execute(KeyboardInput keyboardInput)
            {
                keyboardInput.GetNewKeyListFromKeyboardState();

                if (keyboardInput.HasNoKeysPressed)
                {
                    keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardUnpressedState);
                    return;
                }

                if (keyboardInput.AreKeysLost && keyboardInput.HasNoAddedKeys)
                {
                    keyboardInput.CallHandleKeyboardKeyLost(keyboardInput._newKeyList, keyboardInput._newModifiers);
                }
                else
                {
                    if ((keyboardInput.HasNoAddedKeys) && (keyboardInput._newModifiers != keyboardInput._lastModifiers))
                    {
                        var modifierDiff = keyboardInput._newModifiers & keyboardInput._lastModifiers;

                        keyboardInput._focusKey = Keys.None;

                        if ((modifierDiff == KeyboardModifier.None)
                            || ((modifierDiff & keyboardInput._newModifiers) == (modifierDiff & keyboardInput._lastModifiers)))
                        {
                            // had one key the same but other two were different, send keyboard down
                            keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                        }
                        else if ((modifierDiff & keyboardInput._newModifiers) == modifierDiff)
                        {
                            // new mod bits only had 1, which means it lost one,
                            // send key lost 
                            keyboardInput.CallHandleKeyboardKeyLost(keyboardInput._newKeyList, keyboardInput._newModifiers);
                        }
                        else if ((modifierDiff & keyboardInput._lastModifiers) == modifierDiff)
                        {
                            // old mod bits is less, send down event
                            keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                        }
                        else
                            throw new Exception("code error, unhandled mod key state");
                    }
                    else
                        keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                }

                keyboardInput.UpdateLastKeyListWithNewKeyList();
            }

            public override void Exit(KeyboardInput keyboardInput)
            {
                keyboardInput.UpdateLastKeyListWithNewKeyList();
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
            private double _repeatRunning = -1.0;

            public override void Enter(KeyboardInput keyboardInput)
            {
                _repeatRunning = -1.0;
            }

            public override void Execute(KeyboardInput keyboardInput)
            {
                keyboardInput.GetNewKeyListFromKeyboardState();

                if (keyboardInput._newKeyList.Length == 0)
                {
                    keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardUnpressedState);
                    return;
                }

                if (keyboardInput.AreKeysLost && keyboardInput.HasNoAddedKeys)
                {
                    keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyLostState);
                }
                else
                {
                    if (keyboardInput.HasNoAddedKeys)
                        if (keyboardInput._newModifiers != keyboardInput._lastModifiers)
                        {
                            var modifierDiff = keyboardInput._newModifiers & keyboardInput._lastModifiers;

                            keyboardInput._focusKey = Keys.None;

                            if ((modifierDiff == KeyboardModifier.None)
                                || ((modifierDiff & keyboardInput._newModifiers) == (modifierDiff & keyboardInput._lastModifiers)))
                            {
                                // had one key the same but other two were different, send keyboard down
                                keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                            }
                            else if ((modifierDiff & keyboardInput._newModifiers) == modifierDiff)
                            {
                                // new mod bits only had 1, which means it lost one, change to lost state
                                keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyLostState);
                            }
                            else if ((modifierDiff & keyboardInput._lastModifiers) == modifierDiff)
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

                            if (_repeatRunning < 0)
                            {
                                keyboardInput.CallHandleKeyboardKeyRepeat(keyboardInput._focusKey, keyboardInput._lastModifiers);
                                _repeatRunning = keyboardInput._repeatFrequency;
                            }
                        }
                    else
                        keyboardInput._keyboardStateMachine.ChangeState(keyboardInput._keyboardKeyDownState);
                }

                keyboardInput.UpdateLastKeyListWithNewKeyList();
            }

            public override void Exit(KeyboardInput keyboardInput)
            {
                keyboardInput.UpdateLastKeyListWithNewKeyList();
            }

            public override void Reset(KeyboardInput keyboardInput)
            {
            }
        }
    }
}