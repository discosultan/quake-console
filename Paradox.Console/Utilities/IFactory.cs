namespace Varus.Paradox.Console.Utilities
{
    internal interface IFactory<out T> where T : class
    {
        T New();
    }
}
