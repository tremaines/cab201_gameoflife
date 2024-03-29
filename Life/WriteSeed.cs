﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Life
{
    /// <summary>
    /// Initialises an object with a filename and a DeadOrAlive to enable writing to that file
    /// </summary>
    /// <author>
    /// Tremaine Stroebel
    /// </author>
    /// <date>
    /// October 2020
    /// </date>
    class WriteSeed
    {
        private string file;
        private DeadOrAlive[,] cells;

        /// <summary>
        /// Constructor for a WriteSeed object
        /// </summary>
        /// <param name="file">File name or path</param>
        /// <param name="cells">An array of DeadOrAlive values</param>
        public WriteSeed(string file, DeadOrAlive[,] cells)
        {
            this.file = file;
            this.cells = cells;
        }

        /// <summary>
        /// Writes a DeadOrAlive array to file using the version 2 seed format.
        /// </summary>
        public void WriteToFile()
        {
            using StreamWriter writer = new StreamWriter(file);
            writer.WriteLine("#version=2.0");

            // Loop over cells and write living cells to file
            for (int r = 0; r < cells.GetLength(0); r++)
            {
                for (int c = 0; c < cells.GetLength(1); c++)
                {
                    if (cells[r, c] == DeadOrAlive.alive)
                    {
                        writer.WriteLine($"(o) cell: {r}, {c}");
                    }
                }
            }
        }
    }
}
