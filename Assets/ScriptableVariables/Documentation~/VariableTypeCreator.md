# Create Variable Type Tool

Before being able to create a Variable Asset of a certain Type, that type needs to be created as a Class. The nessary code to create a Variable type can generate from the '**Type**' Dropdown by clicking '**Create new**', this only needs to be done once per type.
[Editor](Assets/ScriptableVariables/Documentation%7E/Media/VariableTypeCreator.png?raw=true)

## Settings

**Filter:** Used to search the `ChangeType` dropdown to quickly find a C# Class.
**Type:** *[required]* The new Variable Type to create. e.g

**Menu Name:** How the Type will be organised in the Change-Type drop down.	(*'e.g. Default/Colour'*)
**Menu Order:** The ordering of the Type in the Change-Type drop down.
&nbsp;  - Lower Numbers will be displayed and the top.
&nbsp; - A gap of 10 or higher will insert a line break in the menu.

**Path:** Directory where .cs file holding the Variable Type will be saved in the project. This is local to the Assets folder.
**Namespace:** The [Namespace](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/types/namespaces) of the Variable Type. (There shouldn't be any need to change this away from `Variables.type`)


 ## Changing Menu Name 
 Currently there is no way to change `Menu Order` or `Menu Name` from the GUI.
 You can change both of these by editing the .cs file of the Variable.

```cs
using Variables;

namespace Variables.Types
{
[VariableMenu(menuName = "Example/ExampleClass", order = 100)]
public class ExampleClassVariable : Variable<ExampleClass> { }
}
```


