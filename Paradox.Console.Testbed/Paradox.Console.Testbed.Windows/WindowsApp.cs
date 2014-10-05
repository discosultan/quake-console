
namespace Varus.Paradox.Console.Testbed.Windows
{
    class WindowsApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new WindowsGame())
            {
                game.Run();
            }
        }
    }
}
