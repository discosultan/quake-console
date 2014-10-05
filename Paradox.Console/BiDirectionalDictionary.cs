using System;
using System.Collections;
using System.Collections.Generic;

namespace Varus.Paradox.Console
{
    /// <remarks>
    /// <see cref="IEnumerable"/> is implemented only to allow collection initializer syntax.
    /// </remarks>
    internal class BiDirectionalDictionary<TForward, TBackward> : IEnumerable
    {
        private readonly Dictionary<TForward, TBackward> _forward;
        private readonly Dictionary<TBackward, TForward> _backward;

        public BiDirectionalDictionary(int capacity = 4)
        {
            _forward = new Dictionary<TForward, TBackward>(capacity);
            _backward = new Dictionary<TBackward, TForward>(capacity);
        }

        public void Add(TForward forwardValue, TBackward backwardValue)
        {
            _forward.Add(forwardValue, backwardValue);
            _backward.Add(backwardValue, forwardValue);
        }

        public TBackward ForwardGetValue(TForward value)
        {
            return _forward[value];
        }

        public bool ForwardTryGetValue(TForward value, out TBackward result)
        {
            return _forward.TryGetValue(value, out result);
        }

        public TForward BackwardGetValue(TBackward value)
        {
            return _backward[value];
        }

        public bool BackwardTryGetValue(TBackward value, out TForward result)
        {
            return _backward.TryGetValue(value, out result);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
