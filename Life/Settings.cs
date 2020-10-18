using System.Collections.Generic;
using System.IO;

namespace Life
{
    /// <summary>
    /// This class holds the settings for the Game of Life board
    /// Defaults are initialised at the beginning of the class and the various methods
    /// change those defaults to user define values (assuming the user's input it valid!)
    /// </summary>
    /// <author>Tremaine Stroebel</author>
    /// <date>August 2020</date>
    class Settings
    {
        private const int DIMENSIONS_MIN = 4;
        private const int DIMENSIONS_MAX = 48;
        private const double RAND_MIN = 0.0;
        private const double RAND_MAX = 1.0;
        private const int GEN_MIN = 1;
        private const double UPDATE_MIN = 1;
        private const double UPDATE_MAX = 30;
        private readonly string[] NEIGHBOURHOOD = { "moore", "vonNeumann" };
        private const int MEM_MIN = 4;
        private const int MEM_MAX = 512;


        //  Settings fields, set to default values
        private int rows = 16;
        private int columns = 16;
        private double random = 0.5;
        private string outputFile = "None";
        private int generations = 50;
        private double updateRate = 5;
        private string neighbourhood = "moore";
        private int order = 1;
        private bool centre = false;
        private List<int> survival = new List<int> { 2, 3 };
        private List<int> birth = new List<int> { 3 };
        private int genMemory = 16;
        private bool ghost = false;

        public int Rows
        {
            get => rows;
            set
            {
                if (value < DIMENSIONS_MIN || value > DIMENSIONS_MAX)
                {
                    throw new ParamValueException("Row", value, DIMENSIONS_MIN, DIMENSIONS_MAX);
                }
                rows = value;
            }
        }

        public int Columns 
        {
            get => columns;
            set
            {
                if (value < DIMENSIONS_MIN || value > DIMENSIONS_MAX)
                {
                    throw new ParamValueException("Column", value, DIMENSIONS_MIN, DIMENSIONS_MAX);
                }
                rows = value;
                columns = value;
            }
        }

        public double Random 
        {
            get => random;
            set
            {
                if (value < RAND_MIN || value > RAND_MAX)
                {
                    throw new ParamValueException("Random factor", value, RAND_MIN, RAND_MAX);
                }
                random = value;
            }
        }
        public string SeedFile { get; set; } = "None";

        public int Generations 
        {
            get => generations;
            set
            {
                if (value < GEN_MIN)
                {
                    throw new ParamValueException($"Generations value {value} is not a positive, non-zero whole number.");
                }
                generations = value;
            }
        }

        public double UpdateRate 
        {
            get => updateRate;
            set
            {
                if (value < UPDATE_MIN || value > UPDATE_MAX)
                {
                    throw new ParamValueException("Update rate", value, UPDATE_MIN, UPDATE_MAX);
                }
                updateRate = value;
            }
        }
        public bool Periodic { get; set; } = false;

        public bool StepMode { get; set; } = false;

        /// <summary>
        /// Default constructor, called when user doesn't change settings or doesn't enter any valid --options.
        /// </summary>
        public Settings() { }


        public override string ToString()
        {
            string settingsOutput = "";

            settingsOutput += $"\t       Number of Rows: {Rows}\n";
            settingsOutput += $"\t    Number of Columns: {Columns}\n";
            settingsOutput += "\tPeriodic Mode Enabled: " + (Periodic ? "Enabled" : "Disabled") + "\n";
            settingsOutput += $"\t        Random Factor: {Random:P2}\n";
            settingsOutput += $"\t            Seed File: {Path.GetFileName(SeedFile)}\n";
            settingsOutput += $"\t   No. of Generations: {Generations}\n";
            settingsOutput += $"\t          Update Rate: {UpdateRate} generations / second\n";
            settingsOutput += "\t    Step Mode Enabled: " + (StepMode ? "Enabled" : "Disabled") + "\n";

            return settingsOutput;
        }
    }
}
