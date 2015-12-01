using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using QuakeConsole.Input;
using QuakeConsole.Output;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace QuakeConsole
{
    // Required due to bug: https://github.com/dotnet/roslyn/issues/3194
    public class Dummy
    {
        public ExpandoObject x;
    }

    public class RoslynInterpreter : ICommandInterpreter
    {
        private const int DefaultRecursionLevel = 3;

        private readonly TypeLoader _typeLoader;
        
        private Task _warmupTask;        
        private Script _previousInput;        

        /// <summary>
        /// Constructs a new instance of <see cref="RoslynInterpreter"/>.
        /// </summary>
        public RoslynInterpreter()
        {            
            _typeLoader = new TypeLoader(this);
            Reset();
        }

        /// <summary>
        /// Gets or sets if the user entered command should be shown in the output.
        /// </summary>
        public bool EchoEnabled { get; set; } = true;

        internal dynamic Globals { get; private set; }

        public void Autocomplete(IConsoleInput input, bool forward)
        {            
        }        

        public void Execute(IConsoleOutput output, string command)
        {            
            if (EchoEnabled)
                output.Append(command);

            try
            {
                if (!_warmupTask.IsCompleted)
                    _warmupTask.Wait();

                Script script = _previousInput.ContinueWith(command);
                ScriptState endState = script.RunAsync(new Dummy { x = Globals }).Result;                
                _previousInput = endState.Script;

                if (endState.ReturnValue != null)
                    output.Append(endState.ReturnValue.ToString());
            }
            catch (CompilationErrorException e)
            {
                output.Append(string.Join(Environment.NewLine, e.Diagnostics));
            }
        }

        /// <summary>
        /// Adds a variable to C# script context.
        /// </summary>
        /// <typeparam name="T">Variable type.</typeparam>
        /// <param name="name">Name of the variable.</param>
        /// <param name="obj">Instance of the variable.</param>
        /// <param name="recursionLevel">
        /// Determines if subtypes of passed type will also be automatically added to script context
        /// and if then how many levels deep this applies.
        /// </param>
        public void AddVariable<T>(string name, T obj, int recursionLevel = DefaultRecursionLevel) =>
            _typeLoader.AddVariable(name, obj, recursionLevel);

        public void Reset()
        {
            _warmupTask = Task.Run(async () =>
            {
                // Assignment and literal evaluation to warm up the scripting context.
                // Without warmup, there is a considerable delay on first command evaluation since scripts
                // are being run synchronously.                
                _previousInput = (await CSharpScript.RunAsync(
                    "int quakeconsole_dummy_value = 1;",
                    globalsType: typeof(ExpandoObject),
                    globals: new ExpandoObject()
                )).Script;
            });

            //_previousInput = CSharpScript.Create(null, globalsType: typeof(Dummy));
            Globals = new ExpandoObject();
            _typeLoader.Reset();            
        }                
    }
}
