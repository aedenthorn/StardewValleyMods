using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using System.Linq;
using Netcode;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Moolah
{
	public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farmer), nameof(Farmer._money))]
        [HarmonyPatch(MethodType.Setter)]
        public class Farmer__money_Setter_Patch
        {
            public static void Prefix(Farmer __instance, ref int value)
            {
                if (!Config.EnableMod)
                    return;
                BigInteger moocha = 0;
                if (__instance.modData.TryGetValue("aedenthorn.Moolah/moocha", out string moochaString))
                    moocha = BigInteger.Parse(moochaString);
                value = AdjustMoney(__instance, value + moocha);

                //SMonitor.Log($"Total money: {value + moocha}");
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.addUnearnedMoney))]
        public class Farmer_addUnearnedMoney_Patch
        {
            public static bool Prefix(Farmer __instance, ref int money)
            {
                if (!Config.EnableMod)
                    return true;
                BigInteger total = GetTotalMoolah(__instance) + money;
                if(total > maxValue)
                {
                    __instance._money = AdjustMoney(__instance, total);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.drawMoneyBox))]
        public class DayTimeMoneyBox_drawMoneyBox_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, SpriteBatch b, int overrideX, int overrideY)
            {
                if (!Config.EnableMod || Game1.player is null)
                    return true;

				BigInteger moocha = GetTotalMoolah(Game1.player);

                AccessTools.Method(typeof(DayTimeMoneyBox), "updatePosition").Invoke(__instance, new object[] { });
                b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(28 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), (float)(172 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), new Rectangle?(new Rectangle(340, 472, 65, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);

				int extraDigits = moocha.ToString().Length - 8;
				if(extraDigits > 0)
                {
					b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(28 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)) - (extraDigits) * 24, (float)(172 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), new Rectangle?(new Rectangle(340, 472, 11, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
					for (int i = 0; i < extraDigits; i++)
                    {
						b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(68 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)) - (i + 1) * 24, (float)(180 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), new Rectangle?(new Rectangle(356, 474, 6, 15)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
					}
				}

				DrawMoneyDial(__instance.moneyDial, b, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(68 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), (float)(196 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), moocha);
                if (__instance.moneyShakeTimer > 0)
                {
                    __instance.moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(MoneyDial), nameof(MoneyDial.draw))]
        public class MoneyDial_draw_Patch
        {
            public static void Postfix(MoneyDial __instance, SpriteBatch b, Vector2 position, int target)
            {
                if (!Config.EnableMod || string.IsNullOrEmpty(Config.Separator))
                    return;

                int xPosition = 0;
                int digitStrip = (int)Math.Pow(10.0, (double)(__instance.numDigits - 1));
                bool significant = false;
                for (int j = 0; j < __instance.numDigits; j++)
                {
                    int currentDigit = __instance.currentValue / digitStrip % 10;
                    if (currentDigit > 0 || j == __instance.numDigits - 1)
                    {
                        significant = true;
                    }
                    if (significant)
                    {
                        if (j < __instance.numDigits - 1 && (__instance.numDigits - j) % Config.SeparatorInterval == 1)
                        {
                            SpriteText.drawString(b, Config.Separator, (int)position.X + xPosition + Config.SeparatorX, (int)position.Y + Config.SeparatorY + (int)((Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShippingMenu && __instance.currentValue >= 1000000) ? ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.53096771240234 + (double)j + 0.5f) * (float)(__instance.currentValue / 1000000)) : 0f));
                        }
                    }
                    xPosition += 24;
                    digitStrip /= 10;
                }
            }
        }
        [HarmonyPatch(typeof(ShippingMenu), nameof(ShippingMenu.draw))]
        public class ShippingMenun_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling ShippingMenu.draw");

                var found1 = false;
                var found2 = false;
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found2 && codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(SpriteText), nameof(SpriteText.drawString)) && codes[i + 1].opcode == OpCodes.Ldc_I4_0 && codes[i + 2].opcode == OpCodes.Stloc_S && codes[i + 3].opcode == OpCodes.Br_S)
                    {
                        SMonitor.Log("Changing number of empty squares");
                        codes[i + 1].opcode = OpCodes.Ldc_I4;
                        codes[i + 1].operand = -4;
                        found2 = true;
                    }
                    if (found1 && found2)
                        break;
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object) })]
        public class LocalizedContentManager_LoadString_Patch
        {
            public static void Prefix(string path, ref object sub1)
            {
                if (!Config.EnableMod || string.IsNullOrEmpty(Config.Separator) || path != "Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020")
                    return;
                sub1 = CheckIntToString(sub1.ToString());
            }
        }
        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new Type[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(IList<Item>) })]
        public class IClickableMenu_drawHoverText_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling IClickableMenu.drawHoverText");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(int), nameof(int.ToString)))
                    {
                        SMonitor.Log("Checking int to string");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckIntToString))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }

        }
        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.draw))]
        public class ShopMenu_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling ShopMenu.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(int), nameof(int.ToString)))
                    {
                        SMonitor.Log("Checking int to string");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckIntToString))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }

        }

    }
}