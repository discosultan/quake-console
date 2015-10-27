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

        public bool IsKeyDown(Keys key) => _currentKeyState.IsKeyDown(key);        

        public bool IsKeyPressed(Keys key) => _previousKeyState.IsKeyUp(key) && _currentKeyState.IsKeyDown(key);

        public bool IsKeyToggled(Keys key)
        {
            switch (key)
            {
                case Keys.CapsLock:
                    return System.Windows.Input.Keyboard.IsKeyToggled(System.Windows.Input.Key.CapsLock);
                case Keys.NumLock:
                    return System.Windows.Input.Keyboard.IsKeyToggled(System.Windows.Input.Key.NumLock);
                default:
                    return false;
            }            
        }

        public void Update()
        {            
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            KeyEvents.Clear();
            Keys[] pressedKeys = _currentKeyState.GetPressedKeys(); // TODO: per frame heap allocs
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
