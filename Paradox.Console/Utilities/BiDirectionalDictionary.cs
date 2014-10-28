using System;
using System.Collections;
using System.Collections.Generic;

namespace Varus.Paradox.Console.Utilities
{
    /// <remarks>
    /// <see cref="IEnumerable"/> is implemented only to allow collection initializer syntax.
    /// </remarks>
    internal class BiDirectionalDictionary<TForward, TBackward> : IEnumerable
    {
        private readonly Dictionary<TForward, List<TBackward>> _forward;
        private readonly Dictionary<TBackward, List<TForward>> _backward;

        public BiDirectionalDictionary()
        {            
            _forward = new Dictionary<TForward, List<TBackward>>();
            _backward = new Dictionary<TBackward, List<TForward>>();
        }

        public void Add(TForward forwardValue, TBackward backwardValue)
        {
            // Add to forward.
            List<TBackward> backwardValues;
            if (!_forward.TryGetValue(forwardValue, out backwardValues))
            {
                backwardValues = new List<TBackward>();
                _forward.Add(forwardValue, backwardValues);
            }
            backwardValues.Add(backwardValue);

            // Add to backward.
            List<TForward> forwardValues;
            if (!_backward.TryGetValue(backwardValue, out forwardValues))
            {
                forwardValues = new List<TForward>();
                _backward.Add(backwardValue, forwardValues);
            }
            forwardValues.Add(forwardValue);
        }

        public bool ForwardTryGetValue(TForward value, out TBackward result)
        {
            List<TBackward> values;
            if (ForwardTryGetValues(value, out values))
            {
                result = values[0];
                return true;
            }
            result = default(TBackward);
            return false;
        }

        public bool ForwardTryGetValues(TForward value, out List<TBackward> result)
        {
            return _forward.TryGetValue(value, out result);
        }

        public bool BackwardTryGetValue(TBackward value, out TForward result)
        {
            List<TForward> values;
            if (BackwardTryGetValues(value, out values))
            {
                result = values[0];
                return true;
            }
            result = default(TForward);
            return false;
        }

        public bool BackwardTryGetValues(TBackward value, out List<TForward> result)
        {
            return _backward.TryGetValue(value, out result);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
