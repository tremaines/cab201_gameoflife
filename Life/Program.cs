using System;
using System.Collections.Generic;
using System.Diagnostics;
using Display;

namespace Life
{
    class Program
    {
        static void Main(string[] args)
        {
            int firstOption = CheckForArguments(args);
            List<List<string>> userInput;

            if (firstOption != -1 && ((userInput = ParseArguments(args, firstOption)).Count) != 0)
            {
                Settings gameSettings = new Settings(userInput);
            }
            else
            {
                Settings gameSettings = new Settings();
            }
        }

        /// <summary>
        /// Parses a list of strings to group options (beginning with "--") with their parameters
        /// as entered by the user. This doesn't do any validation, just checks if there are any options.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>
        /// Returns the index of the first seemingly valid option, otherwise returns -1 if no arguments or no seemingly valid options
        /// </returns>
        public static int CheckForArguments(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You have not provided any arguments, reverting to defaults!");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (!args[i].StartsWith("--"))
                    {
                        Console.WriteLine($"Parameter {args[i]} has been ignored as it was not preceded by an option...");
                    }
                    else
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Checks the options the user has entered and confirms they are valid. If they are,
        /// it groups the options with any parameters that follow. Does NOT validate parameters.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="firstUserOption"></param>
        /// <returns>A list of lists, where each sub-list begins with an option followed by
        /// parameters the user entered.
        /// </returns>
        public static List<List<string>> ParseArguments(string[] args, int firstUserOption)
        {
            List<List<string>> userArguments = new List<List<string>>();
            //  Start grouping options and parameters into individual lists, starting from the first valid option
            for (int i = firstUserOption; i < args.Length; i++)
            {
                //  Checks if the option is in the list of allowed options
                if (!Settings.attributes.Contains(args[i]))
                {
                    Console.WriteLine($"{args[i]} is not a valid option.");
                }
                else
                {
                    //  Sub-list of an option with any relevant parameters
                    List<string> optsAndParams = new List<string> { args[i] };
                    int nextArgument = i + 1;
                    if (nextArgument < args.Length)
                    {
                        //  Keep adding parameters to the new list until we reach the next option OR the end of the args array
                        while ((nextArgument < args.Length) && (!args[nextArgument].StartsWith("--")))
                        {
                            optsAndParams.Add(args[nextArgument]);
                            //  Set i to nextArgument so the outer for loop continues from the next option
                            i = nextArgument;
                            nextArgument++;
                        }
                    }
                    //  Once we have reached the next option or the end of the args array, add the list to the userArguments list
                    userArguments.Add(optsAndParams);
                }
            }
            return userArguments;
        }
    }
}
