using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace FarmAnimalSex
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), new Type[] { typeof(List<Object>) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class PurchaseAnimalsMenu_Patch
        {
            public static void Postfix()
            {
                if (!Config.EnableMod)
                    return;
                
                currentSex = Sexes.Female;

                Point start = new Point(Game1.uiViewport.Width / 2 + PurchaseAnimalsMenu.menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - PurchaseAnimalsMenu.menuHeight - IClickableMenu.borderWidth * 2) / 4) + new Point(84, PurchaseAnimalsMenu.menuHeight - 240);

                femaleButt = new ClickableTextureComponent(SHelper.Translation.Get("female"), new Rectangle(start, new Point(64, 64)), null, SHelper.Translation.Get("female-desc"), buttonTexture, new Rectangle(0, 0, 64, 64), 1f, false)
                {
                    myID = 200,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                };
                maleButt = new ClickableTextureComponent(SHelper.Translation.Get("male"), new Rectangle(start + new Point(0, 64), new Point(64, 64)), null, SHelper.Translation.Get("male-desc"), buttonTexture, new Rectangle(64, 0, 64, 64), 1f, false)
                {
                    myID = 201,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                };
                intersexButt = new ClickableTextureComponent(SHelper.Translation.Get("intersex"), new Rectangle(start + new Point(0, 128), new Point(64, 64)), null, SHelper.Translation.Get("intersex-desc"), buttonTexture, new Rectangle(128, 0, 64, 64), 1f, false)
                {
                    myID = 202,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                };

            }
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1.drawDialogueBox), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool),typeof(bool), typeof(string), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) })]
        public class Game1_drawDialogueBox_Patch
        {
            public static void Prefix()
            {
                if (!Config.EnableMod || Game1.activeClickableMenu is not PurchaseAnimalsMenu || Game1.IsFading() || AccessTools.FieldRefAccess<PurchaseAnimalsMenu, bool>(Game1.activeClickableMenu as PurchaseAnimalsMenu, "onFarm") || AccessTools.FieldRefAccess<PurchaseAnimalsMenu, bool>(Game1.activeClickableMenu as PurchaseAnimalsMenu, "namingAnimal"))
                    return;
                var b = Game1.spriteBatch;
                maleButt.draw(b);
                femaleButt.draw(b);
                intersexButt.draw(b);
                if (hoveredComponent is null)
                    return;
                IClickableMenu.drawHoverText(b, Game1.parseText(hoveredComponent.hoverText, Game1.smallFont, 320), Game1.smallFont, 0, 0, -1, hoveredComponent.name, -1, null, null, 0, -1, -1, -1, -1, 1f, null, null);
            }
        }

        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.performHoverAction))]
        public class PurchaseAnimalsMenu_performHoverAction_Patch
        {
            public static bool Prefix(PurchaseAnimalsMenu __instance, int x, int y, bool ___freeze, bool ___onFarm, bool ___namingAnimal)
            {
                if (!Config.EnableMod || Game1.IsFading() || ___freeze || ___onFarm || ___namingAnimal)
                    return true;
                if (maleButt != null && maleButt.containsPoint(x, y))
                {
                    hoveredComponent = maleButt;
                }
                else if (femaleButt != null && femaleButt.containsPoint(x, y))
                {
                    hoveredComponent = femaleButt;
                }
                else if (intersexButt != null && intersexButt.containsPoint(x, y))
                {
                    hoveredComponent = intersexButt;
                }
                else
                    hoveredComponent = null;
                return true;
            }
        }

        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.receiveLeftClick))]
        public class PurchaseAnimalsMenu_receiveLeftClick_Patch
        {
            public static bool Prefix(PurchaseAnimalsMenu __instance, int x, int y, bool ___freeze, bool ___onFarm, bool ___namingAnimal)
            {
                if (!Config.EnableMod || Game1.IsFading() || ___freeze || ___onFarm || ___namingAnimal)
                    return true;
                if (femaleButt != null && femaleButt.containsPoint(x, y))
                {
                    currentSex = Sexes.Female;
                }
                else if (maleButt != null && maleButt.containsPoint(x, y))
                {
                    currentSex = Sexes.Male;
                }
                else if (intersexButt != null && intersexButt.containsPoint(x, y))
                {
                    currentSex = Sexes.Intersex;
                }
                else
                    return true;
                SMonitor.Log($"Set current sex to {currentSex}");
                return false;
            }
        }

        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.isMale))]
        public class FarmAnimal_isMale_Patch
        {
            public static bool Prefix(FarmAnimal __instance, ref bool __result)
            {
                if (!Config.EnableMod || skipMale || !__instance.modData.TryGetValue(sexKey, out string sex))
                    return true;
                __result = IsMale(sex, false);
                return false;
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.CanHavePregnancy))]
        public class FarmAnimal_CanHavePregnancy_Patch
        {
            public static bool Prefix(FarmAnimal __instance, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.modData.TryGetValue(sexKey, out string sex))
                    return true;
                __result = sex == Sexes.Female+"" && (__instance.home.indoors.Value as AnimalHouse).animals.Values.ToList().Exists(a => IsMale(a, true));
                return false;
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate))]
        public class FarmAnimal_dayUpdate_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FarmAnimal.dayUpdate");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "Animals\\")
                    {
                        SMonitor.Log("replacing Animals\\\\ with method");
                        codes[i].opcode = OpCodes.Ldarg_0;
                        codes[i].operand = null;
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(ModEntry.GetTexturePrefix))));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.reloadData))]
        public class FarmAnimal_reloadData_Patch
        {
            public static void Prefix(FarmAnimal __instance)
            {
                if (!Config.EnableMod || __instance.modData.ContainsKey(sexKey))
                    return;
                if (Game1.activeClickableMenu is PurchaseAnimalsMenu)
                    __instance.modData[sexKey] = currentSex + "";
                else if (Game1.random.Next() < Config.IntersexChance)
                    __instance.modData[sexKey] = Sexes.Intersex + "";
                else if (__instance.myID.Value % 2 == 0)
                    __instance.modData[sexKey] = Sexes.Male + "";
                else
                    __instance.modData[sexKey] = Sexes.Male + "";
                SMonitor.Log($"Set animal sex to {currentSex}");
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FarmAnimal.reloadData");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "Animals\\")
                    {
                        SMonitor.Log("replacing Animals\\\\ with method");
                        codes[i].opcode = OpCodes.Ldarg_0;
                        codes[i].operand = null;
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(ModEntry.GetTexturePrefix))));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(AnimatedSprite), nameof(AnimatedSprite.LoadTexture))]
        public class AnimatedSprite_LoadTexture_Patch
        {
            public static void Prefix(AnimatedSprite __instance, ref string textureName)
            {
                if (!Config.EnableMod || textureName is null)
                    return;

                if(textureName.StartsWith("Animals\\" + Sexes.Male))
                {
                    try
                    {
                        Game1.content.Load<Texture2D>(textureName);
                    }
                    catch
                    {
                        textureName = "Animals\\" + textureName.Substring(("Animals\\" + Sexes.Male).Length);
                    }
                }
                else if(textureName.StartsWith("Animals\\" + Sexes.Intersex))
                {
                    try
                    {
                        Game1.content.Load<Texture2D>(textureName);
                    }
                    catch
                    {
                        textureName = "Animals\\" + textureName.Substring(("Animals\\" + Sexes.Intersex).Length);
                    }
                }
            }
        }

        public static string GetTexturePrefix(FarmAnimal animal)
        {
            if (!Config.EnableMod || !animal.modData.TryGetValue(sexKey, out string sex) || sex == Sexes.Female + "" || (sex == Sexes.Intersex + "" && !Config.InterIsMale))
                return "Animals\\";
            return "Animals\\" + sex;
        }

        private static bool IsMale(FarmAnimal a, bool inter)
        {
            if (!a.modData.TryGetValue(sexKey, out string sex))
            {
                skipMale = true;
                bool result = a.isMale();
                skipMale = false;
                return result;
            }
            return IsMale(sex, inter);
        }
        private static bool IsMale(string sex, bool inter)
        {
            return sex == Sexes.Male + "" || ((inter || Config.InterIsMale) && sex == Sexes.Intersex + "");
        }
    }
}