using SiliconStudio.Core.Mathematics;

namespace Varus.Paradox.Console.Sample
{
    public class Cube
    {
        public Vector3 Position = new Vector3(0, 0, -7);
        public Vector3 RotationSpeed = new Vector3(2, 0f, 0);
        public Vector3 Scale = Vector3.One;
        public Vector3 Rotation;

        public void SetPosition(Vector3 pos)
        {
            Position = pos;
        }

        public void SetPosition(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        public void SetSpeed(CubeSpeed speed)
        {
            switch (speed)
            {
                case CubeSpeed.Fast:
                    RotationSpeed = new Vector3(10);
                    break;
                default:
                    RotationSpeed = new Vector3(1);
                    break;
            }
        }
    }

    public enum CubeSpeed
    {
        Fast,
        Slow
    }
}
