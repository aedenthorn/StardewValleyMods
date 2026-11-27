using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace LuauSoup
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Utility), nameof(Utility.highlightLuauSoupItems))]
        public class Utility_highlightLuauSoupItems_Patch
        {
            public static bool Prefix(Item i, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                var dict = SHelper.GameContent.Load<Dictionary<string, SoupIngredientData>>(dictPath);
                if (dict.ContainsKey(i.QualifiedItemId) || dict.ContainsKey(i.ItemId))
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw))]
        public class ItemGrabMenu_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GameLocation.checkAction");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo info && info == AccessTools.Method(typeof(Item), nameof(Item.getDescription)))
                    {
                        SMonitor.Log($"adding method to show reactions");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(GetDescription));
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Event), "governorTaste")]
        public class Event_governorTaste_Patch
        {
            public static bool Prefix(Event __instance)
            {
                if (!Config.EnableMod)
                    return true;
                var dict = SHelper.GameContent.Load<Dictionary<string, SoupIngredientData>>(dictPath);

                bool? loved = null;
                int likeLevel = int.MaxValue;
                foreach (Item item in Game1.player.team.luauIngredients)
                {

                    Object o = item as Object;
                    if (dict.TryGetValue(item.QualifiedItemId, out var data) || dict.TryGetValue(item.ItemId, out data))
                    {
                        if (data.friendship != null)
                        {
                            Utility.improveFriendshipWithEveryoneInRegion(Game1.player, data.friendship.Value, "Town");
                        }
                        if (data.forceResult || (data.itemLevel != null && data.itemLevel < likeLevel))
                        {
                            loved = data.isLoved;
                            likeLevel = data.itemLevel.Value;
                            if (data.forceResult)
                            {
                                break;
                            }
                        }
                        continue;
                    }
                    if (Event.IsItemMayorShorts(item))
                    {
                        likeLevel = 6;
                        break;
                    }
                    int itemLevel = GetItemLevel(o);
                    int friendship = GetFriendshipForLevel(itemLevel);
                    if (friendship != 0)
                    {
                        Utility.improveFriendshipWithEveryoneInRegion(Game1.player, friendship, "Town");
                    }
                    if (itemLevel < likeLevel)
                    {
                        likeLevel = itemLevel;
                        loved = null;
                    }
                }

                int numPlayers = Game1.numberOfPlayers() - ((Game1.HasDedicatedHost != false) ? 1 : 0);
                if (Config.EveryoneMustContribute && Game1.player.team.luauIngredients.Count < numPlayers)
                {
                    likeLevel = 5;
                }
                __instance.eventCommands[__instance.CurrentCommand + 1] = "switchEvent governorReaction" + likeLevel;
                if (likeLevel == 4 || loved == true)
                {
                    Game1.getAchievement(38, true);
                }
                return false;
            }

        }
    }
}