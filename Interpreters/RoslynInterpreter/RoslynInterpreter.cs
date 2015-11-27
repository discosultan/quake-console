using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using QuakeConsole.Input;
using QuakeConsole.Output;
using System;
using System.Threading.Tasks;

namespace QuakeConsole
{
    public class RoslynInterpreter : ICommandInterpreter
    {
        private Script _previousInput;
        private Task _warmupTask;

        public RoslynInterpreter()
        {
            _warmupTask = Task.Factory.StartNew(() => 
            {
                // Assignment and literal evaluation to warm up the scripting context.
                // Without warmup, there is a considerable delay on first command evaluation since scripts
                // are being run synchronously.
                CSharpScript.EvaluateAsync("int x = 1; 1");
                _previousInput = CSharpScript.Create(null);
            });
        }

        /// <summary>
        /// Gets or sets if the user entered command should be shown in the output.
        /// </summary>
        public bool EchoEnabled { get; set; } = true;

        public void Autocomplete(IConsoleInput input, bool forward)
        {            
        }

        public void Execute(IConsoleOutput output, string command)
        {            
            if (EchoEnabled)
                output.Append(command);

            try
            {                
                _warmupTask.Wait();
                
                Script script = _previousInput.ContinueWith(command);
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
