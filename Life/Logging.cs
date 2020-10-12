using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Life
{
    class Logging
    {

        private List<string> errorMsg;
        private List<string> successMsg;
        private ConsoleColor defaultColour;

        public List<string> ErrorMsg => errorMsg;
        public List<string> SuccessMsg => successMsg;
        public ConsoleColor DefaultColor => defaultColour;

        public Logging()
        {
            errorMsg = new List<string>();
            successMsg = new List<string>();
            defaultColour = ConsoleColor.White;
        }

        /// <summary>
        /// Adds an item (e.g argument, option, setting, etc.) to error or success list
        /// </summary>
        /// <param name="msgList">The list to add to
        /// </param>
        /// <param name="item">The item to be added
        /// </param>
        public void AddItem(List<string> msgList, string item)
        {
            msgList.Add(item);
        }


        public void PrintMessage(string msg, List<string> msgList, ConsoleColor textColour)
        {
            Console.ForegroundColor = textColour;
            Console.WriteLine(msg);
            string currentItem;
            string previousItem = msgList[0];
            int counter = 0;

            if ((!msgList[counter].StartsWith("--")))
            {
                Console.Write("  > [");
                Console.Write(msgList[counter]);
                counter = 1;
            }

            for (int i = counter; i < msgList.Count; i++)
            {
                currentItem = msgList[i];

                if (currentItem.StartsWith("--"))
                {
                    if (i != 0)
                    {
                        if (!previousItem.StartsWith("--"))
                        {
                            Console.Write("]");
                        }
                        Console.WriteLine();
                    }
                    Console.Write($"  > {currentItem}");
                }

                if (!currentItem.StartsWith("--"))
                {
                    if (previousItem.StartsWith("--"))
                    {
                        Console.Write($": [{currentItem}");
                    }
                    else
                    {
                        Console.Write($", {currentItem}");
                    }
                }

                previousItem = currentItem;
            }
            if (!previousItem.StartsWith("--"))
            {
                Console.WriteLine("]");
            }
            Console.ForegroundColor = defaultColour;
        }
    }
}
