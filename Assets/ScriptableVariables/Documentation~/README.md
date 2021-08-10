# Generic Scriptable Variables
[![Unity 2020.1+](https://img.shields.io/badge/Unity-2020.1%2B-blue.svg)](https://unity3d.com/get-unity/download) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://tldrlegal.com/license/mit-license) [![openupm](https://img.shields.io/npm/v/com.jreason.scriptablevariables?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.jreason.scriptablevariables/)

Scriptable Variables is package to implement variables in Unity using Scriptable Objects, taking advantage of Unity 2020.1 ability to serialize generic fields.    


## Warning
This package is considered to be a preview-package. This means:
* Core features may be incomplete.
* Functionality has not yet been fully tested and definitely contains bugs.
* Documentation is incomplete.
* Code may be uncommented and not following standards


## System Requirements
Unity 2020.1.0 or later versions.

## Installation  
The package is available on the [openupm registry](https://openupm.com) at `com.jreason.scriptablevariables`.  
More information can be found at the [Installation page](Installation.md).

## Usage
There are two main classes in the package: [`Variable<>`](Variables.md) and [`Reference<>`](References.md) 

### Variable
Variables are assets created in the Unity Editor which store data.
A new Variable Asset can be created from the '*Assets/Create/Variable*' menu or from the '**+**' Dropdown in the ProjectView.
A Variable's type can be changed in the '*Type*' Dropdown.   

**Variable Types**  
Before being able to create a Variable Asset of a certain Type, that type needs to be created as a Class.
The nessary code to create a Variable type can generate from the '**Type**' Dropdown by clicking '**Create new**', this only needs to be done once per type.
Some Types are pre-setup such as [primitives](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types) and some more common Unity classes.

### Reference
References are how variables are accessed in Code.
(e.g. Use `Reference<float>` to reference a `Float` Variable.)
```cs
    //Reference to Vector3 Variable
    public Reference<Vector3> m_direction;

    private void Update()
    {
        //How to set the value of the variable
        m_direction.Value = transform.forward;

        //Can implicitly cast when getting the value of the variable
        transform.position += m_direction;

        //Sometimes you'll need to do this though
        transform.position += m_direction.Value * Time.deltaTime;
        transform.position += (Vector3)m_direction * Time.deltaTime;  
    }
```


More information can be found in the [Variable page]() and [Reference page]()


## Credits
* The theory for this package is based off [Ryan Hipple's Unite Talk](https://www.youtube.com/watch?v=raQ3iHhE_Kk "Youtube").  
* The code is based off [Wolar-Games Implementation](https://github.com/Wolar-Games/unity-scriptable-object-variables "Github") and has been re-written for Unity 2020.1.