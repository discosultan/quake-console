using System;
using QuakeConsole.Utilities;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Features
{    
    internal class RepeatingInput
    {
        private readonly Timer _repeatedPressTresholdTimer = new Timer { AutoReset = false };
        private readonly Timer _repeatedPressIntervalTimer = new Timer { AutoReset = true };

        private bool _startRepeatedProcess;
        private bool _isFastRepeating;
        private Keys _downKey;

        private Console _console;

        public void LoadContent(Console console) => _console = console;

        public bool Enabled { get; set; } = true;

        public float RepeatingInputCooldown
        {
            get { return _repeatedPressIntervalTimer.TargetTime; }
            set { _repeatedPressIntervalTimer.TargetTime = Math.Max(value, 0); }
        }

        public float TimeUntilRepeatingInput
        {
            get { return _repeatedPressTresholdTimer.TargetTime; }
            set { _repeatedPressTresholdTimer.TargetTime = Math.Max(value, 0); }
        }

        public void Update(float deltaSeconds)
        {
            if (!Enabled || _console.State != ConsoleState.Open) return;

            ConsoleInput input = _console.ConsoleInput;

            if (!input.Input.IsKeyDown(_downKey))
                ResetRepeatingPresses();

            foreach (Keys key in input.Input.DownKeys)
            {
                if (key != _downKey)
                {
                    ResetRepeatingPresses();
                    _downKey = key;
                    _startRepeatedProcess = true;
                }
            }

            if (_startRepeatedProcess && !_isFastRepeating)
            {
                _repeatedPressTresholdTimer.Update(deltaSeconds);
                if (_repeatedPressTresholdTimer.Finished)
                {
                    _isFastRepeating = true;
                    _repeatedPressIntervalTimer.Reset();
                }
            }
            else if (_isFastRepeating)
            {
                _repeatedPressIntervalTimer.Update(deltaSeconds);
                if (_repeatedPressIntervalTimer.Finished)
                    input.HandleKey(_downKey);
            }
        }

        private void ResetRepeatingPresses()
        {
            _downKey = Keys.None;
            _startRepeatedProcess = false;
            _isFastRepeating = false;
            _repeatedPressTresholdTimer.Reset();
            _repeatedPressIntervalTimer.Reset();
        }
    }
}
