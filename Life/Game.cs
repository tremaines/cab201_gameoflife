using Display;
using System;
using System.Diagnostics;
using System.IO;

namespace Life
{
    // Originally just dead and alive, have added light, medium and dark (couldn't think of better names)
    // for --ghost mode in part 2
    public enum DeadOrAlive
    {
        dead,
        light,
        medium,
        dark,
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
        // Array of 2D arrays settings.Memory in size
        private DeadOrAlive[][,] memory;
        // Works in tandem with memory to determine periodicity (if steady-state acheived)
        private int[] generationInMemory;
        private bool steadyState = false;
        private int periodicity = 0;
        private int memoryCounter = 0;
        private int memoryIndex = 0;
        private WriteSeed writeSeed;

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
            memory = new DeadOrAlive[settings.Memory][,];
            generationInMemory = new int[settings.Memory];
        }

        /// ++++++++++++++++++++++++++++++
        /// +       PUBLIC METHODS       +
        /// ++++++++++++++++++++++++++++++

        /// <summary>
        /// Prints the error and success messages for user and the settings that will be used to run the game
        /// </summary>
        public void PrintSettings()
        {
            Console.WriteLine("\nSetting up game with the following values:\n");
            Console.WriteLine(settings);
            if (settings.SeedFile != null)
            {
                Console.WriteLine("Random factor will be ignored as a valid seed file has been provided!");
            }
            if (settings.OutputFile != null && File.Exists(settings.OutputFile))
            {
                Logging.PrintMessage($"Please be aware {Path.GetFullPath(settings.OutputFile)} " +
                    $"already exists. If you continue, it will be overwritten!", ConsoleColor.Yellow);
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
                
                // Only need to create a new generation from i = 1 onwards
                if (i != 0)
                {
                    statusArray = CreateNextGeneration();
                }
                UpdateCellStatus();
                int matchIndex = CompareToMemory();

                // Only need to compare to memory from i = 1 onwards
                if (i != 0)
                {
                    if (matchIndex != -1)
                    {
                        periodicity = i - generationInMemory[matchIndex];
                        steadyState = true;
                        // Break out of loop to end game
                        break;
                    }
                }
                AddToMemory(statusArray, i);
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
            }

            // Write final array to file if valid output file path specified
            if (settings.OutputFile != null)
            {
                try
                {
                    writeSeed = new WriteSeed(settings.OutputFile, statusArray);
                    writeSeed.WriteToFile();
                }
                catch
                {
                    Logging.PrintMessage("Something went wrong while writing the to the output file!", 
                        ConsoleColor.Red);
                }
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

            // If steady-state acheived, alert user and display periodicity
            if (steadyState)
            {
                string detected = "Steady state detected! ";
                string period = ($"Periodicity: {(periodicity == 1 ? "N/A" : periodicity.ToString())}");
                Logging.PrintMessage(detected + period, ConsoleColor.Green);
            }
            else
            {
                Console.WriteLine("Steady-state not detected.");
            }

            // Print FULL PATH of output seed file (so users like me who forget where they put their files no where
            // find them)
            if (settings.OutputFile != null)
            {
                Console.WriteLine($"\nFinal generation written to:\n\"{Path.GetFullPath(settings.OutputFile)}\"");
            }
            CheckForSpace(action: "finish");
        }

        /// <summary>
        /// Prints a prompt for the user then waits for them to push space. Attempts to prevent the user from
        /// holding space to advance the program.
        /// </summary>
        /// <param name="action">Tell the user what space will do when they press it</param>
        /// <param name="writeMsg">Set false to not write a message to the console</param>
        public static void CheckForSpace(string action = "continue", bool writeMsg = true)
        {
            // This value was determined mostly by trial and error
            int delay = 275;
            Stopwatch watch = new Stopwatch();

            if (writeMsg)
            {
                Console.WriteLine($"\nPress SPACE to {action}...");
            }

            while (true)
            {
                // The stopwatch delay here and at the end helps account for the OS delay between a key being pressed
                // and it being registered as held down
                watch.Restart();
                while (watch.ElapsedMilliseconds < delay) ;
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                {
                    while (Console.ReadKey(true).Key == ConsoleKey.Spacebar)
                    {
                        if (!Console.KeyAvailable)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Spacebar) ;
            watch.Restart();
            while (watch.ElapsedMilliseconds < delay) ;
        }

        /// ++++++++++++++++++++++++++++++
        /// +       PRIVATE METHODS      +
        /// ++++++++++++++++++++++++++++++

        /// <summary>
        /// Sets the initial state of the cells in generation 0 based on the seed file (if provided & valid) or the 
        /// random factor
        /// </summary>
        /// <param name="ignoreSeed">
        /// Set to true to initialise from random even in the presence of a non-null SeedFile variable
        /// </param>
        private void SetInitialState(bool ignoreSeed = false)
        {
            if (settings.SeedFile != null && !ignoreSeed)
            {
                InitialiseFromSeed();
            }
            else
            {
                InitialiseFromRandom();
            }
        }

        /// <summary>
        /// Uses StreamReader to determine the version of a seed then initialises a ReadSeed object
        /// to parse the seed file.
        /// </summary>
        /// <exception cref="SeedVersionException">Seed version is not valid</exception>
        /// <exception cref="SeedLineException">Incorrect composition of a line in the seed file</exception>
        /// <exception cref="Exception">Catches any other exceptions thrown</exception>
        private void InitialiseFromSeed()
        {
            string errorMsg = Logging.SubMessageFormatter("Reverting to random factor to initialise game.");
            bool exceptionThrow = false;
            try
            {
                using StreamReader reader = new StreamReader(settings.SeedFile);
                
                // Read the first line to determine the seed version
                string version = reader.ReadLine();

                // Initialise the correct child class of ReadSeed
                ReadSeed seed = version switch
                {
                    "#version=1.0" => new SeedVersionOne(settings.SeedFile, settings.Rows, settings.Columns),
                    "#version=2.0" => new SeedVersionTwo(settings.SeedFile, settings.Rows, settings.Columns),
                    _ => throw new SeedVersionException(version),
                };

                // Read the file and update the statusArray
                seed.ReadFile();
                seed.AlertToOutOfBounds();
                statusArray = seed.CellsArray;
            }
            catch (SeedVersionException e)
            {
                string SubMsg = Logging.SubMessageFormatter("Version must be 1.0 or 2.0\n") + errorMsg;
                Logging.GenericWarning(e.Message, SubMsg);
                exceptionThrow = true;
            }
            catch (SeedLineException e)
            {
                Logging.GenericWarning(e.Message, errorMsg);
                exceptionThrow = true;
            }
            catch (Exception)
            {
                Logging.GenericWarning("There has been an issue reading the provided seed file.", errorMsg);
                exceptionThrow = true;
            }
            finally
            {
                // If an exception was thrown, user will be alerted and SetInitialState will be re-called
                // with the seed file ignored
                if (exceptionThrow)
                {
                    CheckForSpace();
                    SetInitialState(true);
                }
            }
        }

        /// <summary>
        /// Set the initial board based on the random factor
        /// </summary>
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
        /// Add a 2D array of DeadOrAlive values to the memory
        /// </summary>
        /// <param name="generationArray">The array to be added</param>
        /// <param name="generation">The current generation</param>
        private void AddToMemory(DeadOrAlive[,] generationArray, int generation)
        {
            // The memoryIndex needs to be re-set to 0 in order to overwrite older generations
            if (memoryIndex == memory.Length)
            {
                memoryIndex = 0;
            }

            // Add the array to memory as well as the corresponding generation to
            // the generation array and increment the index
            memory[memoryIndex] = generationArray;
            generationInMemory[memoryIndex] = generation;
            memoryIndex++;

            // Memory counter is only used in CompareToMemory(), once it == settings.Memory
            // there will always be settings.Memory arrays held in memory so we can stop incrementing
            if (memoryCounter != settings.Memory)
            {
                memoryCounter++;
            }
        }

        /// <summary>
        /// Traverses each array held in memory and compares it to the newest generation
        /// of the game
        /// </summary>
        /// <returns>The index of the matching array, -1 if no match</returns>
        private int CompareToMemory()
        {
            bool match = false;
            int matchIndex = -1;

            for (int i = 0; i < memoryCounter; i++)
            {
                for (int r = 0; r < settings.Rows; r++)
                {
                    for (int c = 0; c < settings.Columns; c++)
                    {
                        if ((int)memory[i][r, c] / (int)DeadOrAlive.alive != 
                            (int)statusArray[r, c] / (int)DeadOrAlive.alive)
                        {
                            // Break immediately if there is no match
                            match = false;
                            break;
                        }
                        match = true;
                    }
                    // And break here as well
                    if (!match)
                    {
                        break;
                    }
                }
                // If we've got to this point without breaking, we have a match! 
                // Can break out of outer-most loop and return index
                if (match)
                {
                    matchIndex = i;
                    break;
                }
            }

            return matchIndex;
        }

        /// <summary>
        /// Chooses the correct next-gen status of a cell, important for ghost-mode
        /// </summary>
        /// <param name="status">The cell's current status</param>
        /// <returns>The cell's next-gen status</returns>
        private DeadOrAlive ChooseShading(DeadOrAlive status)
        {
            switch (status)
            {
                case DeadOrAlive.alive:
                    return DeadOrAlive.dark;
                case DeadOrAlive.dark:
                    return DeadOrAlive.medium;
                case DeadOrAlive.medium:
                    return DeadOrAlive.light;
                case DeadOrAlive.light:
                    return DeadOrAlive.dead;
                default:
                    break;
            }
            return DeadOrAlive.dead;
        }

        /// <summary>
        /// Checks each cell against the rules of the Game of Life to determine their status next generation
        /// </summary>
        /// <returns>A 2D array of what the board will look like next generation</returns>
        private DeadOrAlive[,] CreateNextGeneration()
        {
            DeadOrAlive[,] nextGenStatusArray = new DeadOrAlive[settings.Rows, settings.Columns];
            Neighbourhood neighbourhood = new Neighbourhood(settings.Neighbourhood, 
                settings.Order, settings.Centre, settings.Periodic, statusArray);

            for (int r = 0; r < settings.Rows; r++)
            {
                for (int c = 0; c < settings.Columns; c++)
                {
                    int livingNeighbours = neighbourhood.CheckNeighbours(r, c);
                    if (statusArray[r, c] != DeadOrAlive.alive)
                    {
                        // Living neighbours of the dead cell must be equal to a value in the --birth rules for it
                        // to be alive
                        if (settings.Birth.Contains(livingNeighbours))
                        {
                            nextGenStatusArray[r, c] = DeadOrAlive.alive;
                        }
                        // Otherwise it stays dead
                        else
                        {
                            // If ghost mode is enabled, need to determine the correct next-gen shading
                            nextGenStatusArray[r, c] = settings.Ghost ? ChooseShading(statusArray[r, c]) : 
                                DeadOrAlive.dead;
                        }
                    }
                    else if (statusArray[r, c] == DeadOrAlive.alive)
                    {
                        // Living neighbours of living cells must be equal to a value in the --survival rules for it
                        // to stay alive
                        if (settings.Survival.Contains(livingNeighbours))
                        {
                            nextGenStatusArray[r, c] = DeadOrAlive.alive;
                        }
                        // Otherwise it will die
                        else
                        {
                            nextGenStatusArray[r, c] = settings.Ghost ? DeadOrAlive.dark : DeadOrAlive.dead;
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
                    else
                    {
                        CellState state = CellState.Blank;
                        if (settings.Ghost)
                        {
                            switch (statusArray[r, c])
                            {
                                case DeadOrAlive.dark:
                                    state = CellState.Dark;
                                    break;
                                case DeadOrAlive.medium:
                                    state = CellState.Medium;
                                    break;
                                case DeadOrAlive.light:
                                    state = CellState.Light;
                                    break;
                            }
                        }
                        grid.UpdateCell(r, c, state);
                    }
                }
            }
        }
    }
}
