using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace ObjectTriggers
{
    public class ObjectTriggerData
    {
        public string objectID;
        public string triggerType;
        public string tripperType;
        public string triggerEffectType;
        public string tripSound;
        public float tripChance = 1;
        public Action<GameLocation, Vector2, string, object> triggerEffectAction;
        public Action<GameLocation, Vector2, string, object> resetEffectAction;
        public string triggerEffectName;
        public bool targetTripper;
        public int radius;
        public float effectAmountMin;
        public float effectAmountMax;
        public float interval;
    }
}