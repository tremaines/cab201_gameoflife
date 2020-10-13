using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Life
{
    class Options
    {
        //  Public list of all attributes available to be altered by user
        public static readonly List<string> attributes = new List<string> 
        { "--dimensions", "--periodic", "--random", "--seed", "--generations", "--max-update", "--step", "--neighbour",
          "--survival", "--birth", "--memory", "--output", "--ghost"
        };

        private static List<string> errors = new List<string>();
        
        public static Settings CheckAndSet(List<List<string>> userArgs)
        {
            Settings gameSettings = new Settings();
            foreach (List<string> option in userArgs)
            {
                try
                {
                    switch (option[0])
                    {
                        case "--dimensions":
                            gameSettings.Rows = (Dimensions(option)[0]);
                            gameSettings.Columns = (Dimensions(option)[1]);
                            break;
                        case "--periodic":
                            gameSettings.Periodic = true;
                            CheckBooleanOptions(option);
                            break;
                        case "--random":
                            gameSettings.Random = RandomFactor(option);
                            break;
                        case "--seed":
                            gameSettings.SeedFile = OpenSeed(option);
                            break;
                        case "--generations":
                            gameSettings.Generations = ChangeGenerations(option);
                            break;
                        case "--max-update":
                            gameSettings.UpdateRate = ChangeMaxUPS(option);
                            break;
                        case "--step":
                            gameSettings.StepMode = true;
                            CheckBooleanOptions(option);
                            break;
                    }
                }
                catch (Exception e)
                {
                    errors.Add(e.Message);
                    continue;
                }
            }

            if (errors.Count > 0)
            {
                Logging.PrintMessage("\nWARNING!", errors, ConsoleColor.Yellow);
            }

            return gameSettings;
        }

        private static void CheckInts(string param, string input, out int value)
        {
            if (!Int32.TryParse(input, out value))
            {
                throw new ArgumentException($"{param} value '{input}' is not a valid whole number.");
            }
        }

        private static void CheckDoubles(string param, string input, out double value)
        {
            if (!double.TryParse(input, out value))
            {
                throw new ArgumentException($"{param} value '{input}' is not a valid floating point number.");
            }
        }
        
        
        private static int[] Dimensions(List<String> userInput)
        {
            int[] rowsAndCols = new int[2];
            int expectedParams = 2;
            int difference = expectedParams - (userInput.Count - 1);

            //  Check user has entered two parameters (one for rows and one for columns)
            if (difference == 0)
            {
                CheckInts("Row", userInput[1], out rowsAndCols[0]);
                CheckInts("Column", userInput[2], out rowsAndCols[1]);
                return rowsAndCols;
            }
            else
            {
                throw new ParamCountException(userInput[0], expectedParams, userInput.Count-1);
            }
        }

        private static void CheckBooleanOptions(List<string> userInput)
        {
            int expectedParams = 0;
            int difference = expectedParams - (userInput.Count - 1);

            // If the user has provided parameters, let the user know they have been ignored
            if (difference != 0)
            {
                string ignoreParams = ($"{userInput[0]} enabled but the following parameters have been ignored: ");
                for (int i = 1; i < userInput.Count; i++)
                {
                    // If we are at the last parameter, add to the errorMsgs list
                    if (i + 1 == userInput.Count)
                    {
                        ignoreParams += ($"'{userInput[i]}'");
                    }
                    else
                    {
                        ignoreParams += ($"'{userInput[i]}', ");
                    }
                }
                errors.Add(ignoreParams);
            }
        }

        private static double RandomFactor(List<string> userInput)
        {
            int expectedParams = 1;
            int difference = expectedParams - (userInput.Count - 1);

            if (difference == 0)
            {
                CheckDoubles("Random factor", userInput[1], out double randomValue);
                return randomValue;
            }
            else
            {
                throw new ParamCountException(userInput[0], expectedParams, userInput.Count - 1);
            }
        }

        private static string OpenSeed(List<string> userInput)
        {
            string file;
            string extension = ".seed";
            int expectedParams = 1;
            int difference = expectedParams - (userInput.Count - 1);

            if (difference == 0)
            {
                file = userInput[1];
                if (!(File.Exists(file) && (Path.GetExtension(file) == extension)))
                {
                    throw new ArgumentException("The seed file provided is not valid. " +
                        "Please ensure you type the path or file name correctly, and that the file ends in '.seed'.");
                }
                return file;
            }
            else
            {
                throw new ParamCountException(userInput[0], expectedParams, userInput.Count - 1);
            }
        }

        private static int ChangeGenerations(List<string> userInput)
        {
            int expectedParams = 1;
            int difference = expectedParams - (userInput.Count - 1);

            if (difference == 0)
            {
                CheckInts("Generations", userInput[1], out int generationValue);
                return generationValue;
            }
            else
            {
                throw new ParamCountException(userInput[0], expectedParams, userInput.Count - 1); ;
            }
        }

        private static double ChangeMaxUPS(List<string> userInput)
        {
            int expectedParams = 1;
            int difference = expectedParams - (userInput.Count - 1);

            if (difference == 0)
            {
                CheckDoubles("Update rate", userInput[1], out double upsValue);
                return upsValue;
            }
            else
            {
                throw new ParamCountException(userInput[0], expectedParams, userInput.Count - 1);
            }
        }
    }
}
