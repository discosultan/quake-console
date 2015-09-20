#if MONOGAME
using MathUtil = Microsoft.Xna.Framework.MathHelper;
#endif

namespace QuakeConsole.Utilities
{
    internal class Timer
    {
        private float _time;

        public Timer(float targetTime = 1f)
        {
            TargetTime = targetTime;
        }

        /// <summary>
        /// Gets or sets target time for timer. After every period of time defined
        /// by this value, the action assigned to the timer will execute and timer is reset.
        /// This value cannot be lesser than or equal to zero.
        /// </summary>
        public float TargetTime { get; set; }

        /// <summary>
        /// Gets the current progress of the timer as a value between zero and one, where
        /// zero indicates the beginning and one the finishing of the timer.
        /// </summary>
        public float Progress => MathUtil.Clamp(_time / TargetTime, 0f, 1f);

        /// <summary>
        /// Resets current time back to zero.
        /// </summary>
        public void Reset()
        {
            _time = 0f;
            Finished = false;
        }

        public bool AutoReset { get; set; }

        /// <summary>
        /// Gets if the timer finished this frame.
        /// </summary>
        public bool Finished { get; protected set; }

        /// <summary>
        /// Updates the timer. If target time is reached, runs the action assigned to the timer.
        /// </summary>
        /// <param name="deltaTime">Time passed since last update.</param>
        public void Update(float deltaTime)
        {
            if (AutoReset && Finished)
            {
                _time -= TargetTime;
                Finished = false;
            }

            _time += deltaTime;

            if (_time >= TargetTime)
            {
                Finished = true;
            }
        }
    }
}
