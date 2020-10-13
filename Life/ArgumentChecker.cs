using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Life
{
    /// <summary>
    /// Contains a collection of methods that process the command line arguments.
    /// Does not do any validity checks, simply grabs valid options and any parameters that follow them and groups
    /// them into lists. I've made this program VERY forgiving for the user for the most part in terms of its
    /// ability to carry on in the face of invalid options.
    /// </summary>
    /// <author>
    /// Tremaine Stroebel
    /// </author>
    /// <date>
    /// October 2020
    /// </date>
    class ArgumentChecker
    {
        private static readonly List<string> errors = new List<string>();

        public static bool IsOption(string input)
        {
            return input.StartsWith("--");
        }

        /// <summary>
        /// Checks the options the user has entered and confirms they are valid. If they are, it groups the options 
        /// with any parameters that follow. Does NOT validate parameters.
        /// </summary>
        /// <param name="args">The string aray of args to be parsed</param>
        /// <param name="firstUserOption">The index at which the first --option appears</param>
        /// <returns>A list of lists, where each sub-list begins with an option followed by
        /// parameters the user entered.
        /// </returns>
        private static List<List<string>> ParseArguments(string[] args)
        {
            List<List<string>> userArguments = new List<List<string>>();
            // Start grouping options and parameters into individual lists, starting from the first valid option
            for (int i = 0; i < args.Length; i++)
            {
                string currentArg = args[i];
                // If the option is not in the list of valid options, print a warning for the user followed by the same
                // for any parameters that may follow it
                if (!Options.attributes.Contains(currentArg))
                {
                    errors.Add(currentArg);
                }
                else
                {
                    // Sub-list of an option with any relevant parameters
                    List<string> optsAndParams = new List<string> { currentArg };
                    int nextArgument = i + 1;
                    if (nextArgument < args.Length)
                    {
                        // Keep adding parameters to the list until we reach the next option/the end of the args array
                        while ((nextArgument < args.Length) && (!IsOption(args[nextArgument])))
                        {
                            optsAndParams.Add(args[nextArgument]);
                            // Set i to nextArgument so the outer for loop continues from the next option
                            i = nextArgument;
                            nextArgument++;
                        }
                    }
                    userArguments.Add(optsAndParams);
                }
            }
            return userArguments;
        }

        /// <summary>
        /// Takes the array of string arguments and checks them through the CheckForArguments and ParseArguments
        /// methods.
        /// </summary>
        /// <param name="args">The string array of args entered into the console</param>
        /// <returns>
        /// An instance of the Settings class to use for a new Game of Life
        /// </returns>
        public static Settings GenerateGameSettings(string[] args)
        {
            Settings gameSettings;
            if (args.Length == 0)
            {
                Console.WriteLine("\nNo arguments provided.");
                gameSettings = new Settings();
            }
            else
            {
                List<List<string>> userInput = ParseArguments(args);
                string warning = "\nWARNING! The following arguments are invalid:";

                if (errors.Count > 0)
                {
                    Logging.PrintMessage(warning, Logging.FormatArgErrors(errors), ConsoleColor.Red);
                }

                // If the user provided arguments AND at least one argument was a valid option
                if (userInput.Count != 0)
                {
                    Console.WriteLine("\nValid arguments detected, processing parameters...");
                    gameSettings = Options.CheckAndSet(userInput);
                }
                else
                {
                    gameSettings = new Settings();
                    Console.WriteLine("\nNo valid arguments provided...");
                }
            }
            return gameSettings;
        }
    }
}
