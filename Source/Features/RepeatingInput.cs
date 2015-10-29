using System;
using QuakeConsole.Utilities;

namespace QuakeConsole.Features
{    
    internal class RepeatingInput
    {
        private readonly Timer _repeatedPressTresholdTimer = new Timer { AutoReset = false };
        private readonly Timer _repeatedPressIntervalTimer = new Timer { AutoReset = true };

        private bool _startRepeatedProcess;
        private bool _isFastRepeating;
        private bool _isActionInsteadOfSymbol;
        private bool _anyInput;
        private Symbol _lastSymbol;
        private ConsoleAction _lastAction;

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

            if (!_anyInput)
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
                    if (_isActionInsteadOfSymbol)
                        input.ProcessAction(_lastAction);
                    else
                        input.ProcessSymbol(_lastSymbol);
            }
            _anyInput = false;
        }

        public void OnSymbol(Symbol symbol)
        {
            if (_isActionInsteadOfSymbol)
            {
                _isActionInsteadOfSymbol = false;
                ResetRepeatingPresses();                
            }
            else if (_lastSymbol != symbol)
            {
                ResetRepeatingPresses();
            }
            _lastSymbol = symbol;
            _startRepeatedProcess = true;
            _anyInput = true;
        }

        public void OnAction(ConsoleAction action)
        {
            if (!_isActionInsteadOfSymbol)
            {
                _isActionInsteadOfSymbol = true;
                ResetRepeatingPresses();
            }
            else if (_lastAction != action)
            {                
                ResetRepeatingPresses();                
            }
            _lastAction = action;
            _startRepeatedProcess = true;
            _anyInput = true;
        }

        private void ResetRepeatingPresses()
        {            
            _startRepeatedProcess = false;
            _isFastRepeating = false;
            _repeatedPressTresholdTimer.Reset();
            _repeatedPressIntervalTimer.Reset();
        }
    }
}
