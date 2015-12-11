using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace QuakeConsole
{
    // Required due to missing support for ExpandoObject as global on Roslyn side: https://github.com/dotnet/roslyn/issues/3194
    public class ExpandoWrapper
    {
        public dynamic globals;
    }

    /// <summary>
    /// Executes <see cref="Console"/> commands in a Roslyn C# scripting context. Supports loading .NET types
    /// and provides autocomplete for them.
    /// </summary>
    public class RoslynInterpreter : ICommandInterpreter
    {        
        private const int DefaultTypeLoaderRecursionLevel = 3;

        private readonly TypeLoader _typeLoader;
        private readonly Autocompleter _autocompleter;
        private readonly AutoResetEvent _executionSignal = new AutoResetEvent(true);

        private Task _warmupTask;
        private Script _previousInput;

        /// <summary>
        /// Constructs a new instance of <see cref="RoslynInterpreter"/>.
        /// </summary>
        public RoslynInterpreter()
        {
            _typeLoader = new TypeLoader(this);
            _autocompleter = new Autocompleter(_typeLoader);
            Reset();
        }

        /// <summary>
        /// Gets or sets if the user entered command should be shown in the output.
        /// </summary>
        public bool EchoEnabled { get; set; } = true;

        /// <summary>
        /// Tries to autocomplete the current input value in the <see cref="Console"/> <see cref="ConsoleInput"/>.
        /// </summary>
        /// <param name="input">Console input.</param>
        /// <param name="forward">True if user wants to autocomplete to the next value; false if to the previous value.</param>
        /// <remarks>Disabled due to missing dynamic global support: https://github.com/dotnet/roslyn/issues/3194</remarks>
        public void Autocomplete(IConsoleInput input, bool forward)
        {            
            //_autocompleter.Autocomplete(input, forward);
        }

        /// <summary>
        /// Executes a console command as C# script.
        /// </summary>
        /// <param name="output">Console output buffer to append any output messages.</param>
        /// <param name="command">Command to execute.</param>
        public void Execute(IConsoleOutput output, string command)
        {
            if (EchoEnabled)
                output.Append(command);


            if (!_warmupTask.IsCompleted)
                _warmupTask.Wait();

            Script script = _previousInput.ContinueWith(command, ScriptOptions);
            Task.Run(async () =>
            {
                try
                {
                    _executionSignal.WaitOne(); // TODO: timeout
                    ScriptState endState = await script.RunAsync(Globals);
                    if (endState.ReturnValue != null)
                        output.Append(endState.ReturnValue.ToString());                    
                    _previousInput = endState.Script;                    
                }
                catch (CompilationErrorException e)
                {
                    output.Append(string.Join(Environment.NewLine, e.Diagnostics));
                }
                finally
                {
                    _executionSignal.Set();
                }
            });
        }

        /// <summary>
        /// Adds a variable to C# script context.
        /// </summary>
        /// <typeparam name="T">Variable type.</typeparam>
        /// <param name="name">Name by which the variable is accessible in console.</param>
        /// <param name="obj">Instance of the variable.</param>
        /// <param name="recursionLevel">
        /// Determines if subtypes of passed type will also be automatically added to script context
        /// and if then how many levels deep this applies.
        /// </param>
        public void AddVariable<T>(string name, T obj, int recursionLevel = DefaultTypeLoaderRecursionLevel) =>
            _typeLoader.AddVariable(name, obj, recursionLevel);

        /// <summary>
        /// Removes a variable from the C# script context.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <returns>True if variable was removed; otherwise false.</returns>
        public bool RemoveVariable(string name) => _typeLoader.RemoveVariable(name);

        /// <summary>
        /// Resets the C# script context, clears any references and imports.
        /// </summary>
        public void Reset()
        {
            Globals = new ExpandoWrapper {globals = new ExpandoObject()};
            ScriptOptions = ScriptOptions.Default.WithReferences("System.Dynamic", "Microsoft.CSharp");
            _warmupTask = Task.Run(async () =>
            {
                // Assignment and literal evaluation to warm up the scripting context.
                // Without warmup, there is a considerable delay on first command evaluation.                
                _previousInput = (await CSharpScript.RunAsync(
                    code: "int quakeconsole_dummy_value = 1;",
                    globalsType: typeof(ExpandoWrapper),
                    globals: Globals                    
                )).Script;
                
            });            
            _typeLoader.Reset();            
        }

        internal ExpandoWrapper Globals { get; private set; }

        internal ScriptOptions ScriptOptions { get; set; }
    }
}
