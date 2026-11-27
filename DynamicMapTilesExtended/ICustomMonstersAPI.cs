using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace DMT
{
    public interface ICustomMonstersAPI
    {
        Monster CreateMonster(string name, Vector2 position);
    }
}