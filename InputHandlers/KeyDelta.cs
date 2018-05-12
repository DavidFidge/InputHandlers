using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace InputHandlers.Keyboard
{
    internal class KeyDelta
    {
        private readonly List<Keys> _unmanagedKeys;
        private Keys[] _lastKeyList;
        private Keys[] _newKeyList;
        private bool _requiresUpdate;

        public Keys FocusKey { get; private set; }
        public bool TreatModifiersAsKeys { get; set; }

        public KeyboardModifier LastModifiers { get; private set; }

        public Keys[] NewKeyList
        {
            get { return _newKeyList; }
        }

        public List<Keys> NewKeyDelta { get; private set; }

        public KeyboardModifier NewModifiers { get; private set; }

        public KeyDelta(List<Keys> unmanagedKeys)
        {
            _lastKeyList = new Keys[0];
            _newKeyList = new Keys[0];
            NewKeyDelta = new List<Keys>();
            FocusKey = Keys.None;
            TreatModifiersAsKeys = false;
            _unmanagedKeys = unmanagedKeys;
        }

        public void Stop()
        {
            _requiresUpdate = false;
        }

        public void Update(KeyboardState currentKeyboardState)
        {
            if (_requiresUpdate)
            {
                if (!TreatModifiersAsKeys)
                {
                    LastModifiers = NewModifiers;
                    NewModifiers = currentKeyboardState.GetModifiers();
                }

                _lastKeyList = _newKeyList;
                _newKeyList = currentKeyboardState.GetPressedKeys();

                StripUnmanagedKeysAndModifiers(ref _newKeyList);

                NewKeyDelta = _newKeyList.Except(_lastKeyList).ToList();

                if (NewKeyDelta.Any())
                    FocusKey = NewKeyDelta.First();
                else if (_lastKeyList.Except(_newKeyList).Any())
                    FocusKey = Keys.None;
            }
        }

        public void Start(KeyboardState currentKeyboardState)
        {
            _requiresUpdate = true;
            _lastKeyList = currentKeyboardState.GetPressedKeys();

            if (!TreatModifiersAsKeys)
                LastModifiers = currentKeyboardState.GetModifiers();

            StripUnmanagedKeysAndModifiers(ref _lastKeyList);
            NewKeyDelta.Clear();
            FocusKey = Keys.None;
        }

        public bool AreKeysLost
        {
            get
            {
                return _newKeyList.Length < _lastKeyList.Length;
            }
        }

        public bool HasNoAddedKeys
        {
            get
            {
                return !NewKeyDelta.Any();
            }
        }

        public bool HasAddedKeys
        {
            get
            {
                return NewKeyDelta.Any();
            }
        }

        public bool HasNoKeysPressed
        {
            get
            {
                return !_newKeyList.Any();
            }
        }

        private bool IsUnmanagedKey(Keys key)
        {
            return _unmanagedKeys.Contains(key);
        }

        private bool IsModifierKeyAndNotTreatingModifiersAsKey(Keys key)
        {
            return !TreatModifiersAsKeys && key.IsModifierKey();
        }

        private void StripUnmanagedKeysAndModifiers(ref Keys[] keyList)
        {
            if (!_unmanagedKeys.Any() && TreatModifiersAsKeys)
                return;

            int keyListIndex;
            var keyListUpdateIndex = 0;

            for (keyListIndex = 0; keyListIndex < keyList.Length; keyListIndex++)
            {
                if (!(IsModifierKeyAndNotTreatingModifiersAsKey(keyList[keyListIndex]) ||
                      IsUnmanagedKey(keyList[keyListIndex])))
                {
                    keyList[keyListUpdateIndex] = keyList[keyListIndex];
                    keyListUpdateIndex++;
                }
            }

            if (keyListUpdateIndex < keyList.Length)
            {
                keyList = keyList.Take(keyListUpdateIndex).ToArray();
            }
        }
    }
}