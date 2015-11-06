using System;
using Microsoft.Xna.Framework;
using QuakeConsole.Utilities;
#if MONOGAME
using MathUtil = Microsoft.Xna.Framework.MathHelper;
#endif

namespace QuakeConsole.Input
{
    internal class InputEntry
    {
        private readonly SpriteFontStringBuilder _buffer = new SpriteFontStringBuilder();
        private readonly ConsoleInput _input;

        private bool _dirty = true;

        public InputEntry(ConsoleInput input)
        {
            _input = input;
            input.PrefixChanged += (s, e) => SetDirty();
            input.Console.FontChanged += (s, e) =>
            {
                SetDirty();
                _buffer.Font = _input.Console.Font;
            };            
            input.Console.WindowAreaChanged += (s, e) => SetDirty();
            input.Caret.Moved += (s, e) => SetDirty(); // TODO: refactor out.

            _buffer.Font = _input.Console.Font;
        }

        public int Length => _buffer.Length;

        public string Value
        {
            get { return _buffer.ToString(); } // Does not allocate if value is cached.
            set
            {
                Clear();
                if (value != null)
                    _buffer.Append(value);
            }
        }

        public string VisibleValue
        {
            get
            {
                if (_dirty)
                {
                    CalculateStartAndEndIndices();
                    _dirty = false;
                }
                return _buffer.Substring(_visibleStartIndex, VisibleLength);
            }
        }

        public void Clear()
        {
            _buffer.Clear();
            SetDirty();
        }

        private int _visibleStartIndex;
        private int _visibleLength;

        public int VisibleStartIndex
        {
            get
            {
                if (_dirty)
                {
                    CalculateStartAndEndIndices();
                    _dirty = false;
                }
                return _visibleStartIndex;
            }
        }

        public int VisibleLength
        {
            get
            {
                if (_dirty)
                {
                    CalculateStartAndEndIndices();
                    _dirty = false;
                }
                return _visibleLength;
            }
        }

        private void SetDirty() => _dirty = true;        

        public void Append(string value)
        {
            _buffer.Append(value);
            SetDirty();
        }

        public void Insert(int index, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            _buffer.Insert(index, value);
            SetDirty();
        }

        public void Remove(int startIndex) => Remove(startIndex, Length - startIndex);        

        public void Remove(int startIndex, int length)
        {
            _buffer.Remove(startIndex, length);
            SetDirty();
        }

        public Vector2 MeasureSubstring(int startIndex, int length) => _buffer.MeasureSubstring(startIndex, length);

        public string Substring(int startIndex, int length) => _buffer.Substring(startIndex, length);

        public string Substring(int startIndex) => _buffer.Substring(startIndex);
        

        public char this[int i]
        {
            get { return _buffer[i]; }
            set
            {
                _buffer[i] = value;
                SetDirty();
            }
        }

        public override string ToString()
        {
            return _buffer.ToString();
        }

        private void CalculateStartAndEndIndices()
        {
            Console console = _input.Console;

            float windowWidth = console.WindowArea.Width - console.Padding * 2 - _input.InputPrefixSize.X;

            if (_input.CaretIndex > Length - 1)
                windowWidth -= _input.Caret.Width;

            while (_input.CaretIndex <= _visibleStartIndex && _visibleStartIndex > 0)
                _visibleStartIndex = Math.Max(_visibleStartIndex - _input.NumPositionsToMoveWhenOutOfScreen, 0);

            //_visibleLength = MathUtil.Clamp(_visibleLength, _input.Caret.Index, _input.Length);

            float widthProgress = 0f;
            _visibleLength = 0;
            int indexer = _visibleStartIndex;
            int targetIndex = _input.CaretIndex;
            while (indexer < Length)
            {
                char c = this[indexer++];

                float charWidth;
                if (!console.CharWidthMap.TryGetValue(c, out charWidth))
                {
                    charWidth = console.Font.MeasureString(c.ToString()).X;
                    console.CharWidthMap.Add(c, charWidth);
                }

                widthProgress += charWidth;

                if (widthProgress > windowWidth)
                {
                    if (targetIndex >= _visibleStartIndex && targetIndex - _visibleStartIndex < _visibleLength || indexer - 1 == _visibleStartIndex)
                        break;

                    if (targetIndex >= _visibleStartIndex)
                    {
                        _visibleStartIndex += _input.NumPositionsToMoveWhenOutOfScreen;
                        _visibleStartIndex = Math.Min(_visibleStartIndex, Length - 1);
                    }
                    CalculateStartAndEndIndices();
                    break;
                }

                _visibleLength++;
                //_visibleLength = indexer - _visibleStartIndex;
            }
        }
    }
}
