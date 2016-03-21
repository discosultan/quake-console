namespace QuakeConsole.Samples.HelloPython
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var game = new HelloPythonGame())
                game.Run();
        }
    }
}
