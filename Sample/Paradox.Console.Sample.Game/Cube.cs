using SiliconStudio.Core.Mathematics;

namespace Varus.Paradox.Console.Sample
{
    public class Cube
    {
        public Vector3 Position = new Vector3(0, 0, -7);
        public Vector3 RotationSpeed = new Vector3(2, 0f, 0);
        public Vector3 Scale = Vector3.One;
        public Vector3 Rotation;

        // Below this point are random members which might provide useful to test interpreter capabilities
        // in terms of type reflection and autocompletion.

        public Pauciloquent Pauciloquent { get; set; }

        private Behen _behen;

        public void SetBehen(Behen behen)
        {
            _behen = behen;
        }

        public void SetBehen(Behen behen, Pauciloquent pauciloquent)
        {
            SetBehen(behen);
            Pauciloquent = pauciloquent;
        }

        public Behen GetBehen()
        {
            return _behen;
        }
    }

    public struct Pauciloquent
    {
        public int Moment;
    }

    public enum Behen
    {
        Wretchedly,
        Razor,
        Verbarfungi
    }
}
