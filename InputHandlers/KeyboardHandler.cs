using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;

using InputHandlers.State;
using InputHandlers.StateMachine;

using Microsoft.Xna.Framework.Input;

namespace InputHandlers.Keyboard
{
    public class KeyboardHandler
    {
        private readonly StateMachine<KeyboardHandler> _keyboardStateMachine;
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
        public Stopwatch LastPollTime { get; private set; }

        /// <summary>
        /// This is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time. 
        /// </summary>
        public uint UpdateNumber { get; private set; }

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

        /// <summary>
        /// Create keyboard handler object and initialise its state to unpressed
        /// </summary>
        public KeyboardHandler()
        {
            _keyboardHandlers = new List<IKeyboardHandler>();

            LastPollTime = new Stopwatch();
            LastPollTime.Start();
            UpdateNumber = 0;
            _newlyFoundKeys = new List<Keys>(0);

            _keyboardUnpressedState = new KeyboardUnpressedState();
            _keyboardKeyDownState = new KeyboardKeyDownState();
            _keyboardKeyLostState = new KeyboardKeyLostState();
            _keyboardKeyRepeatState = new KeyboardKeyRepeatState();

            _keyboardStateMachine = new StateMachine<KeyboardHandler>(this);
            _keyboardStateMachine.SetCurrentState(_keyboardUnpressedState);
            _keyboardStateMachine.SetPreviousState(_keyboardUnpressedState);
            UnmanagedKeys = new List<Keys>(0);
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

            if (UpdateNumber == uint.MaxValue)
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
            LastPollTime = new Stopwatch();
            LastPollTime.Start();

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

        private sealed class KeyboardUnpressedState : State<KeyboardHandler>
        {
            public override void Enter(KeyboardHandler keyboardHandler)
            {
                keyboardHandler.CallHandleKeyboardKeysReleased();
            }

            public override void Execute(KeyboardHandler keyboardHandler)
            {
                // get pressed keys list
                var pressedKeys = keyboardHandler.CurrentKeyboardState.GetPressedKeys();

                foreach (var dwn in pressedKeys)
                    if ((keyboardHandler.UnmanagedKeys.Count == 0) || !keyboardHandler.UnmanagedKeys.Contains(dwn))
                    {
                        keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyDownState);
                        break;
                    }
            }

            public override void Exit(KeyboardHandler keyboardHandler)
            {
            }

            public override void Reset(KeyboardHandler keyboardHandler)
            {
            }
        }

        /// <summary>
        /// Key down state.  A key down event is sent for EVERY new key found.  If one or more modifier keys only then only one
        /// kbkeydown is sent.
        /// </summary>
        private sealed class KeyboardKeyDownState : State<KeyboardHandler>
        {
            private TimeSpan _elapsedTimeSinceKeysChanged;

            public override void Enter(KeyboardHandler keyboardHandler)
            {
                _elapsedTimeSinceKeysChanged = _elapsedTimeSinceKeysChanged = keyboardHandler.LastPollTime.Elapsed;

                if (keyboardHandler._keyboardStateMachine.PreviousState == keyboardHandler._keyboardUnpressedState)
                {
                    // coming from unpressed, need to do processing
                    // if entering this state always reset time and focus key
                    keyboardHandler._focusKey = Keys.None;

                    // get keylist, strip out ctrl,alt,shift,unmanaged keys then send events
                    keyboardHandler.InitialiseLastKeyListFromKeyboardState();

                    if (keyboardHandler._lastKeyList.Length == 0)
                        keyboardHandler.CallHandleKeyboardKeyDown(keyboardHandler._lastKeyList, Keys.None, keyboardHandler._lastModifiers);

                    foreach (var kevent in keyboardHandler._lastKeyList)
                    {
                        // send event for each key (focus being different for each key)
                        keyboardHandler.CallHandleKeyboardKeyDown(keyboardHandler._lastKeyList, kevent, keyboardHandler._lastModifiers);

                        // focus key will always be the last one processed
                        keyboardHandler._focusKey = kevent;
                    }
                }
                else
                {
                    foreach (var newkey in keyboardHandler._newlyFoundKeys)
                    {
                        keyboardHandler.CallHandleKeyboardKeyDown(keyboardHandler._newKeyList, newkey, keyboardHandler._newModifiers);
                        keyboardHandler._focusKey = newkey;
                    }
                }
            }

            public override void Execute(KeyboardHandler keyboardHandler)
            {
                keyboardHandler.GetNewKeyListFromKeyboardState();

                if (keyboardHandler.HasNoKeysPressed)
                {
                    // nothing pressed, change state back to unpressed
                    keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardUnpressedState);
                    return;
                }

                if (keyboardHandler.AreKeysLost && keyboardHandler.HasNoAddedKeys)
                {
                    keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyLostState);
                }
                else
                {
                    if (keyboardHandler.HasNoAddedKeys)
                    {
                        if (keyboardHandler._newModifiers != keyboardHandler._lastModifiers)
                        {
                            var modifierDiff = keyboardHandler._newModifiers & keyboardHandler._lastModifiers;

                            // if we remain in this state in this portion of code, focus key will be none
                            keyboardHandler._focusKey = Keys.None;

                            // since a change has happened make a new timespan
                            _elapsedTimeSinceKeysChanged = keyboardHandler.LastPollTime.Elapsed;

                            if (modifierDiff == KeyboardModifier.None
                                || (modifierDiff & keyboardHandler._newModifiers) == (modifierDiff & keyboardHandler._lastModifiers))
                            {
                                // had one key the same but other two were different, send keyboard down
                                keyboardHandler.CallHandleKeyboardKeyDown(keyboardHandler._newKeyList, Keys.None, keyboardHandler._newModifiers);
                            }
                            else if ((modifierDiff & keyboardHandler._newModifiers) == modifierDiff)
                            {
                                // new mod bits only had 1, which means it lost one, change to lost state
                                keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyLostState);
                            }
                            else if ((modifierDiff & keyboardHandler._lastModifiers) == modifierDiff)
                            {
                                // old mod bits is less, send down event
                                keyboardHandler.CallHandleKeyboardKeyDown(keyboardHandler._newKeyList, Keys.None, keyboardHandler._newModifiers);
                            }
                            else
                                throw new Exception("code error, unhandled mod key state");
                        }
                        else if (keyboardHandler._focusKey != Keys.None
                                 && (keyboardHandler.LastPollTime.Elapsed.TotalMilliseconds - _elapsedTimeSinceKeysChanged.TotalMilliseconds >
                                     keyboardHandler._repeatDelay))
                        {
                            keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyRepeatState);
                        }
                    }
                    else
                        foreach (var newkey in keyboardHandler._newlyFoundKeys)
                        {
                            keyboardHandler.CallHandleKeyboardKeyDown(keyboardHandler._newKeyList, newkey, keyboardHandler._newModifiers);
                            keyboardHandler._focusKey = newkey;
                            _elapsedTimeSinceKeysChanged = keyboardHandler.LastPollTime.Elapsed;
                        }
                }
                
                keyboardHandler.UpdateLastKeyListWithNewKeyList();
            }

            public override void Exit(KeyboardHandler keyboardHandler)
            {
                keyboardHandler.UpdateLastKeyListWithNewKeyList();
            }

            public override void Reset(KeyboardHandler keyboardHandler)
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
        private sealed class KeyboardKeyLostState : State<KeyboardHandler>
        {
            public override void Enter(KeyboardHandler keyboardHandler)
            {
                keyboardHandler.CallHandleKeyboardKeyLost(keyboardHandler._newKeyList, keyboardHandler._newModifiers);
            }

            public override void Execute(KeyboardHandler keyboardHandler)
            {
                keyboardHandler.GetNewKeyListFromKeyboardState();

                if (keyboardHandler.HasNoKeysPressed)
                {
                    keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardUnpressedState);
                    return;
                }

                if (keyboardHandler.AreKeysLost && keyboardHandler.HasNoAddedKeys)
                {
                    // a key was released but keys are still down
                    keyboardHandler.CallHandleKeyboardKeyLost(keyboardHandler._newKeyList, keyboardHandler._newModifiers);
                }
                else
                {
                    if ((keyboardHandler.HasNoAddedKeys) && (keyboardHandler._newModifiers != keyboardHandler._lastModifiers))
                    {
                        var modifierDiff = keyboardHandler._newModifiers & keyboardHandler._lastModifiers;

                        // if we remain in this state in this portion of code, focus key will be none
                        keyboardHandler._focusKey = Keys.None;

                        if ((modifierDiff == KeyboardModifier.None)
                            // had one key the same but other two were different, send keyboard down
                            || ((modifierDiff & keyboardHandler._newModifiers) == (modifierDiff & keyboardHandler._lastModifiers)))
                        {
                            keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyDownState);
                        }
                        else if ((modifierDiff & keyboardHandler._newModifiers) == modifierDiff)
                        {
                            // new mod bits only had 1, which means it lost one,
                            // send key lost 
                            keyboardHandler.CallHandleKeyboardKeyLost(keyboardHandler._newKeyList, keyboardHandler._newModifiers);
                        }
                        else if ((modifierDiff & keyboardHandler._lastModifiers) == modifierDiff)
                        {
                            // old mod bits is less, send down event
                            keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyDownState);
                        }
                        else
                            throw new Exception("code error, unhandled mod key state");
                    }
                    else
                        keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyDownState);
                }

                keyboardHandler.UpdateLastKeyListWithNewKeyList();
            }

            public override void Exit(KeyboardHandler keyboardHandler)
            {
                keyboardHandler.UpdateLastKeyListWithNewKeyList();
            }

            public override void Reset(KeyboardHandler keyboardHandler)
            {
            }
        }

        /// <summary>
        /// Key repeat state is entered when a key is held down for long enough and nothing else occurs.  A key repeat
        /// event happens every single time a poll occurs if the repeat delay time has been exceeded.
        /// </summary>
        private sealed class KeyboardKeyRepeatState : State<KeyboardHandler>
        {
            private TimeSpan _lastTime;
            private double _repeatRunning = -1.0;

            public override void Enter(KeyboardHandler keyboardHandler)
            {
                _repeatRunning = -1.0;
            }

            public override void Execute(KeyboardHandler keyboardHandler)
            {
                keyboardHandler.GetNewKeyListFromKeyboardState();

                if (keyboardHandler._newKeyList.Length == 0)
                {
                    keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardUnpressedState);
                    return;
                }

                if (keyboardHandler.AreKeysLost && keyboardHandler.HasNoAddedKeys)
                {
                    // a key was released but keys are still down
                    keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyLostState);
                }
                else
                {
                    if (keyboardHandler.HasNoAddedKeys)
                        if (keyboardHandler._newModifiers != keyboardHandler._lastModifiers)
                        {
                            var modifierDiff = keyboardHandler._newModifiers & keyboardHandler._lastModifiers;

                            // if we remain in this state in this portion of code, focus key will be none
                            keyboardHandler._focusKey = Keys.None;

                            if ((modifierDiff == KeyboardModifier.None)
                                || ((modifierDiff & keyboardHandler._newModifiers) == (modifierDiff & keyboardHandler._lastModifiers)))
                            {
                                // had one key the same but other two were different, send keyboard down
                                keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyDownState);
                            }
                            else if ((modifierDiff & keyboardHandler._newModifiers) == modifierDiff)
                            {
                                // new mod bits only had 1, which means it lost one, change to lost state
                                keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyLostState);
                            }
                            else if ((modifierDiff & keyboardHandler._lastModifiers) == modifierDiff)
                            {
                                // old mod bits is less, send down event
                                keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyDownState);
                            }
                            else
                                throw new Exception("code error, unhandled mod key state");
                        }
                        else
                        {
                            // nothing at all changed, send a key repeat event (note - repeat time is detected in kbkeydown, so no need to keep track of time)
                            _repeatRunning -= (keyboardHandler.LastPollTime.Elapsed - _lastTime).TotalMilliseconds;
                            _lastTime = keyboardHandler.LastPollTime.Elapsed;

                            if (_repeatRunning < 0)
                            {
                                keyboardHandler.CallHandleKeyboardKeyRepeat(keyboardHandler._focusKey, keyboardHandler._lastModifiers);
                                _repeatRunning = keyboardHandler._repeatFrequency;
                            }
                        }
                    else
                        keyboardHandler._keyboardStateMachine.ChangeState(keyboardHandler._keyboardKeyDownState);
                }

                keyboardHandler.UpdateLastKeyListWithNewKeyList();
            }

            public override void Exit(KeyboardHandler keyboardHandler)
            {
                keyboardHandler.UpdateLastKeyListWithNewKeyList();
            }

            public override void Reset(KeyboardHandler keyboardHandler)
            {
            }
        }
    }
}