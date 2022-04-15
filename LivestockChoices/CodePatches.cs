using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace LivestockChoices
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), new Type[] { typeof(List<Object>) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class PurchaseAnimalsMenu_Patch
        {
            public static void Postfix(PurchaseAnimalsMenu __instance)
            {
                if (!Config.EnableMod)
                    return;
                List<ClickableTextureComponent>  ccl = new List<ClickableTextureComponent>();
                int spacing = 64;
                for(int i = 0; i < __instance.animalsToPurchase.Count; i++)
                {
                    var cc = __instance.animalsToPurchase[i];
                    if (i == 0)
                    {
                        cc.bounds.X += 18;
                        cc.bounds.Y -= 8;
                        ccl.Add(new ClickableTextureComponent(cc.name, new Rectangle(cc.bounds.X, cc.bounds.Y, 32, 32), null, "White Chicken", Game1.content.Load<Texture2D>("Animals/White Chicken"), new Rectangle(0, 0, 16, 16), 2) {
                            item = new Object(100, 1, false, 400, 0)
                            {
                                Name = "White Chicken",
                                Type = ((Game1.getFarm().isBuildingConstructed("Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop") || Game1.getFarm().isBuildingConstructed("Big Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926")),
                                displayName = GetName("White Chicken")
                            },
                            myID = 0,
                            rightNeighborID = 1,
                            leftNeighborID = -1,
                            downNeighborID = 2,
                            upNeighborID = -1
                        });
                        ccl.Add(new ClickableTextureComponent(cc.name, new Rectangle(cc.bounds.X + spacing, cc.bounds.Y, 32, 32), null, "Brown Chicken", Game1.content.Load<Texture2D>("Animals/Brown Chicken"), new Rectangle(0, 0, 16, 16), 2)
                        {
                            item = new Object(100, 1, false, 400, 0)
                            {
                                Name = "Brown Chicken",
                                Type = ((Game1.getFarm().isBuildingConstructed("Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop") || Game1.getFarm().isBuildingConstructed("Big Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926")),
                                displayName = GetName("Brown Chicken")
                            },
                            myID = 1,
                            rightNeighborID = 4,
                            leftNeighborID = 0,
                            downNeighborID = 3,
                            upNeighborID = -1
                        });
                        ccl.Add(new ClickableTextureComponent(Config.VoidChickenPrice + "", new Rectangle(cc.bounds.X, cc.bounds.Y + spacing, 32, 32), null, "Void Chicken", Game1.content.Load<Texture2D>("Animals/Void Chicken"), new Rectangle(0, 0, 16, 16), 2)
                        {
                            item = new Object(100, 1, false, Config.VoidChickenPrice, 0)
                            {
                                Name = "Void Chicken",
                                Type = (Config.VoidChickenPrice >= 0 && (Game1.getFarm().isBuildingConstructed("Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop") || Game1.getFarm().isBuildingConstructed("Big Coop")) ? null : (Config.VoidChickenPrice < 0 ? "" : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926"))),
                                displayName = GetName("Void Chicken")
                            },
                            myID = 2,
                            rightNeighborID = 3,
                            leftNeighborID = -1,
                            downNeighborID = 6,
                            upNeighborID = 0
                        });
                        ccl.Add(new ClickableTextureComponent(Config.GoldenChickenPrice + "", new Rectangle(cc.bounds.X + spacing, cc.bounds.Y + spacing, 32, 32), null, "Golden Chicken", Game1.content.Load<Texture2D>("Animals/Golden Chicken"), new Rectangle(0, 0, 16, 16), 2)
                        {
                            item = new Object(100, 1, false, Config.GoldenChickenPrice, 0)
                            {
                                Name = "Golden Chicken",
                                Type = (Config.GoldenChickenPrice >= 0 && (Game1.getFarm().isBuildingConstructed("Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop") || Game1.getFarm().isBuildingConstructed("Big Coop")) ? null : (Config.GoldenChickenPrice < 0 ? "" : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926"))),
                                displayName = GetName("Golden Chicken")
                            },
                            myID = 3,
                            rightNeighborID = 4,
                            leftNeighborID = 2,
                            downNeighborID = 6,
                            upNeighborID = 1
                        });
                        ccl.Add(new ClickableTextureComponent(Config.BlueChickenPrice + "", new Rectangle(cc.bounds.X + spacing / 2, cc.bounds.Y + spacing / 2, 32, 32), null, "Blue Chicken", Game1.content.Load<Texture2D>("Animals/Blue Chicken"), new Rectangle(0, 0, 16, 16), 2)
                        {
                            item = new Object(100, 1, false, Config.BlueChickenPrice, 0)
                            {
                                Name = "Blue Chicken",
                                Type = (Config.BlueChickenPrice >= 0 && Game1.player.eventsSeen.Contains(3900074) && (Game1.getFarm().isBuildingConstructed("Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop") || Game1.getFarm().isBuildingConstructed("Big Coop") || !Game1.player.eventsSeen.Contains(3900074)) ? null : (Config.BlueChickenPrice < 0 ? "" : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926"))),
                                displayName = GetName("Blue Chicken")
                            },
                            myID = 4,
                            rightNeighborID = 1,
                            leftNeighborID = -1,
                            downNeighborID = 2,
                            upNeighborID = -1
                        });
                    }
                    else if (i == 1)
                    {
                        cc.bounds.Y += 44;
                        cc.bounds.X += 32;
                        ccl.Add(new ClickableTextureComponent(cc.name, new Rectangle(cc.bounds.X, cc.bounds.Y, 64, 48), null, "White Cow", cc.texture, cc.sourceRect, 2) {
                            item = new Object(100, 1, false, 750, 0)
                            {
                                Name = "White Cow",
                                Type = ((Game1.getFarm().isBuildingConstructed("Barn") || Game1.getFarm().isBuildingConstructed("Deluxe Barn") || Game1.getFarm().isBuildingConstructed("Big Barn")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5927")),
                                displayName = GetName("White Cow")
                            },
                            myID = 5,
                            rightNeighborID = 5,
                            leftNeighborID = 3,
                            downNeighborID = 8,
                            upNeighborID = -1
                        });
                        ccl.Add(new ClickableTextureComponent(cc.name, new Rectangle(cc.bounds.X + 64, cc.bounds.Y - 40, 64, 48), null, "Brown Cow", cc.texture, new Rectangle(cc.sourceRect.Location + new Point(32, 32), cc.sourceRect.Size), 2) {
                            item = new Object(100, 1, false, 750, 0)
                            {
                                Name = "Brown Cow",
                                Type = ((Game1.getFarm().isBuildingConstructed("Barn") || Game1.getFarm().isBuildingConstructed("Deluxe Barn") || Game1.getFarm().isBuildingConstructed("Big Barn")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5927")),
                                displayName = GetName("Brown Cow")
                            },
                            myID = 6,
                            rightNeighborID = 7,
                            leftNeighborID = 5,
                            downNeighborID = 10,
                            upNeighborID = -1
                        });
                    }
                    else
                    {
                        cc.bounds = new Rectangle(cc.bounds.X + 32 * ( i % 3), cc.bounds.Y + 16 * (i / 3), 128, 64);
                        cc.myID += 5;
                        if(cc.rightNeighborID >= 0)
                            cc.rightNeighborID += 5;
                        if (cc.leftNeighborID >= 0)
                            cc.leftNeighborID += 5;
                        if (cc.downNeighborID >= 0)
                            cc.downNeighborID += 5;
                        if (cc.upNeighborID >= 0)
                            cc.upNeighborID += 5;
                        ccl.Add(cc);
                    }
                }
                __instance.animalsToPurchase = ccl;
            }


        }
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.draw), new Type[] { typeof(SpriteBatch) })]
        public class PurchaseAnimalsMenu_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling PurchaseAnimalsMenu.draw");
                var codes = new List<CodeInstruction>(instructions);
                bool found = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "Truffle Pig")
                    {
                        SMonitor.Log("switching placeholder text");
                        codes[i].operand = "Golden Chicken";
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.performHoverAction))]
        public class PurchaseAnimalsMenu_performHoverAction_Patch
        {
            public static void Postfix(PurchaseAnimalsMenu __instance, int x, int y, bool ___freeze, bool ___onFarm, bool ___namingAnimal)
            {
                if (!Config.EnableMod || Game1.IsFading() || ___freeze || ___onFarm || ___namingAnimal)
                    return;
                for (int i = 0; i < 5; i++)
                {
                    if (__instance.animalsToPurchase[i].containsPoint(x, y))
                    {
                        __instance.animalsToPurchase[i].scale = 2.2f;
                    }
                    else
                    {
                        __instance.animalsToPurchase[i].scale = 2f;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.getAnimalTitle))]
        public class PurchaseAnimalsMenu_getAnimalTitle_Patch
        {
            public static bool Prefix(string name, ref string __result)
            {
                if (!Config.EnableMod || (!name.EndsWith(" Chicken") && !name.EndsWith(" Cow")))
                    return true;
                string outName = GetName(name);
                //SMonitor.Log($"Got name {outName} for {name}");
                if (outName != null)
                    __result = outName;
                return false;
            }
        }
        
        [HarmonyPatch(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.getAnimalDescription))]
        public class PurchaseAnimalsMenu_getAnimalDescription_Patch
        {
            public static bool Prefix(string name, ref string __result)
            {
                if (!Config.EnableMod)
                    return true;
                if (name.EndsWith(" Chicken"))
                    __result = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11334") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
                else if (name.EndsWith(" Cow"))
                    __result = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11343") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
                else
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), new Type[] { typeof(string), typeof(long), typeof(long) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class FarmAnimal_Patch
        {
            public static void Prefix(FarmAnimal __instance, string type, ref string __state)
            {
                if (!Config.EnableMod)
                    return;
                SMonitor.Log($"Starting creating new {type}");
                __state = type;
            }
            public static void Postfix(FarmAnimal __instance, string type, string __state)
            {
                if (!Config.EnableMod || __state == type || (!type.EndsWith("Chicken") && !type.EndsWith("Cow")))
                    return;
                SMonitor.Log($"Creating new {__state}, was {type}");
                __instance.type.Value = __state;
                __instance.reloadData();
            }
        }

        private static string GetName(string key)
        {
            string rawData;
            Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals").TryGetValue(key, out rawData);
            if (rawData != null)
            {
                return rawData.Split('/', StringSplitOptions.None)[25];
            }
            return null;
        }
    }
}