using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Buffs;
using System;
using System.Linq;
using System.Reflection;

namespace StardewRPG
{
    public partial class ModEntry
    {
        private static bool BuffManager_Apply_Prefix(ref Buff buff)
        {
            if (!Config.EnableMod)
                return true;
            foreach(FieldInfo fi in typeof(BuffEffects).GetFields().Where(f => f.FieldType == typeof(NetFloat)))
            {
                if (((NetFloat)fi.GetValue(buff.effects)).Value > 0)
                    return true;
            }
            if(Config.ConRollToResistDebuff && Game1.random.Next(20) < GetStatValue(Game1.player, "con", Config.BaseStatValue))
            {
                SMonitor.Log($"Resisted debuff {buff.id}");
                return false;
            }
            var newDur = (int)Math.Round(buff.millisecondsDuration * (1 - GetStatMod(GetStatValue(Game1.player, "con", Config.BaseStatValue)) * Config.ConDebuffDurationBonus));
            SMonitor.Log($"Modifying buff duration {buff.millisecondsDuration} => {newDur}");
            buff.millisecondsDuration = newDur;
            return true;
        }
    }
}