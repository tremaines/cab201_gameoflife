using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Life
{
    class WriteSeed
    {
        private string file;
        private DeadOrAlive[,] cells;

        public WriteSeed(string file, DeadOrAlive[,] cells)
        {
            this.file = file;
            this.cells = cells;
        }

        public void WriteToFile()
        {
            using StreamWriter writer = new StreamWriter(file);
            writer.WriteLine("#version=2.0");

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
