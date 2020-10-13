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
            Settings gameSettings;

            // Generate the settings and an instance of the game with those settings
            gameSettings = ArgumentChecker.GenerateGameSettings(args);
            
            Game game = new Game(gameSettings);

            // Play the game
            game.PrintSettings();
            game.CycleThroughGame();
            game.RenderFinalGrid();
        }
    }
}
