using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuakeConsole
{
    internal class SpriteFontStringBuilder
    {        
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly Dictionary<Point, string> _substringCache = new Dictionary<Point, string>();
        // Which is faster, string key lookup vs Font.MeasureString?
        private readonly Dictionary<string, Vector2> _stringSizeCache = new Dictionary<string, Vector2>();

        private bool _dirty = true;
        private string _value;

        public SpriteFont Font { get; set; }

        public int Length => _stringBuilder.Length;

        public void Append(string value)
        {
            _stringBuilder.Append(value);
            SetDirtyAndClearCache();
        }

        public void Append(StringBuilder value)
        {
            for (int i = 0; i < value.Length; i++)
                _stringBuilder.Append(value[i]);
        }

        public void Insert(int startIndex, string value)
        {
            _stringBuilder.Insert(startIndex, value);
            SetDirtyAndClearCache();
        }

        public void Remove(int startIndex, int length)
        {
            _stringBuilder.Remove(startIndex, length);
            SetDirtyAndClearCache();
        }

        public void Remove(int startIndex)
        {
            Remove(startIndex, _stringBuilder.Length - startIndex);
        }

        public string Substring(int startIndex, int length)
        {
            var cacheKey = new Point(startIndex, length);
            string substring;
            if (!_substringCache.TryGetValue(cacheKey, out substring))
            {
                substring = _stringBuilder.Substring(startIndex, length);
                _substringCache[cacheKey] = substring;
            }
            return substring;
        }

        public string Substring(int startIndex)
        {
            const int randomInvalidLength = -1;
            var cacheKey = new Point(startIndex, randomInvalidLength);
            string substring;
            if (!_substringCache.TryGetValue(cacheKey, out substring))
            {
                substring = _stringBuilder.Substring(startIndex);
                _substringCache[cacheKey] = substring;
            }
            return substring;
        }

        public Vector2 MeasureSubstring(int startIndex, int length)
        {
            if (Font == null) return Vector2.Zero;

            string key = Substring(startIndex, length);
            Vector2 stringSize;
            if (!_stringSizeCache.TryGetValue(key, out stringSize))
            {
                stringSize = Font.MeasureString(key);
                _stringSizeCache[key] = stringSize;
            }
            return stringSize;
        }

        public void Clear()
        {
            _stringBuilder.Clear();
            SetDirtyAndClearCache();
        }

        public char this[int i]
        {
            get { return _stringBuilder[i]; }
            set
            {
                _stringBuilder[i] = value;
                SetDirtyAndClearCache();
            }
        }

        public override string ToString()
        {
            if (_dirty)
            {
                _value = _stringBuilder.ToString();
                _dirty = false;
            }
            return _value;
        }

        private void SetDirtyAndClearCache()
        {
            _dirty = true;
            _substringCache.Clear();
            _stringSizeCache.Clear();
        }

        public static implicit operator StringBuilder(SpriteFontStringBuilder value) => value._stringBuilder;
    }
}
