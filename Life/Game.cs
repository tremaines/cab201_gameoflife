using Display;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Life
{
    public enum DeadOrAlive
    {
        dead,
        alive
    }

    /// <summary>
    /// The game class uses a Settings object to play the Game of Life according to the fields
    /// of the Settings instance.
    /// </summary>
    /// <author>
    /// Tremaine Stroebel
    /// </author>
    /// <date>
    /// September 2020
    /// </date>
    class Game
    {
        private readonly Settings settings;
        private readonly Grid grid;
        private DeadOrAlive[,] statusArray;

        /// <summary>
        /// Constructor for a new game of life. Takes in the game settings and sets up a new "board" using the 
        /// statusArray variable.
        /// </summary>
        /// <param name="settings">An instance of the Settings class</param>
        public Game(Settings settings)
        {
            this.settings = settings;
            grid = new Grid(settings.Rows, settings.Columns);
            statusArray = new DeadOrAlive[settings.Rows, settings.Columns];
        }

        /// <summary>
        /// Prints the error and success messages for user and the settings that will be used to run the game
        /// </summary>
        public void PrintSettings()
        {
            Console.WriteLine("\nSetting up game with the following values:\n");
            Console.WriteLine(settings);
            if (settings.SeedFile != "None")
            {
                Console.WriteLine("Random factor will be ignored as a valid seed file has been provided!");
            }
            CheckForSpace();
        }

        /// <summary>
        /// Cycles through the game starting at generation 0 up to the number of generations set in the Settings
        /// </summary>
        public void CycleThroughGame()
        {
            SetInitialState();
            grid.InitializeWindow();
            Stopwatch watch = new Stopwatch();

            for (int i = 0; i <= settings.Generations; i++)
            {
                grid.SetFootnote($"Generation: {i}");
                UpdateCellStatus();
                grid.Render();
                watch.Restart();

                // User cycles through 1 generation at a time by pressing space if step mode enabled
                if (settings.StepMode)
                {
                    CheckForSpace(writeMsg: false);
                }
                // Otherwise game cycles through at the update rate specified in Settings
                else
                {
                    while (watch.ElapsedMilliseconds < ((1 / settings.UpdateRate) * 1000)) ;
                }
                statusArray = CreateNextGeneration();
            }
        }

        /// <summary>
        /// When the Game is complete, render the final scene and prompt the user to
        /// push space to exit the game
        /// </summary>
        public void RenderFinalGrid()
        {
            grid.IsComplete = true;
            grid.Render();
            CheckForSpace(writeMsg: false);
            grid.RevertWindow();
            CheckForSpace(action: "finish");
        }

        /// <summary>
        /// Sets the initial state of the cells in generation 0 based on the seed file (if provided & valid) or the 
        /// random factor
        /// </summary>
        private void SetInitialState(bool ignoreSeed = false)
        {
            if (settings.SeedFile != "None" && !ignoreSeed)
            {
                InitialiseFromSeed();
            }
            else
            {
                InitialiseFromRandom();
            }
        }

        private void InitialiseFromSeed()
        {
            string errorMsg = Logging.SubMessageFormatter("Reverting to random factor to initialise game.");
            try
            {
                
                using StreamReader reader = new StreamReader(settings.SeedFile);
                string version = reader.ReadLine();
                ReadSeed seed = version switch
                {
                    "#version=1.0" => new SeedVersionOne(settings.SeedFile, settings.Rows, settings.Columns),
                    "#version=2.0" => new SeedVersionTwo(settings.SeedFile, settings.Rows, settings.Columns),
                    _ => throw new SeedVersionException(version),
                };
                seed.ReadFile();
                seed.AlertToOutOfBounds();
                statusArray = seed.CellsArray;
            }
            catch (SeedVersionException e)
            {
                string SubMsg = Logging.SubMessageFormatter("Version must be 1.0 or 2.0\n") + errorMsg;
                Logging.GenericWarning(e.Message, SubMsg);
                CheckForSpace();
                SetInitialState(true);
            }
            catch (SeedLineException e)
            {
                Logging.GenericWarning(e.Message, errorMsg);
                CheckForSpace();
                SetInitialState(true);
            }
            catch (Exception)
            {
                Logging.GenericWarning("There has been an issue reading the provided seed file.", errorMsg);
                CheckForSpace();
                SetInitialState(true);
            }
        }

        private void InitialiseFromRandom()
        {
            double chance = settings.Random;
            Random random = new Random();
            for (int r = 0; r < settings.Rows; r++)
            {
                for (int c = 0; c < settings.Columns; c++)
                {
                    if (chance > random.NextDouble())
                    {
                        grid.UpdateCell(r, c, CellState.Full);
                        statusArray[r, c] = DeadOrAlive.alive;
                    }
                }
            }
        }

        /// <summary>
        /// Checks the 8 neighbours of a cell and counts the number of living neighbours it has
        /// </summary>
        /// <param name="row">Row of cell (alias for dimension 0 of statusArray)</param>
        /// <param name="column">Column of cell (alias for dimensions 1 of statusArray)</param>
        /// <returns>The total number of living neighbours of the cell</returns>
        private int CheckNeighbours(int row, int column)
        {
            int livingNeighbours = 0;
            for (int r = -1; r <= 1; r++)
            {
                for (int c = -1; c <=1; c++)
                {
                    int neighbourR = row + r;
                    int neighbourC = column + c;
                    if (!(r == 0 && c == 0))     // Don't want to count a cell as its own neighbour
                    {
                        // If periodic is enabled, use that logic in checking the neighbours
                        if (settings.Periodic)
                        {
                            livingNeighbours += (int)statusArray[((neighbourR + settings.Rows) % settings.Rows),
                                ((neighbourC + settings.Columns) % settings.Columns)];
                        }
                        // Otherwise check if neighbour cell is within the bounds of the board
                        else if ((neighbourR >= 0 && neighbourC >= 0) && 
                            (neighbourR < settings.Rows && neighbourC < settings.Columns))
                        {
                            livingNeighbours += (int)statusArray[neighbourR, neighbourC];
                        }
                    }
                }
            }
            return livingNeighbours;
        }

        /// <summary>
        /// Checks each cell against the rules of the Game of Life to determine their status next generation
        /// </summary>
        /// <returns>A 2D array of what the board will look like next generation</returns>
        private DeadOrAlive[,] CreateNextGeneration()
        {
            DeadOrAlive[,] nextGenStatusArray = new DeadOrAlive[settings.Rows, settings.Columns];
            for (int r = 0; r < settings.Rows; r++)
            {
                for (int c = 0; c < settings.Columns; c++)
                {
                    int livingNeighbours = CheckNeighbours(r, c);
                    if (statusArray[r, c] == DeadOrAlive.dead)
                    {
                        // If a cell is a dead and has exactly 3 living neighbours, it will be alive
                        if (livingNeighbours == 3)
                        {
                            nextGenStatusArray[r, c] = DeadOrAlive.alive;
                        }
                        // Otherwise it stays dead
                        else
                        {
                            nextGenStatusArray[r, c] = DeadOrAlive.dead;
                        }
                    }
                    else if (statusArray[r, c] == DeadOrAlive.alive)
                    {
                        // If a cell is a live and has 2 or 3 living neighbours, it will stay alive
                        if (livingNeighbours == 2 || livingNeighbours == 3)
                        {
                            nextGenStatusArray[r, c] = DeadOrAlive.alive;
                        }
                        // Otherwise it will die
                        else
                        {
                            nextGenStatusArray[r, c] = DeadOrAlive.dead;
                        }
                    }
                }
            }
            return nextGenStatusArray;
        }

        /// <summary>
        /// Taverse through cells and update their state based on their DeadOrAlive status
        /// </summary>
        private void UpdateCellStatus()
        {
            for (int r = 0; r < settings.Rows; r++)
            {
                for (int c = 0; c < settings.Columns; c++)
                {
                    if (statusArray[r, c] == DeadOrAlive.alive)
                    {
                        grid.UpdateCell(r, c, CellState.Full);
                    }
                    else grid.UpdateCell(r, c, CellState.Blank);
                }
            }
        }

        /// <summary>
        /// Clears the console's stdin buffer to remove key presses made before the 'press space' prompt
        /// appears, that way the program won't shoot past the prompt if the user has accidentally pushed space
        /// prior to the prompt appearing. Then loops until the user does press space.
        /// </summary>
        public static void CheckForSpace(string action = "continue", bool writeMsg = true)
        {
            if (writeMsg)
            {
                Console.WriteLine($"\nPress SPACE to {action}...");
            }
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }

            ConsoleKeyInfo keyPress = Console.ReadKey(true);
            while(keyPress.KeyChar != ' ')
            {
                keyPress = Console.ReadKey(true);
            }
        }
    }
}
