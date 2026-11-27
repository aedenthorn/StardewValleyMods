using Microsoft.Xna.Framework;
using StardewValley;

namespace AreaOfEffect
{
    public interface IAOEAPI
    {
        public void ApplyAOEEffect(GameLocation l, Farmer who, Vector2 center, string type, int level = 0);
    }
    public class AOEAPI : IAOEAPI
    {
        public void ApplyAOEEffect(GameLocation l, Farmer who, Vector2 center, string type, int level = 0)
        {
            if(!ModEntry.SpellDict.TryGetValue(type, out var data))
            {
                ModEntry.SMonitor.Log($"Spell for {type} not found!", StardewModdingAPI.LogLevel.Warn);
                return;
            }
            if(level >= data.SpellLevels.Count)
            {
                ModEntry.SMonitor.Log($"Spell level {level} for {type} not found!", StardewModdingAPI.LogLevel.Warn);
                return;
            }
            ModEntry.DoCastSpell(l, who, center,data.SpellLevels[level]);
        }
    }
}