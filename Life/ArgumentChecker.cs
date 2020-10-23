using System;
using System.Collections.Generic;

namespace Life
{
    /// <summary>
    /// Contains a collection of methods that process the command line arguments.
    /// Does not do any validity checks, simply grabs valid options and any parameters that follow them and groups
    /// them into lists.
    /// </summary>
    /// <author>
    /// Tremaine Stroebel
    /// </author>
    /// <date>
    /// October 2020
    /// </date>
    class ArgumentChecker
    {
        // Add errors to this list to be written to console nicely
        private static readonly List<string> errors = new List<string>();

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

                // Check if there were any errors and print them to the console
                if (errors.Count > 0)
                {
                    Logging.PrintMessage(warning, FormatArgErrors(errors), ConsoleColor.Red);
                }

                // If the user provided arguments AND at least one argument was a valid option
                if (userInput.Count != 0)
                {
                    Logging.PrintMessage("\nValid arguments detected! Processing parameters...", ConsoleColor.Green);
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

        /// <summary>
        /// Checks if the provided string is an option (begins with "--")
        /// </summary>
        /// <param name="input">The argument to be checked</param>
        /// <returns>
        /// True if it is an option, false if not
        /// </returns>
        private static bool IsOption(string input)
        {
            return input.StartsWith("--");
        }

        /// <summary>
        /// Checks the options the user has entered and confirms they are valid. If they are, it groups the options 
        /// with any parameters that follow. Does NOT validate parameters.
        /// </summary>
        /// <param name="args">The string aray of args to be parsed</param>
        /// <returns>
        /// A list of lists, where each sub-list begins with an option followed by
        /// parameters the user entered.
        /// </returns>
        private static List<List<string>> ParseArguments(string[] args)
        {
            List<List<string>> userArguments = new List<List<string>>();

            for (int i = 0; i < args.Length; i++)
            {
                string currentArg = args[i];

                // Check if the argument is a valid option
                if (!Options.attributes.Contains(currentArg))
                {
                    errors.Add(currentArg);
                }
                else
                {
                    // Start a new list with the first entry being the valid option
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
        /// Formats and groups invalid options with corresponding parameters
        /// </summary>
        /// <param name="errors">The list of errors</param>
        /// <returns>A formatted list of errors</returns>
        private static List<string> FormatArgErrors(List<string> errors)
        {
            List<string> formattedList = new List<string>();
            int counter = 0;
            string currentItem;
            string previousItem = errors[0];
            string listEntry = "";

            for (int i = counter; i < errors.Count; i++)
            {
                currentItem = errors[i];
                bool currentIsOption = ArgumentChecker.IsOption(currentItem);
                bool previousIsOption = ArgumentChecker.IsOption(previousItem);

                if (currentIsOption)
                {
                    if (i != 0)
                    {
                        if (!previousIsOption)
                        {
                            listEntry += " ]";
                            formattedList.Add(listEntry);
                        }
                    }
                    listEntry = currentItem;
                }
                else
                {
                    if (i == 0)
                    {
                        listEntry += $"[ {currentItem}";
                    }
                    else if (previousIsOption)
                    {
                        listEntry += $": [ {currentItem}";
                    }
                    else
                    {
                        listEntry += $", {currentItem}";
                    }
                }
                previousItem = currentItem;
            }
            if (!ArgumentChecker.IsOption(previousItem))
            {
                listEntry += " ]";
                formattedList.Add(listEntry);
            }
            else
            {
                formattedList.Add(listEntry);
            }

            return formattedList;
        }
    }
}
