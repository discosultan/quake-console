
namespace Varus.Paradox.Console
{
    class ConsoleApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new ConsoleGame())
            {
                game.Run();
            }
        }
    }
}
