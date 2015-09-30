![Quake Console](https://jvcontent.blob.core.windows.net/images/quake_console_logo_h64.png)
===============

## What is it?

Quake-style console is an in-game command-line interface with swappable command interpreters. For example, using a Python interpreter allows for easy game object manipulation at runtime using Python syntax.

## Getting Started

> Currently available only for MonoGame WindowsDX platform!

-------

Install the Python interpreter through Nuget (this will also bring in the console):

`Install-Package QuakeConsole.PythonInterpreter.MonoGame.WindowsDX -Pre`

Or to only install the console shell:

`Install-Package QuakeConsole.MonoGame.WindowsDX -Pre`

-----

In the game constructor, create the console and add to components:

    console = new ConsoleComponent(this);
    Components.Add(console);

Make sure to load the content for the console in the LoadContent method:

`console.LoadContent(font);`

In the update method, open the console (on key press, for example) by using ToggleOpenClose:

`console.ToggleOpenClose();`

## Assemblies



- **QuakeConsole**: The core project for the console. Includes all the behavior associated with handling user input and the look of the console's window.

### Interpreters

- **QuakeConsole.PythonInterpreter**: IronPython interpreter for the console shell. Allows manipulating game objects using Python scripting language. Provides autocompletion for loaded .NET types.
- **QuakeConsole.PythonInterpreter.Tests**: Unit tests covering the expected execution and autocompletion behavior for Python interpreter.
- **QuakeConsole.ManualInterpreter**: Interpreter for manually defined commands. Provides autocompletion.

### Samples

- **Sandbox**: Simple game which sets up the console and allows to manipulate a cube object using either Python or manual interpreter.
