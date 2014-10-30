namespace Varus.Paradox.Console.CustomInterpreter
{
    public interface ICommand
    {
        CommandResult Execute(string[] args);
    }
}
