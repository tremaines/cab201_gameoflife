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
        private Logging log;

        public ArgumentChecker(string[] args) 
        {
            log = new Logging();
        }

        /// <summary>
        /// Parses the console args to find the index of the first seemingly valid option (arg beginning with '--').
        /// </summary>
        /// <param name="args">The string array of args to be checked</param>
        /// <returns>
        /// Returns the index of the first seemingly valid option, otherwise returns -1 if no arguments or no seemingly
        /// valid options
        /// </returns>
        private int CheckForArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("--"))
                {
                    // First argument needs to be an option (begin with --) so warn the user if that's not the case
                    log.AddItem(log.ErrorMsg, args[i]);
                }
                else
                {
                    return i;
                }
            }
            return -1;
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
        private List<List<string>> ParseArguments(string[] args, int firstUserOption)
        {
            List<List<string>> userArguments = new List<List<string>>();
            // Start grouping options and parameters into individual lists, starting from the first valid option
            for (int i = firstUserOption; i < args.Length; i++)
            {
                // If the option is not in the list of valid options, print a warning for the user followed by the same
                // for any parameters that may follow it
                if (!Settings.attributes.Contains(args[i]))
                {
                    log.AddItem(log.ErrorMsg, args[i]);
                }
                else
                {
                    // Sub-list of an option with any relevant parameters
                    List<string> optsAndParams = new List<string> { args[i] };
                    int nextArgument = i + 1;
                    if (nextArgument < args.Length)
                    {
                        // Keep adding parameters to the list until we reach the next option/the end of the args array
                        while ((nextArgument < args.Length) && (!args[nextArgument].StartsWith("--")))
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
        public Settings GenerateGameSettings(string[] args)
        {
            int firstOption = CheckForArguments(args);
            List<List<string>> userInput = ParseArguments(args, firstOption);
            Settings gameSettings;
            string warning = "WARNING! The following arguments are invalid:";
            log.PrintMessage(warning, log.ErrorMsg, ConsoleColor.Yellow);


            // If the user provided arguments AND at least one argument was a valid option
            if (firstOption != -1 && userInput.Count != 0)
            {
                gameSettings = new Settings(userInput);
                Console.WriteLine("\nValid arguments detected, processing parameters...\n");
            }
            else
            {
                gameSettings = new Settings();
                Console.WriteLine("\nNo valid arguments provided...\n");
            }
            return gameSettings;
        }
    }
}
