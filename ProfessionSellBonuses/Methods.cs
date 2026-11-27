using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfessionSellBonuses
{
    public partial class ModEntry
    {
        public static void Item_sellToStorePrice_Postfix(Item __instance, ref int __result)
        {

            foreach(var kvp in Config.Professions.Where(p => p.Value?.Items is List<object> list && (list.Contains(__instance.QualifiedItemId) || list.Contains(__instance.Category) || list.Contains(__instance.ItemId))))
            {
                int i = ProNames.IndexOf(kvp.Key);
                if(i > -1 && Game1.player.professions.Contains(i))
                {
                    __result = ModifyResult(__result, 1, kvp.Value);
                }
            }
            foreach(var kvp in Config.Skills.Where(p => p.Value?.Items is List<object> list && (list.Contains(__instance.QualifiedItemId) || list.Contains(__instance.Category) || list.Contains(__instance.ItemId))))
            {
                int i = SkillNames.IndexOf(kvp.Key);
                if (i > -1)
                {
                    __result = ModifyResult(__result, Game1.player.GetSkillLevel(i), kvp.Value);
                }
            }
            foreach(var kvp in DataDict.Where(p => p.Value?.Items is List<object> list && (list.Contains(__instance.QualifiedItemId) || list.Contains(__instance.Category) || list.Contains(__instance.ItemId))))
            {
                int i = ProNames.IndexOf(kvp.Value.Which);
                if (i > -1)
                {
                    __result = ModifyResult(__result, 1, kvp.Value);
                }
                i = SkillNames.IndexOf(kvp.Value.Which);
                if (i > -1)
                {
                    __result = ModifyResult(__result, Game1.player.GetSkillLevel(i), kvp.Value);
                }
            }
        }

        private static int ModifyResult(int result, int level, SellBonusData data)
        {
            return (int)Math.Round(data.Type switch
            {
                BonusType.Mult => result * data.Amount * level,
                BonusType.Add => result + data.Amount * level,
                _ => result
            });
        }
        public static void CheckDict()
        {
            bool changed = false;
            foreach(var n in ProNames)
            {
                if (!Config.Professions.ContainsKey(n))
                {
                    Config.Professions[n] = new SellBonusData();
                    changed = true;
                }
            }
            foreach(var n in SkillNames)
            {
                if (!Config.Skills.ContainsKey(n))
                {
                    Config.Skills[n] = new SellBonusData();
                    changed = true;
                }
            }
            if(changed)
                SHelper.WriteConfig(Config);
        }
    }
}