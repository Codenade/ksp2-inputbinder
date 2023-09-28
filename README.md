# ksp2-inputbinder

ksp2-inputbinder is a mod for [Kerbal Space Program 2](https://en.wikipedia.org/wiki/Kerbal_Space_Program_2).

### Jump to section

* [How to install](#how-to-install)
* [How to build](#how-to-build)
* [Bug Reports and Feature Requests](#bug-reports-and-feature-requests)
* [Configuration](#configuration)
* [More info](#more-info)

This mod makes it possible to use gamepads for controlling your vessel. Additionally, it brings UI for configuring input actions, which lets the user make more detailed changes.

![Settings Panel](https://github.com/Codenade/Codenade/blob/main/ksp2-inputbinder-panel.png?raw=true)

## How to install

* Option 1: Use [CKAN](https://github.com/KSP-CKAN/CKAN)
* Option 2: Follow instructions below to install manually

### Prerequisites

* Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html), skip if already installed

### Instructions

* Unpack the contents of the downloaded `.zip` file into your KSP 2 installation (eg. `C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2`)

## How to build

Currently only windows is supported, if you have suggestions please open an issue.  
Command arguments enclosed in `[` `]` are optional.

### Prerequisites

* [Python 3.5 or newer](https://www.python.org/downloads/)
* [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)
* [Unity 2020.3.33](https://unity.com/releases/editor/archive)
* [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) installed to your installation of KSP 2

### Instructions

* Clone this repository to a location of your liking

* Add a new environment variable named `KSP2_PATH` with the value set to the path to your installation of KSP 2 (eg. `C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2`)

* Run `build.bat [--unity_executable "path to your Unity.exe"] [--install] [--start]`

## Bug Reports and Feature Requests

If you find any bugs or have good ideas for new features [open an issue on github](https://github.com/Codenade/ksp2-inputbinder/issues/new).

## Configuration

The mod settings can be accessed in-game in the flight view.

![AppBar](https://github.com/Codenade/Codenade/blob/05bb56f4755e55ecd7953ca2ef4baf3d16695c7f/ksp2-inputbinder-appbar.png?raw=true)

### Custom actions

#### Throttle
* Throttle Axis: Control your throttle as an axis

#### Trim
Dedicated actions to change the trim
* Pitch Trim
* Roll Trim
* Yaw Trim
* Trim Reset

### Included game actions

* Throttle Delta
* Throttle Cutoff
* Throttle Max
* Pitch
* Roll
* Yaw
* Toggle Precision Mode
* Wheel Steer
* Wheel Brakes
* Wheel Throttle
* Stage
* Toggle Landing Gear
* Toggle Lights
* Toggle SAS
* Toggle RCS
* Translate X
* Translate Y
* Translate Z
* Trigger Action Group 1
* Trigger Action Group 2
* Trigger Action Group 3
* Trigger Action Group 4
* Trigger Action Group 5
* Trigger Action Group 6
* Trigger Action Group 7
* Trigger Action Group 8
* Trigger Action Group 9
* Trigger Action Group 10
* Camera Pitch Gamepad
* Camera Yaw Gamepad
* Show Map
* Quick Save
* Time Warp Decrease
* Time Warp Increase
* Time Warp Stop
* Toggle Pause Menu
* Toggle UI Visibility
* Camera Zoom

### Reset the configuration

To reset the configuration delete `input.json` from `Kerbal Space Program/BepInEx/plugins/inputbinder` and restart the game.

## More info

For more information see: https://github.com/Codenade/ksp2-inputbinder/wiki
