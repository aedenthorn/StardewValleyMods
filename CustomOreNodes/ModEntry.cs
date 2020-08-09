using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using static Harmony.AccessTools;
using Object = StardewValley.Object;

namespace CustomOreNodes
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod, IAssetEditor
	{

		public static ModEntry context;

		internal static ModConfig Config;
		private static List<CustomOreNode> CustomOreNodes = new List<CustomOreNode>();
		private static IMonitor SMonitor;
		
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
				int extension = (Config.SpriteSheetOffsetRows * 16) + ((CustomOreNodes.Count / (editor.Data.Width / 16) + 1) * 16);
				editor.ExtendImage(minWidth: editor.Data.Width, minHeight: SpringObjectsHeight + extension);
				SMonitor.Log($"extended springobjects by {extension}");
				for (int i = 0; i < CustomOreNodes.Count; i++)
				{
					CustomOreNode node = CustomOreNodes[i];
					SMonitor.Log($"Patching springobjects with {node.spritePath}");
					Texture2D customTexture;
					customTexture = node.texture;
					int x = (i % (editor.Data.Width / 16)) * 16;
					int y = SpringObjectsHeight + (Config.SpriteSheetOffsetRows*16) + (i / (editor.Data.Width / 16)) * 16;
					editor.PatchImage(customTexture, sourceArea: new Rectangle(node.spriteX, node.spriteY, node.spriteW, node.spriteH), targetArea: new Rectangle(x, y, 16, 16));
					SMonitor.Log($"patched springobjects with {node.spritePath}");
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
			SMonitor = this.Monitor;
			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

			harmony.Patch(
			   original: AccessTools.Method(typeof(MineShaft), "chooseStoneType"),
			   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.chooseStoneType_Postfix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(MineShaft), "breakStone"),
			   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.breakStone_Postfix))
			);

            if (Config.AllowCustomOreNodesAboveGround)
            {
				ConstructorInfo ci = typeof(Object).GetConstructor(new Type[] { typeof(Vector2), typeof(int), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool) });
				harmony.Patch(
				   original: ci,
				   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_Prefix)),
				   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_Postfix))
				);
			}


			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			CustomOreNodes.Clear();
			CustomOreData data;
			Dictionary<string, Texture2D> gameTextures = new Dictionary<string, Texture2D>();
			try
			{
				if(File.Exists(Path.Combine(Helper.DirectoryPath, "custom_ore_nodes.json")))
				{
					Dictionary<string, Texture2D> modTextures = new Dictionary<string, Texture2D>();
					data = Helper.Content.Load<CustomOreData>("custom_ore_nodes.json", ContentSource.ModFolder);
					foreach (string nodeInfo in data.nodes)
					{
                        try
						{
							CustomOreNode node = new CustomOreNode(nodeInfo);
							if (node.spriteType == "mod")
							{
                                if (!modTextures.ContainsKey(node.spritePath))
                                {
									modTextures.Add(node.spritePath, Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.ModFolder));
                                }
								node.texture = modTextures[node.spritePath];
							}
							else
							{
								if (!gameTextures.ContainsKey(node.spritePath))
								{
									gameTextures.Add(node.spritePath, Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.GameContent));
								}
								node.texture = gameTextures[node.spritePath];
							}
							CustomOreNodes.Add(node);
						}
						catch(Exception ex)
						{
							SMonitor.Log($"Error parsing node {nodeInfo}: {ex}", LogLevel.Error);
						}
					}
					Monitor.Log($"Got {CustomOreNodes.Count} ores from mod", LogLevel.Debug);

				}
				else
                {
					SMonitor.Log("No custom_ore_nodes.json in mod directory.");
				}
			}
			catch(Exception ex)
			{
				SMonitor.Log("Error processing custom_ore_nodes.json: "+ex, LogLevel.Error);
			}

			foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
			{
				try
				{
					Dictionary<string, Texture2D> modTextures = new Dictionary<string, Texture2D>();
					Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
					data = contentPack.ReadJsonFile<CustomOreData>("custom_ore_nodes.json");
					foreach (string nodeInfo in data.nodes)
					{
						try
						{
							CustomOreNode node = new CustomOreNode(nodeInfo);
							if (node.spriteType == "mod")
							{
								if (!modTextures.ContainsKey(node.spritePath))
								{
									modTextures.Add(node.spritePath, contentPack.LoadAsset<Texture2D>(node.spritePath));
								}
								node.texture = modTextures[node.spritePath];
							}
							else
							{
								if (!gameTextures.ContainsKey(node.spritePath))
								{
									gameTextures.Add(node.spritePath, Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.GameContent));
								}
								node.texture = gameTextures[node.spritePath];
							}
							CustomOreNodes.Add(node);
						}
						catch (Exception ex)
						{
							SMonitor.Log($"Error parsing node {nodeInfo}: {ex}", LogLevel.Error);
						}
					}
					Monitor.Log($"Got {data.nodes.Count} ores from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
				}
				catch(Exception ex)
				{
					SMonitor.Log($"Error processing custom_ore_nodes.json in content pack {contentPack.Manifest.Name} {ex}", LogLevel.Error);
				}
			}
			Monitor.Log($"Got {CustomOreNodes.Count} ores total", LogLevel.Debug);
			Helper.Content.InvalidateCache("Maps/springobjects");
		}

		private static void chooseStoneType_Postfix(MineShaft __instance, ref Object __result, Vector2 tile)
		{
			if (__result == null || __result.parentSheetIndex == null)
				return;

			List<int> ores = new List<int>() { 765, 764, 290, 751 };
			if (!ores.Contains(__result.ParentSheetIndex))
			{
				for(int i = 0; i < CustomOreNodes.Count; i++)
				{
					CustomOreNode node = CustomOreNodes[i];
					if (node.minLevel > -1 && __instance.mineLevel < node.minLevel || node.maxLevel > -1 && __instance.mineLevel > node.maxLevel)
					{
						continue;
					}
					if(Game1.random.NextDouble() < node.spawnChance/100f)
					{
						int index = (SpringObjectsHeight / 16 * SpringObjectsWidth / 16) + (Config.SpriteSheetOffsetRows * SpringObjectsWidth / 16) + i;
						//SMonitor.Log($"Displaying stone at index {index}", LogLevel.Debug);
						__result = new Object(tile, index, "Stone", true, false, false, false)
						{
							MinutesUntilReady = node.durability
						};

						//SMonitor.Log(__result.DisplayName);

						return;
					}
				}
			}
		}

		private static void Object_Prefix(ref int parentSheetIndex, ref string Givenname)
		{
			if (Environment.StackTrace.Contains("chooseStoneType"))
			{
				return;
			}
			if (Givenname == "Stone" || parentSheetIndex == 294 || parentSheetIndex == 295)
            {
				for (int i = 0; i < CustomOreNodes.Count; i++)
				{
					if (CustomOreNodes[i].minLevel > 0)
					{
						continue;
					}
					if (Game1.random.NextDouble() < CustomOreNodes[i].spawnChance / 100f)
					{
						int index = (SpringObjectsHeight / 16 * SpringObjectsWidth / 16) + (Config.SpriteSheetOffsetRows * SpringObjectsWidth / 16) + i;
						parentSheetIndex = index;
						break;
					}
				}
			}
		}
		
		private static void Object_Postfix(Object __instance, ref int parentSheetIndex, ref string Givenname)
		{
            if (Givenname == "Stone" || parentSheetIndex == 294 || parentSheetIndex == 295)
            {
				for (int i = 0; i < CustomOreNodes.Count; i++)
				{
					if(parentSheetIndex == (SpringObjectsHeight / 16 * SpringObjectsWidth / 16) + (Config.SpriteSheetOffsetRows * SpringObjectsWidth / 16) + i)
                    {
						__instance.MinutesUntilReady = CustomOreNodes[i].durability;
						break;
					}
				}
			}
		}


		private static void breakStone_Postfix(GameLocation __instance, ref bool __result, int indexOfStone, int x, int y, Farmer who, Random r)
		{
			SMonitor.Log($"Checking for custom ore in stone {indexOfStone}");
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
			SMonitor.Log($"custom node has {node.dropItems.Count} potential items.");
			foreach (DropItem item in node.dropItems)
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

					Game1.createMultipleObjectDebris(itemId, x, y, addedOres + r.Next(item.minAmount, item.maxAmount+1) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? item.luckyAmount : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? item.minerAmount : 0), who.uniqueMultiplayerID, __instance);
				}
			}
			experience = node.exp;
			who.gainExperience(3, experience);
			__result = experience > 0;
		}
	}
}
 