using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Linq;

namespace LocationFurniture
{
	public partial class ModEntry
	{
        [HarmonyPatch(typeof(GameLocation), new Type[] {typeof(string), typeof(string) })]
        [HarmonyPatch(MethodType.Constructor)]
        public static class GameLocation_Patch
        {
			public static void Postfix(GameLocation __instance)
			{
				if (!Config.ModEnabled)
					return;
                string[] fields = __instance.GetMapPropertySplitBySpaces("LocationFurniture");
                if (!fields.Any())
                    return;
                for (int i = 0; i < fields.Length; i += 5)
                {
                    string error;
                    if (!ArgUtility.TryGet(fields, i, out var id, out error, false, "string id") || !ArgUtility.TryGetVector2(fields, i + 1, out var tile, out error, false, "Vector2 tile") || !ArgUtility.TryGetInt(fields, i + 3, out var rotations, out error, "int rotations") || !ArgUtility.TryGetBool(fields, i + 4, out var canMove, out error, "bool canMove"))
                    {
                        __instance.LogMapPropertyError("LocationFurniture", fields, error, ' ');
                    }
                    else
                    {
                        Furniture newFurniture = ItemRegistry.Create<Furniture>(id.StartsWith("(F)") ? id : "(F)" + id, 1, 0, true);
                        if(newFurniture == null)
                        {
                            __instance.LogMapPropertyError("LocationFurniture", fields, $"Failed to create furniture with id '{id}'", ' ');
                            continue;
                        }
                        newFurniture.InitializeAtTile(tile);
                        newFurniture.IsOn = true;
                        for (int rotation = 0; rotation < rotations; rotation++)
                        {
                            newFurniture.rotate();
                        }
                        if(!canMove)
                        {
                            newFurniture.modData[moveKey] = "false";
                        }
                        Furniture targetFurniture = __instance.GetFurnitureAt(tile);
                        if (targetFurniture != null)
                        {
                            targetFurniture.heldObject.Value = newFurniture;
                        }
                        else
                        {
                            __instance.furniture.Add(newFurniture);
                        }
                    }
                }

            }

            [HarmonyPatch(typeof(Furniture), nameof(Furniture.canBeRemoved))]
            public class Furniture_canBeRemoved_Patch
            {
                public static bool Prefix(Furniture __instance, ref bool __result)
                {
                    if (!Config.ModEnabled || !__instance.modData.ContainsKey(moveKey))
                        return true;
                    __result = false;
                    return false;
                }
            }
        }
	}
}
