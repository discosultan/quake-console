using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Scripting.Hosting;
using QuakeConsole.Utilities;

namespace QuakeConsole
{
    /// <summary>
    /// Runs <see cref="Console"/> commands through an IronPython parser. Supports loading .NET types
    /// and provides autocomplete for them.
    /// </summary>
    public class PythonInterpreter : ICommandInterpreter
    {
        internal const StringComparison StringComparisonMethod = StringComparison.Ordinal;

        private readonly ScriptEngine _scriptEngine = IronPython.Hosting.Python.CreateEngine();
        private readonly Autocompleter _autocompleter;
        private readonly TypeLoader _typeLoader;

        // Autocomplete information.

        private bool _initialized;        

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
        internal Dictionary<Type, MemberCollection> StaticMembers { get; } = new Dictionary<Type, MemberCollection>();
        internal Dictionary<Type, MemberCollection> InstanceMembers { get; } = new Dictionary<Type, MemberCollection>();
        internal Dictionary<string, Member> Instances { get; } = new Dictionary<string, Member>();
        internal Dictionary<string, Member> Statics { get; } = new Dictionary<string, Member>();
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

            if (EchoEnabled)
                outputBuffer.Append(command);

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
        /// Tries to autocomplete the current input value in the <see cref="Console"/> <see cref="InputBuffer"/>.
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
        /// <param name="addSubTypes">
        /// Determines if subtypes of passed variable type will also be automatically added to IronPython environment.
        /// </param>
        public void AddVariable<T>(string name, T obj, bool addSubTypes = true)
        {
            _typeLoader.AddVariable(name, obj, addSubTypes);
        }

        /// <summary>
        /// Loads type to IronPython.
        /// </summary>
        /// <param name="type">Type to load.</param>
        /// <param name="addSubTypes">
        /// Determines if subtypes of passed type will also be automatically added to IronPython environment.
        /// </param>
        public void AddType(Type type, bool addSubTypes = true)
        {
            _typeLoader.AddType(type, addSubTypes);
        }

        /// <summary>
        /// Loads types to IronPython.
        /// </summary>
        /// <param name="types">Types to load.</param>
        /// <param name="addSubTypes">
        /// Determines if subtypes of passed types will also be automatically added to IronPython environment.
        /// </param>
        public void AddTypes(IEnumerable<Type> types, bool addSubTypes = true)
        {
            types.ForEach(type => _typeLoader.AddType(type, addSubTypes));
        }

        /// <summary>
        /// Loads all the public non-nested types from the assembly to IronPython.
        /// </summary>
        /// <param name="assembly">Assembly to get types from.</param>
        /// /// <param name="addSubTypes">
        /// Determines if subtypes of passed assembly's types will also be automatically added to IronPython environment.
        /// </param>
        public void AddAssembly(Assembly assembly, bool addSubTypes = true)
        {
            _typeLoader.AddAssembly(assembly, addSubTypes);
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
            InstanceMembers.Clear();
            StaticMembers.Clear();
            Instances.Clear();
            Statics.Clear();            
            InstancesAndStaticsDirty = true;
            RunScript("import clr");
            RunScript("from System import Array");
        }             
    }
}
