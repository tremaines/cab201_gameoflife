using Display;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using System.IO;
using System.Data;

namespace Life
{ 
    class Game
    {
        private enum DeadOrAlive
        {
            dead,
            alive
        }
        private Settings settings;
        private Grid grid;
        private DeadOrAlive[,] status;
        private DeadOrAlive[,] nextGenStatus;

        public Game(Settings settings)
        {
            this.settings = settings;
            grid = new Grid(settings.Rows, settings.Columns);
            status = new DeadOrAlive[settings.Rows, settings.Columns];
            nextGenStatus = new DeadOrAlive[settings.Rows, settings.Columns];
        }

        public void PrintSettings()
        {
            Console.WriteLine($"\tNumber of rows: {settings.Rows}\n" +
                $"\tNumber of columns: {settings.Columns}\n" +
                $"\tPeriodic status: {settings.Periodic}\n" +
                $"\tRandom Factor: {settings.Random}\n" +
                $"\tSeed file: {settings.SeedFile}\n" +
                $"\tNo. of Generations: {settings.Generations}\n" +
                $"\tUpdate rate (per second): {settings.UpdateRate}\n" +
                $"\tStep mode: {settings.StepMode}");
        }

        public void SetupCells()
        {
            for (int r = 0; r < settings.Rows; r++)
            {
                for (int c = 0; c < settings.Columns; c++)
                {
                    status[r, c] = DeadOrAlive.dead;
                }
            }
        }

        private void ReadSeedFile()
        {
            string fileName = settings.SeedFile;
            using (StreamReader reader = new StreamReader(fileName))
            {
                reader.ReadLine();
                char delimiter = ' ';
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] coordinates = line.Split(delimiter);
                    int row = Int32.Parse(coordinates[0]);
                    int col = Int32.Parse(coordinates[1]);
                    status[row, col] = DeadOrAlive.alive;
                }
            }
        }

        public void SetInitialState()
        {
            if (settings.SeedFile != "None")
            {
                ReadSeedFile();
            }
            else
            {
                float chance = settings.Random * 100;
                Random random = new Random();

                for (int r = 0; r < settings.Rows; r++)
                {
                    for (int c = 0; c < settings.Columns; c++)
                    {
                        if (chance > random.Next(100))
                        {
                            grid.UpdateCell(r, c, CellState.Full);
                            status[r, c] = DeadOrAlive.alive;
                        }
                    }
                }
            }
        }

        private int CheckNeighbours(int row, int column)
        {
            int livingNeighbours = 0;
            for (int r = -1; r <= 1; r++)
            {
                for (int c = -1; c <=1; c++)
                {
                    int neighbourR = row + r;
                    int neighbourC = column + c;
                    if (!(r == 0 && c == 0))
                    {
                        if (settings.Periodic)
                        {
                            livingNeighbours += (int)status[((neighbourR + settings.Rows) % settings.Rows),
                                ((neighbourC + settings.Columns) % settings.Columns)];
                        }
                        else if ((neighbourR >= 0 && neighbourC >= 0) && 
                            (neighbourR < settings.Rows && neighbourC < settings.Columns))
                        {
                            livingNeighbours += (int)status[neighbourR, neighbourC];
                        }
                    }
                }
            }
            
            return livingNeighbours;
        }

        public void NextGenerationStatus()
        {
            for (int r = 0; r < settings.Rows; r++)
            {
                for (int c = 0; c < settings.Columns; c++)
                {
                    int livingNeighbours = CheckNeighbours(r, c);
                    if (status[r, c] == DeadOrAlive.dead)
                    {
                        if (livingNeighbours == 3)
                        {
                            nextGenStatus[r, c] = DeadOrAlive.alive;
                        }
                        else
                        {
                            nextGenStatus[r, c] = DeadOrAlive.dead;
                        }
                    }
                    else if (status[r, c] == DeadOrAlive.alive)
                    {
                        if (livingNeighbours == 2 || livingNeighbours == 3)
                        {
                            nextGenStatus[r, c] = DeadOrAlive.alive;
                        }
                        else
                        {
                            nextGenStatus[r, c] = DeadOrAlive.dead;
                        }
                    }
                }
            }
        }

        public void UpdateCellStatus()
        {
            for (int r = 0; r < settings.Rows; r++)
            {
                for (int c = 0; c < settings.Columns; c++)
                {
                    if (status[r, c] == DeadOrAlive.alive)
                    {
                        grid.UpdateCell(r, c, CellState.Full);
                    }
                    else grid.UpdateCell(r, c, CellState.Blank);
                }
            }
        }

        public void CycleThroughGame()
        {
            
            grid.InitializeWindow();
            SetupCells();
            SetInitialState();
            Stopwatch watch = new Stopwatch();

            for (int i = 0; i <= settings.Generations; i++)
            {
                grid.SetFootnote($"Generation: {i}");
                UpdateCellStatus();
                grid.Render();

                watch.Restart();
                if (settings.StepMode)
                {
                    ConsoleKeyInfo keyPress = Console.ReadKey();
                    while (keyPress.KeyChar != ' ')
                    {
                        keyPress = Console.ReadKey();
                    }
                }
                else
                {
                    while (watch.ElapsedMilliseconds < ((1 / settings.UpdateRate) * 1000)) ;
                }
                NextGenerationStatus();
                status = nextGenStatus;
                nextGenStatus = new DeadOrAlive[settings.Rows, settings.Columns];
            }
        }

        public void RenderFinalGrid()
        {
            grid.IsComplete = true;
            grid.Render();
            Console.ReadKey();
            grid.RevertWindow();
        }
    }
}
