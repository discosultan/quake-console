﻿using Microsoft.CodeAnalysis.CSharp.Scripting;
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

    public class RoslynInterpreter : ICommandInterpreter
    {        
        private const int DefaultRecursionLevel = 3;

        private readonly TypeLoader _typeLoader;
        private readonly AutoResetEvent _signal = new AutoResetEvent(true);

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

        internal ExpandoWrapper Globals { get; } = new ExpandoWrapper();

        public void Autocomplete(IConsoleInput input, bool forward)
        {            
        }

        public void Execute(IConsoleOutput output, string command)
        {
            if (EchoEnabled)
                output.Append(command);


            if (!_warmupTask.IsCompleted)
                _warmupTask.Wait();

            Script script = _previousInput.ContinueWith(command);
            Task.Run(async () =>
            {
                try
                {
                    _signal.WaitOne(); // TODO: timeout
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
                    _signal.Set();
                }
            });
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
            Globals.globals = new ExpandoObject();
            _warmupTask = Task.Run(async () =>
            {
                // Assignment and literal evaluation to warm up the scripting context.
                // Without warmup, there is a considerable delay on first command evaluation.                
                _previousInput = (await CSharpScript.RunAsync(
                    code: "int quakeconsole_dummy_value = 1;",
                    globalsType: typeof(ExpandoWrapper),
                    globals: Globals,
                    options: ScriptOptions.Default.WithReferences("System.Dynamic", "Microsoft.CSharp")
                )).Script;
                
            });            
            _typeLoader.Reset();            
        }                
    }
}