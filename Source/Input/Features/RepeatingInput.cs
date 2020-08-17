using System;

namespace QuakeConsole
{
    internal class RepeatingInput
    {
        private readonly Timer _repeatedPressTresholdTimer = new Timer { AutoReset = false };
        private readonly Timer _repeatedPressIntervalTimer = new Timer { AutoReset = true };

        private bool _startRepeatedProcess;
        private bool _isFastRepeating;
        private readonly InputState _inputToRepeat = new InputState();

        private ConsoleInput _input;

        public void LoadContent(ConsoleInput input)
        {
            _input = input;
            _input.InputChanged += (s, e) => CheckRepeatingProcess();
            _input.Caret.Moved += (s, e) => CheckRepeatingProcess();
        }

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
            if (!Enabled) return;

            if (_input.Input.CurrentKeyboardState != _inputToRepeat.CurrentKeyboardState)
                ResetRepeatingPresses();

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
                    _input.ProcessInput(_inputToRepeat);
            }
        }

        private void CheckRepeatingProcess()
        {
            if (_inputToRepeat.CurrentKeyboardState != _input.Input.CurrentKeyboardState)
            {
                ResetRepeatingPresses();
                _input.Input.CopyTo(_inputToRepeat);
                _startRepeatedProcess = true;
            }
        }

        private void ResetRepeatingPresses()
        {
            _inputToRepeat.Clear();
            _startRepeatedProcess = false;
            _isFastRepeating = false;
            _repeatedPressTresholdTimer.Reset();
            _repeatedPressIntervalTimer.Reset();
        }
    }
}
