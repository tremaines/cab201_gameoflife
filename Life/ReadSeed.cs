using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Life
{
    /// <summary>
    /// An abstract class providing the basis for reading the different types of seed files
    /// </summary>
    /// <author>
    /// Tremaine stroebel
    /// </author>
    /// <date>
    /// October 2020
    /// </date>
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

        /// <summary>
        /// A method used to read the seed file. Implementation varies depending on the version.
        /// </summary>
        public abstract void ReadFile();

        /// <summary>
        /// Checks if a coordinate in the seed file is out of bounds
        /// </summary>
        /// <param name="row">Row value</param>
        /// <param name="col">Column value</param>
        /// <returns>True if out of bounds, false otherwise</returns>
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

        /// <summary>
        /// Prints a warning message to the user if there are cells that are out of the bounds of the board.
        /// Recommends the minimum dimensions based on the seed file (not necessarily the ideal dimensions)
        /// </summary>
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
