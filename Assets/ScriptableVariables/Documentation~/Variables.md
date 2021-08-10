# Variables
Variables are assets which store data and live in the [Project Window](https://docs.unity3d.com/Manual/ProjectView.html)



## Creation

**Asset Creation**
Variables are assets created in the Unity Editor. A new Variable asset can be created from the '_Assets/Create/Variable_' menu or from the '**+**' Dropdown in the ProjectView. A Variable's type can be changed in the '_Type_' Dropdown.
[Example](Assets/ScriptableVariables/Documentation%7E/Media/VariableCreation.gif?raw=true)

**Variable Types**  
Before being able to create a Variable Asset of a certain Type, that type needs to be created as a Class. The nessary code to create a Variable type can generate from the '**Type**' Dropdown by clicking '**Create new**', this only needs to be done once per type. Some Types are pre-setup such as [primitives](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types) and some more common Unity classes.
More details about the *Create Variable Type* window can be found [here](https://github.com/j-reason/ScriptableVariables/blob/main/Assets/ScriptableVariables/Documentation%7E/VariableTypeCreator.md)

[Example](Assets/ScriptableVariables/Documentation%7E/Media/VariableTypeCreation.gif?raw=true)


## API
**Warning!** You probably want to be using [`Reference<T>`](https://github.com/j-reason/ScriptableVariables/blob/main/Assets/ScriptableVariables/Documentation%7E/References.md) in your code instead of `Variable<T>.`

**Decleration**
`public Variable<T> ExampleVariable` where `T` is the Type of Variable.
`public Variable<float> FloatVariable` *(example)*

**Parameters**

| Type          | Name                 | Description|
|--------------|-----------------------|--------------------------------------------------------------------------------------------------|
| `T`          | *Value*               | The instanced Value held by the Variable. It is reset OnPlay(Just like objects in the Scene)     |
| `Action<T>`  | *OnValueChanged*      | An event which is called everytime `Value` is changed                                            |
| `bool`       | *allowValueRepeating* | Should `OnValueCanged` be called when Value is set but doesn't change                            |

  

**Functions**
**`void SetValue (T value)`**   
   Sets the instance value of the Variable. Can also be set through `Variable.Value`.
   
**`void SetValue (Variable<T> variable)`**   
   Sets the instance value of the Variable to the instance value of an other Variable
   
**`T GetValue ()`**     
Gets the instance value of the Variable. Can also be accessed through `Variable.Value`.

## Example

#TODO add Example code