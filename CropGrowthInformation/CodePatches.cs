using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using Object = StardewValley.Object;

namespace CropGrowthInfo
{
	public partial class ModEntry
	{
		public class Crop_drawWithOffset_Patch
		{
			public static void Prefix(Crop __instance, Vector2 tileLocation, ref Color __state, ref Color toTint)
			{
				if (!Config.EnableMod || !Context.CanPlayerMove || (Config.RequireToggle && !SHelper.Input.IsDown(Config.ToggleButton)))
					return;

				Rectangle r = new(Utility.Vector2ToPoint(Game1.GlobalToLocal(tileLocation * 64)) - new Point(0, 32), new Point(64, 64));
				var pos = Game1.getMousePosition();

				if (!r.Contains(pos))
					return;
				__state = __instance.tintColor.Value;
				__instance.tintColor.Value *= Config.CropOpacity;
				toTint *= Config.CropOpacity;
			}

			public static void Postfix(Crop __instance, Vector2 tileLocation, ref Color __state)
			{
				if (!Config.EnableMod || !Context.CanPlayerMove || (Config.RequireToggle && !SHelper.Input.IsDown(Config.ToggleButton)) || tileLocation != Game1.currentCursorTile)
					return;

				__instance.tintColor.Value = __state;
			}
		}

		public class Crop_draw_Patch
		{
			public static void Prefix(Crop __instance, Vector2 tileLocation, ref Color __state, ref Color toTint)
			{
				if (!Config.EnableMod || !Context.CanPlayerMove || (Config.RequireToggle && !SHelper.Input.IsDown(Config.ToggleButton)) || tileLocation != Game1.currentCursorTile)
					return;

				__state = __instance.tintColor.Value;
				__instance.tintColor.Value *= Config.CropOpacity;
				toTint *= Config.CropOpacity;
			}

			public static void Postfix(Crop __instance, Vector2 tileLocation, ref Color __state)
			{
				if (!Config.EnableMod || !Context.CanPlayerMove || (Config.RequireToggle && !SHelper.Input.IsDown(Config.ToggleButton)) || tileLocation != Game1.currentCursorTile)
					return;

				__instance.tintColor.Value = __state;
			}
		}

		public class Game1_drawMouseCursor_Patch
		{
			public static void Postfix()
			{
				if (!Config.EnableMod || !Context.CanPlayerMove || (Config.RequireToggle && !SHelper.Input.IsDown(Config.ToggleButton)))
					return;

				Crop crop;
				Vector2 tile = Game1.currentCursorTile;
				Vector2 tilePos = tile * 64;

				if (Game1.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out TerrainFeature feature) && feature is HoeDirt dirt && dirt.crop is not null)
				{
					crop = dirt.crop;
				}
				else if (Game1.currentLocation.objects.TryGetValue(tile, out Object obj) && obj is IndoorPot pot && pot.hoeDirt.Value is not null && pot.hoeDirt.Value.crop is not null)
				{
					crop = pot.hoeDirt.Value.crop;
					tilePos -= new Vector2(0, 32);
				}
				else
				{
					return;
				}

				List<TextData> list = new();

				if (crop.currentPhase.Value >= crop.phaseDays.Count - 1)
				{
					if (Config.ShowReadyText)
						list.Add(new TextData(SHelper.Translation.Get("ready"), Config.ReadyColor));
					else
						return;
				}
				else
				{
					int phase = crop.currentPhase.Value;
					int days = crop.dayOfCurrentPhase.Value;
					int maxdays = crop.phaseDays[crop.currentPhase.Value];
					int totaldays = 0;
					int totalmaxdays = 0;

					for (int i = 0; i < crop.phaseDays.Count - 1; i++)
					{
						int p = crop.phaseDays[i];
						totalmaxdays += p;
						if (i < phase)
						{
							totaldays += p;
						}
						else if (i == phase)
						{
							totaldays += days;
						}
					}
					if (Config.ShowCropName)
					{
						CropData cropData = crop.GetData();

						if (cropData is not null && Game1.objectData.TryGetValue(cropData.HarvestItemId, out ObjectData objectData))
						{
							list.Add(new TextData(TokenParser.ParseText(objectData.DisplayName), Config.NameColor));
						}
					}
					if (Config.ShowCurrentPhase)
					{
						list.Add(new TextData(string.Format(SHelper.Translation.Get("phase"), phase + 1, crop.phaseDays.Count), Config.CurrentPhaseColor));
					}
					if (Config.ShowDaysInCurrentPhase)
					{
						list.Add(new TextData(string.Format(SHelper.Translation.Get("current"), days + 1, maxdays + 1), Config.CurrentGrowthColor));
					}
					if (Config.ShowTotalGrowth)
					{
						list.Add(new TextData(string.Format(SHelper.Translation.Get("total"), totaldays + 1, totalmaxdays + 1), Config.TotalGrowthColor));
					}
				}

				float scale = Config.TextScale;
				float height = 32f * scale;
				float yOffset = 32 - height * list.Count / 2f;

				foreach(var d in list)
				{
					float xOffset = Game1.smallFont.MeasureString(d.text).X / 2 * scale;

					Game1.spriteBatch.DrawString(Game1.smallFont, d.text, Game1.GlobalToLocal(tilePos + new Vector2(32 - xOffset - 1, yOffset - 1)), Color.Black, 0, Vector2.Zero, scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 1);
					Game1.spriteBatch.DrawString(Game1.smallFont, d.text, Game1.GlobalToLocal(tilePos + new Vector2(32 - xOffset, yOffset)), d.color, 0, Vector2.Zero, scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 1);
					yOffset += height;
				}
			}
		}
	}
}
