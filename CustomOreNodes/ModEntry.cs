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
using static Harmony.AccessTools;
using Object = StardewValley.Object;

namespace CustomOreNodes
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod, IAssetEditor
	{

		public static ModEntry context;

		internal static ModConfig Config;
		private static CustomOreData CustomOreNodeData;
		private static List<CustomOreNode> CustomOreNodes = new List<CustomOreNode>();
		private static IMonitor PMonitor;
		
		private static int FirstIndex = 816;
		private static int SpringObjectsHeight = 544;
		private static int SpringObjectsWidth = 384;


		/// <summary>Get whether this instance can edit the given asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public bool CanEdit<T>(IAssetInfo asset)
		{
			if (asset.AssetNameEquals("Maps/springobjects") || asset.AssetNameEquals("Data/ObjectInformation"))
			{
				return true;
			}

			return false;
		}

		/// <summary>Edit a matched asset.</summary>
		/// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
		public void Edit<T>(IAssetData asset)
		{
			if (asset.AssetNameEquals("Maps/springobjects") && CustomOreNodes.Count > 0)
			{

				var editor = asset.AsImage();

				editor.ExtendImage(minWidth: editor.Data.Width, minHeight: SpringObjectsHeight + (Config.SpriteSheetOffsetRows * 16) + ((CustomOreNodes.Count / (editor.Data.Width / 16) + 1) * 16));

				for (int i = 0; i < CustomOreNodes.Count; i++)
				{
					CustomOreNode node = CustomOreNodes[i];
					Texture2D customTexture;
					if (node.spriteType == "mod")
					{
						customTexture = node.texture;
					}
					else
					{
						customTexture = this.Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.GameContent);
					}
					int x = (i % (editor.Data.Width / 16)) * 16;
					int y = SpringObjectsHeight + (Config.SpriteSheetOffsetRows*16) + (i / (editor.Data.Width / 16)) * 16;
					editor.PatchImage(customTexture, sourceArea: new Rectangle(node.spriteX, node.spriteY, node.spriteW, node.spriteH), targetArea: new Rectangle(x, y, 16, 16));
				}
			}
			else if (asset.AssetNameEquals("Data/ObjectInformation"))
			{
				var editor = asset.AsDictionary<int, string>();
				for (int i = 0; i < CustomOreNodes.Count; i++)
				{
					editor.Data[i + 816] = $"Stone/0/15/Basic/Stone/{CustomOreNodes[i].nodeDesc}";
				}
			}
		}


		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			context = this;
			Config = this.Helper.ReadConfig<ModConfig>();
			PMonitor = this.Monitor;
			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

			harmony.Patch(
			   original: AccessTools.Method(typeof(MineShaft), "chooseStoneType"),
			   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.chooseStoneType_Postfix))
			);
			harmony.Patch(
			   original: AccessTools.Method(typeof(MineShaft), "breakStone"),
			   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.breakStone_Postfix))
			);

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			CustomOreNodes.Clear();
			CustomOreData data;
			try
			{
				data = Helper.Content.Load<CustomOreData>("custom_ore_nodes.json", ContentSource.ModFolder);
				foreach (string nodeInfo in data.nodes)
				{
					CustomOreNode node = new CustomOreNode(nodeInfo);
					if (node.spriteType == "mod")
					{
						node.texture = Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.ModFolder);
					}
					CustomOreNodes.Add(node);
				}
				Monitor.Log($"Got {CustomOreNodes.Count} ores from mod",LogLevel.Debug);
			}
			catch
			{
				PMonitor.Log("custom_ore_nodes.json file not found in mod, checking for content packs.", LogLevel.Debug);
			}

			foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
			{
				try
				{
					this.Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
					data = contentPack.ReadJsonFile<CustomOreData>("custom_ore_nodes.json");
					foreach (string nodeInfo in data.nodes)
					{
						CustomOreNode node = new CustomOreNode(nodeInfo);
						if (node.spriteType == "mod")
						{
							node.texture = contentPack.LoadAsset<Texture2D>(node.spritePath);
						}
						CustomOreNodes.Add(node);
					}
					Monitor.Log($"Got {data.nodes.Count} ores from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
				}
				catch
				{
					PMonitor.Log($"custom_ore_nodes.json file not found in content pack {contentPack.Manifest.Name}", LogLevel.Debug);
				}
			}
			Monitor.Log($"Got {CustomOreNodes.Count} ores total", LogLevel.Debug);
		}

		private static void chooseStoneType_Postfix(MineShaft __instance, ref Object __result, Vector2 tile)
		{
			List<int> ores = new List<int>() { 765, 764, 290, 751 };
			if (!ores.Contains(__result.ParentSheetIndex))
			{
				for(int i = 0; i < CustomOreNodes.Count; i++)
				{
					CustomOreNode node = CustomOreNodes[i];
					if (node.minLevel > 0 && __instance.mineLevel < node.minLevel || node.maxLevel > 0 && __instance.mineLevel > node.maxLevel)
					{
						continue;
					}
					if(Game1.random.NextDouble() < node.spawnChance/100f)
					{
						__result = new StardewValley.Object(tile, (SpringObjectsHeight/16 * SpringObjectsWidth/16) + (Config.SpriteSheetOffsetRows * SpringObjectsWidth/16) + i, "Stone", true, false, false, false)
						{
							MinutesUntilReady = node.durability
						};
						break;
					}
				}
			}
		}
		private static void breakStone_Postfix(GameLocation __instance, bool __result, int indexOfStone, int x, int y, Farmer who, Random r)
		{
			PMonitor.Log($"Checking for custom ore in stone {indexOfStone}");
			int firstIndex = FirstIndex + (Config.SpriteSheetOffsetRows * SpringObjectsWidth / 16);
			if (indexOfStone - firstIndex < 0 || indexOfStone - firstIndex >= CustomOreNodes.Count)
			{
				return;
			}

			CustomOreNode node = CustomOreNodes[indexOfStone - firstIndex];

			if (node.minLevel > 0 && !(__instance is MineShaft))
			{
				return;
			}
			if(node.minLevel > 0 && (__instance as MineShaft).mineLevel < node.minLevel)
			{
				return;
			}
			if(__instance is MineShaft && node.maxLevel > 0 && (__instance as MineShaft).mineLevel > node.maxLevel)
			{
				return;
			}

			int addedOres = who.professions.Contains(18) ? 1 : 0;
			int experience = 0;
			PMonitor.Log($"custom node has {node.dropItems.Count} potential items.");
			foreach (DropItem item in node.dropItems)
			{
				if (Game1.random.NextDouble() < item.dropChance/100) 
				{
					PMonitor.Log($"dropping item {item.itemIdOrName}");

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

					Game1.createMultipleObjectDebris(itemId, x, y, addedOres + r.Next(item.minAmount, item.maxAmount+1) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? item.luckyAmount : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? item.minerAmount : 0), who.uniqueMultiplayerID, __instance);
				}
			}
			experience = node.exp;
			who.gainExperience(3, experience);
			__result = experience > 0;
		}
	}
}
 