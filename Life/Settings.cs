using System;
using System.Collections.Generic;
using System.Data;
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
                                                                            "--seedFile",
                                                                            "--generations",
                                                                            "--updateRate",
                                                                            "--step"
                                                                          };
        
        //  Initialise game attributes with default values
        private int rows, columns;
        private bool periodic;
        private float random;
        private string seedFile;
        private int generations;
        private float updateRate;
        private bool stepMode;

        //  Constructor for Settings when the user does not parse any arguments into the CLI
        public Settings()
        {
            rows = 16; 
            columns = 16;
            periodic = false;
            random = 0.5F;
            seedFile = "";
            generations = 50;
            updateRate = 5;
            stepMode = false;
        }

    //  Constructor for Settings when the user DOES parse arguments into the CLI
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

        private static int CheckNumOfArgs(int numReceived, int numExpected)
        {
            return numReceived - numExpected;
        }

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
        /// Changes dimensions of board based on valid user input
        /// </summary>
        /// <param name="userInput"></param>
        public void Dimensions(List<String> userInput, out int rows, out int cols)
        {
            
            rows = 16;  //  These are the default values
            cols = 16;  //  and will only be changed if the user input is valid!

            int userRows, userCols;
            int numExpectedArgs = 3;
            int argCountDifference = CheckNumOfArgs(userInput.Count, numExpectedArgs);

            //  Check user has entered two parameters (one for rows and one for columns)
            //  Should be three (including the 0 index "--dimensions" argument)
            if (argCountDifference == 0)
            {
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
                    + " Please specify at most TWO positive integers between 4 and 48 inclusive.");
            }
        }

        /// <summary>
        /// If the user has entered --periodic, will change the behaviour type to be periodic.
        /// Does not take any parameters so they will be ignored and the user will be told as such
        /// </summary>
        /// <param name="userInput"></param>
        public static void PeriodicAndStep(List<string> userInput, out bool status)
        {
            int numExpectedArgs = 1;
            //  Because the periodic/step option was called, it will return true and ignore the parameters
            status = true;

            // Check for erroneous parameters following periodic/step and let the user know these are unecessary/ignored
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
        /// If --random is called, validates the user's input and changes the value of the random variable if OK
        /// </summary>
        /// <param name="userInput"></param>
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
        public static void OpenSeed(List<string> options, out string seedFile)
        {
            seedFile = "";
            return;
        }

        /// <summary>
        /// If the --generation argument is called, validates user input and changes number of generations run
        /// </summary>
        /// <param name="userInput"></param>
        public static void ChangeGenerations(List<string> userInput, out int gens)
        {
            gens = 50;   //  The default value;
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

        public static void ChangeMaxUPS(List<string> userInput, out float ups)
        {
            ups = 5;    //  The default, will only be changed with valid user input
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
