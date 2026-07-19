using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Characters;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace FriendshipTweaks
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(SocialPage), nameof(SocialPage.drawNPCSlotHeart))]
        public class SocialPage_drawNPCSlotHeart_Patch
        {
            public static bool Prefix(SocialPage __instance, SpriteBatch b, int npcIndex, SocialPage.SocialEntry entry, int hearts, bool isDating, bool isCurrentSpouse)
            {
                if (!Config.ModEnabled)
                    return true;

                bool locked = entry.IsDatable && !isDating && !isCurrentSpouse && hearts >= 8;

                int x = ((hearts < entry.HeartLevel || locked) ? 211 : 218);
                Color color = (locked ? (Color.Black * 0.35f) : Color.White);

                const int heartsPerRow = 10;
                const int colSpacing = 32;
                const int rowSpacing = 28;
                const int singleRowYOffset = 36; // vanilla's original hearts<10 offset (64 - 28)

                int totalHearts = Config.MaxHearts;
                int rowsNeeded = Math.Max(1, (totalHearts + heartsPerRow - 1) / heartsPerRow); // ceiling division
                int baseYOffset = singleRowYOffset - (rowsNeeded - 1) * (rowSpacing / 2); // keep block roughly centered as rows grow

                int row = hearts / heartsPerRow;
                int col = hearts % heartsPerRow;

                Vector2 pos = new Vector2(
                    __instance.xPositionOnScreen + 320 - 4 + col * colSpacing,
                    __instance.sprites[npcIndex].bounds.Y + baseYOffset + row * rowSpacing);

                b.Draw(Game1.mouseCursors, pos,
                    new Microsoft.Xna.Framework.Rectangle(x, 428, 7, 6),
                    color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);

                return false;
            }
        }

        [HarmonyPatch(typeof(Utility), nameof(Utility.GetMaximumHeartsForCharacter))]
        public class Utility_GetMaximumHeartsForCharacter_Patch
        {
            public static bool Prefix(Character character, ref int __result)
            {
                if (!Config.ModEnabled || character is not NPC npc || npc is Child)
                    return true;

                __result = Config.MaxHearts;
                return false;
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.changeFriendship))]
        public class Farmer_changeFriendship_Patch
        {
            public static void Prefix(ref int amount)
            {
                if (!Config.ModEnabled)
                    return;
                amount = (int)Math.Round(amount * (Math.Sign(amount) > 0 ? Config.IncreaseModifier : Config.DecreaseModifier));
            }
        }

        public class Friendship_Points_Patch
        {
            public static void Prefix(Friendship __instance, ref int value)
            {
                if (!Config.ModEnabled)
                    return;
                var change = value - __instance.Points;
                change = (int)Math.Round(change * (Math.Sign(change) > 0 ? Config.IncreaseModifier : Config.DecreaseModifier));
                value = __instance.Points + change;
            }
        }

        public static float CustomBirthdayMult()
        {
            if (!Config.ModEnabled)
                return 8f;
            return Config.BirthdayMultiplier;
        }

        public static float CustomWinterStarMult()
        {
            if (!Config.ModEnabled)
                return 5f;
            return Config.WinterStarMultiplier;
        }

        public static int CustomStardropTeaMult()
        {
            if (!Config.ModEnabled)
                return 750;
            return (int)System.Math.Round(((750 / 3) * Config.StardropTeaMultiplier), MidpointRounding.AwayFromZero);
        }

        public class New_Patches
        {
            private static IMonitor Monitor { get; set; } = null;

            static MethodInfo BirthdayMultMethod = SymbolExtensions.GetMethodInfo(() => ModEntry.CustomBirthdayMult());
            static MethodInfo WinterStarMultMethod = SymbolExtensions.GetMethodInfo(() => ModEntry.CustomWinterStarMult());
            static MethodInfo StardropteaMultMethod = SymbolExtensions.GetMethodInfo(() => ModEntry.CustomStardropTeaMult());

            public static IEnumerable<CodeInstruction> GiftPatch(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.LoadsConstant(8f))
                    {
                        yield return new CodeInstruction(OpCodes.Call, BirthdayMultMethod);
                        continue;
                    }
                    if (instruction.LoadsConstant(750))
                    {
                        yield return new CodeInstruction(OpCodes.Call, StardropteaMultMethod);
                        continue;
                    }
                    yield return instruction;
                }
            }

            public static IEnumerable<CodeInstruction> WinterStarPatch(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.LoadsConstant(5f))
                    {
                        yield return new CodeInstruction(OpCodes.Call, WinterStarMultMethod);
                        continue;
                    }
                    yield return instruction;
                }
            }
        }
    }
}
