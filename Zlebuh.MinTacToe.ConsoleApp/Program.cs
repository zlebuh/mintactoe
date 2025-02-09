using System.Text.Json;
using Zlebuh.MinTacToe.ConsoleApp.Configuration;
using Zlebuh.MinTacToe.ConsoleApp.Services;

//AppConfiguration c = new()
//{
//    Games = new()
//    {
//        new OneGame()
//        {
//            Rules = new(),
//            MaxMilisecondsPerMove = 0,
//            MinMilisecondsPerMove = 0,
//            Opponents = new()
//            {
//                X = new()
//                {
//                    PlayerType = "Human"
//                },
//                O = new()
//                {
//                    PlayerType = "Random",
//                    PlayerProperties = new()
//                    {
//                        { "MaxDistance", 1 } 
//                    }
//                }
//            }
//        }
//    }
//};

//File.WriteAllText("config.json", JsonSerializer.Serialize(c));

AppConfiguration? config = JsonSerializer.Deserialize<AppConfiguration>(File.ReadAllText("config.json"));
if (config == null)
{
    Console.WriteLine("Config file is not valid.");
    return;
}

foreach (OneGame gameConfig in config.Games)
{
    GameSimulator gameSimulator = new(gameConfig);
    gameSimulator.Play();
    Console.ReadKey();
}