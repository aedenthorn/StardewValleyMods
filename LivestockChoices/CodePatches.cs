using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace LivestockChoices
{
	public partial class ModEntry
	{
		public class PurchaseAnimalsMenu_Patch
		{
			public static void Postfix(PurchaseAnimalsMenu __instance)
			{
				if (!Config.EnableMod)
					return;

				List<ClickableTextureComponent> ccl = new();
				int spacing = 64;

				for(int i = 0; i < __instance.animalsToPurchase.Count; i++)
				{
					var cc = __instance.animalsToPurchase[i];

					if (cc.hoverText.Equals("White Chicken"))
					{
						cc.bounds.X += 18;
						cc.bounds.Y -= 8;
						ccl.Add(new ClickableTextureComponent("800", new Rectangle(cc.bounds.X, cc.bounds.Y, 32, 32), null, "White Chicken", Game1.content.Load<Texture2D>("Animals/White Chicken"), new Rectangle(0, 0, 16, 16), 2f, true) {
							item = new Object("100", 1, false, 400)
							{
								Name = "White Chicken",
								Type = (__instance.TargetLocation.isBuildingConstructed("Coop") || __instance.TargetLocation.isBuildingConstructed("Deluxe Coop") || __instance.TargetLocation.isBuildingConstructed("Big Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926"),
							},
							myID = 0,
							rightNeighborID = 1,
							leftNeighborID = -1,
							downNeighborID = 4,
							upNeighborID = -1
						});
						ccl.Add(new ClickableTextureComponent("800", new Rectangle(cc.bounds.X + spacing, cc.bounds.Y, 32, 32), null, "Brown Chicken", Game1.content.Load<Texture2D>("Animals/Brown Chicken"), new Rectangle(0, 0, 16, 16), 2f, true)
						{
							item = new Object("100", 1, false, 400)
							{
								Name = "Brown Chicken",
								Type = (__instance.TargetLocation.isBuildingConstructed("Coop") || __instance.TargetLocation.isBuildingConstructed("Deluxe Coop") || __instance.TargetLocation.isBuildingConstructed("Big Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926"),
							},
							myID = 1,
							rightNeighborID = 7,
							leftNeighborID = 0,
							downNeighborID = 4,
							upNeighborID = -1
						});
						ccl.Add(new ClickableTextureComponent(Config.VoidChickenPrice.ToString(), new Rectangle(cc.bounds.X, cc.bounds.Y + spacing, 32, 32), null, "Void Chicken", Game1.content.Load<Texture2D>("Animals/Void Chicken"), new Rectangle(0, 0, 16, 16), 2f, true)
						{
							item = new Object("100", 1, false, Config.VoidChickenPrice / 2)
							{
								Name = "Void Chicken",
								Type = Config.VoidChickenPrice >= 0 && (__instance.TargetLocation.isBuildingConstructed("Coop") || __instance.TargetLocation.isBuildingConstructed("Deluxe Coop") || __instance.TargetLocation.isBuildingConstructed("Big Coop")) ? null : (Config.VoidChickenPrice < 0 ? "" : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926")),
							},
							myID = 2,
							rightNeighborID = 3,
							leftNeighborID = -1,
							downNeighborID = 5,
							upNeighborID = 4
						});
						ccl.Add(new ClickableTextureComponent(Config.GoldenChickenPrice.ToString(), new Rectangle(cc.bounds.X + spacing, cc.bounds.Y + spacing, 32, 32), null, "Golden Chicken", Game1.content.Load<Texture2D>("Animals/Golden Chicken"), new Rectangle(0, 0, 16, 16), 2f, true)
						{
							item = new Object("100", 1, false, Config.GoldenChickenPrice / 2)
							{
								Name = "Golden Chicken",
								Type = Config.GoldenChickenPrice >= 0 && (__instance.TargetLocation.isBuildingConstructed("Coop") || __instance.TargetLocation.isBuildingConstructed("Deluxe Coop") || __instance.TargetLocation.isBuildingConstructed("Big Coop")) ? null : (Config.GoldenChickenPrice < 0 ? "" : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926")),
							},
							myID = 3,
							rightNeighborID = 7,
							leftNeighborID = 2,
							downNeighborID = 6,
							upNeighborID = 4
						});
						ccl.Add(new ClickableTextureComponent(Config.BlueChickenPrice.ToString(), new Rectangle(cc.bounds.X + spacing / 2, cc.bounds.Y + spacing / 2, 32, 32), null, "Blue Chicken", Game1.content.Load<Texture2D>("Animals/Blue Chicken"), new Rectangle(0, 0, 16, 16), 2f, true)
						{
							item = new Object("100", 1, false, Config.BlueChickenPrice / 2)
							{
								Name = "Blue Chicken",
								Type = Config.BlueChickenPrice >= 0 && Game1.player.eventsSeen.Contains("3900074") && (__instance.TargetLocation.isBuildingConstructed("Coop") || __instance.TargetLocation.isBuildingConstructed("Deluxe Coop") || __instance.TargetLocation.isBuildingConstructed("Big Coop")) ? null : ((!__instance.TargetLocation.isBuildingConstructed("Coop") && !__instance.TargetLocation.isBuildingConstructed("Deluxe Coop") && !__instance.TargetLocation.isBuildingConstructed("Big Coop")) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926") : (Config.BlueChickenPrice < 0 ? "" : "???")),
							},
							myID = 4,
							rightNeighborID = -1,
							leftNeighborID = -1,
							downNeighborID = 2,
							upNeighborID = 0
						});
					}
					else if (cc.hoverText.Equals("White Cow"))
					{
						cc.bounds.Y += 72;
						ccl.Add(new ClickableTextureComponent("1500", new Rectangle(cc.bounds.X, cc.bounds.Y, 64, 48), null, "White Cow", cc.texture, cc.sourceRect, 2f, true) {
							item = new Object("100", 1, false, 750)
							{
								Name = "White Cow",
								Type = (__instance.TargetLocation.isBuildingConstructed("Barn") || __instance.TargetLocation.isBuildingConstructed("Deluxe Barn") || __instance.TargetLocation.isBuildingConstructed("Big Barn")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5931"),
							},
							myID = 5,
							rightNeighborID = 6,
							leftNeighborID = -1,
							downNeighborID = 12,
							upNeighborID = 2
						});
						ccl.Add(new ClickableTextureComponent("1500", new Rectangle(cc.bounds.X + 64, cc.bounds.Y - 40, 64, 48), null, "Brown Cow", cc.texture, new Rectangle(cc.sourceRect.Location + new Point(32, 32), cc.sourceRect.Size), 2f, true) {
							item = new Object("100", 1, false, 750)
							{
								Name = "Brown Cow",
								Type = (__instance.TargetLocation.isBuildingConstructed("Barn") || __instance.TargetLocation.isBuildingConstructed("Deluxe Barn") || __instance.TargetLocation.isBuildingConstructed("Big Barn")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5931"),
							},
							myID = 6,
							rightNeighborID = 10,
							leftNeighborID = 5,
							downNeighborID = 12,
							upNeighborID = 3
						});
					}
					else
					{
						cc.bounds = new Rectangle(cc.bounds.X + 32 * ( i % 3), cc.bounds.Y + 16 * (i / 3), 128, 64);
						cc.myID += 6;
						if(cc.rightNeighborID >= 0)
							cc.rightNeighborID += 6;
						if (cc.leftNeighborID >= 0)
							cc.leftNeighborID += 6;
						if (cc.downNeighborID >= 0)
							cc.downNeighborID += 6;
						if (cc.upNeighborID >= 0)
							cc.upNeighborID += 6;
						if (i > 0 && __instance.animalsToPurchase[i - 1].hoverText.Equals("White Chicken"))
						{
							cc.leftNeighborID = 1;
							cc.bounds.X -= 16;
						}
						else if (i > 1 && __instance.animalsToPurchase[i - 2].hoverText.Equals("White Chicken"))
						{
							cc.bounds.X -= 56;
						}
						else if (i > 0 && __instance.animalsToPurchase[i - 1].hoverText.Equals("White Cow"))
						{
							cc.leftNeighborID = 6;
							cc.bounds.X -= 16;
						}
						else if (i > 1 && __instance.animalsToPurchase[i - 2].hoverText.Equals("White Cow"))
						{
							cc.bounds.X -= 56;
						}
						else if (i > 0 && __instance.animalsToPurchase[i - 3].hoverText.Equals("White Cow"))
						{
							cc.upNeighborID = 5;
						}
						ccl.Add(cc);
					}
				}
				__instance.animalsToPurchase = ccl;
				if (Game1.options.SnappyMenus)
				{
					__instance.populateClickableComponentList();
					__instance.snapToDefaultClickableComponent();
				}
			}
		}

		public class PurchaseAnimalsMenu_draw_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling PurchaseAnimalsMenu.draw");
				var codes = new List<CodeInstruction>(instructions);

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

		public class FarmAnimal_GetShopDescription_Patch
		{
			public static bool Prefix(string id, ref string __result)
			{
				if (!Config.EnableMod)
					return true;

				if (id.EndsWith(" Chicken"))
				{
					__result = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11334") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
					return false;
				}
				else if (id.EndsWith(" Cow"))
				{
					__result = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11343") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
					return false;
				}
				return true;
			}
		}

		public class PurchaseAnimalsMenu_receiveLeftClick_Patch
		{
			public static bool Prefix(PurchaseAnimalsMenu __instance, int x, int y, ref string __state)
			{
				if (!Config.EnableMod || Game1.IsFading() || __instance.freeze || __instance.namingAnimal)
					return true;

				if (!__instance.onFarm)
				{
					foreach (ClickableTextureComponent item in __instance.animalsToPurchase)
					{
						if (__instance.readOnly || !item.containsPoint(x, y) || (item.item as StardewValley.Object).Type != null)
						{
							continue;
						}
						if (Game1.player.Money >= item.item.salePrice())
						{
							__state = item.hoverText;
						}
					}
				}
				return true;
			}

			public static void Postfix(PurchaseAnimalsMenu __instance, string __state)
			{
				if (!Config.EnableMod || string.IsNullOrEmpty(__state))
					return;

				__instance.animalBeingPurchased = new FarmAnimal(__state, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
			}
		}
	}
}
