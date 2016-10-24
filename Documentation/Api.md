# MonoGame.QuakeConsole

<table>
<tbody>
<tr>
<td><a href="#consoleaction">ConsoleAction</a></td>
<td><a href="#consoleactionmap">ConsoleActionMap</a></td>
</tr>
<tr>
<td><a href="#consoleclearflags">ConsoleClearFlags</a></td>
<td><a href="#consolecomponent">ConsoleComponent</a></td>
</tr>
<tr>
<td><a href="#consoleoutput">ConsoleOutput</a></td>
<td><a href="#icommandinterpreter">ICommandInterpreter</a></td>
</tr>
<tr>
<td><a href="#iconsoleinput">IConsoleInput</a></td>
<td><a href="#iconsoleoutput">IConsoleOutput</a></td>
</tr>
<tr>
<td><a href="#stubcommandinterpreter">StubCommandInterpreter</a></td>
<td><a href="#symbol">Symbol</a></td>
</tr>
</tbody>
</table>


## ConsoleAction

An action or modifier (other than symbol input) available in the console.


## ConsoleActionMap

A map specifying which input keys are translated to which <a href="#consoleaction">ConsoleAction</a>s.

#### Remarks

<a href="#system.collections.ienumerable">System.Collections.IEnumerable</a> is implemented only to allow collection initializer syntax. Iteration will fail!

### Add(Microsoft.Xna.Framework.Input.Keys,Microsoft.Xna.Framework.Input.Keys,Microsoft.Xna.Framework.Input.Keys,QuakeConsole.ConsoleAction)

Adds a mapping from keyboard keys to <a href="#consoleaction">ConsoleAction</a>

### Add(Microsoft.Xna.Framework.Input.Keys,Microsoft.Xna.Framework.Input.Keys,QuakeConsole.ConsoleAction)

Adds a mapping from keyboard keys to <a href="#consoleaction">ConsoleAction</a>

### Add(Microsoft.Xna.Framework.Input.Keys,QuakeConsole.ConsoleAction)

Adds a mapping from keyboard key to <a href="#consoleaction">ConsoleAction</a>

### Remove(Microsoft.Xna.Framework.Input.Keys,Microsoft.Xna.Framework.Input.Keys,Microsoft.Xna.Framework.Input.Keys)

Removes a mapping from keyboard keys to <a href="#consoleaction">ConsoleAction</a>

### Remove(Microsoft.Xna.Framework.Input.Keys,Microsoft.Xna.Framework.Input.Keys)

Removes a mapping from keyboard keys to <a href="#consoleaction">ConsoleAction</a>

### Remove(Microsoft.Xna.Framework.Input.Keys)

Removes a mapping from keyboard key to <a href="#consoleaction">ConsoleAction</a>

### Remove(QuakeConsole.ConsoleAction)

Removes all mappings to <a href="#consoleaction">ConsoleAction</a>


## ConsoleClearFlags

Defines which subparts of the <a href="#console">Console</a> to clear.

### All

Clears everything.

### InputBuffer

Clears the text in the input part of the console and resets Caret position.

### InputHistory

Removes any history of user input.

### None

Does not clear anything.

### OutputBuffer

Clears the text in the output part of the console.


## ConsoleComponent

Command-line interface with swappable command interpreters.

### Constructor(Microsoft.Xna.Framework.Game)

### ActionMappings

Gets the dictionary that is used to map keyboard keys to corresponding console actions (such as ExecuteCommand or ToggleUppercase).

### BackgroundColor

Gets or sets the background color. Supports transparency.

### BackgroundTexture

Gets or sets the texture used to render as the console background. Set this to NULL to disable textured background.

### BackgroundTextureTransform

Gets or sets the transformation applied to texture coordinates if background texture is set.

### BottomBorderColor

Gets or sets the color of the border at the bottom of the console window. Supports transparency.

### BottomBorderThickness

Gets or sets the thickness of the border at the bottom of the console window in pixels. To disable border, set this value less than or equal to zero.

### CaretBlinkingInterval

Gets or sets the time in seconds to toggle caret's visibility.

### CaretSymbol

Gets or sets the symbol which is used as the caret. This symbol is used to indicate where the user input will be appended.

### CharacterMappings

Gets or sets the dictionary that is used to map keyboard keys to corresponding symbols shown in the <a href="#console">Console</a>.

### Clear(clearFlags)

Clears the subparts of the <a href="#console">Console</a>.

| Name | Description |
| ---- | ----------- |
| clearFlags | *QuakeConsole.ConsoleClearFlags*<br>Specifies which subparts to clear. |

### Dispose(System.Boolean)

### Draw(Microsoft.Xna.Framework.GameTime)

### Font

Gets or sets the font used to render console text.

### FontColor

Gets or sets the font color. Supports transparency.

### HeightRatio

Gets or sets the percentage of height the <a href="#console">Console</a> window takes in relation to application window height. Value in between [0...1].

### Initialize

### Input

Gets the input writer of the console.

### InputPrefix

Gets or sets the textual symbol(s) that is shown in the beginning of console input line.

### InputPrefixColor

Gets or sets the color for the input prefix symbol. See InputPrefix for more information.

### Interpreter

Gets or sets the command interpreter. This defines how user input commands are evaluated and operated on. Optionally provides autocompletion. Pass NULL to use a stub interpreter instead (useful for testing just the shell).

### IsAcceptingInput

Gets if the console is currently reading keyboard input.

### IsVisible

Gets if any part of the <a href="#console">Console</a> is visible.

### LogInput

Gets or sets the input command logging delegate. Set this property to log the user input commands to the given delegate. For example WriteLine(String).

### NumSymbolsToMoveWhenCaretOutOfView

Gets or sets the number of symbols that will be brought into console input view once the user moves the caret out of the visible area.

### Output

Gets the output writer of the console.

### Padding

Gets or sets the padding to apply to the borders of the <a href="#console">Console</a> window. Note that padding will be automatically decreased if the available window area becomes too low.

### RemoveOverflownEntries

Gets or sets if rows which run out of the visible area of the console output window should be removed.

### Reset

Clears the <a href="#console">Console</a> and sets all the settings to their default values.

### SelectionColor

Gets or sets the color used to draw the background of the selected portion of user input.

### TabSymbol

Gets or sets the symbol used to represent a tab.

#### Remarks

By default, four spaces are used to simulate a tab since a lot of <a href="#microsoft.xna.framework.graphics.spritefont">Microsoft.Xna.Framework.Graphics.SpriteFont</a>s don't support the \t char.

### TimeToCooldownRepeatingInput

Gets or sets the time in seconds it takes before another character is appended when repeating input is enabled. See TimeToTriggerRepeatingInput for more information.

### TimeToToggleOpenClose

Gets or sets the time in seconds it takes to fully open or close the <a href="#console">Console</a>.

### TimeToTriggerRepeatingInput

Gets or sets the time in seconds it takes to trigger repeating input feature. Repeating input is triggered when a key is held down. When repeating input is triggered, keys which are held down will be repeatedly processed or appended to the console (normally they are processed or appended only upon key press).

### ToggleOpenClose

Opens the console windows if it is closed. Closes it if it is opened.

### UnloadContent

### Update(Microsoft.Xna.Framework.GameTime)


## ConsoleOutput

Output part of the <a href="#consoleoutput.console">ConsoleOutput.Console</a>. Command execution info and results will be appended here.

### Append(message)

Appends a message to the buffer.

| Name | Description |
| ---- | ----------- |
| message | *System.String*<br>Message to append. |

### Clear

Clears all the information in the buffer.


## ICommandInterpreter

A contract for a <a href="#console">Console</a> command interpreter. Manages command execution and autocompletion features.

#### Remarks

Used, for example, to autocomplete user input.

### Autocomplete(input, forward)

Tries to autocomplete the current user input in the <a href="#consoleinput">ConsoleInput</a>.

| Name | Description |
| ---- | ----------- |
| input | *QuakeConsole.IConsoleInput*<br>Buffer to read from and autocomplete user input. |
| forward | *System.Boolean*<br>Indicator which indicates whether we should move forward or backward with the autocomplete entries. |

### Execute(output, command)

Executes a <a href="#console">Console</a> command.

| Name | Description |
| ---- | ----------- |
| output | *QuakeConsole.IConsoleOutput*<br>Buffer to append data which is shown to the user. |
| command | *System.String*<br>Command to execute. |


## IConsoleInput

A contract for the input part of the <a href="#console">Console</a>. Defines properties and methods required to manipulate the input.

### Append(value)

Writes to the <a href="#consoleinput">ConsoleInput</a>.

| Name | Description |
| ---- | ----------- |
| value | *System.String*<br>Message to append. |

### CaretIndex

Gets or sets the location of the caret. This is where user input will be appended.

### Clear

Clears the input from the buffer.

### Indexer(i)

Gets the symbol at the specified index.

| Name | Description |
| ---- | ----------- |
| i | *System.Int32*<br>Index to take symbol from. |

#### Returns

Indexed symbol.

### LastAutocompleteEntry

Gets or sets the last autocomplete entry which was added to the buffer. Note that this value will be set to null whenever anything from the normal <a href="#console">Console</a> input pipeline gets appended here.

### Length

Gets the number of characters currently in the buffer.

### Remove(startIndex, length)

Removes symbols from the <a href="#consoleinput">ConsoleInput</a>.

| Name | Description |
| ---- | ----------- |
| startIndex | *System.Int32*<br>Index from which to start removing. |
| length | *System.Int32*<br>Number of symbols to remove. |

### Substring(startIndex, length)

Gets a substring of the buffer.

| Name | Description |
| ---- | ----------- |
| startIndex | *System.Int32*<br>Index ta take substring from. |
| length | *System.Int32*<br>Number of symbols to include in the substring. |

#### Returns

Substring of the buffer.

### Substring(startIndex)

Gets a substring of the buffer.

| Name | Description |
| ---- | ----------- |
| startIndex | *System.Int32*<br>Index ta take all the following symbols from. |

#### Returns

Substring of the buffer.

### ToString

### Value

Gets or sets the value typed into the buffer.


## IConsoleOutput

A contract for the output part of the <a href="#console">Console</a>. Defines methods manipulating the output.

#### Remarks

Used, for example, to clear the output window or append results from outside the console.

### Append(message)

Appends a message to the buffer.

| Name | Description |
| ---- | ----------- |
| message | *System.String*<br>Message to append. |

### Clear

Clears all the information in the buffer.


## StubCommandInterpreter

Provides a stub command interpreter which does nothing.

### Autocomplete(input, forward)

Does nothing.

| Name | Description |
| ---- | ----------- |
| input | *QuakeConsole.IConsoleInput*<br>Console input. |
| forward | *System.Boolean*<br>True if user wants to autocomplete to the next value; false if to the previous value. |

### Execute(output, command)

Does nothing.

| Name | Description |
| ---- | ----------- |
| output | *QuakeConsole.IConsoleOutput*<br>Console output buffer to append any output messages. |
| command | *System.String*<br>Command to execute. |


## Symbol

Represents a pair of lowercase and uppercase symbols.

### Constructor(lowercase, uppercase)

Initializes a new instance of <a href="#symbol">Symbol</a>.

| Name | Description |
| ---- | ----------- |
| lowercase | *System.String*<br>Lowercase symbol of the pair. |
| uppercase | *System.String*<br>Uppercase symbol of the pair. |

### Lowercase

Gets the lowercase symbol.

### Uppercase

Gets the uppercase symbol.
