# Installation
There are multiple methods to install this package.
The preferred method is using a Scoped Registery since it allows you to easily keep the package update or select a previous Release Version.

## Scoped Registery
The package is available on the [openupm registry](https://openupm.com/packages/com.jreason.scriptablevariables/), you can add it to the project as a [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html).

**Adding the Scoped Registery**  
Before adding this package using the Package Manager you need to add OpenUPM to your Project Registery.

 - Open the Package Manager Settings `Edit > Project Settings > PackageManager`
 - Add a Scoped Registery
	 - Name: `OpenUPM` (The name can be anything)
	 - URL: `https://package.openupm.com`
	 - Scope: `com.jreason.scriptablevariables`
 - Click 'Save'
(If you are already using other packages on OpenUPM you can just add the Scope to the Registery already setup.)

**Add the Package**  
 - Open the [Package Manager](https://docs.unity3d.com/Manual/upm-ui.html) `Window > Package Manager`
 - Set the the packages [drop-down menu](https://docs.unity3d.com/Manual/upm-ui-filter.html) to 'My Registries'
 - Install `Generic Scriptable Variables`

## Git
You can use the following path to add the latest release from GIT using the Package Manager.
`https://github.com/j-reason/ScriptableVariables.git?path=/Assets/ScriptableVariables`

Follow the [Unity Documentation](https://docs.unity3d.com/Manual/upm-ui-giturl.html) for more information.
Information on how To declare a specific revision can be found [here.](https://docs.unity3d.com/Manual/upm-git.html#revision)

## File
Alternatively you find [all releases](https://github.com/j-reason/ScriptableVariables/releases) as a .unitypackage on GitHub.