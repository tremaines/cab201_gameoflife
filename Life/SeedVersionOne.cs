using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Life
{
    class SeedVersionOne : ReadSeed
    {
        public SeedVersionOne(string file, int rows, int cols) : base(file, rows, cols) { }

        public override void ReadFile()
        {
            using StreamReader reader = new StreamReader(file);
            // First line of a seed file is not relevant, so read it to get it out of the way
            reader.ReadLine();
            char delimiter = ' ';
            string line;
            int row, col;

            while ((line = reader.ReadLine()) != null)
            {
                string[] coordinates = line.Split(delimiter);
                row = int.Parse(coordinates[0]);
                col = int.Parse(coordinates[1]);

                if (!CheckOutOfBounds(row, col))
                {
                    cellsArray[row, col] = DeadOrAlive.alive;
                }
            }
        }
    }
}
