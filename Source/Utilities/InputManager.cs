#if MONOGAME
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Utilities
{
    internal struct KeyEvent
    {
        public KeyEventType Type;
        public Keys Key;
    }

    internal enum KeyEventType
    {
        Pressed,
        Released
    }

    internal class InputManager
    {
        private KeyboardState _previousKeyState;
        private KeyboardState _currentKeyState;

        public bool IsKeyDown(Keys key)
        {
            return _currentKeyState.IsKeyDown(key);
            //return _previousKeyState.IsKeyUp(key) && _currentKeyState.IsKeyDown(key);
        }

        public void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            KeyEvents.Clear();
            Keys[] pressedKeys = _currentKeyState.GetPressedKeys();
            foreach (Keys key in pressedKeys)
            {
                if (_previousKeyState.IsKeyDown(key) && _currentKeyState.IsKeyUp(key))
                    KeyEvents.Add(new KeyEvent {Key = key, Type = KeyEventType.Released});
                if (_previousKeyState.IsKeyUp(key) && _currentKeyState.IsKeyDown(key))
                    KeyEvents.Add(new KeyEvent { Key = key, Type = KeyEventType.Pressed });
            }
        }

        public List<KeyEvent> KeyEvents { get; } = new List<KeyEvent>();
    }
}
#endif