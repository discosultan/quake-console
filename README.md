# What is this sorcery?

Quake-style console is an in-game command-line interface with swappable command interpreters. It can be used during development to easily manipulate game objects *at runtime* or allow players to enter cheat codes, for example.

![Screenshot](http://az695587.vo.msecnd.net/images/console_merged.png)

- https://www.youtube.com/watch?v=Is2m2oQ68Gc
- https://www.youtube.com/watch?v=oVWqy16W0ak

# Getting Started

- [Building source and samples](#setup1)
- [Using QuakeConsole](#setup2)
- [Setting up console to use PythonInterpreter](#setup3)
- [Setting up console to use ManualInterpreter](#setup4)
- [Setting up console to use RoslynInterpreter](#setup5)


<h2 id="setup1">Building source and samples</h2>


The following is required to successfully compile the QuakeConsole MonoGame solution:

- Visual studio 2015+
- .NET Framework 4.6+
- [DirectX End-User Runtimes (June 2010)](http://www.microsoft.com/en-us/download/details.aspx?id=8109)
- MonoGame 3.5+


<h2 id="setup2">Using QuakeConsole</h2>

> Nuget packages currently available only for MonoGame WindowsDX platform!

### Requirements

- MonoGame.WindowsDX 3.5+
- .NET Framework 4.5+

### Setup

Install the console assembly through NuGet:

```powershell
PM> Install-Package QuakeConsole.MonoGame.WindowsDX -Pre
```

The console itself is a typical `DrawableGameComponent`. The following steps will go through setting it up in a game.

1) In the `Game` constructor, create the console and add it to the components collection (console itself should be stored in a variable since it must be initialized and manually opened/closed later):

```cs
ConsoleComponent console;

public Game1()
{
  // ...
  console = new ConsoleComponent(this);
  Components.Add(console);
}
```

2) The console must be opened for it to accept user input. This is usually done in the update method by checking for a key press (the tilde key, for example):

```cs
protected override void Update(GameTime gameTime)
{
  // ...
  // manage previous and current keyboard states
  if (previousKeyboardState.IsKeyUp(Keys.OemTilde) && currentKeyboardState.IsKeyDown(Keys.OemTilde))
    console.ToggleOpenClose();
}
```

This has setup the console shell. For the console to actually do anything useful on user input, an interpreter must be configured (see below).

Sometimes it is desirable to prevent other game systems from accepting input while the console window is open. For this, it is required to know if the console is currently open (accepting input) or closed. This can be checked by the  `console.IsAcceptingInput` property.


<h2 id="setup3">Setting up console to use PythonInterpreter</h2>

Python interpreter can be used to interpret user input as Python code. It is extremely useful to, for example, manipulate game objects *at runtime*.

### Requirements

- MonoGame.WindowsDX 3.5+
- .NET Framework 4.5+

### Setup

Install the interpreter assembly through NuGet (this will also bring in the console if it hasn't been installed already):

```powershell
PM> Install-Package QuakeConsole.PythonInterpreter.MonoGame.WindowsDX -Pre
```

1) Create the interpreter and set it as the interpreter for the console:

```cs
var interpreter = new PythonInterpreter();
console.Interpreter = Interpreter;
```

2) To be able to modify game objects through the console, the objects must be added as variables to the IronPython engine (this creates the connection between the CLR and Python object):

```cs
interpreter.AddVariable("name", myVariable);
```

The object's public members can now be accessed from the console using the passed variable's name (press ctrl + space [by default] to autocomplete input to known variables/types/members).


<h2 id="setup4">Setting up console to use ManualInterpreter</h2>

Manual interpreter can be used to define commands and their corresponding actions for the console manually. Useful to execute some behavior on command or provide players means to input cheat codes, for example.

### Requirements

- MonoGame.WindowsDX 3.5+
- .NET Framework 4.5+

### Setup

Install the interpreter assembly through NuGet (this will also bring in the console if it hasn't been installed already):

```powershell
PM> Install-Package QuakeConsole.ManualInterpreter.MonoGame.WindowsDX -Pre
```

1) Create the interpreter and set it as the interpreter for the console:

```cs
var interpreter = new ManualInterpreter();
console.Interpreter = Interpreter;
```

A command is essentially a delegate that is invoked when user inputs the name of the command. The delegate provides an array of arguments separated by spaces (similar to arguments in a Windows console application) and optionally can return a string value that is output to the console.

2) To register a command:

```cs
interpreter.RegisterCommand("name", action);
```

where action is of type `Action<string[]>` or `Func<string[], string>`.

Provides autocompletion for registered command names (ctrl + space by default).


<h2 id="setup5">Setting up console to use RoslynInterpreter</h2>


Roslyn interpreter can be used to interpret user input as C# code using the [Roslyn scripting API](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples). It is useful to, for example, manipulate game objects *at runtime*.

### Requirements

- MonoGame.WindowsDX 3.5+
- .NET Framework 4.6+

### Setup

Install the interpreter assembly through NuGet (this will also bring in the console if it hasn't been installed already):

```powershell
PM> Install-Package QuakeConsole.RoslynInterpreter.MonoGame.WindowsDX -Pre
```

1) Create the interpreter and set it as the interpreter for the console:

```cs
var interpreter = new RoslynInterpreter();
console.Interpreter = Interpreter;
```

2) To be able to modify game objects through the console, the objects must be added as variables to the C# scripting context:

```cs
interpreter.AddVariable("name", myVariable);
```

The object's public members can now be accessed from the console using the passed variable's name.

> Due to [an issue at Roslyn side](https://github.com/dotnet/roslyn/issues/3194), global variables *must be accessed through a 'globals' wrapper object*: `globals.myVariable`

> RoslynInterpreter does not provide any autocompletion features.


# Assemblies

- **QuakeConsole**: The core project for the console. Contains the behavior associated with handling user input and the visual side of the console's window.

## Interpreters

- **QuakeConsole.PythonInterpreter**: IronPython interpreter for the console shell. Allows manipulating game objects using Python scripting language. Provides autocompletion for loaded .NET types.
- **QuakeConsole.PythonInterpreter.Tests**: Unit tests covering the expected execution and autocompletion behavior for Python interpreter.
- **QuakeConsole.ManualInterpreter**: Interpreter for manually defined commands. Provides autocompletion for command names.
- **QuakeConsole.RoslynInterpreter**: Interpreter using the [Roslyn scripting API](https://github.com/dotnet/roslyn/wiki/Scripting-API-Samples) to execute console input as C# script.

## Samples

- **Sandbox**: Simple game which sets up the console and allows to manipulate a cube object using either Python, manual or Roslyn interpreter.
