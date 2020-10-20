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
            Settings settings = new Settings();
            int smallestDimension = settings.Rows > settings.Columns ? settings.Columns : settings.Rows;
            bool userSetNeighbour = false;
            bool userSetRules = false;
            foreach (List<string> option in userArgs)
            {
                try
                {
                    switch (option[0])
                    {
                        case "--dimensions":
                            int[] dimensions = Dimensions(option);
                            settings.Rows = dimensions[0];
                            settings.Columns = dimensions[1];
                            smallestDimension = settings.Rows > settings.Columns ? settings.Columns : settings.Rows;
                            break;
                        case "--periodic":
                            settings.Periodic = true;
                            CheckBooleanOptions(option);
                            break;
                        case "--random":
                            settings.Random = CheckSingleNumberArgs(option, Settings.RAND_DEFAULT, "Random");
                            break;
                        case "--seed":
                            settings.SeedFile = OpenSeed(option);
                            break;
                        case "--generations":
                            settings.Generations = CheckSingleNumberArgs(option, Settings.GEN_DEFAULT, "Generations");
                            break;
                        case "--max-update":
                            settings.UpdateRate = CheckSingleNumberArgs(option, Settings.UPDATE_DEFAULT, "Update rate");
                            break;
                        case "--step":
                            settings.StepMode = true;
                            CheckBooleanOptions(option);
                            break;
                        case "--neighbour":
                            string type;
                            int order;
                            bool centre;
                            NeighbourhoodSettings(option, out type, out order, out centre);
                            settings.Neighbourhood = type;
                            settings.Centre = centre;
                            settings.Order = order;
                            userSetNeighbour = true;
                            break;
                        case "--birth":
                            settings.Birth = ChangeRules(option, out List<string> birthText);
                            settings.BirthText = birthText;
                            userSetRules = true;
                            break;
                        case "--survival":
                            settings.Survival = ChangeRules(option, out List<string> survivalText);
                            settings.SurvivalText = survivalText;
                            userSetRules = true;
                            break;
                        case "--memory":
                            settings.Memory = CheckSingleNumberArgs(option, Settings.MEM_DEFAULT, "Memory");
                            break;
                        case "--output":
                            settings.OutputFile = CheckFilePath(option);
                            break;
                        case "--ghost":
                            settings.Ghost = true;
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

            // Check order, survival and birth are within their respective boundaries
            // That is: order must be less than half the smallest dimension
            // Survival rules
            if (userSetNeighbour && VerifyOrder(settings.Order, smallestDimension))
            {
                settings.Order = Settings.ORDER_MIN;
                errors.Add("Neighbourhood order must be less than half the smallest dimension.");
            }

            if (userSetRules)
            {
                if (VerifyRules(settings.Birth, settings.Neighbourhood, settings.Order, settings.Centre))
                {
                    settings.Birth = Settings.BIRTH;
                    settings.BirthText = Settings.BIRTH_TEXT;
                    errors.Add("Numbers specified for birth rules must be less than or equal to the number of" +
                        " neighbours a cell has.");
                }

                if (VerifyRules(settings.Survival, settings.Neighbourhood, settings.Order, settings.Centre))
                {
                    settings.Survival = Settings.SURVIVAL;
                    settings.SurvivalText = Settings.SURVIVAL_TEXT;
                    errors.Add("Numbers specified for survival rules must be less than or equal to the number of" +
                        " neighbours a cell has.");
                }
            }

            if (errors.Count > 0)
            {
                Logging.PrintMessage("\nWARNING!", errors, ConsoleColor.Yellow);
                Logging.PrintMessage("Invalid values have been reset to defaults.", ConsoleColor.Yellow);
            }

            return settings;
        }

        private static void CheckInts(string param, string input, int defaultValue, out int value)
        {
            if (!Int32.TryParse(input, out value))
            {
                errors.Add($"{param} value '{input}' is not a valid whole number.");
                value = defaultValue;
            }
        }

        private static void CheckDoubles(string param, string input, double defaultValue, out double value)
        {
            if (!double.TryParse(input, out value))
            {
                errors.Add($"{param} value '{input}' is not a valid floating point number.");
                value = defaultValue;
            }
        }

        private static bool VerifyOrder(int order, int dimension)
        {
            return order > ((double)dimension / 2.0);
        }
        
        private static bool VerifyRules(List<int> rules, string neighbourhood, int order, bool centre)
        {
            int maxVal = 0;
            int numOfNeighbours = (int)(Math.Pow(order * 2 + 1, 2) - 1);
            
            if (neighbourhood != "moore")
            {
                numOfNeighbours /= 2;
            }
            
            foreach (int number in rules)
            {
                maxVal = number > maxVal ? number : maxVal;
            }

            if (!centre)
            {
                return maxVal > numOfNeighbours;
            }
            else
            {
                return maxVal > numOfNeighbours + 1;
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
                CheckInts("Row", userInput[1], Settings.DIMENSIONS_DEFAULT, out rowsAndCols[0]);
                CheckInts("Column", userInput[2], Settings.DIMENSIONS_DEFAULT, out rowsAndCols[1]);
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

        private static double CheckSingleNumberArgs(List<string> userInput, double defaultValue, string argument)
        {
            int expectedParams = 1;
            int difference = expectedParams - (userInput.Count - 1);

            if (difference == 0)
            {
                CheckDoubles(argument, userInput[1], defaultValue, out double userValue);
                return userValue;
            }
            else
            {
                throw new ParamCountException(userInput[0], expectedParams, userInput.Count - 1);
            }
        }

        private static int CheckSingleNumberArgs(List<string> userInput, int defaultValue, string argument)
        {
            int expectedParams = 1;
            int difference = expectedParams - (userInput.Count - 1);

            if (difference == 0)
            {
                CheckInts(userInput[0], userInput[1], defaultValue, out int userValue);
                return userValue;
            }
            else
            {
                throw new ParamCountException(userInput[0], expectedParams, userInput.Count - 1); ;
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
                    throw new ParamValueException("The seed file provided is not valid. " +
                        "Please ensure you type the path or file name correctly, and that the file ends in '.seed'.");
                }
                return file;
            }
            else
            {
                throw new ParamCountException(userInput[0], expectedParams, userInput.Count - 1);
            }
        }

        private static string CheckFilePath(List<string> userInput)
        {
            string file;
            string extension = ".seed";
            string path;
            int expectedParams = 1;
            int difference = expectedParams - (userInput.Count - 1);

            if (difference == 0)
            {
                file = userInput[1];
                path = Path.GetDirectoryName(file);
                if (Path.GetExtension(file) != extension)
                {
                    throw new ParamValueException("The output file must end in '.seed'.");
                }
                if (path != "" && !Directory.Exists(path))
                {
                    throw new ParamValueException("The path to the specified output file is not valid.");
                }
                return file;
            }
            else
            {
                throw new ParamCountException(userInput[0], expectedParams, userInput.Count - 1);
            }
        }

        private static void NeighbourhoodSettings(List<string> userInput, out string neighbourhood, out int order, 
            out bool centre)
        {
            int expectedParams = 3;
            int difference = expectedParams - (userInput.Count - 1);

            if (difference == 0)
            {
                neighbourhood = userInput[1].ToLower();
                CheckInts("Order", userInput[2], Settings.ORDER_MIN, out order);
                if (!bool.TryParse(userInput[3].ToLower(), out centre))
                {
                    centre = false;
                    errors.Add("Centre count must be set as either 'true' or 'false'.");
                }
            }
            else
            {
                throw new ParamCountException(userInput[0], expectedParams, userInput.Count - 1);
            }
        }

        private static List<int> ChangeRules(List<string> userInput, out List<string> rules)
        {
            rules = new List<string>();
            List<int> newRules = new List<int>();

            for (int i = 1; i < userInput.Count; i++)
            {
                if (int.TryParse(userInput[i], out int number))
                {
                    newRules.Add(number);
                }
                else
                {
                    string[] range = userInput[i].Split("...");
                    if (!(int.TryParse(range[0], out int startRange) && int.TryParse(range[1], out int endRange)))
                    {
                        throw new ParamValueException("Survival and birth rules must be whole numbers, using '...'" +
                            "to indicate a range");
                    }

                    for (int j = startRange; j <= endRange; j++)
                    {
                        newRules.Add(j);
                    }
                }
                rules.Add(userInput[i]);
            }

            return newRules;
        }
    }
}
