
using Microsoft.Xna.Framework;

namespace SmartBlocks
{
    internal class MyMessage
    {
        public MyMessage(string location, Vector2 tile)
        {
            Location = location;
            Tile = tile;
        }

        public string Location { get; }
        public Vector2 Tile { get; }
    }
}