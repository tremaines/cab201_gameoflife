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
                                                                            "--step",
                                                                            "--neighbour",
                                                                            "--survival",
                                                                            "--birth",
                                                                            "--memory",
                                                                            "--output",
                                                                            "--ghost"
                                                                          };

        //  Settings fields, set to default values
        private int rows = 16; 
        private int columns = 16;
        private bool periodic = false;
        private double random = 0.5F;
        private string seedFile = "None";
        private int generations = 50;
        private float updateRate = 5;
        private bool stepMode = false;
        private List<string> errorMsgs = new List<string>();
        private List<string> successMsgs = new List<string>();

        //  Settings properties

        public int Rows => rows;
        public int Columns => columns;
        public bool Periodic => periodic;
        public double Random => random;
        public string SeedFile => seedFile;
        public int Generations => generations;
        public float UpdateRate => updateRate;
        public bool StepMode => stepMode;
        public List<string> ErrorMsgs => errorMsgs;
        public List<string> SuccessMsgs => successMsgs;

        /// <summary>
        /// Default constructor, called when user doesn't change settings or doesn't enter any valid --options.
        /// </summary>
        public Settings() { }

        /// <summary>
        /// Constructor called when the user enters at least one valid --option.
        /// It traverses the list of lists and calls the relevant validity-check method for any
        /// options entered by the user.
        /// </summary>
        /// <param name="userArgs">List of lists containing options and parameters</param>
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
        /// Produces a generic error message if the arg count difference != 0
        /// </summary>
        /// <param name="option">The option argument</param>
        /// <param name="difference">The difference between args received and expected</param>
        /// <returns>
        /// A string to use print out. Can append more specific information depending on the option.
        /// </returns>
        private string ParamCountErrorMessage(string option, int difference)
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
        /// Validates the parameters entered by the user for --dimensions and changes the rows and columns attributes
        /// accordingly. If the user input is invalid, the default value is retained.
        /// </summary>
        /// <param name="userInput">List containing --dimension and user-entered parameters</param>
        /// <param name="rows">Variable to output num of rows to</param>
        /// <param name="columns">Variable to output num of columns to</param>
        private void Dimensions(List<String> userInput, out int rows, out int columns)
        {
            // Set rows and columns to default
            rows = this.rows;
            columns = this.columns;
            string defaultMsg = $"Using default: {rows} rows X {columns} columns";
            int userRows, userCols;
            int numExpectedArgs = 3;
            int argCountDifference = userInput.Count.CompareTo(numExpectedArgs);

            //  Check user has entered two parameters (one for rows and one for columns)
            if (argCountDifference == 0)
            {
                // If they have, validate those values are both ints between 4 and 48 inclusive
                if ((Int32.TryParse(userInput[1], out userRows) && Int32.TryParse(userInput[2], out userCols))
                    && ((userRows >= 4 && userRows <= 48) && (userCols >= 4 && userCols <= 48)))
                {
                    rows = userRows;
                    columns = userCols;
                    successMsgs.Add(userInput[0]);
                }
                else
                {
                    errorMsgs.Add("--dimensions requires two positive integers, " +
                        "both between 4 and 48 inclusive.\n    - " + defaultMsg);
                }
            }
            else
            {
                errorMsgs.Add(ParamCountErrorMessage(userInput[0], argCountDifference) 
                    + " Please specify exactly TWO positive integers between 4 and 48 inclusive.\n    - " + defaultMsg);
            }
        }

        /// <summary>
        /// If --periodic and/or --step are called, they will be set to true regardless of what parameters the user
        /// enters after them. This functions will check for any parameters and let the user know it doesn't know
        /// what to do with them.
        /// </summary>
        /// <param name="userInput">List containing --periodic or --step and any parameters</param>
        private void PeriodicAndStep(List<string> userInput, out bool status)
        {
            int numExpectedArgs = 1;
            int argCountDifference = userInput.Count.CompareTo(numExpectedArgs);
            status = true;
            successMsgs.Add(userInput[0]);

            // If the user has provided parameters, let the user know they have been ignored
            if (argCountDifference != 0)
            {
                string ignoreParams = ($"{userInput[0]} enabled but the following parameters have been ignored: ");
                for (int i = numExpectedArgs; i < userInput.Count; i++)
                {
                    // If we are at the last parameter, add to the errorMsgs list
                    if (i + 1 == userInput.Count)
                    {
                        ignoreParams += ($"'{userInput[i]}'");
                        errorMsgs.Add(ignoreParams);
                    }
                    else
                    {
                        ignoreParams += ($"'{userInput[i]}', ");
                    }
                }
            }
        }

        /// <summary>
        /// Validates the parameter for --random and changes the random attribute accordingly. Stays at the default 
        /// value if invalid user input.
        /// </summary>
        /// <param name="userInput">List containing --random and any parameters</param>
        /// <param name="random">Variable to output random setting to</param>
        private void RandomFactor(List<string> userInput, out double random)
        {
            random = this.random;  // Default value
            string defaultMsg = $"Using default: {random:P2}";
            double userRand;
            int numExpectedArgs = 2;
            int argCountDifference = userInput.Count.CompareTo(numExpectedArgs);

            if (argCountDifference == 0)
            {
                if ((double.TryParse(userInput[1], out userRand)) && (userRand >= 0 && userRand <= 1))
                {
                    random = userRand;
                    successMsgs.Add(userInput[0]);
                }
                else
                {
                    errorMsgs.Add("--random requires a floating point value between 0 and 1 inclusive." +
                        "\n    - " + defaultMsg);
                }
            }
            else
            {
                errorMsgs.Add(ParamCountErrorMessage(userInput[0], argCountDifference)
                    + " Please specify a single floating point value between 0 and 1 inclusive.\n    - " + defaultMsg);
            }
        }

        /// <summary>
        /// Confirms the seed file exists and that it ends in a '.seed' extension
        /// </summary>
        /// <param name="userInput">List containing --seed and any parameters</param>
        /// <param name="seedFile">Variable to output seed file to</param>
        private void OpenSeed(List<string> userInput, out string seedFile)
        {
            seedFile = this.seedFile;
            int numExpectedArgs = 2;
            string file;
            string extension = ".seed";
            int argCountDifference = userInput.Count.CompareTo(numExpectedArgs);

            if (argCountDifference == 0)
            {
                file = userInput[1];
                if (File.Exists(file) && (Path.GetExtension(file) == extension))
                {
                    seedFile = file;
                    successMsgs.Add(userInput[0]);
                }
                else
                {
                    errorMsgs.Add("The seed file provided is not valid. " +
                        "Please ensure you type the path or file name correctly, and that the file ends in '.seed'.");
                }
            }
            else
            {
                errorMsgs.Add(ParamCountErrorMessage(userInput[0], argCountDifference)
                   + " Please specify a single file name or path.");
            }
        }

        /// <summary>
        /// Validates the parameter for --generations and changes the generations attribute accordingly.
        /// Stays at the default value if user input is invalid.
        /// </summary>
        /// <param name="userInput">List containing --generations and any parameters</param>
        /// <param name="generations">Variable to output no. of generations to</param>
        private void ChangeGenerations(List<string> userInput, out int generations)
        {
            generations = this.generations;   // Default value
            string defaultMsg = $"Using default: {generations} generations.";
            int numExpectedArgs = 2;
            int userGens;
            int argCountDifference = userInput.Count.CompareTo(numExpectedArgs);

            if (argCountDifference == 0)
            {
                if (Int32.TryParse(userInput[1], out userGens) && userGens > 0)
                {
                    generations = userGens;
                    successMsgs.Add(userInput[0]);
                }
                else
                {
                    errorMsgs.Add("--generations must be a positive, non-zero integer.\n    - " + defaultMsg);
                }
            }
            else
            {
                errorMsgs.Add(ParamCountErrorMessage(userInput[0], argCountDifference)
                    + " Please specify a single positive, non-zero integer.\n    - " + defaultMsg);
            }   
        }

        /// <summary>
        /// Validates the parameter for --max-update and changes the updateRate attribute accordingly.
        /// Stays at the default value if user input is invalid.
        /// </summary>
        /// <param name="userInput">List containing --max-update and any parameters</param>
        /// <param name="updateRate">Variable to output update rate to</param>
        private void ChangeMaxUPS(List<string> userInput, out float updateRate)
        {
            updateRate = this.updateRate;
            string defaultMsg = $"Using default: {updateRate} generations / second.";
            int numExpectedArgs = 2;
            float userUPS;
            int argCountDifference = userInput.Count.CompareTo(numExpectedArgs);

            if (argCountDifference == 0)
            {
                if (float.TryParse(userInput[1], out userUPS) && (userUPS >= 1 && userUPS <= 30))
                {
                    updateRate = userUPS;
                    successMsgs.Add(userInput[0]);
                }
                else
                {
                    errorMsgs.Add("--max-update must be a floating point value between 1 and " +
                        "30 inclusive.\n    - " + defaultMsg);
                }
            }
            else
            {
                errorMsgs.Add(ParamCountErrorMessage(userInput[0], argCountDifference)
                    + " Please specify a single floating point value between 1 and 30 inclusive.\n    - " + defaultMsg);
            }
        }
    }
}
