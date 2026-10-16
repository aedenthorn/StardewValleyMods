using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;

namespace NozzleAndEnricher
{
    public partial class ModEntry
    {
        public static Item ConsumeEnricherFertilizer(Item item, int amount)
        {
            if (Config.ModEnabled && Config.InfiniteEnricherFertilizer)
            {
                return item;
            }
            return item.ConsumeStack(amount);
        }

        public static MethodBase GetEnricherConsumeMethod()
        {
            var consume = AccessTools.Method(typeof(Item), nameof(Item.ConsumeStack));
            var plant = AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.plant));
            foreach (var type in typeof(StardewValley.Object).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
            {
                foreach (var method in AccessTools.GetDeclaredMethods(type))
                {
                    if (method.IsAbstract || method.GetMethodBody() is null)
                        continue;
                    var codes = PatchProcessor.ReadMethodBody(method).ToList();
                    if (codes.Any(c => c.Value is MethodInfo m && m == consume) && codes.Any(c => c.Value is MethodInfo m && m == plant))
                    {
                        return method;
                    }
                }
            }
            return null;
        }
    }
}