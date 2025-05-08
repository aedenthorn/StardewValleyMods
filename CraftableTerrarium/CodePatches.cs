using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using Object = StardewValley.Object;

namespace CraftableTerrarium
{
	public partial class ModEntry
	{
		public class CraftingRecipe_Constructor_Patch
		{
			public static void Postfix(CraftingRecipe __instance, string name)
			{
				if (!name.Equals("aedenthorn.CraftableTerrarium_Terrarium"))
					return;

				__instance.description = TokenParser.ParseText("[aedenthorn.CraftableTerrarium_i18n item.Terrarium.description]");
			}
		}

		public class CraftingRecipe_GetItemData_Patch
		{
			public static bool Prefix(CraftingRecipe __instance, bool useFirst, ref ParsedItemData __result)
			{
				if (!__instance.name.Equals("aedenthorn.CraftableTerrarium_Terrarium"))
					return true;

				string text = useFirst ? __instance.itemToProduce.FirstOrDefault() : Game1.random.ChooseFrom(__instance.itemToProduce);

				__result = ItemRegistry.GetDataOrErrorItem("(F)" + text);
				return false;
			}
		}

		public class CraftingPage_spaceOccupied_Patch
		{
			public static void Postfix(ClickableTextureComponent[,] pageLayout, int x, int y, CraftingRecipe recipe, ref bool __result)
			{
				if (!recipe.name.Equals("aedenthorn.CraftableTerrarium_Terrarium"))
					return;

				(int width, int height) = GetRecipeTilesheetSize(recipe);

				if (width == 0 && height == 0)
					return;

				for (int i = 0; i < width; i++)
				{
					if (x + i >= 10)
					{
						__result = true;
						return;
					}
					for (int j = 0; j < height; j++)
					{
						if (y + j >= 4)
						{
							__result = true;
							return;
						}
						if (pageLayout[x + i, y + j] != null)
						{
							__result = true;
							return;
						}
					}
				}
				for (int i = 0; i < width; i++)
				{
					for (int j = 0; j < height; j++)
					{
						pageLayout[x + i, y + j] = new ClickableTextureComponent(null, Rectangle.Empty, null, null, null, Rectangle.Empty, 0f);
					}
				}
			}
		}

		public class CraftingPage_layoutRecipes_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				List<CodeInstruction> list = instructions.ToList();
				int n = 0;

				for (int i = 0; i < list.Count; i++)
				{
					if (i + 4 < list.Count && list[i].opcode.Equals(OpCodes.Ldc_I4_S) && list[i].operand.Equals((sbyte)64))
					{
						if (n == 1)
						{
							list.Insert(i, new CodeInstruction(OpCodes.Ldloc_S, 9));
							list.Insert(i + 2, new CodeInstruction(OpCodes.Call, typeof(CraftingPage_layoutRecipes_Patch).GetMethod(nameof(GetWidth))));
							i += 2;
						}
						else if (n == 2)
						{
							list.Insert(i - 3, new CodeInstruction(OpCodes.Ldloc_S, 9));
							list.Insert(i + 4, new CodeInstruction(OpCodes.Call, typeof(CraftingPage_layoutRecipes_Patch).GetMethod(nameof(GetHeight))) { labels = list[i + 4].labels });
							i += 2;
							break;
						}
						n++;
					}
				}
				return list;
			}

			public static int GetWidth(CraftingRecipe recipe, int defaultValue)
			{
				if (!recipe.name.Equals("aedenthorn.CraftableTerrarium_Terrarium"))
					return defaultValue;

				(int width, int height) = GetRecipeTilesheetSize(recipe);

				if (width == 0 && height == 0)
					return defaultValue;

				return width * defaultValue;
			}

			public static int GetHeight(CraftingRecipe recipe, int defaultValue)
			{
				if (!recipe.name.Equals("aedenthorn.CraftableTerrarium_Terrarium"))
					return defaultValue;

				(int width, int height) = GetRecipeTilesheetSize(recipe);

				if (width == 0 && height == 0)
					return defaultValue;

				return height * defaultValue;
			}
		}

		public class Furniture_loadDescription_Patch
		{
			public static bool Prefix(Furniture __instance, ref string __result)
			{
				if (!__instance.QualifiedItemId.Equals("(F)aedenthorn.CraftableTerrarium_Terrarium"))
					return true;

				__result = TokenParser.ParseText("[aedenthorn.CraftableTerrarium_i18n item.Terrarium.description]");
				return false;
			}
		}

		public class Object_placementAction_Patch
		{
			public static void Postfix(Object __instance, GameLocation location)
			{
				if (IsCraftableTerrarium(__instance))
				{
					ShowFrogs(location);
				}
			}
		}

		public class Object_performRemoveAction_Patch
		{
			public static void Postfix(Object __instance)
			{
				if (IsCraftableTerrarium(__instance))
				{
					if (!string.IsNullOrEmpty(Config.Sound))
					{
						Game1.playSound(Config.Sound);
					}
					DelayedShowCraftableTerrariums(__instance.Location);
				}
			}
		}

		public class Object_checkForAction_Patch
		{
			public static void Prefix(Object __instance, Farmer who, bool justCheckingForActivity)
			{
				if (IsCraftableTerrarium(__instance) && !justCheckingForActivity)
				{
					TemporaryAnimatedSprite sprite = who.currentLocation.temporarySprites.FirstOrDefault(s => s is TerrariumFrogs && (s as TerrariumFrogs).tile == __instance.TileLocation);

					if (sprite is TerrariumFrogs)
					{
						context.Monitor.Log($"Animating terrarium at tile {__instance.TileLocation}");
						TerrariumFrogs.DoAction();
					}
				}
			}
		}

		public class SebsFrogs_update_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (CodeInstruction instruction in instructions)
				{
					if (instruction.opcode.Equals(OpCodes.Ldstr) && instruction.operand.Equals("croak"))
					{
						instruction.opcode = OpCodes.Call;
						instruction.operand = typeof(SebsFrogs_update_Patch).GetMethod(nameof(GetSound));
						break;
					}
				}
				return instructions;
			}

			public static string GetSound()
			{
				return !string.IsNullOrEmpty(Config.Sound) ? Config.Sound : "croak";
			}
		}
	}
}
