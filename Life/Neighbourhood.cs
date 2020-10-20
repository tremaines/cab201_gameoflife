using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Life
{
    class Neighbourhood
    {
        private string type;
        private int order;
        private bool centre;
        private bool periodic;
        private DeadOrAlive[,] cells;
        public int LivingNeighbours { get; private set; }

        public Neighbourhood(string type, int order, bool centre, bool periodic, DeadOrAlive[,] cells)
        {
            this.type = type;
            this.order = order;
            this.centre = centre;
            this.periodic = periodic;
            this.cells = cells;
            LivingNeighbours = 0;
        }

        public int CheckNeighbours(int row, int col)
        {
            return type == "moore" ? MooreNeighbourhood(row, col) : VonNeumannNeighbourhood(row, col);
        }

        private int MooreNeighbourhood(int row, int col)
        {
            int neighboursCount = 0;
            int totalRows = cells.GetLength(0);
            int totalCols = cells.GetLength(1);
            for (int r = -order; r <= order; r++)
            {
                for (int c = -order; c <= order; c++)
                {
                    int neighbourR = row + r;
                    int neighbourC = col + c;
                    if (centre || !(r == 0 && c == 0))
                    {
                        // If periodic is enabled, use that logic in checking the neighbours
                        if (periodic)
                        {
                            neighboursCount += (int)cells[((neighbourR + totalRows) % totalRows),
                                ((neighbourC + totalCols) % totalCols)] / (int)DeadOrAlive.alive;
                        }
                        // Otherwise check if neighbour cell is within the bounds of the board
                        else if ((neighbourR >= 0 && neighbourC >= 0) &&
                            (neighbourR < totalRows && neighbourC < totalCols))
                        {
                            neighboursCount += (int)cells[neighbourR, neighbourC] / (int)DeadOrAlive.alive;
                        }
                    }
                }
            }
            return neighboursCount;
        }

        private int VonNeumannNeighbourhood(int row, int col)
        {
            // Height determines how many rows heigh the neighbourhood is
            int height = order * 2 + 1;
            int totalRows = cells.GetLength(0);
            int totalCols = cells.GetLength(1);
            int neighboursCount = 0;

            for (int i = 0; i < height; i++)
            {
                // Checkrow is the current "level" we are checking
                // Needs to be adjusted once we move past the middle level/row of the neighbourhood
                int checkRow = i >= (height - order) ? (i - (height - order)) : i;

                for (int c = col - checkRow; c <= (col + checkRow); c++)
                {
                    int neighbourR = row + (i - order);

                    if (centre || !(neighbourR == row && c == col))
                    {
                        if (periodic)
                        {
                            neighboursCount += (int)cells[(neighbourR + totalRows) % totalRows,
                                    (c + totalCols) % totalCols] / (int)DeadOrAlive.alive;
                        }
                        else if ((neighbourR >= 0 && c >= 0) && (neighbourR < totalRows && c < totalCols))
                        {
                            neighboursCount += (int)cells[neighbourR, c] / (int)DeadOrAlive.alive;
                        }
                    }
                }
            }
            return neighboursCount;
        }
    }
}
