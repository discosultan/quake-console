using System.Text;

namespace QuakeConsole.Interpreters.Tests
{
    public class FakeInputBuffer : IInputBuffer
    {
        private readonly StringBuilder _stringBuffer = new StringBuilder();
        private readonly ICaret _caret = new FakeCaret();

        public string LastAutocompleteEntry { get; set; }
        public ICaret Caret { get { return _caret; } }
        public int Length { get { return _stringBuffer.Length; } }

        public void Write(string symbol)
        {
            _stringBuffer.Append(symbol);
        }

        public void Remove(int startIndex, int length)
        {
            _stringBuffer.Remove(startIndex, length);
        }

        public string Value
        {
            get { return _stringBuffer.ToString(); }
            set
            {
                _stringBuffer.Clear();
                _stringBuffer.Append(value);
            }
        }

        public string Substring(int startIndex, int length)
        {
            return _stringBuffer.ToString().Substring(startIndex, length);
        }

        public string Substring(int startIndex)
        {
            return _stringBuffer.ToString().Substring(startIndex);
        }

        public void Clear()
        {
            _stringBuffer.Clear();
        }

        public char this[int i]
        {
            get { return _stringBuffer[i]; }
        }
    }
}
