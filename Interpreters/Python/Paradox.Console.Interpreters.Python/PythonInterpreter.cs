using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Scripting.Hosting;

namespace Varus.Paradox.Console.Interpreters.Python
{
    /// <summary>
    /// Runs <see cref="ConsoleShell"/> commands through an IronPython parser. Supports loading .NET types
    /// and provides autocomplete for them.
    /// </summary>
    public class PythonInterpreter : ICommandInterpreter
    {
        internal const StringComparison StringComparisonMethod = StringComparison.Ordinal;

        private readonly Autocompleter _autocompleter;
        private readonly TypeLoader _typeLoader;

        private readonly ScriptEngine _scriptEngine = IronPython.Hosting.Python.CreateEngine();             

        private bool _initialized;

        // Autocomplete entries.
        private readonly Dictionary<Type, MemberCollection> _staticMembers = new Dictionary<Type, MemberCollection>();
        private readonly Dictionary<Type, MemberCollection> _instanceMembers = new Dictionary<Type, MemberCollection>();
        private readonly Dictionary<string, Member> _instances = new Dictionary<string, Member>();
        private readonly Dictionary<string, Member> _statics = new Dictionary<string, Member>();        

        /// <summary>
        /// Constructs a new instance of <see cref="PythonInterpreter"/>.
        /// </summary>
        public PythonInterpreter()
        {
            _autocompleter = new Autocompleter(this);
            _typeLoader = new TypeLoader(this);
            Reset();            
            EchoEnabled = true;            
        }

        /// <summary>
        /// Gets or sets if the user entered command should be shown in the output.
        /// </summary>
        public bool EchoEnabled { get; set; }

        internal ScriptScope ScriptScope { get; private set; }
        internal Dictionary<Type, MemberCollection> StaticMembers { get { return _staticMembers; } }
        internal Dictionary<Type, MemberCollection> InstanceMembers { get { return _instanceMembers; } }
        internal Dictionary<string, Member> Instances { get { return _instances; }}
        internal Dictionary<string, Member> Statics { get { return _statics; } }
        internal bool InstancesAndStaticsDirty { get; set; }

        /// <summary>
        /// Executes a command by running it through the IronPython parser.
        /// </summary>
        /// <param name="outputBuffer">Console output buffer to append any output messages.</param>
        /// <param name="command">Command to execute.</param>
        public void Execute(IOutputBuffer outputBuffer, string command)
        {
            if (!_initialized)
            {
                var memStream = new MemoryStream();
                var pythonWriter = new OutputBufferWriter(memStream, outputBuffer);
                _scriptEngine.Runtime.IO.SetOutput(memStream, pythonWriter);
                _scriptEngine.Runtime.IO.SetErrorOutput(memStream, pythonWriter);
                _initialized = true;
            }

            if (EchoEnabled) outputBuffer.Append(command);

            string resultStr;            
            try
            {                
                dynamic result = RunScript(command);
                resultStr = result == null ? null : result.ToString();                
            }
            catch (Exception ex)
            {
                resultStr = ex.Message;
            }
            
            outputBuffer.Append(resultStr);
        }

        /// <summary>
        /// Tries to autocomplete the current input value in the <see cref="ConsoleShell"/> <see cref="InputBuffer"/>.
        /// </summary>
        /// <param name="inputBuffer">Console input.</param>
        /// <param name="forward">True if user wants to autocomplete to the next value; false if to the previous value.</param>
        public void Autocomplete(IInputBuffer inputBuffer, bool forward)
        {
            _autocompleter.Autocomplete(inputBuffer, forward);
        }

        /// <summary>
        /// Adds a search path for the IronPython engine to look for when importing modules.
        /// </summary>
        /// <param name="path">Path to add.</param>
        public void AddSearchPath(string path)
        {
            string dir = Path.GetDirectoryName(path);

            if (string.IsNullOrWhiteSpace(dir)) return;

            ICollection<string> paths = _scriptEngine.GetSearchPaths();
            paths.Add(dir);             
            _scriptEngine.SetSearchPaths(paths);
        }

        /// <summary>
        /// Adds a variable to the IronPython environment.
        /// </summary>
        /// <typeparam name="T">Type of variable to add.</typeparam>
        /// <param name="name">Name of the variable.</param>
        /// <param name="obj">Object to add.</param>
        public void AddVariable<T>(string name, T obj)
        {
            _typeLoader.AddVariable(name, obj);
        }

        /// <summary>
        /// Loads types to IronPython.
        /// </summary>
        /// <param name="types">Types to load.</param>
        public void AddTypes(params Type[] types)
        {
            _typeLoader.AddTypes(types);
        }

        /// <summary>
        /// Loads all the public non-nested types from the assembly to IronPython.
        /// </summary>
        /// <param name="assembly">Assembly to get types from.</param>
        public void AddAssembly(Assembly assembly)
        {
            _typeLoader.AddAssembly(assembly);
        }

        /// <summary>
        /// Runs a script straight on IronPython engine.
        /// </summary>
        /// <param name="script">Script to run.</param>
        /// <returns>Value returned by the IronPython engine.</returns>
        public dynamic RunScript(string script)
        {
            return _scriptEngine.CreateScriptSourceFromString(script).Compile().Execute(ScriptScope);
        }

        /// <summary>
        /// Resets the IronPython engine scope, clears any imported modules and .NET types.
        /// </summary>
        public void Reset()
        {
            ScriptScope = _scriptEngine.CreateScope();
            _typeLoader.Reset();
            _autocompleter.Reset();
            _instanceMembers.Clear();
            _staticMembers.Clear();
            _instances.Clear();
            _statics.Clear();            
            InstancesAndStaticsDirty = true;
            RunScript("import clr");
        }             
    }
}
