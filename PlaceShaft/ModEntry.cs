using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PlaceShaft
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor
	{

		public static ModEntry context;

		internal static ModConfig Config;
		/// <summary>Get whether this instance can edit the given asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public bool CanEdit<T>(IAssetInfo asset)
		{
			if (asset.AssetNameEquals("Maps/Mines/mine") || asset.AssetNameEquals("Maps/Mines/mine_dark") || asset.AssetNameEquals("Maps/Mines/mine_dino") || asset.AssetNameEquals("Maps/Mines/mine_frost") || asset.AssetNameEquals("Maps/Mines/mine_lava") || asset.AssetNameEquals("Maps/Mines/mine_frost_dark") || asset.AssetNameEquals("Maps/Mines/minequarryshaft") || asset.AssetNameEquals("Maps/Mines/mine_lava_dark") || asset.AssetNameEquals("Maps/Mines/mine_slime") || asset.AssetNameEquals("Maps/Mines/mine") || asset.AssetNameEquals("Maps/Mines/mine_dark") || asset.AssetNameEquals("TileSheets/Craftables") || asset.AssetNameEquals("Data/CraftingRecipes") || asset.AssetNameEquals("Data/BigCraftablesInformation"))
			{
				return true;
			}

			return false;
		}

		/// <summary>Edit a matched asset.</summary>
		/// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
		public void Edit<T>(IAssetData asset)
		{
			if (asset.AssetNameEquals("Maps/Mines/mine") || asset.AssetNameEquals("Maps/Mines/mine_dark") || asset.AssetNameEquals("Maps/Mines/mine_dino") || asset.AssetNameEquals("Maps/Mines/mine_frost") || asset.AssetNameEquals("Maps/Mines/mine_lava") || asset.AssetNameEquals("Maps/Mines/mine_frost_dark") || asset.AssetNameEquals("Maps/Mines/minequarryshaft") || asset.AssetNameEquals("Maps/Mines/mine_lava_dark") || asset.AssetNameEquals("Maps/Mines/mine_slime") || asset.AssetNameEquals("Maps/Mines/mine") || asset.AssetNameEquals("Maps/Mines/mine_dark"))
			{
				Texture2D customTexture = this.Helper.Content.Load<Texture2D>("Maps/Mines/mine_desert", ContentSource.GameContent);
				asset
					.AsImage()
					.PatchImage(customTexture, sourceArea: new Rectangle(224, 160, 16, 16), targetArea: new Rectangle(224, 160, 16, 16));
			}
			else if (asset.AssetNameEquals("TileSheets/Craftables"))
			{
				Texture2D customTexture = this.Helper.Content.Load<Texture2D>("Maps/Mines/mine_desert", ContentSource.GameContent);
				asset
					.AsImage()
					.PatchImage(customTexture, sourceArea: new Rectangle(224, 160, 16, 16), targetArea: new Rectangle(112, 144, 16, 16));
			}
			else if (asset.AssetNameEquals("Data/CraftingRecipes"))
			{
				IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
				data.Add("Mine Shaft", $"{Config.ShaftCost}/Field/39/true/{Config.SkillReq}");
			}
			else if (asset.AssetNameEquals("Data/BigCraftablesInformation"))
			{
				IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
				data.Add(39, "Mine Shaft/0/-300/Crafting -9/Use this to move down several levels in the mines./true/false/1/Mine Shaft");
			}
		}


		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

			harmony.Patch(
			   original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
			   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.placementAction_prefix))
			);
			harmony.Patch(
			   original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.enterMineShaft)),
			   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.enterMineShaft_prefix))
			);
			harmony.Patch(
			   original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue),new Type[] { typeof(string),typeof (Response[]), typeof(string) }),
			   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.createQuestionDialogue_prefix))
			);
		}

		private static bool createQuestionDialogue_prefix(GameLocation __instance, string dialogKey)
		{
			if (dialogKey == "Shaft" && Config.SkipConfirmOnShaftJump) 
			{
				(__instance as MineShaft).enterMineShaft();
				return false;
			}
			return true;
		}

		private static bool enterMineShaft_prefix(MineShaft __instance, ref bool ___isFallingDownShaft)
		{
			if (Config.PercentDamage == 100 && Config.PercentLevels == 100 && !Config.PreventGoingToSkullCave)
				return true;

			DelayedAction.playSoundAfterDelay("fallDown", 1200, null, -1);
			DelayedAction.playSoundAfterDelay("clubSmash", 2200, null, -1);
			Random random = new Random(__instance.mineLevel + (int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays);
			int levelsDown = Math.Max(1,random.Next((int)Math.Round(3 * Config.PercentLevels / 100f),(int)Math.Round(9 * Config.PercentLevels / 100f)));
			if (random.NextDouble() < 0.1)
			{
				levelsDown = levelsDown * 2 - 1;
			}
			if (__instance.mineLevel < 220 && __instance.mineLevel + levelsDown > 220)
			{
				levelsDown = 220 - __instance.mineLevel;
			}
			if ( Config.PreventGoingToSkullCave && __instance.mineLevel < 120 && __instance.mineLevel + levelsDown > 120)
			{
				levelsDown = 120 - __instance.mineLevel;
			}

				levelsFallen = levelsDown;
			mineLevel = __instance.mineLevel;

			MethodInfo afterFallInfo = __instance.GetType().GetMethod("afterFall",
			BindingFlags.NonPublic | BindingFlags.Instance);

			Game1.player.health = Math.Max(1, Game1.player.health - levelsDown * (int)Math.Round(3f * Config.PercentDamage/100f));
			___isFallingDownShaft = true;
			Game1.globalFadeToBlack(new Game1.afterFadeFunction(ModEntry.afterFall), 0.045f);
			Game1.player.CanMove = false;
			Game1.player.jump();

			return false;
		}
		private static int mineLevel;
		private static int levelsFallen;
		private static void afterFall()
		{
			Game1.drawObjectDialogue(Game1.content.LoadString((levelsFallen > 7) ? "Strings\\Locations:Mines_FallenFar" : "Strings\\Locations:Mines_Fallen", levelsFallen));
			Game1.messagePause = true;
			Game1.enterMine(mineLevel + levelsFallen);
			Game1.fadeToBlackAlpha = 1f;
			Game1.player.faceDirection(2);
			Game1.player.showFrame(5, false);
		}

		private static bool placementAction_prefix(StardewValley.Object __instance, ref bool __result, GameLocation location, int x, int y)
		{
			int num = __instance.ParentSheetIndex;
			if (num != 39)
				return true;

			Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
			if (location is MineShaft)
			{
				if ((location as MineShaft).shouldCreateLadderOnThisLevel() && recursiveTryToCreateLadderDown(location as MineShaft, placementTile, "hoeHit", 16))
				{
					__result = true;
					return false;
				}
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
			}
			__result = false;
			return false;
		}

		private static bool recursiveTryToCreateLadderDown(MineShaft location, Vector2 centerTile, string sound, int maxIterations)
		{

			int iterations = 0;
			Queue<Vector2> positionsToCheck = new Queue<Vector2>();
			positionsToCheck.Enqueue(centerTile);
			List<Vector2> closedList = new List<Vector2>();
			while (iterations < maxIterations && positionsToCheck.Count > 0)
			{
				Vector2 currentPoint = positionsToCheck.Dequeue();
				closedList.Add(currentPoint);
				if (!location.isTileOccupied(currentPoint, "ignoreMe", false) && location.isTileOnClearAndSolidGround(currentPoint) && location.isTileOccupiedByFarmer(currentPoint) == null && location.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back") != null && location.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back").Equals("Stone"))
				{
					location.playSound("hoeHit", NetAudio.SoundContext.Default);
					location.createLadderDown((int)currentPoint.X, (int)currentPoint.Y, true);
					return true;
				}
				foreach (Vector2 v in Utility.DirectionsTileVectors)
				{
					if (!closedList.Contains(currentPoint + v))
					{
						positionsToCheck.Enqueue(currentPoint + v);
					}
				}
				iterations++;
			}

			return false;
		}
	}

}