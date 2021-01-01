using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace InputHandlers.Keyboard
{
    internal class KeyDelta
    {
        private readonly IList<Keys> _unmanagedKeys;
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

        public IList<Keys> NewKeyDelta { get; private set; }

        public KeyboardModifier NewModifiers { get; private set; }

        public KeyDelta(IList<Keys> unmanagedKeys)
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
                _lastKeyList = _newKeyList;
                _newKeyList = currentKeyboardState.GetPressedKeys();

                StripUnmanagedKeys(ref _newKeyList);

                if (!TreatModifiersAsKeys)
                {
                    LastModifiers = NewModifiers;
                    NewModifiers = _newKeyList.GetModifiers();
                    StripModifiers(ref _newKeyList);
                }

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

            StripUnmanagedKeys(ref _lastKeyList);

            if (!TreatModifiersAsKeys)
            {
                LastModifiers = _lastKeyList.GetModifiers();
                StripModifiers(ref _lastKeyList);
            }

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

        private void StripUnmanagedKeys(ref Keys[] keyList)
        {
            if (!_unmanagedKeys.Any())
                return;

            var keyListUpdateIndex = 0;

            StripKeys(key => !IsUnmanagedKey(key), ref keyList, keyListUpdateIndex);
        }

        private void StripModifiers(ref Keys[] keyList)
        {
            var keyListUpdateIndex = 0;

            StripKeys(key => !key.IsModifierKey(), ref keyList, keyListUpdateIndex);
        }

        private void StripKeys(Func<Keys, bool> keepKeyFunc, ref Keys[] keyList, int keyListUpdateIndex)
        {
            int keyListIndex;
            for (keyListIndex = 0; keyListIndex < keyList.Length; keyListIndex++)
            {
                if (keepKeyFunc(keyList[keyListIndex]))
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