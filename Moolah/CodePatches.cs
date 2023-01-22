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

namespace Moolah
{
	public partial class ModEntry
    {
        private static int maxValue = 1000000000;
        private static BigInteger previousTargetValue;
        private static BigInteger currentValue;
        private static long speed;
        private static long soundTimer;

        [HarmonyPatch(typeof(Farmer), nameof(Farmer._money))]
        [HarmonyPatch(MethodType.Setter)]
        public class Farmer__money_Setter_Patch
        {
            public static void Prefix(Farmer __instance, ref int value)
            {
                if (!Config.EnableMod)
                    return;
                BigInteger moocha = 0;
                if(__instance.modData.TryGetValue("aedenthorn.Moolah/moocha", out string moochaString))
                    moocha = BigInteger.Parse(moochaString);
                if(value > maxValue)
                {
                    //SMonitor.Log($"Storing excess money: {value - maxValue} with {moocha}");
                    moocha += value - maxValue;
                    value = maxValue;
                    __instance.modData["aedenthorn.Moolah/moocha"] = moocha + "";
                }
                else if(moocha > 0 && value < maxValue)
                {
                    //SMonitor.Log($"Retrieving excess money: {maxValue - value} from {moocha}");
                    if (maxValue - value > moocha)
                    {
                        value += (int)moocha;
                        moocha = 0;
                    }
                    else
                    {
                        moocha -= maxValue - value;
                        value = maxValue;
                    }
                    __instance.modData["aedenthorn.Moolah/moocha"] = moocha + "";
                }
                //SMonitor.Log($"Total money: {value + moocha}");
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.drawMoneyBox))]
        public class DayTimeMoneyBox_drawMoneyBox_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, SpriteBatch b, int overrideX, int overrideY)
            {
                if (!Config.EnableMod || Game1.player is null)
                    return true;

				BigInteger moocha = GetTotalMoolah();

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