---
title: Game of Life
author: Tremaine
date: 25/10/20
---

## Build Instructions

1. Unzip the folder with your unzipper of choice.
2. Navigate into the "Life" folder.
3. Double click the "Life.sln" file to open in Visual Studio.
4. Once open, ensure the "Solution Configuration" (located below "Debug" and to the right of the undo and redo buttons) is set to release mode.
5. Click on "Build" and then click "Build Solution". Alternatively, you can use the shortcut "ctrl + shift + B".

## Usage 

##### To Start a Game
1. To run the Game of Life, you will first need to open up Windows Command Prompt.
2. Type: `cd "C:\..\..\CAB201_2020S2_ProjectPartA_n10689877\Life\Life\bin\Release\netcoreapp3.1"`. You will need to replace `C:\..\..\` with your path to the CAB201_2020S2_ProjectPartA_n10689877 folder.
3. Type: `dotnet Life.dll`
4. Press `Enter` to start the game.

*Alternatively:*
1. Navigate to `CAB201_2020S2_ProjectPartA_n10689877\Life\Life\bin\Release\netcoreapp3.1`.
2. Click into the address bar of the Windows Explorer window and press the `DEL` key.
3. Type "cmd" and press `Enter`.
4. This should open up a Command Prompt window pointed at the correct directory.


##### Optional Arguments
If you start a game without entering any arguments, it will proceed with the default settings. These settings are as follows:
|Setting           |Default Value            |
|------------------|-------------------------|
|No. of Rows       |16                       |
|No. of Columns    |16                       |
|Neighbourhood Type|Moore                    |
|Order             |1                        |
|Centre Count      |Disabled                 |
|Birth Rules       |3                        |
|Survival Rules    |2, 3                     |
|Periodic Mode     |Disabled                 |
|Random Factor     |50%                      |
|Seed File         |None (uses random factor)|
|Output File       |None                     |
|No. of Generations|50                       |
|Update Rate       |5 generations / second   |
|Step Mode         |Disabled                 |
|Memory            |16 generations           |
|Ghost Mode        |Disabled                 |

However, if you'd prefer, you can alter settings with the following flags:
|Flag/Option  |Function                                                                                                  |Parameters                                                                                   |Example                    |
|-------------|----------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------|---------------------------|
|--dimensions |Sets the number of rows and columns respectively.                                                         |Exactly two whole numbers, between 4 & 48 inclusive.                                         |`--dimensions 16 16`       |
|--periodic   |Enables periodic behaviour, i.e. neighbours wrap around the boundaries.                                   |N/A                                                                                          |`--periodic`               |
|--random     |Sets the chance of a cell being set to alive during setup.                                                |Exactly one floating point number between 0 and 1 inclusive.                                 |`--random 0.5`             |
|--seed       |Choose a seed file to set the initial state of the cells.                                                 |A valid seed file (or path to seed file). Must end in '.seed' and be a version 1 or 2 seed.  |`--seed glider.seed`       |
|--generations|Sets the total number of generations the game will run for.                                               |A single positive, non-zero whole number.                                                    |`--generations 50`         |
|--max-update |Sets the number of generational updates per second.                                                       |A single floating point number between 1 and 30 inclusive.                                   |`--max-update 5`           |
|--step       |Enables step mode, allowing you to step through each generation sequentially by pressing space.           |N/A                                                                                          |`--step`                   |
|--neighbour  |Changes neighbourhood type, the neighbourhood order and whether the centre cell is counted as a neighbour.|Exactly three: (Moore or Von Neumann), a single whole number between 1 and 10, true or false.|`--neighbour moore 1 false`|
|--birth      |Sets the number of living neighbours a dead cell requires to be changed to alive next generation.         |Unlimited number of whole numbers separated by spaces or '...' to indicate a range.          |`--birth 2 4 12...20`      |
|--survival   |Sets the number of living neighbours a living cell requires to stay alive next generation.                |Unlimited number of whole numbers separated by spaces or '...' to indicate a range.          |`--survival 3 4`           |
|--output     |The file name/path to which the final generation can be saved (in version 2 seed format)                  |A valid path to a file or file name. Must end in '.seed'.                                    |`--output C:\output.seed`  |
|--memory     |The number of generations store in memory for the detection of steady-state.                              |A single whole number, between 4 & 512 inclusive.                                            |`--memory 16`              |
|--ghost      |Enables a fading effect that allows dead cells to disappear over a number of generations.                 |N/A                                                                                          |`--ghost`                  |

Some additional validation requirements:
* --neighbour
  * The neighbourhood type is case insensitive, e.g. `mOoRe` is just as valid as `moore`
  * The order must be less than half smallest dimension
* --birth and --survival
  * The largest value in either must be equal to or less than the number of neighbours a cell has 

An example of an argument with valid flags and parameters:
`dotnet Life.dll --generations 30 --dimensions 32 40 --step --ghost --neighbour VONneumaNN 3 true`

Please note, the order of the arguments does not matter.

## Notes 

* The number of parameters following options that require them is strict. Too many or too few will result in the parameters being ignored and the program reverting to defaults.
    * For example, `--dimensions 30 23 42` contains too many parameters and the game will revert to the default number of rows and columns.
    * Birth and survival are exceptions to this rule as they take an unlimited number of parameters, from 0 and up. Providing no parameters will cause those values to be set at 0.
* The program is generally forgiving when parsing invalid paramets for arguments that take more than one parameter
  * --dimensions
    * Providing one valid and one invalid parameter will only cause the invalid parameter to default, e.g. `--dimensions ten 34` will result in rows being set to 16 (default) and columns being set to 34.
  * --neighbour
    * An invalid neighbourhood type will cause all neighbour values to reset to default, e.g. `--neighbour conway 3 false` will revert to Moore, 1 and false for neighbourhood, order and centre-count respectively.
    * An invalid order value or invalid centre-count setting will reset that distinct setting to its default, e.g. `--neighbour vonneumann 3 yes` will result in Von Neumann, 3 and false for neighbourhood, order and centre-count.
  * --birth and --survival
    * Any invalid values will cause that option to revert to defaults.  
* When a valid seed file is called:
  * If any cells in the seed file are outside of the bounds of the grid, the game will recommend a minimum grid size and continue with the cells it can generate (this may be none)
  * The random factor will be ignored (as the initial state is determine by the seed file)
* When a valid output path is provided:
  * The output will always be written in version 2 format using the cell structure.
* If step mode is enabled:
  * The update rate will be ignored and the game will progress at 1 generation per press of the spacebar.
* For parameterless flags (--step, --periodic and --ghost):
  * If these flags are called their respective modes will be enabled. Any parameters following them will be ignored by the program (the program will warn you this is the case).
