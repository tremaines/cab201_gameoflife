using Display;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using System.IO;
using System.Data;

namespace Life
{ 
    /// <summary>
    /// The game class use a Settings object to play the Game of Life according to the variables
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
        private enum DeadOrAlive
        {
            dead,
            alive
        }
        private Settings settings;
        private Grid grid;
        private DeadOrAlive[,] statusArray;

        /// <summary>
        /// Constructor for a new game of life. Takes in the game settings and sets up a new "board" using
        /// the statusArray variable.
        /// </summary>
        /// <param name="settings"></param>
        public Game(Settings settings)
        {
            this.settings = settings;
            grid = new Grid(settings.Rows, settings.Columns);
            statusArray = new DeadOrAlive[settings.Rows, settings.Columns];
        }

        /// <summary>
        /// Prints the settings used by this instance of the Game.
        /// </summary>
        public void PrintSettings()
        {
            Console.WriteLine($"\t       Number of Rows: {settings.Rows}\n" +
                $"\t    Number of Columns: {settings.Columns}\n" +
                $"\tPeriodic Mode Enabled: {settings.Periodic}\n" +
                $"\t        Random Factor: {settings.Random:P2}\n" +
                $"\t            Seed File: {Path.GetFileName(settings.SeedFile)}\n" +
                $"\t   No. of Generations: {settings.Generations}\n" +
                $"\t          Update Rate: {settings.UpdateRate} generations / second\n" +
                $"\t    Step Mode Enabled: {settings.StepMode}");
            Console.WriteLine("\nPress any key to start...");
            Console.ReadKey();
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
                    CheckForSpace();
                }
                // Otherwise game cycles through at the update rate specified in Settings
                else
                {
                    while (watch.ElapsedMilliseconds < ((1 / settings.UpdateRate) * 1000)) ;
                }
                statusArray = NextGenerationStatus();
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
            Console.Write("\n\nPress SPACE to finish...");
            CheckForSpace();
            grid.RevertWindow();
        }

        /// <summary>
        /// Sets the initial state of the cells in generation 0 based on the seed file (if provided & valid)
        /// or the random factor
        /// </summary>
        private void SetInitialState()
        {
            if (settings.SeedFile != "None")
            {
                ReadSeedFile();
            }
            else
            {
                float chance = settings.Random * 100;
                Random random = new Random();
                for (int r = 0; r < settings.Rows; r++)
                {
                    for (int c = 0; c < settings.Columns; c++)
                    {
                        if (chance > random.Next(100))
                        {
                            grid.UpdateCell(r, c, CellState.Full);
                            statusArray[r, c] = DeadOrAlive.alive;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads a seed file and sets cells to alive given the information in the seed file
        /// if that cell is within the bounds of the board.
        /// </summary>
        private void ReadSeedFile()
        {
            string fileName = settings.SeedFile;
            using (StreamReader reader = new StreamReader(fileName))
            {
                reader.ReadLine();
                char delimiter = ' ';
                string line;
                // Keep track of the highest row and column value in the seed file
                int rowMax = 0;
                int colMax = 0;
                bool outOfBoundsValue = false;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] coordinates = line.Split(delimiter);
                    int row = Int32.Parse(coordinates[0]);
                    int col = Int32.Parse(coordinates[1]);
                    CheckSeedValues(row, ref rowMax);
                    CheckSeedValues(col, ref colMax);

                    // If either the row or column value are out of bounds, continue through next iteration
                    // of loop instead of changing cell to alive
                    if (row + 1 > settings.Rows || col + 1 > settings.Columns)
                    {
                        outOfBoundsValue = true;
                        continue;
                    }
                    statusArray[row, col] = DeadOrAlive.alive;
                }
                
                // If there were any out of bounds values, print recommended dimensions based on max row
                // and column values
                if (outOfBoundsValue)
                {
                    Console.WriteLine("\nWARNING! Game will continue but some cells in seed file are out of bounds:");
                    Console.WriteLine($"  - Recommended minimum dimensions based on seed file: {rowMax + 1} rows " +
                                      $"X {colMax + 1} columns.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Updates the value of the variable storing the maximum dimension in a seed file
        /// </summary>
        /// <param name="seedValue"></param>
        /// <param name="maxValue"></param>
        private void CheckSeedValues(int seedValue, ref int maxValue)
        {
            if (seedValue > maxValue)
            {
                maxValue = seedValue;
            }
        }

        /// <summary>
        /// Checks the 8 neighbours of a cell and counts the number of living neighbours it has
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
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
                        if (settings.Periodic)
                        {
                            livingNeighbours += (int)statusArray[((neighbourR + settings.Rows) % settings.Rows),
                                ((neighbourC + settings.Columns) % settings.Columns)];
                        }
                        // Check if neighbour cell is within the bounds of the board
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
        private DeadOrAlive[,] NextGenerationStatus()
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
        /// Loops until user pushes the spacebar key
        /// </summary>
        private void CheckForSpace()
        {
            ConsoleKeyInfo keyPress = Console.ReadKey(true);
            while(keyPress.KeyChar != ' ')
            {
                keyPress = Console.ReadKey(true);
            }
        }
    }
}
