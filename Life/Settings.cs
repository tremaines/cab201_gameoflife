using Microsoft.VisualBasic.CompilerServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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
        public const int DIMENSIONS_DEFAULT = 16;
        private const double RAND_MIN = 0.0;
        private const double RAND_MAX = 1.0;
        public const double RAND_DEFAULT = 0.5;
        private const int GEN_MIN = 1;
        public const int GEN_DEFAULT = 50;
        private const double UPDATE_MIN = 1;
        private const double UPDATE_MAX = 30;
        public const double UPDATE_DEFAULT = 5;
        private readonly string[] NEIGHBOURHOODS = { "moore", "vonneumann" };
        private readonly int ORDER_MAX = 10;
        public const int ORDER_MIN = 1;
        public static readonly List<int> BIRTH = new List<int> { 3 };
        public static readonly List<string> BIRTH_TEXT = new List<string> { "3" };
        public static readonly List<int> SURVIVAL = new List<int> { 2, 3 };
        public static readonly List<string> SURVIVAL_TEXT = new List<string> { "2", "3" };
        private const int MEM_MIN = 4;
        private const int MEM_MAX = 512;
        public const int MEM_DEFAULT = 16;


        //  Settings fields, set to default values
        private int rows = DIMENSIONS_DEFAULT;
        private int columns = DIMENSIONS_DEFAULT;
        private double random = RAND_DEFAULT;
        private int generations = GEN_DEFAULT;
        private double updateRate = UPDATE_DEFAULT;
        private string neighbourhood = "moore";
        private List<int> birth = BIRTH;
        private List<int> survival = SURVIVAL;
        private int order = ORDER_MIN;
        private int memory = MEM_DEFAULT;

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
        public string SeedFile { get; set; } = null;
        public string OutputFile { get; set; } = null;

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
        public bool Ghost { get; set; } = false;

        public string Neighbourhood
        {
            get => neighbourhood;
            set
            {
                if (!NEIGHBOURHOODS.Contains(value))
                {
                    throw new ParamValueException("Neighbourhood must be one of either 'Moore' or vonNeumann");
                }
                neighbourhood = value;
            }
        }

        public int Order
        {
            get => order;
            set
            {
                if (value < ORDER_MIN || value > ORDER_MAX)
                {
                    throw new ParamValueException("Order", value, ORDER_MIN, ORDER_MAX);
                }
                order = value;
            }
        }

        public bool Centre { get; set; } = false;

        public List<int> Birth
        { get => birth;
            set
            {
                foreach (int number in value)
                {
                    if (number < 0)
                    {
                        throw new ParamValueException("Birth rules must only contain positive, whole numbers.");
                    }
                }
                birth = value;
            } 
        }

        public List<string> BirthText { get; set; } = BIRTH_TEXT;

        public List<int> Survival 
        {
            get => survival;
            set
            {
                foreach (int number in value)
                {
                    if (number < 0)
                    {
                        throw new ParamValueException("Survival rules must only contain positive, whole numbers.");
                    }
                }
                survival = value;
            } 
        }

        public List<string> SurvivalText { get; set; } = SURVIVAL_TEXT;

        public int Memory
        {
            get => memory;
            set
            {
                if (value < MEM_MIN || value > MEM_MAX)
                {
                    throw new ParamValueException("Memory", value, MEM_MIN, MEM_MAX);
                }

                memory = value;
            }
        }


        /// <summary>
        /// Default constructor, called when user doesn't change settings or doesn't enter any valid --options.
        /// </summary>
        public Settings() { }

        private string GenerateRulesString(List<string> rules)
        {
            string output = "{";

            foreach (string item in rules)
            {
                output += $" {item} ";
            }
            output += "}";
            return output;
        }

        public override string ToString()
        {
            string settingsOutput = "";
            int padding = 35;

            settingsOutput += "Number of Rows:".PadLeft(padding) + $" {Rows}\n";
            settingsOutput += "Number of Columns:".PadLeft(padding) + $" {Columns}\n";
            settingsOutput += "Neighbourhood:".PadLeft(padding) + 
                (Neighbourhood == "vonneumann" ? " VonNeumann" : " Moore") + $" ({Order})" + 
                " [Centre count: " + (Centre ? "Enabled]" : "Disabled]") + "\n";
            settingsOutput += "Rules:".PadLeft(padding) + $" Survival {GenerateRulesString(SurvivalText)} " +
                $"Birth {GenerateRulesString(BirthText)}\n";
            settingsOutput += "Periodic Mode:".PadLeft(padding) + (Periodic ? " Enabled" : " Disabled") + "\n";
            settingsOutput += "Random Factor:".PadLeft(padding) + $" {Random:P2}\n";
            settingsOutput += "Seed File:".PadLeft(padding) + 
                $" {(SeedFile is null ? "None" : Path.GetFileName(SeedFile))}\n";
            settingsOutput += $"Output File:".PadLeft(padding) +
                $" {(OutputFile is null? "None" : Path.GetFullPath(OutputFile))}\n";
            settingsOutput += "No. of Generations:".PadLeft(padding) + $" {Generations}\n";
            settingsOutput += "Update Rate:".PadLeft(padding) + $" {UpdateRate} generations / second\n";
            settingsOutput += "Step Mode:".PadLeft(padding) + (StepMode ? " Enabled" : " Disabled") + "\n";
            settingsOutput += "Memory:".PadLeft(padding) + $" {Memory} generations\n";
            settingsOutput += "Ghost Mode:".PadLeft(padding) + (Ghost ? " Enabled" : " Disabled") + "\n";

            return settingsOutput;
        }
    }
}
