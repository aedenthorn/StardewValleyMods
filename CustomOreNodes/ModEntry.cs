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

				editor.ExtendImage(minWidth: editor.Data.Width, minHeight: 544+((CustomOreNodes.Count/(editor.Data.Width/16)+1)*16));

				for(int i = 0; i < CustomOreNodes.Count; i++)
				{
					Texture2D customTexture = this.Helper.Content.Load<Texture2D>(CustomOreNodes[i].spritePath, CustomOreNodes[i].spriteType == "mod"? ContentSource.ModFolder : ContentSource.GameContent);
					int x = (i % (editor.Data.Width / 16))*16;
					int y = 544 + (i / (editor.Data.Width / 16))*16;
					editor.PatchImage(customTexture, sourceArea: new Rectangle(CustomOreNodes[i].spriteX, CustomOreNodes[i].spriteY, CustomOreNodes[i].spriteW, CustomOreNodes[i].spriteH), targetArea: new Rectangle(x, y, 16, 16));
				}
			}
			else if (asset.AssetNameEquals("Data/ObjectInformation"))
			{
				var editor = asset.AsDictionary<int, string>();
				for (int i = 0; i < CustomOreNodes.Count; i++)
				{
					editor.Data[i+816] = $"Stone/0/15/Basic/Stone/{CustomOreNodes[i].nodeDesc}";
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

			CustomOreNodeData = Helper.Data.ReadJsonFile<CustomOreData>("custom_ore_nodes.json") ?? new CustomOreData();
			Monitor.Log($"Got {CustomOreNodeData.nodes.Count} ore strings");
			foreach (string nodeInfo in CustomOreNodeData.nodes)
			{
				CustomOreNode node = new CustomOreNode(nodeInfo);
				CustomOreNodes.Add(node);
			}
			Monitor.Log($"Got {CustomOreNodes.Count} ores");
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
						__result = new StardewValley.Object(tile, 816+i, "Stone", true, false, false, false)
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
			if (indexOfStone - 816 < 0 || indexOfStone - 816 >= CustomOreNodes.Count)
			{
				return;
			}

			CustomOreNode node = CustomOreNodes[indexOfStone-816];

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
					PMonitor.Log($"dropping item {item.itemId}");
					Game1.createMultipleObjectDebris(item.itemId, x, y, addedOres + r.Next(item.minAmount, item.maxAmount+1) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? item.luckyAmount : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? item.minerAmount : 0), who.uniqueMultiplayerID, __instance);
				}
			}
			experience = node.exp;
			who.gainExperience(3, experience);
			__result = experience > 0;
		}
	}
}