# Inputbinder

### Jump to section

* [Description](#description)
* [How to install](#how-to-install)
* [How to build](#how-to-build)
* [Main features](#main-features)
* [Configuration](#configuration)
  * [Where are my bindings stored?](#where-are-my-bindings-stored)
  * [Reset the configuration](#reset-the-configuration)
  * [Slider settings](#slider-settings)
  * [Adding more actions from the game to Inputbinder](#adding-more-actions-from-the-game-to-inputbinder)
  * [Make Inputbinder automatically add processors](#make-inputbinder-automatically-add-processors)
* [Bug Reports and Feature Requests](#bug-reports-and-feature-requests)
* [More info](#more-info)

## Description

Inputbinder aims to allow users to bind their input devices to the various game actions. It comes with a UI accessible through the game's in-flight app bar:

__Inputbinder in the App-Bar menu__  
![AppBar](./resources/inputbinder-app-bar.png)

__Inputbinder App__  
![App](./resources/inputbinder-app.png)

## How to install

* Option 1: Use [CKAN](https://github.com/KSP-CKAN/CKAN) to install _(recommended)_
* Option 2: Follow instructions below to install manually

### Prerequisites

* Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html), skip if already installed

### Instructions

* Unpack the contents of the downloaded `.zip` file into your KSP 2 installation (eg. `C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2`)

## How to build

Currently only Windows is supported.

### Prerequisites

* [Python 3.5 or newer](https://www.python.org/downloads/)
* [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)
* [Unity 2020.3.33](https://unity.com/releases/editor/archive)
* [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) installed to KSP 2

### Instructions

* Clone this repository to a location of your choice

* Add a new environment variable named `KSP2_PATH` with the value set to the path to your installation of KSP 2 (eg. `C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2`)

* Run `build.bat`

#### build.bat usage

`build.py [-h] [-e UNITY_EXECUTABLE] [-i] [-s] [-d] [-n] [--skip-assembly-build] [--skip-assets]`
  
  | Option                                                   | Description |
  |----------------------------------------------------------|-------------|
  | -h, --help                                               | show this help message and exit |
  | -e UNITY_EXECUTABLE, --unity-executable UNITY_EXECUTABLE |If your unity installation is not located at `C:/Program  Files/Unity/Hub/Editor/2020.3.33f1/Editor/Unity.exe` use this option|
  |-i, --install                                             |Install this mod|
  |-s, --start                                               |Install and start this mod, --install is redundant when using this option|
  |-d, --debug                                               |Produces a debug build with full debug information|
  |-n, --no-archive                                          |Do not create an archive file when completed|
  |--skip-assembly-build                                     |Skips the assembly build|
  |--skip-assets                                             |Skips the addressables build|

## Main features

* Directly modifies the game's bindings using the [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/index.html)
* Add [processors](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/Processors.html) to your bindings
* Save your bindings as different profiles
* Makes gamepads usable, should also work with other input devices if they are supported by the [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/SupportedDevices.html)

### Custom actions created by Inputbinder

#### Throttle
* Throttle Axis: Control your throttle as an axis

#### Trim
Dedicated actions to change the trim
* Pitch Trim
* Roll Trim
* Yaw Trim
* Trim Reset

## Configuration

The mod settings can be accessed in-game in the flight view.

![AppBar](./resources/inputbinder-app-bar.png)

There also is a configuration file for advanced settings: `Kerbal Space Program 2/BepInEx/config/inputbinder/inputbinder.cfg`

### Where are my bindings stored?

The bindings and processors are stored in the folder `Kerbal Space Program 2/BepInEx/config/inputbinder/profiles` inside `.json` files. Every file holds a complete set of all bindings.  
__These files are referenced as "input profiles" by this mod's documentation.__

### Reset the configuration

To reset all bindings click the "Reset All" button in the app's title bar.

### Slider settings

To change the range of the sliders inside the processor editing menus, open the file `Kerbal Space Program 2/BepInEx/config/inputbinder/inputbinder.cfg` inside a text editor.  
Now change the values after `SliderMin=` and `SliderMax=` to suit your needs.  
Then save the file and load/reload any input profile.

### Adding more actions from the game to Inputbinder

It is possible to change the bindings for other game actions too.

* Create this file: `Kerbal Space Program 2/BepInEx/config/inputbinder/game_actions_to_add.txt`

* Follow [this page](https://github.com/Codenade/ksp2-inputbinder/wiki/Configuration#game_actions_to_addtxt) to add what you want, it is also possible to specify how the InputAction should be set up.

* Save the file and load/reload any input profile

### Make Inputbinder automatically add processors

Open `Kerbal Space Program 2/BepInEx/config/inputbinder/inputbinder.cfg` in a text editor  
_(if you didn't delete it yet there should be a default template commented out by the leading `#` for you to use and modify)_

After the default config values add a section: `[auto-add-processors]`

After this section heading you can configure the automatic adding of processors in the following format:

`When<controlA>MappedTo<controlB>="<processors>"`

The `<control1>` `<control2>` and `<processors>` placeholders should be replaced by the following:

`<controlA>`: The newly bound control must match this control-type, one of: `Axis`, `Button`, `Key`, `Vector2`, `Delta`, `Stick`, `Dpad`, `*`, `Any`
`<controlB>`: The binding's expected control-type to match, on of: `Axis`, `Button`, `Key`, `Vector2`, `Delta`, `Stick`, `Dpad`, `*`, `Any`

The control types `*` and `Any` match any control

`<processors>`: The processors to add as they are displayed on the main page of the mod's UI.  
The enclosing quotation marks are required.

If you want to find out what the control types of bindings of controls are you can rebind them and open the game's log `Kerbal Space Program 2/Ksp2.log` and look out for:  

`Binding complete: <action name> <binding name> with path <bound path>; bound control of type <controlA> to binding of type <controlB>`

Your finished configuration file should look something like this:

```ini
[main]
SliderMin=-2
SliderMax=2

[auto-add-processors]
WhenButtonMappedToAxis="Scale(factor=0.5)"
WhenAxisMappedToAxis="Scale(factor=0.25);Invert()"
```

## Bug Reports and Feature Requests

Found any bugs🦗? Have an idea to improve things💡? → [Open an issue on GitHub](https://github.com/Codenade/ksp2-inputbinder/issues).

## More info

For more information see: https://github.com/Codenade/ksp2-inputbinder/wiki
