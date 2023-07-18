# ksp2-inputbinder

ksp2-inputbinder is a mod for [Kerbal Space Program 2](https://en.wikipedia.org/wiki/Kerbal_Space_Program_2).

### Jump to section

* [Configuration](#configuration)
* [How to install](#how-to-install)
* [How to build](#how-to-build)

This mod makes it possible to use gamepads for controlling your vessel. Additionally, it brings UI for configuring input actions, which lets the user make more detailed changes.

![Settings Panel](https://github.com/Codenade/Codenade/blob/main/ksp2-inputbinder-panel.png?raw=true)

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

### Reset the configuration

To reset the configuration delete `input.json` from `Kerbal Space Program/BepInEx/plugins/inputbinder`.

### Other

For more information see: https://github.com/Codenade/ksp2-inputbinder/wiki

## How to install

### Prerequisites

* [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

### Instructions

* unpack the contents of `build.zip` into your KSP 2 installation (eg. `C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2`)

## How to build

### Prerequisites

* [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)
* [Unity 2020.3.33](https://unity.com/releases/editor/archive)
* [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) installed to your installation of KSP 2

### Instructions

* clone this repository to a location of your liking

* initialize submodules `git submodule --init --recursive --remote`

* change the path to your unity installation (2020.3.33) after `echo Building assets` in `build.bat`

* add a new environment variable named `KSP2_PATH` with the value set to the path to your installation of KSP 2 (eg. `C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2`)

* run `build.bat`
