using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Life
{
    public abstract class ReadSeed
    {
        protected string file;
        protected int rows;
        protected int cols;
        protected int maxRow = 0;
        protected int maxCol = 0;
        protected bool outOfBounds = false;
        protected DeadOrAlive[,] cellsArray;

        public int MaxRow => maxRow;
        public int MaxCol => maxCol;
        public bool OutOfBounds => outOfBounds;
        public DeadOrAlive[,] CellsArray => cellsArray;

        public ReadSeed(string file, int rows, int cols)
        {
            this.file = file;
            this.rows = rows;
            this.cols = cols;
            cellsArray = new DeadOrAlive[rows, cols];
        }

        public abstract void ReadFile();

        protected bool CheckOutOfBounds(int row, int col)
        {
            if (row >= this.rows || col >= this.cols)
            {
                outOfBounds = true;
                maxRow = row > maxRow ? row : maxRow;
                maxCol = col > maxCol ? col : maxCol;
                return true;
            }

            return false;
        }

        public void AlertToOutOfBounds()
        {
            if (outOfBounds)
            {
                string subMsg = Logging.SubMessageFormatter($"Recommended dimensions based on seed file: " +
                    $"{maxRow + 1} rows X {maxCol + 1} columns");
                Logging.GenericWarning("Game will continue but some cells in the seed file are out of bounds" +
                    " and have been ignored.", subMsg);
                Game.CheckForSpace();
            }
        }
    }
}
