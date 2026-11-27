using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace CustomMonsters
{
    public interface ICustomMonstersAPI
    {
        Monster CreateMonster(string id, Vector2 position);
    }
    public class CustomMonstersAPI : ICustomMonstersAPI
    {
        public Monster CreateMonster(string id, Vector2 position)
        {        
            return ModEntry.CreateMonster(id, position);
        }
    }
}