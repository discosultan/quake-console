using System.Collections;
using System.Collections.Generic;

namespace Varus.Paradox.Console.Utilities
{
    /// <summary>
    /// Implements <see cref="IEnumerable{T}"/> for LINQ capabilities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CircularArray<T> : IEnumerable<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            private readonly CircularArray<T> _q;
            private int _index;            
            private T _currentElement;

            public T Current
            {
                get { return _currentElement; }
            }

            object IEnumerator.Current
            {
                get { return _currentElement; }
            }

            internal Enumerator(CircularArray<T> q)
            {
                _q = q;                
                _index = -1;
                _currentElement = default(T);
            }

            public void Dispose()
            {
                _index = -2;
                _currentElement = default(T);
            }

            public bool MoveNext()
            {
                if (_index == -2)
                {
                    return false;
                }
                _index++;
                if (_index == _q.Length)
                {
                    _index = -2;
                    _currentElement = default(T);
                    return false;
                }
                _currentElement = _q[_index];
                return true;
            }

            void IEnumerator.Reset()
            {
                _index = -1;
                _currentElement = default(T);
            }
        }

        private const int DefaultCapacity = 4;

        private T[] _array = new T[DefaultCapacity];
        private int _startIndex;

        public int Length { get; private set; }

        public T this[int index]
        {
            get { return _array[(_startIndex + index) % _array.Length]; }
            set { _array[(_startIndex + index) % _array.Length] = value; }
        }

        public void Clear()
        {
            _startIndex = 0;
            Length = 0;
        }

        public void Enqueue(T item)
        {
            EnsureArrayCapacity();
            _array[(_startIndex + Length++) % _array.Length] = item;
        }

        public T Dequeue()
        {
            Length--;
            T result = _array[_startIndex];
            _startIndex = (_startIndex + 1) % _array.Length;
            return result;
        }

        private void EnsureArrayCapacity()
        {
            if (Length >= _array.Length)
            {
                T[] tempArray = _array;

                // Double the array size.
                _array = new T[_array.Length * 2];

                // Copy elements from previous array to new array starting from 0.
                for (int i = _startIndex, j = 0; j < Length; i = (i + 1) % tempArray.Length, j++)
                {
                    _array[j] = tempArray[i];
                }

                _startIndex = 0;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
