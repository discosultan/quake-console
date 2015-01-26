namespace Varus.Paradox.Console.Interpreters.Python.Tests
{
    public class Kickup
    {
        public string Cymidine;

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
        public Pauciloquent(int moment)
        {
            Moment = moment;
        }

        public int Moment;
    }

    public enum Behen
    {
        Wretchedly,
        Razor,
        Verbarfungi
    }
}
