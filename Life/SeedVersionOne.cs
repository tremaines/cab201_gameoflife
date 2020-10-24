using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Life
{
    /// <summary>
    /// A class for reading version 1 seed files based on the ReadSeed class
    /// </summary>
    /// <author>
    /// Tremaine Stroebel
    /// </author>
    /// <date>
    /// October 2020
    /// </date>
    class SeedVersionOne : ReadSeed
    {
        public SeedVersionOne(string file, int rows, int cols) : base(file, rows, cols) { }

        /// <summary>
        /// Parses the file and adds cells to the cellsArray variable
        /// </summary>
        public override void ReadFile()
        {
            using StreamReader reader = new StreamReader(file);
            // First line of a seed file is not relevant, so read it to get it out of the way
            reader.ReadLine();
            char delimiter = ' ';
            string line;
            int lineCount = 2;

            while ((line = reader.ReadLine()) != null)
            {
                string[] coordinates = line.Split(delimiter);

                if (int.TryParse(coordinates[0], out int row) && int.TryParse(coordinates[1], out int col))
                {
                    if (!CheckOutOfBounds(row, col))
                    {
                        cellsArray[row, col] = DeadOrAlive.alive;
                    }
                }
                else
                {
                    throw new SeedLineException("Coordinates must be whole numbers", lineCount);
                }
                lineCount++;
            }
        }
    }
}
