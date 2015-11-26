using System;
using System.Collections;
using System.Collections.Generic;

namespace QuakeConsole.Utilities
{
    /// <remarks>
    /// <see cref="IEnumerable"/> is implemented only to allow collection initializer syntax.
    /// </remarks>
    internal class BiDirectionalMultiValueDictionary<TForward, TBackward> : IEnumerable
    {
        private readonly Dictionary<TForward, List<TBackward>> _forward = new Dictionary<TForward, List<TBackward>>();
        private readonly Dictionary<TBackward, List<TForward>> _backward = new Dictionary<TBackward, List<TForward>>();

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

        public bool Remove(TForward forwardValue)
        {
            List<TBackward> backwards;
            if (_forward.TryGetValue(forwardValue, out backwards))
            {
                bool hasItems = backwards.Count > 0;
                backwards.ForEach(x => _backward[x].Remove(forwardValue));
                backwards.Clear();
                return hasItems;
            }
            return false;
        }

        public bool Remove(TBackward backwardValue)
        {
            List<TForward> forwards;
            if (_backward.TryGetValue(backwardValue, out forwards))
            {
                bool hasItems = forwards.Count > 0;
                forwards.ForEach(x => _forward[x].Remove(backwardValue));
                forwards.Clear();
                return hasItems;
            }
            return false;
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
