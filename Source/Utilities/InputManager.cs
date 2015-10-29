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

        public List<Keys> DownKeys { get; private set; }
        public List<Keys> PressedKeys { get; } = new List<Keys>();

        public void Update()
        {            
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            DownKeys.Clear();
            PressedKeys.Clear();
            Keys[] keys = _currentKeyState.GetPressedKeys(); // TODO: per frame heap allocs
            foreach (Keys key in keys)
            {
                if (_previousKeyState.IsKeyUp(key) && _currentKeyState.IsKeyDown(key))
                    PressedKeys.Add(key);
                else //if (_previousKeyState.IsKeyDown(key) && _currentKeyState.IsKeyDown(key))
                    DownKeys.Add(key);
            }
        }

        //public List<KeyEvent> KeyEvents { get; } = new List<KeyEvent>();
    }
}
