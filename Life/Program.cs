using System;
using System.Collections.Generic;
using System.Diagnostics;
using Display;

namespace Life
{
    class Program
    {
        static void Main(string[] args)
        {
            // Add an empty line to avoid clutter
            Console.WriteLine("");
            // Generate the settings and an instance of the game with those settings
            ArgumentChecker arguments = new ArgumentChecker(args);
            Settings gameSettings = arguments.GenerateGameSettings(args);
            Game game = new Game(gameSettings);

            // Play the game
            game.PrintMsgsAndSettings();
            game.CycleThroughGame();
            game.RenderFinalGrid();
        }
    }
}
