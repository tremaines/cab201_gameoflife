using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Life
{
    /// <summary>
    /// This class holds the settings for the Game of Life board
    /// Defaults are initialised at the beginning of the class and the various methods
    /// change those defaults to user define values (assuming the user's input it valid!)
    /// </summary>
    /// <author>Tremaine Stroebel</author>
    /// <date>August 2020</date>
    class Settings
    {
        //  Public list of all attributes available to be altered by user
        public static readonly List<string> attributes = new List<string> { "--dimensions",
                                                                            "--periodic",
                                                                            "--random",
                                                                            "--seed",
                                                                            "--generations",
                                                                            "--max-update",
                                                                            "--step"
                                                                          };

        //  Settings fields, set to default values
        private int rows = 16; 
        private int columns = 16;
        private bool periodic = false;
        private float random = 0.5F;
        private string seedFile = "None";
        private int generations = 50;
        private float updateRate = 5;
        private bool stepMode = false;

        //  Settings properties

        public int Rows { get { return rows; } }
        public int Columns { get { return columns; } }
        public bool Periodic { get { return periodic; } }
        public float Random { get { return random; } }
        public string SeedFile { get { return seedFile; } }
        public int Generations { get { return generations; } }
        public float UpdateRate { get { return updateRate; } }
        public bool StepMode { get { return stepMode; } }

        /// <summary>
        /// Default constructor, called when user doesn't change settings or doesn't enter any valid --options.
        /// </summary>
        public Settings() { }

        /// <summary>
        /// Constructor called when the user enters at least one valid --option.
        /// It traverses the list of lists and calls the relevant validity-check method for any
        /// options entered by the user.
        /// </summary>
        /// <param name="userArgs"></param>
        public Settings(List<List<string>> userArgs)
            {
                userArgs.ForEach(delegate (List<string> options)
                {
                    switch (options[0])
                    {
                        case "--dimensions":
                            Dimensions(options, out rows, out columns);
                            break;
                        case "--periodic":
                            PeriodicAndStep(options, out periodic);
                            break;
                        case "--random":
                            RandomFactor(options, out random);
                            break;
                        case "--seed":
                            OpenSeed(options, out seedFile);
                            break;
                        case "--generations":
                            ChangeGenerations(options, out generations);
                            break;
                        case "--max-update":
                            ChangeMaxUPS(options, out updateRate);
                            break;
                        case "--step":
                            PeriodicAndStep(options, out stepMode);
                            break;
                        default:
                            break;
                    }
                });
            }

        /// <summary>
        /// This subtracts the number of arguments expected when an option is called by a user
        /// verse the number received by the user. NB, the number expected includes the --option argument.
        /// </summary>
        /// <param name="numReceived"></param>
        /// <param name="numExpected"></param>
        /// <returns>
        /// The difference between the number of arguments received and number expected.
        /// </returns>
        private static int CheckNumOfArgs(int numReceived, int numExpected)
        {
            return numReceived - numExpected;
        }

        /// <summary>
        /// Produces a generic error message if the arg count difference != 0
        /// </summary>
        /// <param name="option"></param>
        /// <param name="difference"></param>
        /// <returns>
        /// A string to use print out. Can append more specific information depending on the option.
        /// </returns>
        private static string ParamCountErrorMessage(string option, int difference)
        {
            if (difference > 0)
            {
                return $"Too many parameters received for {option}.";
            }
            else
            {
                return $"Not enough parameters received for {option}.";
            }
        }

        /// <summary>
        /// Validates the parameters entered by the user for --dimensions and changes the rows
        /// and columns attribute accordingly. If the user input is invalid, the default value is retained.
        /// </summary>
        /// <param name="userInput"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        public void Dimensions(List<String> userInput, out int rows, out int cols)
        {
            rows = 16;  //  These are the default values
            cols = 16;  //  and will only be changed if the user input is valid!
            int userRows, userCols;
            int numExpectedArgs = 3;
            int argCountDifference = CheckNumOfArgs(userInput.Count, numExpectedArgs);

            //  Check user has entered two parameters (one for rows and one for columns)
            if (argCountDifference == 0)
            {
                // If they have, validate those values are both ints between 4 and 48 inclusive
                if ((Int32.TryParse(userInput[1], out userRows) && Int32.TryParse(userInput[2], out userCols))
                    && ((userRows >= 4 && userRows <= 48) && (userCols >= 4 && userCols <= 48)))
                {
                    rows = userRows;
                    cols = userCols;
                }
                else
                {
                    Console.WriteLine("--dimensions requires two positive integers between 4 and 48 inclusive.");
                }
            }
            else
            {
                Console.WriteLine(ParamCountErrorMessage(userInput[0], argCountDifference) 
                    + " Please specify exactly TWO positive integers between 4 and 48 inclusive.");
            }
        }

        /// <summary>
        /// If --periodic and/or --step are called, they will be set to true regardless of what parameters the user
        /// enters after them. This functions will check for any parameters and let the user know it doesn't know
        /// what to do with them.
        /// </summary>
        /// <param name="userInput"></param>
        public static void PeriodicAndStep(List<string> userInput, out bool status)
        {
            int numExpectedArgs = 1;
            status = true;

            // Check for parameters following periodic/step and let the user know these are unecessary/ignored
            if (CheckNumOfArgs(userInput.Count, numExpectedArgs) != 0)
            {
                Console.Write($"Unknow parameter(s) for {userInput[0]}: ");
                for (int i = numExpectedArgs; i < userInput.Count; i++)
                {
                    if (i + 1 == userInput.Count)
                    {
                        Console.WriteLine($"'{userInput[i]}'");
                    }
                    else
                    {
                        Console.Write($"'{userInput[i]}', ");
                    }
                }
                Console.WriteLine($"{userInput[0]} has been set to TRUE but, for future reference, " +
                    $"does not require parameters.");
            }
        }

        /// <summary>
        /// Validates the parameter for --random and changes the random attribute accordingly.
        /// Stays at the default value if invalid user input.
        /// </summary>
        /// <param name="userInput"></param>
        /// <param name="random"></param>
        public static void RandomFactor(List<string> userInput, out float random)
        {
            
            random = 0.5F;  //  This is the default that will change only if the user input is valid
            float userRand;
            int numExpectedArgs = 2;
            int argCountDifference = CheckNumOfArgs(userInput.Count, numExpectedArgs);

            if (argCountDifference == 0)
            {
                if ((float.TryParse(userInput[1], out userRand)) && (userRand >= 0 && userRand <= 1))
                {
                    random = userRand;
                }
                else
                {
                    Console.WriteLine("--random requires a float parameter between 0 and 1 inclusive.");
                }
            }
            else
            {
                Console.WriteLine(ParamCountErrorMessage(userInput[0], argCountDifference)
                    + " Please specify a single float parameter between 0 and 1 inclusive.");
            }
        }

        /// <summary>
        /// Open and read seed file ****************TO DO**********TO DO***********TO DO*********************
        /// </summary>
        /// <param name="options"></param>
        public static void OpenSeed(List<string> userInput, out string seedFile)
        {
            seedFile = "None";  // Default value
            int numExpectedArgs = 2;
            string file;
            string extension = ".seed";
            int argCountDifference = CheckNumOfArgs(userInput.Count, numExpectedArgs);
            
            if (argCountDifference == 0)
            {
                file = userInput[1];
                if (File.Exists(file) && (Path.GetExtension(file) == extension))
                {
                    seedFile = file;
                }
                else
                {
                    Console.WriteLine("The seed file provided is not valid. Please ensure you type the path or file name correctly," +
                        " and that the file ends in '.seed'.");
                }
            }
            else
            {
                Console.WriteLine(ParamCountErrorMessage(userInput[0], argCountDifference)
                   + " Please specify a single file name or path.");
            }
        }

        /// <summary>
        /// Validates the parameter for --generations and changes the generations attribute accordingly.
        /// Stays at the default value if user input is invalid.
        /// </summary>
        /// <param name="userInput"></param>
        /// <param name="gens"></param>
        public static void ChangeGenerations(List<string> userInput, out int gens)
        {
            gens = 50;   //  The default value
            int numExpectedArgs = 2;
            int userGens;
            int argCountDifference = CheckNumOfArgs(userInput.Count, numExpectedArgs);

            if (argCountDifference == 0)
            {
                if (Int32.TryParse(userInput[1], out userGens) && userGens > 0)
                {
                    gens = userGens;
                }
                else
                {
                    Console.WriteLine("Number of generations must be a positive, non-zero integer.");
                }
            }
            else
            {
                Console.WriteLine(ParamCountErrorMessage(userInput[0], argCountDifference)
                    + " Please specify a single positive, non-zero integer.");
            }   
        }

        /// <summary>
        /// Validates the parameter for --max-update and changes the updateRate attribute accordingly.
        /// Stays at the default value if user input is invalid.
        /// </summary>
        /// <param name="userInput"></param>
        /// <param name="ups"></param>
        public static void ChangeMaxUPS(List<string> userInput, out float ups)
        {
            ups = 5;    //  The default value
            int numExpectedArgs = 2;
            float userUPS;
            int argCountDifference = CheckNumOfArgs(userInput.Count, numExpectedArgs);

            if (argCountDifference == 0)
            {
                if (float.TryParse(userInput[1], out userUPS) && (userUPS >= 1 && userUPS <= 30))
                {
                    ups = userUPS;
                }
                else
                {
                    Console.WriteLine("Number of updates per second must be a float between 1 and 30 inclusive.");
                }
            }
            else
            {
                Console.WriteLine(ParamCountErrorMessage(userInput[0], argCountDifference)
                    + " Please specify a single float parameter between 1 and 30 inclusive.");
            }
        }
    }
}
