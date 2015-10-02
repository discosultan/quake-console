## What is this?

Quake-style console is an in-game command-line interface with swappable command interpreters. For example, using Python interpreter allows for easy game object manipulation *at runtime* using Python syntax.

![Screenshot](http://az695587.vo.msecnd.net/images/console_merged.png)

- https://www.youtube.com/watch?v=Is2m2oQ68Gc
- https://www.youtube.com/watch?v=oVWqy16W0ak

## Getting Started

> Currently available only for MonoGame WindowsDX platform targeting .NET 4.0!

### Basic setup

Install the console shell through NuGet:

```powershell
Install-Package QuakeConsole.MonoGame.WindowsDX -Pre
```

In the game constructor, create the console and add to components:
```cs
console = new ConsoleComponent(this);
Components.Add(console);
```
Make sure to load the graphics resources for the console in the LoadContent method (requires a `SpriteFont` to render its output):

```cs
console.LoadContent(font);
```

In the update method, open the console (on key press, for example) by calling ToggleOpenClose:

```cs
console.ToggleOpenClose();
```

To know when to prevent input handling in other game systems while the console is open, the component exposes a property `console.IsAcceptingInput`.

### Python interpreter setup

Python interpreter can be used to interpret user input as Python code. It is extremely useful to, for example, modify game object properties *at runtime*.

Install the interpreter through NuGet (this will also bring in the console if it hasn't been installed already):

```powershell
Install-Package QuakeConsole.PythonInterpreter.MonoGame.WindowsDX -Pre
```

Create the interpreter and set it as the interpreter for the console:

```cs
var interpreter = new PythonInterpreter();
console.Interpreter = Interpreter;
```

To be able to modify game objects through the console, the objects must be added as variables to the IronPython engine (this creates the connection between the CLR and Python objects):

```cs
interpreter.AddVariable("name", object);
```

The object's public members can now be accessed from the console using the passed variable's name (press ctrl + space [default] to autocomplete input to known variables/types/members).

### Manual interpreter setup

Manual interpreter can be used to define commands and actions for the console manually.

Install the interpreter through NuGet (this will also bring in the console if it hasn't been installed already):

```powershell
Install-Package QuakeConsole.ManualInterpreter.MonoGame.WindowsDX -Pre
```

Create the interpreter and set it as the interpreter for the console:

```cs
var interpreter = new ManualInterpreter();
console.Interpreter = Interpreter;
```

A command is essentially a delegate that is invoked when user inputs the name of the command. The delegate provides an array of arguments separated by spaces (similar to arguments in a Windows console application) and optionally can return a string value that is output to the console.

To register a command:

```cs
interpreter.RegisterCommand("name", action);
```

where action is of type `Action<string[]>` or `Func<string[], string>`.

Provides autocompletion for registered command names (ctrl + space [default]).

## Assemblies

- **QuakeConsole**: The core project for the console. Contains the behavior associated with handling user input and the visual side of the console's window.

### Interpreters

- **QuakeConsole.PythonInterpreter**: IronPython interpreter for the console shell. Allows manipulating game objects using Python scripting language. Provides autocompletion for loaded .NET types.
- **QuakeConsole.PythonInterpreter.Tests**: Unit tests covering the expected execution and autocompletion behavior for Python interpreter.
- **QuakeConsole.ManualInterpreter**: Interpreter for manually defined commands. Provides autocompletion for command names.

### Samples

- **Sandbox**: Simple game which sets up the console and allows to manipulate a cube object using either Python or manual interpreter.
