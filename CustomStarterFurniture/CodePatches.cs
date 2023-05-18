using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = StardewValley.Object;

namespace CustomStarterFurniture
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FarmHouse), new Type[] { typeof(string), typeof(string) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class FarmHouse_Patch
        {
            public static void Postfix(FarmHouse __instance)
            {
                if (!Config.ModEnabled)
                    return;
                dataDict = Game1.content.Load<Dictionary<string, StarterFurnitureData>>(dictPath);
                foreach (var d in dataDict.Values)
                {
                    if (IsFarm(d.FarmType) && d.Clear)
                    {
                        __instance.furniture.Clear();
                        break;
                    }
                }
                foreach (var d in dataDict.Values)
                {
                    if (!IsFarm(d.FarmType))
                        continue;
                    foreach (var f in d.Furniture)
                    {
                        int index = GetObjectIndex(f.NameOrIndex, "Data/Furniture");
                        if (index == -1)
                            continue;
                        Furniture furniture = Furniture.GetFurnitureInstance(index, new Vector2(f.X, f.Y));
                        if (furniture is null)
                            continue;
                        if (f.HeldObjectNameOrIndex is not null)
                        {
                            Object heldItem = GetObject(f.HeldObjectNameOrIndex, f.HeldObjectType);
                            furniture.heldObject.Value = heldItem;
                        }
                        for (int i = 0; i < f.Rotation; i++)
                        {
                            furniture.rotate();
                        }
                        __instance.furniture.Add(furniture);
                    }
                }
            }

        }
    }
}