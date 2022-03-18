using Microsoft.Xna.Framework;
using StardewValley;
using System;

namespace ObjectTriggers
{
    public class ObjectTriggerData
    {
        public string objectID;
        public string triggerType;
        public string tripperType;
        public string triggerEffectType;
        public Action<GameLocation, Vector2, string, object> triggerEffectAction;
        public Action<GameLocation, Vector2, string, object> resetEffectAction;
        public string triggerEffectName;
        public bool targetTripper;
        public int radius;
        public float effectAmount;
        public float interval;
    }
}