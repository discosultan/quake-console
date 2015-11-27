using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using QuakeConsole.Input;
using QuakeConsole.Output;
using System;

namespace QuakeConsole
{
    public class RoslynInterpreter : ICommandInterpreter
    {
        private Script _previousInput;

        /// <summary>
        /// Gets or sets if the user entered command should be shown in the output.
        /// </summary>
        public bool EchoEnabled { get; set; } = true;

        public void Autocomplete(IConsoleInput input, bool forward)
        {
            input.Value = "your mother";
        }

        public void Execute(IConsoleOutput output, string command)
        {            
            if (EchoEnabled)
                output.Append(command);

            try
            {
                Script script;
                if (_previousInput == null)
                    script = CSharpScript.Create(command);
                else
                    script = _previousInput.ContinueWith(command);

                ScriptState endState = script.RunAsync().Result;
                _previousInput = endState.Script;

                if (endState.ReturnValue != null)
                    output.Append(endState.ReturnValue.ToString());
            }
            catch (CompilationErrorException e)
            {
                output.Append(string.Join(Environment.NewLine, e.Diagnostics));
            }
        }
    }
}
