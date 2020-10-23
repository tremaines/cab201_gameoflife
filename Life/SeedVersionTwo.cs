using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Life
{
    /// <summary>
    /// A class for reading version 2 seed files based on the ReadSeed class
    /// </summary>
    /// <author>
    /// Tremaine Stroebel
    /// </author>
    /// <date>
    /// October 2020
    /// </date>
    class SeedVersionTwo : ReadSeed
    {
        private readonly string[] statusTypes = { "o", "x" };
        private readonly string[] structureTypes = { "cell", "rectangle", "ellipse" };

        public SeedVersionTwo(string file, int rows, int cols) : base(file, rows, cols) { }

        /// <summary>
        /// Parses the file and adds cells to the cellsArray variable
        /// </summary>
        public override void ReadFile()
        {
            using StreamReader reader = new StreamReader(file);
            // First line of a seed file is not relevant, so read it to get it out of the way
            reader.ReadLine();
            string line;
            int lineCount = 2;

            while ((line = reader.ReadLine()) != null)
            {
                string[] newString = GetStructureComponents(SanitiseString(line));
                CheckComponents(newString, lineCount, out DeadOrAlive status, 
                    out string structureType, out int[] coordinates);

                switch (structureType)
                {
                    case "cell":
                        DrawCell(status, coordinates, lineCount);
                        break;
                    case "rectangle":
                        DrawRectangle(status, coordinates, lineCount);
                        break;
                    case "ellipse":
                        DrawEllipse(status, coordinates, lineCount);
                        break;
                }
                lineCount++;
            }
        }

        /// <summary>
        /// Removes white spaces from the provided string
        /// </summary>
        /// <param name="line">The string to be sanitised</param>
        /// <returns>A new string with no whitespace characters</returns>
        private string SanitiseString(string line)
        {
            string newString = "";
            foreach (char c in line)
            {
                if (!Char.IsWhiteSpace(c))
                {
                    newString += c;
                }
            }

            return newString;
        }

        /// <summary>
        /// Splits the provided string up into the main components of a version 2 seed line (status, structure, coords)
        /// </summary>
        /// <param name="sanitisedLine">A string with no whitespace characters</param>
        /// <returns>A string array of the three structures</returns>
        private string[] GetStructureComponents(string sanitisedLine)
        {
            char[] delimiters = { '(', ')', ':' };
            string[] structComponents = sanitisedLine.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return structComponents;
        }

        /// <summary>
        /// Validates the components of a version 2 seed line
        /// </summary>
        /// <param name="components">An array of the components</param>
        /// <param name="lineNum">The line number</param>
        /// <param name="status">Output the status (dead or alive)</param>
        /// <param name="structure">Output the structure (cell, rectangle or ellipse)</param>
        /// <param name="coords">Output the coordinates</param>
        /// <exception cref="SeedLineException">Errors in the composition of the seed file</exception>
        private void CheckComponents(string[] components, int lineNum, 
            out DeadOrAlive status, out string structure, out int[] coords)
        {
            int numExpectedComponents = 3;
            
            // Check there is the correct number of components first
            if (components.Length != numExpectedComponents)
            {
                throw new SeedLineException("Incorrect formatting.", lineNum);
            }
            string structureStatus = components[0].ToLower();
            string structureType = components[1].ToLower();
            string structureCoords = components[2];

            // Get status & structure
            if (!(statusTypes.Contains(structureStatus) && structureTypes.Contains(structureType)))
            {
                throw new SeedLineException("Unknown status or structure.", lineNum);
            }
            else
            {
                status = structureStatus == "o" ? DeadOrAlive.alive : DeadOrAlive.dead;
                structure = structureType;
                coords = GetCoordinates(structureCoords, lineNum);

            }
        }

        /// <summary>
        /// Convert the coordinates from string to int
        /// </summary>
        /// <param name="stringCoords">Coordinates in string form</param>
        /// <param name="lineNum">The line number of the seed</param>
        /// <returns>An array of coordinates</returns>
        /// <exception cref="SeedLineException">Coordinates cannot be converted to int</exception>
        private int[] GetCoordinates(string stringCoords, int lineNum)
        {
            string[] splitCoords = stringCoords.Split(',');
            int[] intCoords = new int[splitCoords.Length];

            for (int i = 0; i < splitCoords.Length; i++)
            {
                if (!int.TryParse(splitCoords[i], out intCoords[i]))
                {
                    throw new SeedLineException("Structure coordinates must be comma separated integers.", lineNum);
                }
            }
            return intCoords;
        }

        /// <summary>
        /// "Draw" a cell structure type
        /// </summary>
        /// <param name="status">Structure status</param>
        /// <param name="coords">Structure coordinates</param>
        /// <param name="lineNum">Seed line number</param>
        /// <exception cref="SeedLineException">Incorrect number of coordinates</exception>
        private void DrawCell (DeadOrAlive status, int[] coords, int lineNum)
        {
            int expectedCoords = 2;

            if (coords.Length != expectedCoords)
            {
                throw new SeedLineException(lineNum, expectedCoords);
            }

            int row = coords[0];
            int col = coords[1];

            if (!CheckOutOfBounds(row, col))
            {
                cellsArray[row, col] = status;
            }
        }

        /// <summary>
        /// "Draw" a rectangle structure type
        /// </summary>
        /// <param name="status">Structure status</param>
        /// <param name="coords">Structure coordinates</param>
        /// <param name="lineNum">Seed line number</param>
        /// <exception cref="SeedLineException">Incorrect number of coordinates</exception>
        private void DrawRectangle(DeadOrAlive status, int[] coords, int lineNum)
        {
            int expectedCoords = 4;

            if (coords.Length != expectedCoords)
            {
                throw new SeedLineException(lineNum, expectedCoords);
            }

            int bottomRow = coords[0];
            int bottomCol = coords[1];
            int topRow = coords[2];
            int topCol = coords[3];

            for (int r = bottomRow; r <= topRow; r++)
            {
                for (int c = bottomCol; c <= topCol; c++)
                {
                    if (!CheckOutOfBounds(r, c))
                    {
                        cellsArray[r, c] = status;
                    }
                }
            }
        }

        /// <summary>
        /// "Draw" an ellipse structure type
        /// </summary>
        /// <param name="status">Structure status</param>
        /// <param name="coords">Structure coordinates</param>
        /// <param name="lineNum">Seed line number</param>
        /// <exception cref="SeedLineException">Incorrect number of coordinates</exception>
        private void DrawEllipse(DeadOrAlive status, int[] coords, int lineNum)
        {
            int expectedCoords = 4;

            if (coords.Length != expectedCoords)
            {
                throw new SeedLineException(lineNum, expectedCoords);
            }

            int bottomRow = coords[0];
            int bottomCol = coords[1];
            int topRow = coords[2];
            int topCol = coords[3];

            double rowLen = (topRow - bottomRow) + 1.0;
            double colLen = (topCol - bottomCol) + 1.0;
            double colCen = ((topCol - bottomCol) / 2.0) + bottomCol;
            double rowCen = ((topRow - bottomRow) / 2.0) + bottomRow;

            for (int r = bottomRow; r <= topRow; r++)
            {
                for (int c = bottomCol; c <= topCol; c++)
                {
                    // Only record that cell as part of the ellipse if its centre falls within the range of the
                    // ellipse
                    if (CheckCellCentre((double)r, (double)c, rowCen, colCen, rowLen, colLen) 
                        && !CheckOutOfBounds(r, c))
                    {
                        cellsArray[r, c] = status;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a cell's centre is within range of the ellipse
        /// </summary>
        /// <param name="row">Cell row</param>
        /// <param name="col">Cell column</param>
        /// <param name="rowCen">Centre row value of ellipse</param>
        /// <param name="colCen">Centre column value of ellipse</param>
        /// <param name="rowLen">Row length of ellipse</param>
        /// <param name="colLen">Column length of ellipse</param>
        /// <returns>True if centre is within ellipse range, false otherwise</returns>
        private bool CheckCellCentre(double row, double col, double rowCen, double colCen, double rowLen, double colLen)
        {
            double checkRowVal = 4.0 * Math.Pow(row - rowCen, 2.0) / Math.Pow(rowLen, 2.0);
            double checkColVal = 4.0 * Math.Pow(col - colCen, 2.0) / Math.Pow(colLen, 2.0);

            return (checkRowVal + checkColVal) <= 1.0;
        }
    }
}
