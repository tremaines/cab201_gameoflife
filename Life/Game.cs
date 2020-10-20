using Display;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata.Ecma335;

namespace Life
{
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
        private DeadOrAlive[][,] memory;
        private bool steadyState = false;
        private int periodicity = 0;
        private int memoryCounter = 0;
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
        }

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
                if (i != 0)
                {
                    statusArray = CreateNextGeneration();
                }
                UpdateCellStatus();
                int matchIndex = CompareToMemory();
                if (i != 0)
                {
                    if (matchIndex != -1)
                    {
                        periodicity = memoryCounter - matchIndex;
                        steadyState = true;
                        break;
                    }
                }
                AddToMemory(statusArray);
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

            // Write final array to file
            if (settings.OutputFile != null)
            {
                writeSeed = new WriteSeed(settings.OutputFile, statusArray);
                writeSeed.WriteToFile();
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
            if (steadyState)
            {
                Console.Write("Steady state detected! ");
                Console.WriteLine("Periodicity: {0}", periodicity == 1 ? "N/A" : periodicity.ToString());
            }
            else
            {
                Console.WriteLine("Steady state not detected.");
            }
            if (settings.OutputFile != null)
            {
                Console.WriteLine($"Final generation written to {Path.GetFullPath(settings.OutputFile)}");
            }
            CheckForSpace(action: "finish");
        }

        /// <summary>
        /// Sets the initial state of the cells in generation 0 based on the seed file (if provided & valid) or the 
        /// random factor
        /// </summary>
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

        private void AddToMemory(DeadOrAlive[,] generation)
        {
            if (memoryCounter + 1 == memory.Length)
            {
                memoryCounter = 0;
            }

            memory[memoryCounter] = generation;
            memoryCounter++;
        }

        private int CompareToMemory()
        {
            bool match = false;
            int matchIndex = -1;
            using StreamWriter writer = new StreamWriter("debug.txt", true);
            writer.WriteLine($"+++++++Generation {memoryCounter}+++++++");

            for (int i = 0; i < memoryCounter; i++)
            {
                writer.WriteLine($"-------Memory index:{i}-------");
                for (int r = 0; r < settings.Rows; r++)
                {
                    for (int c = 0; c < settings.Columns; c++)
                    {
                        if ((int)memory[i][r, c] / (int)DeadOrAlive.alive != 
                            (int)statusArray[r, c] / (int)DeadOrAlive.alive)
                        {
                            match = false;
                            writer.WriteLine($"No match: {r}, {c}");
                            break;
                        }
                        writer.WriteLine($"Match: {r}, {c}");
                        match = true;
                    }
                    if (!match)
                    {
                        break;
                    }
                }
                if (match)
                {
                    matchIndex = i;
                    writer.WriteLine("MATCH FOUND!!!");
                    break;
                }
            }

            return matchIndex;
        }

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
                        // If a cell is a dead and has exactly 3 living neighbours, it will be alive
                        if (settings.Birth.Contains(livingNeighbours))
                        {
                            nextGenStatusArray[r, c] = DeadOrAlive.alive;
                        }
                        // Otherwise it stays dead
                        else
                        {
                            nextGenStatusArray[r, c] = settings.Ghost ? ChooseShading(statusArray[r, c]) : 
                                DeadOrAlive.dead;
                        }
                    }
                    else if (statusArray[r, c] == DeadOrAlive.alive)
                    {
                        // If a cell is a live and has 2 or 3 living neighbours, it will stay alive
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
