using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomResourceClumps
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor
	{

		public static ModEntry context;

		internal static ModConfig Config;
		public static List<CustomResourceClump> customClumps = new List<CustomResourceClump>();
		public static IMonitor SMonitor;
        private static IModHelper SHelper;
        public static int firstIndex = 816;
		public static int springObjectsHeight = 544;
		public static int springObjectsWidth = 384;
        private int addedHeight;
		public bool finishedLoadingClumps = false;
		public static Dictionary<string, Type> tools = new Dictionary<string, Type>()
		{
			{ "axe", typeof(Axe) },
			{ "pick", typeof(Pickaxe) },
			{ "hoe", typeof(Hoe) },
			{ "sword", typeof(Sword) },
			{ "wateringcan", typeof(WateringCan) },
			{ "wand", typeof(Wand) },
		};
		public static Dictionary<string, int> expTypes = new Dictionary<string, int>()
		{
			{ "farming", 0 },
			{ "foraging", 2 },
			{ "mining", 3 },
			{ "combat", 4 },
		};

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			SMonitor = Monitor;
			SHelper = Helper;
			var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

			harmony.Patch(
			   original: AccessTools.Method(typeof(MineShaft), "populateLevel"),
			   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MineShaft_populateLevel_Postfix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(ResourceClump), nameof(ResourceClump.performToolAction)),
			   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ResourceClump_performToolAction_prefix))
			);

            if (Config.AllowCustomResourceClumpsAboveGround)
            {

			}


			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			customClumps.Clear();
			CustomResourceClumpData data;
			Dictionary<string, Texture2D> gameTextures = new Dictionary<string, Texture2D>();
			try
			{
				if(File.Exists(Path.Combine(Helper.DirectoryPath, "custom_resource_clumps.json")))
				{
					Monitor.Log($"Checking for clumps in mod", LogLevel.Debug);
					Dictionary<string, Texture2D> modTextures = new Dictionary<string, Texture2D>();
					data = Helper.Content.Load<CustomResourceClumpData>("custom_resource_clumps.json", ContentSource.ModFolder);
					foreach (CustomResourceClump clump in data.clumps)
					{
						if (clump.spriteType == "mod")
						{
                            if (!modTextures.ContainsKey(clump.spritePath))
                            {
								modTextures.Add(clump.spritePath, Helper.Content.Load<Texture2D>(clump.spritePath, ContentSource.ModFolder));
                            }
							clump.texture = modTextures[clump.spritePath];
						}
						else
						{
							if (!gameTextures.ContainsKey(clump.spritePath))
							{
								gameTextures.Add(clump.spritePath, Helper.Content.Load<Texture2D>(clump.spritePath, ContentSource.GameContent));
							}
							clump.texture = gameTextures[clump.spritePath];
						}
						customClumps.Add(clump);
					}
					Monitor.Log($"Got {customClumps.Count} clumps from mod", LogLevel.Debug);

				}
				else
                {
					SMonitor.Log("No custom_resource_clumps.json in mod directory.");
				}
			}
			catch(Exception ex)
			{
				SMonitor.Log("Error processing custom_resource_clumps.json: "+ex, LogLevel.Error);
			}

			foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
			{
				try
				{
					Dictionary<string, Texture2D> modTextures = new Dictionary<string, Texture2D>();
					Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
					data = contentPack.ReadJsonFile<CustomResourceClumpData>("custom_resource_clumps.json");
					foreach (CustomResourceClump clump in data.clumps)
					{

						if (clump.spriteType == "mod")
						{
							if (!modTextures.ContainsKey(clump.spritePath))
							{
								modTextures.Add(clump.spritePath, contentPack.LoadAsset<Texture2D>(clump.spritePath));
							}
							clump.texture = modTextures[clump.spritePath];
						}
						else
						{
							if (!gameTextures.ContainsKey(clump.spritePath))
							{
								gameTextures.Add(clump.spritePath, Helper.Content.Load<Texture2D>(clump.spritePath, ContentSource.GameContent));
							}
							clump.texture = gameTextures[clump.spritePath];
						}
						customClumps.Add(clump);

					}
					Monitor.Log($"Got {data.clumps.Count} clumps from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
				}
				catch(Exception ex)
				{
					SMonitor.Log($"Error processing custom_resource_clumps.json in content pack {contentPack.Manifest.Name} {ex}", LogLevel.Error);
				}
			}
			finishedLoadingClumps = true;
			Monitor.Log($"Got {customClumps.Count} clumps total", LogLevel.Debug);
			Helper.Content.InvalidateCache("Maps/springobjects");
		}

		private static void MineShaft_populateLevel_Postfix(MineShaft __instance)
		{
			SMonitor.Log($"checking for custom clumps after populate level. clumps: {__instance.resourceClumps.Count}");

			for (int i = 0; i < __instance.resourceClumps.Count; i++)
			{
				float currentChance = 0;

				foreach (CustomResourceClump clump in customClumps)
                {
					if (clump.minLevel > -1 && __instance.mineLevel < clump.minLevel || clump.maxLevel > -1 && __instance.mineLevel > clump.maxLevel)
					{
						continue;
					}
					currentChance += clump.baseSpawnChance + clump.additionalChancePerLevel * __instance.mineLevel;
					SMonitor.Log($"Current chance: {currentChance} for {clump.index} ");

					if (Game1.random.NextDouble() < currentChance / 100f)
					{
						SMonitor.Log($"Converting clump at {__instance.resourceClumps[i].currentTileLocation} to {clump.index} ");
						__instance.resourceClumps[i] = new ResourceClump(clump.index, clump.tileWidth, clump.tileHeight, __instance.resourceClumps[i].tile.Value);
						__instance.resourceClumps[i].health.Value = clump.durability;
						break;
					}
				}
			}
		}

		private static bool ResourceClump_performToolAction_prefix(ref ResourceClump __instance, Tool t, int damage, Vector2 tileLocation, GameLocation location, ref bool __result)
		{
			int indexOfClump = __instance.parentSheetIndex;
			if (indexOfClump - firstIndex < 0)
			{
				return true;
			}
			CustomResourceClump clump = customClumps.FirstOrDefault(c => c.index == indexOfClump);
			if (clump == null)
				return true;

			if (t == null)
			{
				return false;
			}
			SMonitor.Log($"hitting custom clump {indexOfClump}");

			if (!tools.ContainsKey(clump.toolType) || t.GetType() != tools[clump.toolType])
            {
				return false;
            }
			if(t.upgradeLevel < clump.toolMinLevel)
            {
				foreach (string sound in clump.failSounds)
					location.playSound(sound, NetAudio.SoundContext.Default);
				
				Game1.drawObjectDialogue(string.Format(SHelper.Translation.Get("failed"), t.DisplayName));

				Game1.player.jitterStrength = 1f;
				return false;
			}

			float power = Math.Max(1f, (float)(t.upgradeLevel + 1) * 0.75f);
			__instance.health.Value -= power;
			Game1.createRadialDebris(Game1.currentLocation, clump.debrisType, (int)tileLocation.X + Game1.random.Next(__instance.width / 2 + 1), (int)tileLocation.Y + Game1.random.Next(__instance.height / 2 + 1), Game1.random.Next(4, 9), false, -1, false, -1);

			if (__instance.health > 0f)
            {
				foreach (string sound in clump.hitSounds)
					location.playSound(sound, NetAudio.SoundContext.Default);
                if (clump.shake != 0)
                {
					SHelper.Reflection.GetField<float>(__instance, "shakeTimer").SetValue(clump.shake);
					__instance.NeedsUpdate = true;
				}
				return false;
			}

			__result = true;

			foreach (string sound in clump.breakSounds)
				location.playSound(sound, NetAudio.SoundContext.Default);

			Farmer who = t.getLastFarmerToUse();

			int addedItems = who.professions.Contains(18) ? 1 : 0;
			int experience = 0;
			SMonitor.Log($"custom clump has {clump.dropItems.Count} potential items.");
			foreach (DropItem item in clump.dropItems)
			{
				if (Game1.random.NextDouble() < item.dropChance/100) 
				{
					SMonitor.Log($"dropping item {item.itemIdOrName}");

					if(!int.TryParse(item.itemIdOrName, out int itemId))
					{
						foreach(KeyValuePair<int,string> kvp in Game1.objectInformation)
						{
							if (kvp.Value.StartsWith(item.itemIdOrName + "/"))
							{
								itemId = kvp.Key;
								break;
							}
						}
					}
					int amount = addedItems + Game1.random.Next(item.minAmount, Math.Max(item.minAmount + 1, item.maxAmount + 1)) + ((Game1.random.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? item.luckyAmount : 0);
					Game1.createMultipleObjectDebris(itemId, (int)tileLocation.X, (int)tileLocation.Y, amount, who.uniqueMultiplayerID, location);
				}
			}
            if (expTypes.ContainsKey(clump.expType))
            {
				experience = clump.exp;
				who.gainExperience(expTypes[clump.expType], experience);
			}
            else
            {
				SMonitor.Log($"Invalid experience type {clump.expType}", LogLevel.Warn);
            }
			return false;
		}


		/// <summary>Get whether this instance can edit the given asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public bool CanEdit<T>(IAssetInfo asset)
		{
			if (asset.AssetNameEquals("Maps/springobjects"))
			{
				return true;
			}

			return false;
		}

		/// <summary>Edit a matched asset.</summary>
		/// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
		public void Edit<T>(IAssetData asset)
		{
			if (asset.AssetNameEquals("Maps/springobjects") && finishedLoadingClumps && customClumps.Count > 0)
			{
				CalculatePositions();
				var editor = asset.AsImage();
				int extension = (Config.SpriteSheetOffsetRows * 16) + addedHeight * 16;
				editor.ExtendImage(minWidth: editor.Data.Width, minHeight: springObjectsHeight + extension);
				SMonitor.Log($"extended springobjects by {extension}");
				foreach (CustomResourceClump clump in customClumps)
				{
					SMonitor.Log($"Patching springobjects with {clump.spritePath}, index {clump.index}");
					Texture2D customTexture = clump.texture;
					int x = (clump.index % (editor.Data.Width / 16)) * 16;
					int y = clump.index / (editor.Data.Width / 16) * 16;
					SMonitor.Log($"clump pos {x},{y}");
					editor.PatchImage(customTexture, sourceArea: new Rectangle(clump.spriteX, clump.spriteY, clump.tileWidth * 16, clump.tileHeight * 16), targetArea: new Rectangle(x, y, 16 * clump.tileWidth, 16 * clump.tileHeight));
					SMonitor.Log($"patched springobjects with {clump.spritePath}");
				}
			}
		}

		private void CalculatePositions()
		{
			addedHeight = 0;
			int currentAddedHeight = 0;
			int offsetX = 0;
			for (int i = 0; i < customClumps.Count; i++)
			{
				if (offsetX + customClumps[i].tileWidth > springObjectsWidth / 16)
				{
					addedHeight += currentAddedHeight;
					currentAddedHeight = 0;
					offsetX = 0;
				}
				if (customClumps[i].tileHeight > currentAddedHeight)
				{
					currentAddedHeight = customClumps[i].tileHeight;
				}
				customClumps[i].index = firstIndex + (Config.SpriteSheetOffsetRows + addedHeight) * (springObjectsWidth / 16) + offsetX;
				SMonitor.Log($"clump index {customClumps[i].index}");
				offsetX += customClumps[i].tileWidth;
			}
			addedHeight += currentAddedHeight;
			SMonitor.Log($"added height {addedHeight}");
		}

	}
}
 