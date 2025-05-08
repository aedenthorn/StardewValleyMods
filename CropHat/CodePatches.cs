using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CropHat
{
	public partial class ModEntry
	{
		public class FarmerRenderer_drawHairAndAccesories_Patch
		{
			public static void Prefix(Farmer who, ref Hat __state)
			{
				if (!Config.EnableMod || who.hat.Value is null || !who.hat.Value.modData.ContainsKey(seedKey))
					return;
				__state = who.hat.Value;
				who.hat.Value = null;
			}

			public static void Postfix(FarmerRenderer __instance, Vector2 ___positionOffset, Hat __state, SpriteBatch b, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, float layerDepth)
			{
				if (!Config.EnableMod || __state is null)
					return;
				who.hat.Value = __state;

				bool flip = who.FarmerSprite.CurrentAnimationFrame.flip;
				float layer_offset = 3.9E-05f;
				var sourceRect = new Rectangle(Convert.ToInt32(who.hat.Value.modData[xKey]), Convert.ToInt32(who.hat.Value.modData[yKey]), 16, 32);

				b.Draw(Game1.cropSpriteSheet, position + origin + ___positionOffset + new Vector2(-8 + (flip ? -1 : 1) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (float)(-16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (who.hat.Value.ignoreHairstyleOffset.Value ? 0 : FarmerRenderer.hairstyleHatOffset[who.hair.Value % 16]) + 4 + __instance.heightOffset.Value)) + new Vector2(8, -80), sourceRect, Color.White, rotation, origin, 4f * scale, who.FacingDirection < 2 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + layer_offset);
			}
		}

		public class Hat_draw_Patch
		{
			public static bool Prefix(Hat __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
			{
				if (!Config.EnableMod || !__instance.modData.TryGetValue(seedKey, out _))
					return true;

				spriteBatch.Draw(Game1.cropSpriteSheet, location + new Vector2(10f, 10f), new Rectangle?(new Rectangle(Convert.ToInt32(__instance.modData[xKey]), Convert.ToInt32(__instance.modData[yKey]), 16, 32)), Color.White * transparency, 0f, new Vector2(3f, 3f), 3f * scaleSize, SpriteEffects.None, layerDepth);
				return false;
			}
		}

		public class Hat_drawInMenu_Patch
		{
			public static bool Prefix(Hat __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, Color color)
			{
				if (!Config.EnableMod || !__instance.modData.TryGetValue(seedKey, out _))
					return true;

				scaleSize *= 0.75f;
				spriteBatch.Draw(Game1.cropSpriteSheet, location + new Vector2(38f, 0), new Rectangle?(new Rectangle(Convert.ToInt32(__instance.modData[xKey]), Convert.ToInt32(__instance.modData[yKey]), 16, 32)), color * transparency, 0f, new Vector2(10f, 10f), 4f * scaleSize, SpriteEffects.None, layerDepth);
				return false;
			}
		}

		public class Hat_loadDisplayFields_Patch
		{
			public static bool Prefix(Hat __instance, ref bool __result)
			{
				if (!Config.EnableMod || !__instance.modData.TryGetValue(seedKey, out string seedIndex))
					return true;
				if (!Game1.cropData.TryGetValue(seedIndex, out CropData cropData))
					return true;
				if (!Game1.objectData.TryGetValue(cropData.HarvestItemId, out ObjectData objectData))
					return true;

				__instance.displayName = string.Format(SHelper.Translation.Get("x-hat"), TokenParser.ParseText(objectData.DisplayName));
				__instance.description = TokenParser.ParseText(objectData.Description);
				__result = true;
				return false;
			}
		}

		public class GameLocation_checkAction_Patch
		{
			public static bool Prefix(Farmer who, ref bool __result)
			{
				if (!Config.EnableMod)
					return true;

				if (Config.AllowOthersToPick)
				{
					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						var loc = farmer.Position + new Vector2(32, -88);

						if (who.currentLocation == farmer.currentLocation && farmer.hat.Value is not null && Vector2.Distance(Game1.GlobalToLocal(loc), Game1.getMousePosition().ToVector2()) < 32 && farmer.hat.Value.modData.TryGetValue(phaseKey, out string phaseString))
						{
							if (ReadyToHarvest(farmer.hat.Value) && Utility.withinRadiusOfPlayer((int)farmer.Position.X, (int)farmer.Position.Y, 1, Game1.player))
							{
								HarvestHatCrop(farmer);
								__result = true;
								return false;
							}
						}
					}
				}
				else
				{
					var loc = Game1.player.Position + new Vector2(32, -88);
					if (Vector2.Distance(Game1.GlobalToLocal(loc), Game1.getMousePosition().ToVector2()) < 32 && Game1.player.hat.Value is not null && Game1.player.hat.Value.modData.TryGetValue(phaseKey, out _))
					{
						if (ReadyToHarvest(Game1.player.hat.Value))
						{
							HarvestHatCrop(Game1.player);
							__result = true;
							return false;
						}
					}
				}
				return true;
			}
		}

		public class InventoryPage_receiveLeftClick_Patch
		{
			public static bool Prefix(InventoryPage __instance, int x, int y)
			{
				if (!Config.EnableMod || (Game1.player.CursorSlotItem is null) || Game1.player.hat.Value is not null)
					return true;
				if (!Game1.cropData.TryGetValue(Game1.player.CursorSlotItem.ItemId, out CropData cropData))
					return true;

				foreach (ClickableComponent c in __instance.equipmentIcons)
				{
					if (c.name == "Hat" && c.containsPoint(x, y))
					{
						SMonitor.Log($"Trying to wear {Game1.player.CursorSlotItem.Name}");

						Hat hat = new("0");
						hat.modData[seedKey] = Game1.player.CursorSlotItem.ItemId;
						hat.modData[daysKey] = "0";
						hat.modData[phaseKey] = "0";
						hat.modData[phasesKey] = (cropData.DaysInPhase.Count + 1).ToString();
						hat.modData[rowKey] = cropData.SpriteIndex.ToString();
						hat.modData[grownKey] = "false";
						hat.modData[xKey] = GetSourceX(cropData.SpriteIndex, 0, 0, false, false).ToString();
						hat.modData[yKey] = GetSourceY(cropData.SpriteIndex).ToString();
						if (Game1.objectData.TryGetValue(cropData.HarvestItemId, out ObjectData objectData))
						{
							hat.displayName = string.Format(SHelper.Translation.Get("x-hat"), TokenParser.ParseText(objectData.DisplayName));
							hat.description = TokenParser.ParseText(objectData.Description);
						}
						Game1.player.CursorSlotItem.Stack--;
						if (Game1.player.CursorSlotItem.Stack <= 0)
							Game1.player.CursorSlotItem = null;
						Game1.player.hat.Value = hat;
						Game1.playSound("grassyStep");
						return false;
					}
				}
				return true;
			}
		}
	}
}
