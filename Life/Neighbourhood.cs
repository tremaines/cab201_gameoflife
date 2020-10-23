namespace Life
{
    /// <summary>
    /// A group of cells centred around a single cell, forming a neighbourhood.
    /// Contains methods for analysing Moore and Von Neumann neighbourhoods
    /// </summary>
    /// <author>
    /// Tremaine Stroebel
    /// </author>
    /// <date>
    /// October 2020
    /// </date>
    class Neighbourhood
    {
        private readonly string type;
        private readonly int order;
        private readonly bool centre;
        private readonly bool periodic;
        private readonly DeadOrAlive[,] cells;
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

        /// <summary>
        /// Calls the relevant method to count neighbours based on the neighbourhood type
        /// </summary>
        /// <param name="row">Row of the central cell</param>
        /// <param name="col">Column of the central cell</param>
        /// <returns>The number of living neighbours that cell has</returns>
        public int CheckNeighbours(int row, int col)
        {
            return type == "moore" ? MooreNeighbourhood(row, col) : VonNeumannNeighbourhood(row, col);
        }

        /// <summary>
        /// Check the neighbouring cells of a central cell based on a Moore neighbourhood
        /// </summary>
        /// <param name="row">Row of the central cell</param>
        /// <param name="col">Column of the central cell</param>
        /// <returns>The number of living neighbours that cell has</returns>
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
                    
                    // Only counts r = 0, c = 0 (i.e. centre cell) if centre count is true
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

        /// <summary>
        /// Check the neighbouring cells of a central cell based on a Von Neumann neighbourhood
        /// </summary>
        /// <param name="row">Row of the central cell</param>
        /// <param name="col">Column of the central cell</param>
        /// <returns>The number of living neighbours that cell has</returns>
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
                int checkRow = i >= (height - order) ? ((2 * order) - i) : i;

                for (int c = col - checkRow; c <= (col + checkRow); c++)
                {
                    int neighbourR = row + (i - order);

                    // Similar to Moore, however neighbourR and c won't be equal to 0 when on the central cell
                    // but instead be equal to the row and col values
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
