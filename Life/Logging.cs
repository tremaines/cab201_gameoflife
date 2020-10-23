using System;
using System.Collections.Generic;

namespace Life
{
    /// <summary>
    /// A collection of methods that make displaying certain messages a bit easier.
    /// Inspired by the Part 1 Solution posted by the CAB201 teaching team.
    /// </summary>
    /// <author>
    /// Tremaine Stroebel
    /// </author>
    /// <date>
    /// October 2020
    /// </date>
    class Logging
    {
        /// <summary>
        /// Prints a message in yellow font with a WARNING! pre-fix. Can also include a sub-message that is printed
        /// below.
        /// </summary>
        /// <param name="msg">The message to be printed</param>
        /// <param name="subMsg">Optional sub-message</param>
        public static void GenericWarning(string msg, string subMsg = null)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"WARNING! {msg}");
            Console.WriteLine(subMsg);
            Console.ForegroundColor = defaultColor;
        }

        /// <summary>
        /// Can optionally use this to format a sub-message
        /// </summary>
        /// <param name="msg">The message to be formatted</param>
        /// <returns></returns>
        public static string SubMessageFormatter(string msg)
        {
            return $"  > {msg}";
        }

        /// <summary>
        /// Prints a primary message followed by a sublist formatted with the SubMessageFormatter() method.
        /// </summary>
        /// <param name="msg">The primary message</param>
        /// <param name="msgList">A list of messages to be printed using the SubMessageFormatter() method</param>
        /// <param name="textColour">The font colour of the message</param>
        public static void PrintMessage(string msg, List<string> msgList, ConsoleColor textColour)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = textColour;
            Console.WriteLine(msg);

            foreach (string item in msgList)
            {
                Console.WriteLine(SubMessageFormatter(item));
            }

            Console.ForegroundColor = defaultColor;
        }

        /// <summary>
        /// Overloaded PrintMessage() to print a message with a specific font colour
        /// </summary>
        /// <param name="msg">The message to be printed</param>
        /// <param name="textColour">The font colour of the message</param>
        public static void PrintMessage(string msg, ConsoleColor textColour)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = textColour;
            Console.WriteLine(msg);
            Console.ForegroundColor = defaultColor;
        }
    }
}
