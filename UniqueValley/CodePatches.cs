using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;

namespace UniqueValley
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
        public class Game1_loadForNewGame_Patch
        {

            public static void Postfix(bool loadedGame)
            {
                if (!Config.ModEnabled || loadedGame)
                    return;

				Dictionary<string, string> replaced = new Dictionary<string, string>();
				Dictionary<string, string> dispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				List<string> forbidden = Config.ForbiddenList.Split(',').ToList();
				for (int i = 0; i < Game1.locations.Count; i++)
				{
					if (!(Game1.locations[i] is MovieTheater))
					{
						foreach (NPC c in Game1.locations[i].getCharacters())
						{
							if (c.isVillager() && dispositions.TryGetValue(c.Name, out string dispo) && !forbidden.Contains(c.Name))
							{
								List<string> potential = new List<string>();
								string[] split = dispo.Split('/');
								foreach (var kvp in dispositions)
                                {
									if (kvp.Key == c.Name || replaced.ContainsKey(kvp.Key) || forbidden.Contains(kvp.Key))
										continue;
									string[] split2 = kvp.Value.Split('/');
									if (Config.MaintainAge && split[0] != split2[0])
										continue;
									if (Config.MaintainGender && split[4] != split2[4])
										continue;
									if (Config.MaintainDatable && split[5] != split2[5])
										continue;
									potential.Add(kvp.Key);
                                }
								if(potential.Count == 0)
                                {
									SMonitor.Log($"no potential swaps for {c.Name}");
									continue;
								}
								string choice = potential[Game1.random.Next(potential.Count)];
								SMonitor.Log($"Converting {c.Name} to {choice}");
								c.modData[nameKey] = choice;
								replaced[choice] = c.Name;
							}
						}
					}
				}
			}
        }
        [HarmonyPatch(typeof(Character), nameof(Character.displayName))]
        [HarmonyPatch(MethodType.Getter)]
        public class Character_displayName_Patch
		{
			public static bool ignore;
            public static bool Prefix(Character __instance, ref string __result)
            {
                if (ignore || !Config.ModEnabled || __instance is not NPC || !(__instance as NPC).isVillager() || !subDict.TryGetValue(__instance.Name, out string sub))
                    return true;
				NPC n = Game1.getCharacterFromName(sub);
				if (n is null)
					return true;
				ignore = true;
				__result = n.displayName;
				ignore = false;
				return false;
			}
        }
        [HarmonyPatch(typeof(Character), nameof(Character.Sprite))]
        [HarmonyPatch(MethodType.Getter)]
        public class Character_Sprite_Patch
        {
			public static bool ignore;
            public static bool Prefix(Character __instance, ref AnimatedSprite __result)
            {
                if (ignore || !Config.ModEnabled || __instance is not NPC ||  !(__instance as NPC).isVillager() || !subDict.TryGetValue(__instance.Name, out string sub))
                    return true;
				NPC n = Game1.getCharacterFromName(sub);
				if (n is null)
					return true;
				ignore = true;
				__result = n.Sprite;
				ignore = false;
				return false;
			}
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.Portrait))]
        [HarmonyPatch(MethodType.Getter)]
        public class NPC_Portrait_Patch
        {
			public static bool ignore;
            public static bool Prefix(NPC __instance, ref Texture2D __result)
            {
                if (ignore || !Config.ModEnabled || !__instance.isVillager() || !subDict.TryGetValue(__instance.Name, out string sub))
                    return true;
				NPC n = Game1.getCharacterFromName(sub);
				if (n is null)
					return true;
				ignore = true;
				__result = n.Portrait;
				ignore = false;
				return false;
			}
        }
    }
}