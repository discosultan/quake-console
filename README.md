**Compatible with Paradox 1.0.0 only!**

![Quake Console](https://jvcontent.blob.core.windows.net/images/quake_console_logo.png)
===============

Quake style console for [Paradox Game Engine](http://paradox3d.net/).<br/>
[Video of the console in action.](https://www.youtube.com/watch?v=oVWqy16W0ak)<br/>

![Quake style console](http://az695587.vo.msecnd.net/images/console.png)

## Getting Started

- [Console with IronPython interpreter nuget.](http://www.nuget.org/packages/Paradox.Console.PythonInterpreter)
- [Console with custom interpreter nuget.](http://www.nuget.org/packages/Paradox.Console.CustomInterpreter)
- [Console without interpreter nuget.](http://www.nuget.org/packages/Paradox.Console)
- [Small blog post describing how to setup the console.](http://jaanusvarus.com/quake-style-console-for-paradox-game-engine)

## Assemblies

### Core

- **Paradox.Console**: The core project for the console. Includes console shell and all the behavior associated with the shell's window.

### Interpreters

- **Paradox.Console.Interpreters.Python**: IronPython interpreter for the console shell. Allows manipulating game objects using Python scripting language. Provides autocompletion for loaded .NET types. Windows platform only.
- **Paradox.Console.Interpreters.Python.Tests**: Unit tests covering the expected execution and autocompletion behavior for PythonInterpreter.
- **Paradox.Console.Interpreters.Custom**: Custom interpreter for the console shell. Allows to register user defined commands and provides autocompletion for them. Cross platform.

### Sample

- **Paradox.Console.Sample.Game**: Sample game to try out the console and its command interpreters.
- **Paradox.Console.Sample.Windows**: Windows entry point to the sample game. Uses PythonInterpreter.
- **Paradox.Console.Sample.WindowsStore**: WindowsStore entry point to the sample game. Uses CustomInterpreter.
