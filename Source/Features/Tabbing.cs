using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Features
{
    internal class Tabbing
    {
        private Console _console;        

        public bool Enabled { get; set; } = true;

        public string Tab { get; set; } = "    ";

        public void LoadContent(Console console) => _console = console;

        public bool ProcessAction(ConsoleAction action)
        {
            if (!Enabled) return false;            

            ConsoleInput input = _console.ConsoleInput;

            bool hasProcessedAction = false;
            switch (action)
            {
                case ConsoleAction.Tab:
                    Keys modifier;
                    _console.ActionDefinitions.BackwardTryGetValue(ConsoleAction.TabModifier, out modifier);
                    if (_console.Input.IsKeyDown(modifier))
                        input.RemoveTab();
                    else
                        input.Write(Tab);
                    hasProcessedAction = true;
                    break;
            }
            return hasProcessedAction;
        }
    }
}
