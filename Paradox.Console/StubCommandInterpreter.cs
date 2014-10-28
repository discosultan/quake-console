namespace Varus.Paradox.Console
{
    class StubCommandInterpreter : ICommandInterpreter
    {
        public void Execute(OutputBuffer viewBuffer, string command)
        {            
        }

        public void Autocomplete(InputBuffer inputBuffer, bool isNextValue)
        {            
        }
    }
}
