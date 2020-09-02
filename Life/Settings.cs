using System;
using System.Collections.Generic;
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
        //  Initialise game attributes with default values
        private static int rows = 16, columns = 16;
        private static bool periodic = false;
        private static float random = 0.5F;
        private static string seedFile = "";
        private static int generations = 50;
        private static float updateRate = 5;
        private static bool stepMode = false;


        private static bool CheckNumOfArgs(List<string> args, int numExpected)
        {
            string argument = args[0];
            bool rightNumOfArgs = false;
            
            if (args.Count != numExpected)
            {
                Console.WriteLine($"The argument '{argument}' requires {numExpected - 1} parameters.");
            }
            else
            {
                rightNumOfArgs = true;
            }

            return rightNumOfArgs;
        }

        /// <summary>
        /// Changes dimensions of board based on valid user input
        /// </summary>
        /// <param name="userInput"></param>
        public static void Dimensions(List<String> userInput)
        {
            int userRows;
            int userCols;
            int numExpectedArgs = 3;
            bool changeDefaults = false;

            //  Check user has entered two parameters (one for rows and one for columns)
            //  Should be three (including the 0 index "--dimensions" argument)
            if (CheckNumOfArgs(userInput, numExpectedArgs))
            {
                if ((Int32.TryParse(userInput[1], out userRows) && Int32.TryParse(userInput[2], out userCols))
                    && ((userRows >= 4 && userRows <= 48) && (userCols >= 4 && userCols <= 48)))
                {
                    rows = userRows;
                    columns = userCols;
                    changeDefaults = true;
                }
                else
                {
                    Console.WriteLine("--dimensions requires two positive integers between 4 and 48 inclusive.");
                }
            }
            
            if (!changeDefaults)
            {
                Console.WriteLine("Using default dimensions of 16 rows and 16 columns due to invalid user input.");
            }
        }

        /// <summary>
        /// If the user has entered --periodic, will change the behaviour type to be periodic.
        /// Does not take any parameters so they will be ignored and the user will be told as such
        /// </summary>
        /// <param name="userInput"></param>
        public static void Periodic(List<string> userInput)
        {
            if (userInput.Count > 1)
            {
                Console.Write("Unknow parameter(s) for --periodic: ");
                for (int i = 1; i < userInput.Count; i++)
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
                Console.WriteLine("--periodic does not require parameters");
            }

            periodic = true;
        }

        /// <summary>
        /// If --random is called, validates the user's input and changes the value of the random variable if OK
        /// </summary>
        /// <param name="userInput"></param>
        public static void RandomFactor(List<string> userInput)
        {
            float userRand;
            int numExpectedArgs = 2;

            if (CheckNumOfArgs(userInput, numExpectedArgs))
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
                Console.WriteLine("Using default value of 50%");
            }
        }

        /// <summary>
        /// Open and read seed file ****************TO DO**********TO DO***********TO DO*********************
        /// </summary>
        /// <param name="options"></param>
        public static void OpenSeed(List<string> options)
        {
            return;
        }

        /// <summary>
        /// If the --generation argument is called, validates user input and changes number of generations run
        /// </summary>
        /// <param name="userInput"></param>
        public static void ChangeGenerations(List<string> userInput)
        {
            int numOfExpectedArgs = 2;
            int userGens;

            if (CheckNumOfArgs(userInput, numOfExpectedArgs))
            {
                if (Int32.TryParse(userInput[1], out userGens) && userGens > 0)
                {
                    generations = userGens;
                }
                else
                {
                    Console.WriteLine("Number of generations must be a positive, non-zero integer.");
                }
            }
            else
            {
                Console.WriteLine("Using default value of 50");
            }
                
        }

        public static void ChangeMaxUPS(List<string> userInput)
        {
            int numOfExpectedArgs = 2;
            float userUPS;

            if (CheckNumOfArgs(userInput, numOfExpectedArgs))
            {
                if (float.TryParse(userInput[1], out userUPS) && (userUPS >= 1 && userUPS <= 30))
                {
                    updateRate = userUPS;
                }
                else
                {
                    Console.WriteLine("Number of updates per second must be a float between 1 and 30 inclusive.");
                }
            }
            else
            {
                Console.WriteLine("Using default value of 5 updates per second.");
            }
        }
    }
}
