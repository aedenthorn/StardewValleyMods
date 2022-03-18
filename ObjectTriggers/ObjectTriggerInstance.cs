using Microsoft.Xna.Framework;

namespace ObjectTriggers
{
    public class ObjectTriggerInstance
    {
        public ObjectTriggerInstance(string triggerKey, Vector2 tile)
        {
            TriggerKey = triggerKey;
            Tile = tile;
        }

        public string TriggerKey { get; }
        public Vector2 Tile { get; }
        public int elapsed { get; set; }
    }
}