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
    class SeedVersionTwo : ReadSeed
    {
        private readonly string[] statusTypes = { "o", "x" };
        private readonly string[] structureTypes = { "cell", "rectangle", "ellipse" };

        public SeedVersionTwo(string file, int rows, int cols) : base(file, rows, cols) { }

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

        private string[] GetStructureComponents(string sanitisedLine)
        {
            char[] delimiters = { '(', ')', ':' };
            string[] structComponents = sanitisedLine.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return structComponents;
        }

        private void CheckComponents(string[] components, int lineNum, 
            out DeadOrAlive status, out string structure, out int[] coords)
        {
            int numExpectedComponents = 3;
            if (components.Length != numExpectedComponents)
            {
                throw new SeedLineException("Incorrect formatting.", lineNum);
            }
            string structureStatus = components[0];
            string structureType = components[1];
            string structureCoords = components[2];

            // Get status
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
                    if (CheckCellCentre((double)r, (double)c, rowCen, colCen, rowLen, colLen) 
                        && !CheckOutOfBounds(r, c))
                    {
                        cellsArray[r, c] = status;
                    }
                }
            }
        }

        private bool CheckCellCentre(double row, double col, double rowCen, double colCen, double rowLen, double colLen)
        {
            double checkRowVal = 4.0 * Math.Pow(row - rowCen, 2.0) / Math.Pow(rowLen, 2.0);
            double checkColVal = 4.0 * Math.Pow(col - colCen, 2.0) / Math.Pow(colLen, 2.0);

            return (checkRowVal + checkColVal) <= 1.0;
        }
    }
}
