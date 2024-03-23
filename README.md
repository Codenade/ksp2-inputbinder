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

Inputbinder aims to allow users to bind their input devices to the various game actions. It comes with a UI accessible through the game's in-flight app bar or through clicking on the input settings in the pause menu:

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

### Prerequisites

* [.NET SDK](https://dotnet.microsoft.com/en-us/download)
* [Unity 2022.3.5](https://unity.com/releases/editor/archive)

### Instructions

* Clone this repository to a location of your choice

* Run `dotnet tool restore`

* Run `dotnet cake`

#### build script custom arguments

You can see Cake's built-in options by typing `dotnet cake --help`

`dotnet cake [--target {Clean|Build|Pack|Install|Uninstall|Start}] [--configuration {Release|Debug}] [--ksp2-root <path>]`
  
  | Option                                                   | Description |
  |----------------------------------------------------------|-------------|
  | --target {Clean\|Build\|Pack\|Install\|Uninstall\|Start}      | Select a build target. The default if not specified is Pack.<br>If any of Install, Uninstall or Start are used the path to your installation of KSP2 must be specified by either: setting an Environment variable called KSP2_PATH or using the argument `--ksp2-root <path>` |
  | --configuration {Release\|Debug} | Select the build configuration. The default is Release.<br>When the Debug configuration is used a symbols file will be included in the build directory for ease of debugging. |
  | --ksp2-root \<path\>                                            | Used to specify where KSP2 is installed on your computer.<br>Alternatively you can specify the path by setting a new the Environment Variable: `KSP2_PATH` and assigning it the path to your game installation. |

## Main features

* Directly modifies the game's bindings using the [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/index.html)
* Add [processors](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/Processors.html) to your bindings
* Save your bindings as different profiles
* Makes gamepads usable, should also work with other input devices if they are supported by the [InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/SupportedDevices.html)

### Custom actions created by Inputbinder

#### Throttle
* Throttle Axis: Control your throttle as an axis

> [!NOTE]
> __Throttle Axis Behavior:__
> - When the throttle level axis is moved, it is marked as _active_.
> - When the throttle delta buttons (default `Shift` and `Ctrl`) are pressed, the throttle level axis is marked as _inactive_.
> - While held, the throttle max and cutoff buttons (default `Z`/`X`) take priority over the throttle level axis.
> - When the throttle max and cutoff buttons are released, the throttle level is set back to the axis level if it is active.

#### Trim
Dedicated actions to change the trim
* Pitch Trim
* Roll Trim
* Yaw Trim
* Trim Reset

#### SAS Mode (AP Mode)
* Set AP Mode Stability Assist
* Set AP Mode Prograde
* Set AP Mode Retrograde
* Set AP Mode Normal
* Set AP Mode Antinormal
* Set AP Mode Radial In
* Set AP Mode Radial Out
* Set AP Mode Target
* Set AP Mode Anti Target
* Set AP Mode Maneuver
* Set AP Mode Navigation
* Set AP Mode Autopilot

## Configuration

The mod settings can be accessed in-game in the flight view through the app-bar or anywhere from the settings menu.

![AppBar](./resources/inputbinder-app-bar.png)

There also is a configuration file for advanced settings: `Kerbal Space Program 2/BepInEx/config/inputbinder/inputbinder.cfg`

### Where are my bindings stored?

The bindings and processors are stored in the folder `Kerbal Space Program 2/BepInEx/config/inputbinder/profiles` inside `.json` files. Every file holds a set of bindings.  
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
