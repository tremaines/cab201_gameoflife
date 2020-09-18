---
title: Game of Life
author: Tremaine Stroebel - n10689877
date: 13/09/2020
---

## Build Instructions

1. Unzip the Project folder with your unzipper of choice.
2. Navigate into the "Life" folder.
3. Double click the "Life.sln" file to open in Visual Studio.
4. Once open, ensure the "Solution Configuration" (located below "Debug" and to the right of the undo and redo buttons) is set to release mode.
5. Click on "Build" and then click "Build Solution". Alternatively, you can use the shortcut "ctrl + shift + B".

## Usage 

##### To Start a Game
1. To run the Game of Life, you will first need to open up Windows command prompt.
2. Type: `cd "C:\..\..\CAB201_2020S2_ProjectPartA_n10689877\Life\Life\bin\Release\netcoreapp3.1"`. You will need to replace `C:\..\..\` with your path to the CAB201_2020S2_ProjectPartA_n10689877 folder.
3. Type: `dotnet Life.dll`
4. Press `Enter` to start the game.

##### Optional Arguments
If you start a game without entering any arguments, it will proceed with the default settings. However, if you'd prefer, you can alter settings with the following flags:
|Flag/Option  |Function                                                                                       |Parameters                                                   |Example             |
|-------------|-----------------------------------------------------------------------------------------------|-------------------------------------------------------------|--------------------|
|--dimensions |Sets the number of rows and columns respectively.                                              |Exactly two whole numbers, between 4 & 48 inclusive.         |`--dimensions 16 16`|
|--periodic   |Enables periodic behaviour, i.e. neighbours wrap around the boundaries.                        |N/A                                                          |`--periodic`        |
|--random     |Sets the chance of a cell being set to alive during setup.                                     |Exactly one floating point number between 0 and 1 inclusive. |`--random 0.5`      |
|--seed       |Choose a seed file to set the initial state of the cells.                                      |A valid seed file (or path to seed file). Must end in '.seed'|`--seed glider.seed`|
|--generations|Sets the total number of generations the game will run for.                                    |A single positive, non-zero whole number.                    |`--generations 50`  |
|--max-update |Sets the number of generational updates per second.                                            |A single floating point number between 1 and 30 inclusive.   |`--max-update 5`    |
|--step       |Enables step mode, allowing you to step through each generation sequentially by pressing space.|N/A                                                          |`--step`            |

An example of an argument with valid flags and parameters:
`dotnet Life.dll --generations 30 --dimensions 32 40 --step`

Please note, the order of the arguments does not matter.

## Notes 

* The number of parameters following options that require them is strict. Too many or too few will result in the parameters being ignored and the program reverting to defaults.
    * For example, `--dimensions 30 23 42` contains too many parameters and the game will rever to the default number of rows and columns.
* For arguments that take more than one parameter (just --dimensions at this stage), an invalid parameter will cause both parameters to revert to the default.
  * For example, `--dimensions 30 54`, contains a parameter outside the 4 - 48 range, causing BOTH parameters to revert to the default
* When a valid seed file is called:
  * If any cells in the seed file are outside of the bounds of the grid, the game will recommend a minimum grid size and continue with the cells it can generate
  * The random factor will be ignored (as the initial state is determine by the seed file)
* If step mode is enabled:
  * The update rate will be ignored and the game will progress at 1 generation per press of the spacebar.
* For parameterless flags (--step and --periodic):
  * If these flags are called their respective modes will be enabled. Any parameters following them will be ignored by the program (and the user will be told as much).