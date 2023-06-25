using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace MayoMart
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Utility), nameof(Utility.getJojaStock))]
        public class Utility_getJojaStock_Patch
        {
            public static void Postfix(ref Dictionary<ISalable, int[]> __result)
            {
                if(!Config.ModEnabled)
                    return;
                List<Object> list = new List<Object>(){
                    new Object(Vector2.Zero, 306, int.MaxValue),
                    new Object(Vector2.Zero, 307, int.MaxValue),
                    new Object(Vector2.Zero, 308, int.MaxValue),
                    new Object(Vector2.Zero, 807, int.MaxValue)
                };
                Dictionary<ISalable, int[]> newResult = new();
                foreach(var v in __result.Values)
                {
                    newResult[list[Game1.random.Next(list.Count)]] = v;
                }
                __result = newResult;
            }
        }
        [HarmonyPatch(typeof(Dialogue), "parseDialogueString")]
        public class Dialogue_parseDialogueString_Patch
        {
            public static void Prefix(ref string masterString)
            {
                if(!Config.ModEnabled)
                    return;
                masterString = masterString.Replace("Joja", "Mayo");
            }
        }
    }
}