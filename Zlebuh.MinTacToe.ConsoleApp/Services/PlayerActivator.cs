using System.Reflection;

namespace Zlebuh.MinTacToe.ConsoleApp.Services
{
    public class PlayerActivator
    {
        private readonly Type[] supportedTypes; 
        public PlayerActivator()
        {
            Type iAIPlayerType = typeof(IAIPlayer);
            Assembly aiPlayersAssembly = Assembly.LoadFile(
                Path.Combine(Environment.CurrentDirectory, "Zlebuh.MinTacToe.AIPlayers.dll"));
            List<Type> playerTypes = [];
            foreach (Assembly assembly in new Assembly[] { aiPlayersAssembly, Assembly.GetExecutingAssembly() })
            {
                Type[] playerTypesInThisAssembly = assembly.GetTypes()
                .Where(t => iAIPlayerType.IsAssignableFrom(t) && !t.IsAbstract).ToArray();
                playerTypes.AddRange(playerTypesInThisAssembly);
            }        
            supportedTypes = [.. playerTypes];
        }
        internal IAIPlayer Activate(Configuration.Player o)
        {
            Type playerType = GetType(o.PlayerType);
            object? playerObject = Activator.CreateInstance(playerType)
                ?? throw new InvalidOperationException($"Player type {o.PlayerType} not found.");

            foreach (var kvp in o.PlayerProperties)
            {
                PropertyInfo property = playerType.GetProperty(kvp.Key)
                    ?? throw new InvalidOperationException($"Property {kvp.Key} not found.");
                object value = Convert.ChangeType(kvp.Value.ToString(), property.PropertyType)
                    ?? throw new InvalidOperationException($"Cannot convert {kvp.Value} to {property.PropertyType}.");
                property.SetValue(playerObject, value);
            }

            IAIPlayer player = (IAIPlayer)playerObject;
            return player;

        }

        private Type GetType(string typeName)
        {
            return supportedTypes.FirstOrDefault(t => t.Name == typeName)
                ?? throw new InvalidOperationException($"Type {typeName} not found.");
        }
    }
}