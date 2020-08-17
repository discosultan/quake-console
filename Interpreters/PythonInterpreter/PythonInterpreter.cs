using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Scripting.Hosting;

namespace QuakeConsole
{
    /// <summary>
    /// Runs <see cref="Console"/> commands through an IronPython parser. Supports loading .NET types
    /// and provides autocomplete for them.
    /// </summary>
    public class PythonInterpreter : ICommandInterpreter
    {
        private const int DefaultRecursionLevel = 3;
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
        /// <param name="output">Console output buffer to append any output messages.</param>
        /// <param name="command">Command to execute.</param>
        public void Execute(IConsoleOutput output, string command)
        {
            if (!_initialized)
            {
                var memStream = new MemoryStream();
                var pythonWriter = new OutputBufferWriter(memStream, output);
                _scriptEngine.Runtime.IO.SetOutput(memStream, pythonWriter);
                _scriptEngine.Runtime.IO.SetErrorOutput(memStream, pythonWriter);
                _initialized = true;
            }

            if (EchoEnabled)
                output.Append(command);

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

            output.Append(resultStr);
        }

        /// <summary>
        /// Tries to autocomplete the current input value in the <see cref="Console"/> <see cref="ConsoleInput"/>.
        /// </summary>
        /// <param name="input">Console input.</param>
        /// <param name="forward">True if user wants to autocomplete to the next value; false if to the previous value.</param>
        public void Autocomplete(IConsoleInput input, bool forward) => _autocompleter.Autocomplete(input, forward);

        /// <summary>
        /// Adds a search path for the IronPython engine to look for when importing modules.
        /// </summary>
        /// <param name="path">Path to add.</param>
        public void AddSearchPath(string path)
        {
            string dir = Path.GetDirectoryName(path);

            if (string.IsNullOrWhiteSpace(dir))
                return;

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
        /// <param name="recursionLevel">
        /// Determines if subtypes of passed type will also be automatically added to IronPython environment
        /// and if then how many levels deep this applies.
        /// </param>
        public void AddVariable<T>(string name, T obj, int recursionLevel = DefaultRecursionLevel) =>
            _typeLoader.AddVariable(name, obj, recursionLevel);

        /// <summary>
        /// Removes a variable from the IronPython environment.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <returns>True if variable was removed; otherwise false.</returns>
        public bool RemoveVariable(string name) => _typeLoader.RemoveVariable(name);

        /// <summary>
        /// Loads type to IronPython.
        /// </summary>
        /// <param name="type">Type to load.</param>
        /// <param name="recursionLevel">
        /// Determines if subtypes of passed type will also be automatically added to IronPython environment
        /// and if then how many levels deep this applies.
        /// </param>
        public void AddType(Type type, int recursionLevel = DefaultRecursionLevel) =>
            _typeLoader.AddType(type, recursionLevel);

        /// <summary>
        /// Loads types to IronPython.
        /// </summary>
        /// <param name="types">Types to load.</param>
        /// <param name="recursionLevel">
        /// Determines if subtypes of passed types will also be automatically added to IronPython environment
        /// and if then how many levels deep this applies.
        /// </param>
        public void AddTypes(IEnumerable<Type> types, int recursionLevel = DefaultRecursionLevel) =>
            types.ForEach(type => _typeLoader.AddType(type, recursionLevel));

        /// <summary>
        /// Loads all the public non-nested types from the assembly to IronPython.
        /// </summary>
        /// <param name="assembly">Assembly to get types from.</param>
        /// <param name="recursionLevel">
        /// Determines if subtypes of types in assembly will also be automatically added to IronPython environment
        /// and if then how many levels deep this applies.
        /// </param>
        public void AddAssembly(Assembly assembly, int recursionLevel = DefaultRecursionLevel) =>
            _typeLoader.AddAssembly(assembly, recursionLevel);

        /// <summary>
        /// Runs a script straight on IronPython engine.
        /// </summary>
        /// <param name="script">Script to run.</param>
        /// <returns>Value returned by the IronPython engine.</returns>
        public dynamic RunScript(string script) =>
            _scriptEngine.CreateScriptSourceFromString(script).Compile().Execute(ScriptScope);

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
