using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
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
				Dictionary<string, string> giftTastes = Game1.content.Load<Dictionary<string, string>>("Data\\NPCGiftTastes");
				List<string> forbidden = Config.ForbiddenList.Split(',').ToList();
				foreach (NPC c in Utility.getAllCharacters())
				{
					if (c.isVillager() && c is not Child && giftTastes.ContainsKey(c.Name) && dispositions.TryGetValue(c.Name, out string dispo) && !forbidden.Contains(c.Name))
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
						if (potential.Count == 0)
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
        [HarmonyPatch(typeof(Character), nameof(Character.displayName))]
        [HarmonyPatch(MethodType.Getter)]
        public class Character_displayName_Patch
		{
			public static bool ignore;
            public static bool Prefix(Character __instance, ref string __result)
            {
                if (ignore || !Config.ModEnabled || __instance is not NPC || !(__instance as NPC).isVillager() || !subDict.TryGetValue(__instance.Name, out SubData sub))
                    return true;
				if (sub.displayName is not null)
                {
					__result = sub.displayName;
					return false;
                }
				NPC n = Game1.getCharacterFromName(sub.name);
				if (n is null)
					return true;
				ignore = true;
				__result = n.displayName;
				ignore = false;
				subDict[__instance.Name].displayName = __result;
				return false;
			}
        }
        [HarmonyPatch(typeof(Character), nameof(Character.Sprite))]
        [HarmonyPatch(MethodType.Getter)]
        public class Character_Sprite_Patch
        {
			public static bool ignore;
            public static void Postfix(Character __instance, ref AnimatedSprite __result)
            {
                if (ignore || !Config.ModEnabled || __result is null || __instance is not NPC ||  !(__instance as NPC).isVillager() || !subDict.TryGetValue(__instance.Name, out SubData sub))
                    return;
				if (sub.sprite is not null)
				{
					__result.spriteTexture = sub.sprite.Texture;
					__result.textureName.Value = sub.sprite.textureName.Value;
					__result.loadedTexture = sub.sprite.loadedTexture;
					return;
				}
				ignore = true;
				__result.LoadTexture("Characters\\" + NPC.getTextureNameForCharacter(sub.name));
				ignore = false;
				subDict[__instance.Name].sprite = __result;
			}
		}
        [HarmonyPatch(typeof(NPC), nameof(NPC.Portrait))]
        [HarmonyPatch(MethodType.Getter)]
        public class NPC_Portrait_Patch
        {
			public static bool ignore;
            public static bool Prefix(NPC __instance, ref Texture2D __result)
            {
                if (ignore || !Config.ModEnabled || !__instance.isVillager() || !subDict.TryGetValue(__instance.Name, out SubData sub))
                    return true;
				if (sub.portrait is not null)
				{
					__result = sub.portrait;
					return false;
				}
				NPC n = Game1.getCharacterFromName(sub.name);
				if (n is null)
					return true;
				ignore = true;
				__result = n.Portrait;
				ignore = false;
				subDict[__instance.Name].portrait = __result;
				return false;
			}
        }
    }
}