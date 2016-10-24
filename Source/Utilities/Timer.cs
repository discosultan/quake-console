using Microsoft.Xna.Framework;

namespace QuakeConsole
{
    internal class Timer
    {
        private float _time;

        public Timer(float targetTime = 1f)
        {
            TargetTime = targetTime;
        }

        public float TargetTime { get; set; }

        public float Progress => MathHelper.Clamp(_time / TargetTime, 0f, 1f);

        public bool AutoReset { get; set; }

        public bool Finished { get; protected set; }

        public void Reset()
        {
            _time = 0f;
            Finished = false;
        }        

        public void Update(float deltaTime)
        {
            if (AutoReset && Finished)
            {
                _time -= TargetTime;
                Finished = false;
            }

            _time += deltaTime;
            Finished |= _time >= TargetTime;
        }
    }
}
