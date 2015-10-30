using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Utilities
{
    internal class InputState
    {
        public KeyboardState CurrentKeyboardState { get; set; }
        public KeyboardState PreviousKeyboardState { get; set; }

        public bool IsKeyDown(Keys key) => CurrentKeyboardState.IsKeyDown(key);

        public bool IsKeyPressed(Keys key) => PreviousKeyboardState.IsKeyUp(key) && CurrentKeyboardState.IsKeyDown(key);

        public bool IsKeyReleased(Keys key) => PreviousKeyboardState.IsKeyDown(key) && CurrentKeyboardState.IsKeyUp(key);

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

        public void Clear()
        {
            DownKeys.Clear();
            PressedKeys.Clear();
            CurrentKeyboardState = default(KeyboardState);
            PreviousKeyboardState = default(KeyboardState);
        }

        public List<Keys> DownKeys { get; } = new List<Keys>();
        public List<Keys> PressedKeys { get; } = new List<Keys>();

        public void CopyTo(InputState other)
        {
            other.PreviousKeyboardState = PreviousKeyboardState;
            other.CurrentKeyboardState = CurrentKeyboardState;
            other.DownKeys.Clear();
            other.PressedKeys.Clear();                     
            other.DownKeys.AddRange(DownKeys);
            other.PressedKeys.AddRange(PressedKeys);
        }

        public void Update()
        {
            PreviousKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            DownKeys.Clear();
            PressedKeys.Clear();
            Keys[] keys = CurrentKeyboardState.GetPressedKeys(); // TODO: per frame heap allocs
            foreach (Keys key in keys)
            {
                if (PreviousKeyboardState.IsKeyUp(key) && IsKeyDown(key))
                    PressedKeys.Add(key);
                //else //if (_previousKeyState.IsKeyDown(key) && _currentKeyState.IsKeyDown(key))
                //    DownKeys.Add(key);
                if (IsKeyDown(key))
                    DownKeys.Add(key);
            }       
        }
    }
}
