using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Life
{
    class Logging
    {

        public static void GenericWarning(string msg, string subMsg = null)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"WARNING! {msg}");
            Console.WriteLine(subMsg);
            Console.ForegroundColor = defaultColor;
        }

        public static string SubMessageFormatter(string msg)
        {
            return $"  > {msg}";
        }
 
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

        public static List<string> FormatArgErrors(List<string> errors)
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
