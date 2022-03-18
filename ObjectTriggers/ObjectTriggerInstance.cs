using Microsoft.Xna.Framework;

namespace ObjectTriggers
{
    public class ObjectTriggerInstance
    {
        public ObjectTriggerInstance(string key, Vector2 tile)
        {
            triggerKey = key;
            tilePosition = tile;
        }

        public string triggerKey;
        public Vector2 tilePosition;
        public int elapsed;
    }
}