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
            if (args.Length == 0)
            {
                Console.WriteLine("You have not provided any arguments, reverting to defaults!");
                return;
            }
            else
            {
                List<List<string>> userArgs = ParseArgs(args);

                CheckArgs(userArgs);

                /*for (int i = 0; i < userArgs.Count; i++)
                {
                    for (int j = 0; j < userArgs[i].Count; j++)
                    {
                        Console.WriteLine(userArgs[i][j]);
                    }
                    Console.WriteLine("<------------>");
                }*/

            }

        }

        /// <summary>
        /// Parses a list of strings to group options (beginning with "--") with their parameters
        /// as entered by the user
        /// </summary>
        /// <param name="args"></param>
        /// <returns>
        /// A string type list of lists. Each sublist comprises of at least an option
        /// and any parameters if entered.
        /// </returns>
        public static List<List<string>> ParseArgs(string[] args)
        {

            List<List<string>> arguments = new List<List<string>>();
            
            bool atLeastOneOption = false;  //  Set to true if there is at least one seemingly valid option
            int firstValidOption = 0;       //  Set to the index of the first seemingly valid option
            int counter = 0;                //  Counter for while loop
           
            while (!atLeastOneOption)
            {
                if (!args[counter].StartsWith("--"))
                {
                    Console.WriteLine($"Argument {args[counter]} has been ignored: ");
                    Console.WriteLine($"\t- Precede all arguments with '--', e.g. '--{args[counter]}' followed by the parameters.");
                    Console.WriteLine("\t- Refer to README.md for all valid arguments and more details instructions.");
                }
                else
                {
                    atLeastOneOption = true; // We now know there is at least one valid argument we can use
                    firstValidOption = counter;
                    break;
                }
                
                counter++;
                //  If we reach the end of the array and there haven't been any valid options, return the empty list and tell the user
                //  the defaults will be used
                if (counter == args.Length)
                {
                    Console.WriteLine("\nThere are no valid arguments, reverting to default values for this Game of Life!");
                    return arguments;
                }
            }

            //  Start grouping options and parameters into individual lists, starting from the first valid option
            for (int i = firstValidOption; i < args.Length; i++)
            {
                List<string> optsAndParams = new List<string>();
                int j = i + 1;
                //  Add the option to the first index of the new list
                optsAndParams.Add(args[i]);
                if (j < args.Length)
                {
                    //  Keep adding parameters to the new list until we reach the next option OR the end of the args array
                    while ((j < args.Length) && (!args[j].StartsWith("--")))
                    {
                        optsAndParams.Add(args[j]);
                        //  Set i to j so the loop counts from the current index rather than the index of the most recent option
                        i = j;
                        j++;
                    }
                }
                //  Once we have reached the next option or the end of the args array, add the list to the arguments list
                arguments.Add(optsAndParams);
            }

            return arguments;
        }

        /// <summary>
        /// Checks each sublist of options and parameters and calls the relevant methods for those
        /// </summary>
        /// <param name="args"></param>
        public static void CheckArgs(List<List<string>> args)
        {
            args.ForEach(delegate (List<string> options)
            {
                switch (options[0])
                {
                    case "--dimensions":
                        Console.WriteLine($"1 - {options[0]}");
                        Settings.Dimensions(options);
                        break;
                    case "--periodic":
                        Console.WriteLine($"2 - {options[0]}");
                        Settings.Periodic(options);
                        break;
                    case "--random":
                        Console.WriteLine($"3 - {options[0]}");
                        Settings.RandomFactor(options);
                        break;
                    case "--seed":
                        Console.WriteLine($"4 - {options[0]}");
                        Settings.OpenSeed(options);
                        break;
                    case "--generations":
                        Console.WriteLine($"5 - {options[0]}");
                        Settings.ChangeGenerations(options);
                        break;
                    case "--max-update":
                        Console.WriteLine($"6 - {options[0]}");
                        break;
                    case "--step":
                        Console.WriteLine($"7 - {options[0]}");
                        break;
                    default:
                        Console.WriteLine($"Unknown argument '{options[0]}'.");
                        break;
                }
            });
        }
    }
}
