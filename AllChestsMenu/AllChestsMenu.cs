using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using Object = StardewValley.Object;

namespace AllChestsMenu
{
	public class AllChestsMenu : IClickableMenu
	{
		public const string chestsAnywhereNameKey = "Pathoschild.ChestsAnywhere/Name";
		public const string CSIndexKey = "aedenthorn.AllChestsMenu/CustomSortIndex";
		internal const int windowWidth = 64 * 26;
		internal const int xSpace = 64;

		internal static int scrolled;

		public int scrollInterval = 32;
		public bool focusBottom = false;
		public enum Sort
		{
			LA,
			LD,
			NA,
			ND,
			CA,
			CD,
			IA,
			ID,
			CS
		}
		public Sort currentSort = Sort.LA;
		public List<ChestData> allChestDataList = new();
		public List<ChestData> chestDataList = new();
		public InventoryMenu playerInventoryMenu;
		public bool canScroll;
		public Item heldItem;
		public TemporaryAnimatedSprite poof;
		public List<ClickableComponent> inventoryCells = new();
		public List<ClickableComponent> sortCCList = new();
		public List<ClickableTextureComponent> inventoryButtons = new();
		public List<Rectangle> widgetSources = new()
		{
			new Rectangle(257, 284, 16, 16),
			new Rectangle(162, 440, 16, 16),
			new Rectangle(420, 457, 14, 14),
			new Rectangle(420, 471, 14, 14),
			new Rectangle(240, 320, 16, 16),
			new Rectangle(653, 205, 44, 44)
		};
		public ClickableTextureComponent trashCan;
		public ClickableTextureComponent organizeButton;
		public TextBox locationText;
		public ClickableComponent lastTopSnappedCC;
		public ClickableComponent locationTextCC;
		public ClickableComponent renameBoxCC;
		public Item hoveredItem;
		public string hoverText;
		public string chestLocation;
		public int hoverAmount;
		public float trashCanLidRotation;
		public int heldMenu = -1;
		public int ccMagnitude = 10000000;
		public int cutoff;
		public string whichLocation;
		public string[] widgetText;
		public Dictionary<string, string> sortNames = new();
		public ChestData targetChest;
		public ChestData renamingChest;
		public TextBox renameBox;
		public ClickableTextureComponent okButton;
		public ClickableTextureComponent storeAlikeButton;
		public string filterString;
		public string nameString;
		public string fridgeString;
		public string sortString;

		public AllChestsMenu() : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, -borderWidth - 64, windowWidth + borderWidth * 2, Game1.uiViewport.Height + borderWidth * 2 + 64, false)
		{
			currentSort = ModEntry.Config.CurrentSort;
			cutoff = Game1.uiViewport.Height - 64 * 3 - 8 - borderWidth;
			widgetText = new string[]{
				ModEntry.SHelper.Translation.Get("open"),
				Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"),
				ModEntry.SHelper.Translation.Get("put"),
				ModEntry.SHelper.Translation.Get("take"),
				ModEntry.SHelper.Translation.Get("rename"),
				ModEntry.SHelper.Translation.Get("target")
			};
			filterString = ModEntry.SHelper.Translation.Get("filter");
			nameString = ModEntry.SHelper.Translation.Get("name");
			fridgeString = ModEntry.SHelper.Translation.Get("fridge");
			sortString = ModEntry.SHelper.Translation.Get("sort");

			int columns = 12;
			int rows = Math.Min(3, (int)Math.Ceiling((double)Game1.player.Items.Count / columns));
			int capacity = rows * columns;

			playerInventoryMenu = new InventoryMenu((Game1.uiViewport.Width - 64 * columns) / 2, Game1.uiViewport.Height - 64 * 3 - borderWidth / 2, false, Game1.player.Items, null, capacity, rows);
			SetPlayerInventoryNeighbours();
			trashCan = new ClickableTextureComponent(new Rectangle(playerInventoryMenu.xPositionOnScreen + playerInventoryMenu.width + 64 + 32 + 8, playerInventoryMenu.yPositionOnScreen + 64 + 16, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f, false)
			{
				myID = 4 * ccMagnitude + 2,
				leftNeighborID = 11,
				upNeighborID = 4 * ccMagnitude,
				rightNeighborID = 5 * ccMagnitude
			};
			organizeButton = new ClickableTextureComponent("", new Rectangle(playerInventoryMenu.xPositionOnScreen + playerInventoryMenu.width + 64, playerInventoryMenu.yPositionOnScreen, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f, false)
			{
				myID = 4 * ccMagnitude,
				downNeighborID = 4 * ccMagnitude + 2,
				leftNeighborID = 11,
				rightNeighborID = 4 * ccMagnitude + 1
			};
			storeAlikeButton = new ClickableTextureComponent("", new Rectangle(playerInventoryMenu.xPositionOnScreen + playerInventoryMenu.width + 64 + 64 + 16, playerInventoryMenu.yPositionOnScreen, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"), Game1.mouseCursors, new Rectangle(103, 469, 16, 16), 4f, false)
			{
				myID = 4 * ccMagnitude + 1,
				downNeighborID = 4 * ccMagnitude + 2,
				leftNeighborID = 4 * ccMagnitude,
				rightNeighborID = 5 * ccMagnitude,
				upNeighborID = 4 * ccMagnitude
			};
			locationText = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = xPositionOnScreen + borderWidth,
				Width = (width - playerInventoryMenu.width) / 2 - borderWidth * 2 - 32,
				Y = cutoff + borderWidth + 32,
				Text = whichLocation
			};
			locationTextCC = new ClickableComponent(new Rectangle(locationText.X, locationText.Y, locationText.Width, locationText.Height), "")
			{
				myID = 2 * ccMagnitude,
				upNeighborID = 1 * ccMagnitude,
				rightNeighborID = 0,
				downNeighborID = 2 * ccMagnitude + 1
			};
			renameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = locationText.X,
				Width = locationText.Width,
				Y = locationText.Y + locationText.Height + 48
			};
			renameBoxCC = new ClickableComponent(new Rectangle(renameBox.X, renameBox.Y, renameBox.Width, renameBox.Height), "")
			{
				myID = 2 * ccMagnitude + 1,
				upNeighborID = 2 * ccMagnitude,
				rightNeighborID = 2 * ccMagnitude + 2
			};
			locationText.Selected = false;
			renameBox.Selected = false;
			okButton = new ClickableTextureComponent(new Rectangle(renameBox.X + renameBox.Width + 4, renameBox.Y, 48, 48), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 0.75f, false)
			{
				myID = 2 * ccMagnitude + 2,
				leftNeighborID = 2 * ccMagnitude + 1,
				rightNeighborID = 0
			};

			string[] s = Enum.GetNames(typeof(Sort));

			for (int i = 0; i < s.Length - 1; i++)
			{
				int row = i % 2;
				string name = s[i];
				int idx = 5 * ccMagnitude;

				sortNames[name] = ModEntry.SHelper.Translation.Get("sort-" + name);
				sortCCList.Add(new ClickableComponent(new Rectangle(organizeButton.bounds.X + 156 + i / 2 * 48 + 32, organizeButton.bounds.Y + 64 + row * 48 + 16, 32, 32), name, name)
				{
					myID = idx + i,
					leftNeighborID = i > 2 ? idx + i - 2: 4 * ccMagnitude + 1,
					rightNeighborID = i < s.Length - 2 ? idx + i + 2 : -1,
					downNeighborID = row == 0 ? idx + i + 1 : -1,
					upNeighborID = row == 1 ? idx + i - 1 : -1
				});
			}
			exitFunction = emergencyShutDown;
			PopulateMenus(true);
			snapToDefaultClickableComponent();
		}

		private void PopulateMenus(bool resetAllChestDataList = false)
		{
			if (resetAllChestDataList || (ModEntry.Config.LimitToCurrentLocation && chestLocation != Game1.currentLocation.Name))
			{
				ResetAllChestList();
			}
			ResetChestList();
		}

		private void ResetAllChestList()
		{
			bool shippingBinAlreadyAdded = false;
			HashSet<string> globalInventoryKeys = new();

			chestLocation = Game1.currentLocation.Name;
			allChestDataList.Clear();
			AddChestsFromLocation(Game1.currentLocation);
			if (!ModEntry.Config.LimitToCurrentLocation)
			{
				foreach (GameLocation location in Game1.locations)
				{
					if (location != Game1.currentLocation)
					{
						AddChestsFromLocation(location);
					}
					if (location.IsBuildableLocation())
					{
						foreach (Building building in location.buildings)
						{
							if (building.indoors.Value is not null)
							{
								if (building.indoors.Value != Game1.currentLocation)
								{
									AddChestsFromLocation(building.indoors.Value);
								}
							}
						}
					}
				}
			}

			void AddChestsFromLocation(GameLocation location)
			{
				GameLocation parentLocation = location.GetParentLocation();
				string locationDisplayName = location.DisplayName;

				if (parentLocation is not null)
				{
					Building building = parentLocation.getBuildingByName(location.NameOrUniqueName);

					if (building is not null)
					{
						locationDisplayName = TokenParser.ParseText(building.GetData().Name);
					}
				}
				if (location is FarmHouse && ModEntry.Config.IncludeFridge)
				{
					FarmHouse farmhouse = location as FarmHouse;

					if (farmhouse.upgradeLevel > 0)
					{
						string label = locationDisplayName;
						Chest fridge = farmhouse.fridge.Value;

						RestoreNulls(fridge);
						if (!fridge.modData.TryGetValue(chestsAnywhereNameKey, out string fridgeName) || string.IsNullOrEmpty(fridgeName))
						{
							label = $"{locationDisplayName} ({fridgeString})";
							fridgeName = fridgeString;
						}
						else
						{
							label = $"{fridgeName} ({fridgeString})";
						}
						allChestDataList.Add(new ChestData() { chest = fridge, name = fridgeName, location = farmhouse.NameOrUniqueName, tile = new Vector2(-1, -1), label = label, originalIndex = allChestDataList.Count, index = allChestDataList.Count });
					}
				}
				if (!shippingBinAlreadyAdded && ModEntry.Config.IncludeShippingBin && location.IsBuildableLocation())
				{
					foreach (Building building in location.buildings)
					{
						if (building is ShippingBin)
						{
							string label = $"{Game1.content.LoadString("Strings\\Buildings:ShippingBin_Name")} ({building.tileX.Value},{building.tileY.Value})";
							ShippingBinChest shippingBin = new();

							shippingBinAlreadyAdded = true;
							RestoreNulls(shippingBin);
							allChestDataList.Add(new ChestData() { chest = shippingBin, name = "", location = location.NameOrUniqueName, tile = new Vector2(building.tileX.Value, building.tileY.Value), label = label, originalIndex = allChestDataList.Count, index = allChestDataList.Count });
							break;
						}
					}
				}
				if (!shippingBinAlreadyAdded && ModEntry.Config.IncludeShippingBin && location.Name.Equals("IslandWest") && Game1.MasterPlayer.hasOrWillReceiveMail("Island_UpgradeHouse") && !CompatibilityUtility.IsBuildableGingerIslandFarmLoaded)
				{
					string label = $"{Game1.content.LoadString("Strings\\Buildings:ShippingBin_Name")} ({90},{39})";
					ShippingBinChest shippingBin = new();

					shippingBinAlreadyAdded = true;
					RestoreNulls(shippingBin);
					allChestDataList.Add(new ChestData() { chest = shippingBin, name = "", location = location.NameOrUniqueName, tile = new Vector2(90, 39), label = label, originalIndex = allChestDataList.Count, index = allChestDataList.Count });
				}
				foreach (KeyValuePair<Vector2, Object> kvp in location.objects.Pairs)
				{
					string label = $"{locationDisplayName} ({kvp.Key.X},{kvp.Key.Y})";
					Object obj = kvp.Value;
					Chest chest;

					if (obj is Chest objAsChest && objAsChest.playerChest.Value && objAsChest.CanBeGrabbed && (!objAsChest.fridge.Value || ModEntry.Config.IncludeMiniFridges) && (objAsChest.SpecialChestType != Chest.SpecialChestTypes.MiniShippingBin || ModEntry.Config.IncludeMiniShippingBins) && (objAsChest.SpecialChestType != Chest.SpecialChestTypes.JunimoChest || ModEntry.Config.IncludeJunimoChests) && !objAsChest.modData.ContainsKey("aedenthorn.AdvancedLootFramework/IsAdvancedLootFrameworkChest"))
					{
						chest = objAsChest;
					}
					else if (obj.heldObject.Value is Chest objHeldObjectAsChest && ModEntry.Config.IncludeAutoGrabbers)
					{
						chest = objHeldObjectAsChest;
					}
					else
					{
						continue;
					}
					if (chest.GlobalInventoryId is not null)
					{
						if (globalInventoryKeys.Contains(chest.GlobalInventoryId))
							continue;

						globalInventoryKeys.Add(chest.GlobalInventoryId);
						chest.netItems.Value = Game1.player.team.GetOrCreateGlobalInventory(chest.GlobalInventoryId);
					}
					if (chest.specialChestType.Value == Chest.SpecialChestTypes.JunimoChest)
					{
						if (globalInventoryKeys.Contains("JunimoChests"))
							continue;

						globalInventoryKeys.Add("JunimoChests");
						chest.netItems.Value = Game1.player.team.GetOrCreateGlobalInventory("JunimoChests");
					}
					RestoreNulls(chest);
					if (chest.modData.TryGetValue(chestsAnywhereNameKey, out string chestName) && !string.IsNullOrEmpty(chestName))
					{
						label = $"{chestName} ({kvp.Key.X},{kvp.Key.Y})";
					}
					else
					{
						chestName = "";
					}
					allChestDataList.Add(new ChestData() { chest = chest, name = chestName, location = location.NameOrUniqueName, tile = new Vector2(kvp.Key.X, kvp.Key.Y), label = label, originalIndex = allChestDataList.Count, index = allChestDataList.Count });
				}
			}

			SortAllChestDataList();
		}

		private void ResetChestList()
		{
			int menusAlready = 0;
			int rowsAlready = 0;
			bool even = false;
			int oddRows = 0;

			chestDataList.Clear();
			for (int i = 0; i < allChestDataList.Count; i++)
			{
				allChestDataList[i].index = i;

				ChestData chestData = allChestDataList[i];

				if (!string.IsNullOrEmpty(whichLocation) && !chestData.label.ToLower().Contains(whichLocation.ToLower()))
				{
					continue;
				}

				int columns = 12;
				int rows = (int)Math.Ceiling(chestData.chest.GetActualCapacity() / (float)columns);

				chestData.menu = new ChestMenu(xPositionOnScreen + borderWidth + (even ? (64 * 13) : 0), yPositionOnScreen - scrolled * scrollInterval + borderWidth + 64 + 64 * rowsAlready + xSpace * (1 + menusAlready), false, chestData.chest.Items, null, chestData.chest.GetActualCapacity(), rows);
				if (chestData.chest is ShippingBinChest && !ModEntry.Config.UnrestrictedShippingBin)
				{
					chestData.menu.highlightMethod = (Item i) => {
						return i == Game1.getFarm().lastItemShipped;
					};
				}
				if (!even)
				{
					oddRows = !chestData.collapsed ? Math.Max(chestData.menu.rows, 3) : 0;
				}
				else
				{
					rowsAlready += Math.Max(!chestData.collapsed ? Math.Max(chestData.menu.rows, 3) : 0, oddRows);
					menusAlready++;
				}
				even = !even;
				if (chestDataList.Count >= 1000)
				{
					ModEntry.SMonitor.Log("More than 1000 chests. Giving up while we're ahead.", LogLevel.Warn);
					break;
				}
				chestDataList.Add(chestData);
			}
			inventoryButtons.Clear();
			inventoryCells.Clear();
			for (int i = 0; i < chestDataList.Count; i++)
			{
				const int columns = 12;
				ChestData chestData = chestDataList[i];
				int count = chestData.menu.inventory.Count;
				int lastCount = i > 0 ? chestDataList[i - 1].menu.inventory.Count : 0;
				// int nextCount = i < chestDataList.Count - 1 ? chestDataList[i + 1].menu.inventory.Count : 0;
				int lastLastCount = i > 1 ? chestDataList[i - 2].menu.inventory.Count : 0;
				int nextNextCount = i < chestDataList.Count - 2 ? chestDataList[i + 2].menu.inventory.Count : 0;
				int index = ccMagnitude + i * ccMagnitude / 1000;
				int lastIndex = ccMagnitude + (i - 1) * ccMagnitude / 1000;
				int nextIndex = ccMagnitude + (i + 1) * ccMagnitude / 1000;
				int lastLastIndex = ccMagnitude + (i - 2) * ccMagnitude / 1000;
				int nextNextIndex = ccMagnitude + (i + 2) * ccMagnitude / 1000;

				for (int j = 0; j < count; j++)
				{
					int rowIndex = j / columns;
					int columnIndex = j % columns;

					chestData.menu.inventory[j].myID = index + j;
					if (columnIndex == 0)
					{
						if (i > 0 && lastCount > 0)
						{
							int widgetIndex;

							if (chestDataList[i - 1].chest is ShippingBinChest)
							{
								widgetIndex = rowIndex switch
								{
									0 => 0,
									_ => widgetText.Length - 1
								};
							}
							else
							{
								widgetIndex = rowIndex switch
								{
									0 => 0,
									1 => 2,
									2 => 4,
									_ => widgetText.Length - 1
								};
							}
							chestData.menu.inventory[j].leftNeighborID = lastIndex + lastCount + widgetIndex;
						}
						else
						{
							chestData.menu.inventory[j].leftNeighborID = -1;
						}
					}
					else
					{
						chestData.menu.inventory[j].leftNeighborID = index + j - 1;
					}
					if (columnIndex == columns - 1 || (rowIndex == (int)Math.Ceiling((double)count / columns) - 1 && columnIndex == (count % columns) - 1))
					{
						int widgetIndex;

						if (chestData.chest is ShippingBinChest)
						{
							widgetIndex = rowIndex switch
							{
								0 => 0,
								_ => widgetText.Length - 1
							};
						}
						else
						{
							widgetIndex = rowIndex switch
							{
								0 => 0,
								1 => 2,
								2 => 4,
								_ => widgetText.Length - 1
							};
						}
						chestData.menu.inventory[j].rightNeighborID = index + count + widgetIndex;
					}
					else
					{
						chestData.menu.inventory[j].rightNeighborID = index + j + 1;
					}
					if (j >= count - columns)
					{
						if (i < chestDataList.Count - 2)
						{
							chestData.menu.inventory[j].downNeighborID = nextNextIndex + columnIndex;
						}
						else
						{
							chestData.menu.inventory[j].downNeighborID = -1;
						}
					}
					else
					{
						chestData.menu.inventory[j].downNeighborID = index + j + columns;
					}
					if (j < columns)
					{
						int lastLastLastRowColumns = lastLastCount % 12;

						if (i > 1)
						{
							if (columnIndex < lastLastLastRowColumns)
							{
								int lastLastLastRowIndex = lastLastCount - lastLastLastRowColumns;

								chestData.menu.inventory[j].upNeighborID = lastLastIndex + lastLastLastRowIndex + columnIndex;
							}
							else
							{
								int lastLastSecondLastRowIndex = lastLastCount - lastLastLastRowColumns - columns;

								if (lastLastSecondLastRowIndex >= 0)
								{
									chestData.menu.inventory[j].upNeighborID = lastLastIndex + lastLastSecondLastRowIndex + columnIndex;
								}
								else
								{
									chestData.menu.inventory[j].upNeighborID = lastLastIndex + lastLastCount - 1;
								}
							}
						}
						else
						{
							chestData.menu.inventory[j].upNeighborID = -1;
						}
					}
					else
					{
						chestData.menu.inventory[j].upNeighborID = index + j - columns;
					}
					inventoryCells.Add(chestData.menu.inventory[j]);
				}
				chestData.inventoryButtons.Clear();
				if (chestData.chest is ShippingBinChest)
				{
					const int widgetTextLength = 2;

					for (int j = 0, k = 0; j < widgetTextLength; j++, k += 5)
					{
						int LeftRowIndex = Math.Min(j / 2, (int)Math.Ceiling((double)count / columns) - 1);
						int RightRowIndex = i < chestDataList.Count - 1 ? Math.Min(j / 2, (int)Math.Ceiling((double)chestDataList[i + 1].menu.inventory.Count / columns) - 1) : -1;
						ClickableTextureComponent cc = new("", GetWidgetRectangle(chestData, j), "", widgetText[k], Game1.mouseCursors, widgetSources[k], 32f / widgetSources[k].Width, false)
						{
							myID = index + count + k,
							downNeighborID = k < widgetTextLength - 1 ? index + count + k + 5 : (i < chestDataList.Count - 2 ? nextNextIndex + nextNextCount : -1),
							leftNeighborID = index + 11 + LeftRowIndex * columns - ((LeftRowIndex == (int)Math.Ceiling((double)count / columns) - 1) ? columns - (count % columns) : 0),
							rightNeighborID = i < chestDataList.Count - 1 ? nextIndex + RightRowIndex * columns : -1,
							upNeighborID = k > 0 ? index + count + k - 5: (i > 1 ? lastLastIndex + lastLastCount + widgetTextLength - 1: -1)
						};

						chestData.inventoryButtons.Add(cc);
						inventoryButtons.Add(cc);
					}
				}
				else
				{
					for (int j = 0; j < widgetText.Length; j++)
					{
						int LeftRowIndex = Math.Min(j / 2, (int)Math.Ceiling((double)count / columns) - 1);
						int RightRowIndex = i < chestDataList.Count - 1 ? Math.Min(j / 2, (int)Math.Ceiling((double)chestDataList[i + 1].menu.inventory.Count / columns) - 1) : -1;
						ClickableTextureComponent cc = new("", GetWidgetRectangle(chestData, j), "", widgetText[j], Game1.mouseCursors, widgetSources[j], 32f / widgetSources[j].Width, false)
						{
							myID = index + count + j,
							downNeighborID = j < widgetText.Length - 1 ? index + count + j + 1 : (i < chestDataList.Count - 2 ? nextNextIndex + nextNextCount : -1),
							leftNeighborID = index + 11 + LeftRowIndex * columns - ((LeftRowIndex == (int)Math.Ceiling((double)count / columns) - 1) ? columns - (count % columns) : 0),
							rightNeighborID = i < chestDataList.Count - 1 ? nextIndex + RightRowIndex * columns : -1,
							upNeighborID = j > 0 ? index + count + j - 1: (i > 1 ? lastLastIndex + lastLastCount + widgetText.Length - 1: -1)
						};

						chestData.inventoryButtons.Add(cc);
						inventoryButtons.Add(cc);
					}
				}
			}
			populateClickableComponentList();
		}

		public void UpdateShippingBinChestMenu(Chest chest, Item item)
		{
			if (chest is ShippingBinChest)
			{
				Game1.getFarm().lastItemShipped = item;
				foreach (ChestData chestData in allChestDataList)
				{
					if (chestData.chest is ShippingBinChest)
					{
						RestoreNulls(chestData.chest);
					}
				}
				PopulateMenus(false);
			}
		}

		public static void RestoreNulls(Chest chest)
		{
			if (chest is ShippingBinChest)
			{
				for (int i = chest.Items.Count - 1; i >= 0; i--)
				{
					if (chest.Items[i] == null)
					{
						chest.Items.RemoveAt(i);
					}
				}
				chest.Items.Add(null);
			}
			else
			{
				int capacity = chest.GetActualCapacity();

				for (int i = chest.Items.Count; i < capacity; i++)
				{
					chest.Items.Add(null);
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
			canScroll = (chestDataList.Count > 0 && chestDataList[^1].menu.yPositionOnScreen + Math.Max(chestDataList[^1].menu.rows, 3) * 64 + borderWidth > cutoff) || (chestDataList.Count > 1 && chestDataList[^2].menu.yPositionOnScreen + Math.Max(chestDataList[^2].menu.rows, 3) * 64 + borderWidth > cutoff);
			for (int i = 0; i < chestDataList.Count; i++)
			{
				ChestData chestData = chestDataList[i];

				if (canScroll && chestData.menu.yPositionOnScreen - 48 > cutoff + 64 * 4)
				{
					break;
				}
				if (i == heldMenu - (chestDataList[i].index - i))
				{
					continue;
				}
				SpriteText.drawString(b, chestData.label, chestData.menu.xPositionOnScreen, chestData.menu.yPositionOnScreen - 48);
				if (!chestData.collapsed)
				{
					chestData.menu.draw(b);
					for (int j = 0; j < chestData.inventoryButtons.Count; j++)
					{
						chestData.inventoryButtons[j].draw(b, targetChest?.index != chestData.index && j == chestData.inventoryButtons.Count - 1 ? Color.Purple : Color.White, 1);
					}
				}
			}
			Game1.drawDialogueBox(xPositionOnScreen, cutoff - borderWidth * 2, width, 64 * 4 + borderWidth * 2, false, true, null, false, true);
			playerInventoryMenu.draw(b);
			SpriteText.drawString(b, filterString, locationText.X + 16, locationText.Y - 48);
			locationText.Draw(b);
			if (renamingChest is not null)
			{
				SpriteText.drawString(b, nameString, renameBox.X + 16, renameBox.Y - 48);
				renameBox.Draw(b);
				okButton.draw(b);
			}
			SpriteText.drawStringHorizontallyCenteredAt(b, sortString, organizeButton.bounds.X + 156 + 32 * 2 + 24 + 32, organizeButton.bounds.Y + 16);
			foreach (ClickableComponent cc in sortCCList)
			{
				b.DrawString(Game1.smallFont, cc.label, cc.bounds.Location.ToVector2() + new Vector2(-1, 1), currentSort.ToString() == cc.label ? Color.Green : Color.Black);
				b.DrawString(Game1.smallFont, cc.label, cc.bounds.Location.ToVector2(), currentSort.ToString() == cc.label ? Color.LightGreen : Color.White);
			}
			trashCan.draw(b);
			organizeButton.draw(b);
			storeAlikeButton.draw(b);
			b.Draw(Game1.mouseCursors, new Vector2(trashCan.bounds.X + 60, trashCan.bounds.Y + 40), new Rectangle?(new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10)), Color.White, trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
			Game1.spriteBatch.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + 16, -4, 24, 16), new Rectangle(16, 16, 24, 16), Color.White);
			Game1.spriteBatch.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + width - 32, -4, 16, 16), new Rectangle(225, 16, 16, 16), Color.White);
			Game1.spriteBatch.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + 40, -4, width - 72, 16), new Rectangle(40, 16, 1, 16), Color.White);
			if (hoverText != null && hoveredItem == null)
			{
				if (hoverAmount > 0)
				{
					drawToolTip(b, hoverText, "", null, true, -1, 0, null, -1, null, hoverAmount);
				}
				else
				{
					drawHoverText(b, hoverText, Game1.smallFont);
				}
			}
			if (hoveredItem != null)
			{
				drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
			}
			heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
			if (heldMenu > -1)
			{
				SpriteText.drawString(b, allChestDataList[heldMenu].label, Game1.getOldMouseX(), Game1.getOldMouseY() - 48);
				b.Draw(Game1.staminaRect, new Rectangle(Game1.getOldMouseX(), Game1.getOldMouseY(), 64 * 12, allChestDataList[heldMenu].menu.rows * 64), Color.LightGray * 0.5f);
			}
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			emergencyShutDown();
			Game1.activeClickableMenu = new AllChestsMenu();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Item held = heldItem;
			Rectangle rect;

			renameBox.Selected = false;
			locationText.Selected = false;
			if (y >= cutoff)
			{
				if (heldMenu > -1)
				{
					return;
				}
				heldItem = playerInventoryMenu.leftClick(x, y, heldItem, false);
				if (heldItem != held)
				{
					if (heldItem != null)
					{
						Game1.playSound("bigSelect");
						if (targetChest is not null)
						{
							Item lastItemShipped = heldItem;

							heldItem = AddItemToInventory(targetChest.chest.Items, heldItem, targetChest.chest is ShippingBinChest, targetChest.chest.specialChestType.Value == Chest.SpecialChestTypes.MiniShippingBin);
							if (heldItem != lastItemShipped)
							{
								UpdateShippingBinChestMenu(targetChest.chest, lastItemShipped);
							}
						}
					}
					else
					{
						Game1.playSound("bigDeSelect");
					}
					return;
				}
				if (renamingChest is not null)
				{
					renameBox.Update();
					if (okButton.containsPoint(x, y))
					{
						RenameChest();
						return;
					}
				}
				locationText.Update();
				if (trashCan != null && trashCan.containsPoint(x, y) && heldItem != null && heldItem.canBeTrashed())
				{
					Utility.trashItem(heldItem);
					heldItem = null;
					return;
				}
				if (organizeButton.containsPoint(x, y))
				{
					Game1.playSound("Ship");
					ItemGrabMenu.organizeItemsInList(Game1.player.Items);
					return;
				}
				if (storeAlikeButton.containsPoint(x, y))
				{
					Game1.playSound("Ship");
					foreach (ChestData s in chestDataList)
					{
						SwapContents(Game1.player.Items, s, true);
					}
					return;
				}
				foreach (ClickableComponent cc in sortCCList)
				{
					if (cc.containsPoint(x, y))
					{
						Game1.playSound("bigSelect");
						currentSort = (Sort)Enum.Parse(typeof(Sort), cc.name);
						ModEntry.Config.CurrentSort = currentSort;
						ModEntry.SHelper.WriteConfig(ModEntry.Config);
						SortAllChestDataList();
						PopulateMenus(false);
						return;
					}
				}
			}
			else
			{
				for (int i = 0; i < chestDataList.Count; i++)
				{
					for (int j = 0; j < chestDataList[i].inventoryButtons.Count; j++)
					{
						if (chestDataList[i].inventoryButtons[j].containsPoint(x, y))
						{
							ClickWidget(chestDataList[i], j);
							return;
						}
					}
					rect = new Rectangle(chestDataList[i].menu.xPositionOnScreen, chestDataList[i].menu.yPositionOnScreen - 48, (width - borderWidth * 2 - 64) / 2, 48);

					static bool isWithinBounds(int x, int y, ChestMenu chestMenu, int otherChestMenuHeight)
					{
						if (x - chestMenu.xPositionOnScreen < chestMenu.width && x - chestMenu.xPositionOnScreen >= 0 && y - chestMenu.yPositionOnScreen < Math.Max(208, Math.Max(chestMenu.height, otherChestMenuHeight)))
						{
							return y - chestMenu.yPositionOnScreen >= 0;
						}
						return false;
					}

					int otherChestMenuIndex = i % 2 == 0 ? i + 1 : i - 1;
					bool otherChestIsInRange = 0 <= otherChestMenuIndex && otherChestMenuIndex < chestDataList.Count;

					if (rect.Contains(new Point(x, y)) || (heldMenu > -1 && isWithinBounds(x, y, chestDataList[i].menu, otherChestIsInRange ? chestDataList[otherChestMenuIndex].menu.height : 0)))
					{
						if (heldMenu > -1)
						{
							SwapMenus(heldMenu, chestDataList[i].index);
						}
						else
						{
							heldMenu = chestDataList[i].index;
							Game1.playSound("bigSelect");
						}
						return;
					}
					if (chestDataList[i].collapsed || heldMenu > -1)
					{
						continue;
					}
					heldItem = chestDataList[i].menu.LeftClick(x, y, heldItem, false, chestDataList[i].chest is ShippingBinChest, chestDataList[i].chest.specialChestType.Value == Chest.SpecialChestTypes.MiniShippingBin);
					if (heldItem != held)
					{
						if (heldItem != null)
						{
							Game1.playSound("bigSelect");
							if (ModEntry.SHelper.Input.IsDown(ModEntry.Config.ModKey))
							{
								heldItem = AddItemToInventory(Game1.player.Items, heldItem);
							}
						}
						else
						{
							Game1.playSound("bigDeSelect");
						}
						UpdateShippingBinChestMenu(chestDataList[i].chest, held);
						return;
					}
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (heldMenu > -1)
				return;

			Item held = heldItem;

			if (y >= cutoff)
			{
				heldItem = playerInventoryMenu.rightClick(x, y, heldItem, false);
				if (heldItem != held)
				{
					if (heldItem != null)
					{
						Game1.playSound("bigSelect");
					}
					else
					{
						Game1.playSound("bigDeSelect");
					}
					return;
				}
			}
			else
			{
				for (int i = 0; i < chestDataList.Count; i++)
				{
					heldItem = chestDataList[i].menu.rightClick(x, y, heldItem, false);
					if (heldItem != held)
					{
						if (heldItem != null)
						{
							Game1.playSound("bigSelect");
						}
						else
						{
							Game1.playSound("bigDeSelect");
						}
						return;
					}
				}
			}
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (Game1.getMousePosition().Y >= cutoff)
			{
				return;
			}
			scrollInterval = 64;
			if (direction < 0)
			{
				if (!canScroll)
					return;
				Game1.playSound("shiny4");
				scrolled++;
			}
			else if (scrolled > 0)
			{
				Game1.playSound("shiny4");
				scrolled--;
			}
			PopulateMenus(false);
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				applyMovementKey(key);
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
			{
				exitThisMenu(true);
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && heldItem != null)
			{
				Game1.setMousePosition(trashCan.bounds.Center);
			}
			if (key == Keys.Delete && heldItem != null && heldItem.canBeTrashed())
			{
				Utility.trashItem(heldItem);
				heldItem = null;
			}
			if (key.Equals(Keys.Delete) && heldItem != null && heldItem.canBeTrashed())
			{
				Utility.trashItem(heldItem);
			}
			if (key.Equals(Keys.Enter) && renameBox.Selected)
			{
				if (renamingChest is not null)
				{
					RenameChest();
				}
				renameBox.Selected = false;
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
				return;

			if (currentlySnappedComponent == null)
			{
				if (focusBottom)
				{
					currentlySnappedComponent = getComponentWithID(0);
				}
				else
				{
					if (lastTopSnappedCC is not null)
					{
						currentlySnappedComponent = lastTopSnappedCC;
						lastTopSnappedCC = null;
					}
					else
					{
						currentlySnappedComponent = getComponentWithID(ccMagnitude);
					}
				}
			}
			snapCursorToCurrentSnappedComponent();
		}

		public override void applyMovementKey(int direction)
		{
			if (currentlySnappedComponent != null)
			{
				ClickableComponent next;
				ClickableComponent old = currentlySnappedComponent;

				switch (direction)
				{
					case 0:
						next = getComponentWithID(currentlySnappedComponent.upNeighborID);
						if (focusBottom)
						{
							if (currentlySnappedComponent.myID < playerInventoryMenu.inventory.Count)
							{
								base.applyMovementKey(direction);
								SetPlayerInventoryNeighbours();
								return;
							}
						}
						else
						{
							if (next is not null)
							{
								if (next.bounds.Y < 0)
								{
									int id = currentlySnappedComponent.myID;

									scrolled -= (int)Math.Round(64f / scrollInterval);
									PopulateMenus(false);
									currentlySnappedComponent = getComponentWithID(id);
									snapCursorToCurrentSnappedComponent();
									break;
								}
							}
						}
						if (next is not null)
						{
							currentlySnappedComponent = next;
							snapCursorToCurrentSnappedComponent();
						}
						break;
					case 1:
						if (currentlySnappedComponent.rightNeighborID != -1)
						{
							currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.rightNeighborID);
							snapCursorToCurrentSnappedComponent();
						}
						break;
					case 2:
						next = getComponentWithID(currentlySnappedComponent.downNeighborID);
						if (!focusBottom && next is not null && next.bounds.Y + next.bounds.Height > cutoff)
						{
							int id = currentlySnappedComponent.myID;

							scrolled += (int)Math.Round(64f / scrollInterval);
							PopulateMenus(false);
							currentlySnappedComponent = getComponentWithID(id);
							snapCursorToCurrentSnappedComponent();
							break;
						}
						if (focusBottom && currentlySnappedComponent.myID < playerInventoryMenu.inventory.Count)
						{
							base.applyMovementKey(direction);
							SetPlayerInventoryNeighbours();
							return;
						}
						if (next is not null)
						{
							currentlySnappedComponent = next;
							snapCursorToCurrentSnappedComponent();
						}
						break;
					case 3:
						if (currentlySnappedComponent.leftNeighborID != -1)
						{
							currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.leftNeighborID);
							snapCursorToCurrentSnappedComponent();
						}
						break;
				}
				if (currentlySnappedComponent != old)
				{
					Game1.playSound("shiny4");
				}
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (whichLocation?.ToLower() != locationText.Text.ToLower())
			{
				whichLocation = locationText.Text;
				scrolled = 0;
				PopulateMenus(false);
				lastTopSnappedCC = getComponentWithID(ccMagnitude);
			}
			if (poof != null && poof.update(time))
			{
				poof = null;
			}
		}

		public override void performHoverAction(int x, int y)
		{
			if (heldMenu > -1)
				return;

			hoveredItem = null;
			hoverText = "";
			base.performHoverAction(x, y);

			Item item_grab_hovered_item;

			if (Game1.getMousePosition().Y >= cutoff)
			{
				item_grab_hovered_item = playerInventoryMenu.hover(x, y, heldItem);
				if (item_grab_hovered_item != null)
				{
					hoveredItem = item_grab_hovered_item;
					return;
				}
				organizeButton.tryHover(x, y, 0.1f);
				if (organizeButton.containsPoint(x, y))
				{
					hoverText = organizeButton.hoverText;
					return;
				}
				storeAlikeButton.tryHover(x, y, 0.1f);
				if (storeAlikeButton.containsPoint(x, y))
				{
					hoverText = storeAlikeButton.hoverText;
					return;
				}
				hoverAmount = 0;
				if (trashCan.containsPoint(x, y))
				{
					if (trashCanLidRotation <= 0f)
					{
						Game1.playSound("trashcanlid");
					}
					trashCanLidRotation = Math.Min(trashCanLidRotation + 0.06544985f, 1.57079637f);
					if (heldItem != null && Utility.getTrashReclamationPrice(heldItem, Game1.player) > 0)
					{
						hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
						hoverAmount = Utility.getTrashReclamationPrice(heldItem, Game1.player);
					}
					return;
				}
				else
				{
					trashCanLidRotation = Math.Max(trashCanLidRotation - 0.06544985f, 0f);
				}
				locationText.Hover(x, y);
				foreach (ClickableComponent cc in sortCCList)
				{
					if (cc.containsPoint(x, y))
					{
						hoverText = sortNames[cc.name];
						return;
					}
				}
			}
			else
			{
				for (int i = 0; i < chestDataList.Count; i++)
				{
					for (int j = 0; j < chestDataList[i].inventoryButtons.Count; j++)
					{
						chestDataList[i].inventoryButtons[j].tryHover(x, y);
						if (chestDataList[i].inventoryButtons[j].containsPoint(x, y))
						{
							hoverText = chestDataList[i].inventoryButtons[j].hoverText;
							return;
						}
					}
					if (chestDataList[i].collapsed)
					{
						continue;
					}
					item_grab_hovered_item = chestDataList[i].menu.hover(x, y, heldItem);
					if (item_grab_hovered_item != null)
					{
						hoveredItem = item_grab_hovered_item;
						return;
					}
				}
			}
		}

		public void SwapMenus(int idx1, int idx2)
		{
			if (allChestDataList[idx1].chest is ShippingBinChest || allChestDataList[idx2].chest is ShippingBinChest)
				return;

			if (ModEntry.SHelper.Input.IsDown(ModEntry.Config.ModKey))
			{
				SwapContents(allChestDataList[idx1], allChestDataList[idx2]);
			}
			else
			{
				SwapPositions(idx1, idx2);
			}
			heldMenu = -1;
			Game1.playSound("bigDeSelect");
		}

		public void SwapPositions(int idx1, int idx2)
		{
			if (allChestDataList[idx1].chest is ShippingBinChest || allChestDataList[idx2].chest is ShippingBinChest)
				return;

			(allChestDataList[idx1], allChestDataList[idx2]) = (allChestDataList[idx2], allChestDataList[idx1]);
			allChestDataList[idx1].index = idx1;
			allChestDataList[idx2].index = idx2;
			if (ModEntry.Config.CurrentSort != Sort.CS)
			{
				foreach (GameLocation location in Game1.locations)
				{
					if (location.IsBuildableLocation())
					{
						foreach (Building building in location.buildings)
						{
							if (building.indoors.Value is not null)
							{
								ClearCSIndexKeysFromLocation(building.indoors.Value);
							}
						}
					}
					ClearCSIndexKeysFromLocation(location);
				}
				foreach (ChestData chestData in allChestDataList)
				{
					chestData.chest.modData[CSIndexKey] = $"{chestData.index}";
				}
				currentSort = Sort.CS;
				ModEntry.Config.CurrentSort = Sort.CS;
				ModEntry.SHelper.WriteConfig(ModEntry.Config);
			}
			allChestDataList[idx1].chest.modData[CSIndexKey] = $"{allChestDataList[idx1].index}";
			allChestDataList[idx2].chest.modData[CSIndexKey] = $"{allChestDataList[idx2].index}";

			static void ClearCSIndexKeysFromLocation(GameLocation location)
			{
				GameLocation parentLocation = location.GetParentLocation();

				if (location is FarmHouse)
				{
					FarmHouse farmhouse = location as FarmHouse;

					if (farmhouse.upgradeLevel > 0)
					{
						Chest fridge = farmhouse.fridge.Value;

						fridge.modData.Remove(CSIndexKey);
					}
				}
				foreach (KeyValuePair<Vector2, Object> kvp in location.objects.Pairs)
				{
					Object obj = kvp.Value;

					if (obj is Chest objAsChest && objAsChest.playerChest.Value && objAsChest.CanBeGrabbed)
					{
						objAsChest.modData.Remove(CSIndexKey);
					}
					else if (obj.heldObject.Value is Chest objHeldObjectAsChest && ModEntry.Config.IncludeAutoGrabbers)
					{
						objHeldObjectAsChest.modData.Remove(CSIndexKey);
					}
				}
			}

			if (targetChest is not null)
			{
				if (targetChest.index == idx1)
				{
					targetChest = allChestDataList[idx1];
				}
				else if (targetChest.index == idx2)
				{
					targetChest = allChestDataList[idx2];
				}
			}
			PopulateMenus(false);
		}

		public static void SwapContents(Inventory inventory, ChestData chestData, bool same = false)
		{
			if (chestData.chest is ShippingBinChest)
				return;

			SwapContents(inventory, chestData.chest.Items, same);
		}

		public static void SwapContents(ChestData chestData, Inventory inventory, bool same = false)
		{
			if (chestData.chest is ShippingBinChest)
				return;

			SwapContents(chestData.chest.Items, inventory, same);
		}

		public static void SwapContents(ChestData chestData1, ChestData chestData2, bool same = false)
		{
			if (chestData1.chest is ShippingBinChest || chestData2.chest is ShippingBinChest)
				return;

			SwapContents(chestData1.chest.Items, chestData2.chest.Items, same);
		}

		public static void SwapContents(Inventory inventory1, Inventory inventory2, bool same = false)
		{
			if (!same)
			{
				same = ModEntry.SHelper.Input.IsDown(ModEntry.Config.ModKey2);
			}
			for (int i = 0; i < inventory1.Count; i++)
			{
				Item item = inventory1[i];

				if (item is null)
				{
					continue;
				}
				if (same)
				{
					bool contains = false;

					foreach (Item m in inventory2)
					{
						if (m is not null && m.Name == item.Name)
						{
							contains = true;
							break;
						}
					}
					if (!contains)
					{
						continue;
					}
				}

				Item newItem = AddItemToInventory(inventory2, item);

				if (newItem is null)
				{
					inventory1[i] = null;
				}
				else
				{
					inventory1[i].Stack = newItem.Stack;
				}
			}
		}

		public static Item AddItemToInventory(Inventory inventory, Item toPlace, bool isShippingBinChest = false, bool isMiniShippingBinChest = false)
		{
			if ((isShippingBinChest || isMiniShippingBinChest) && toPlace is not null && !Utility.highlightShippableObjects(toPlace))
				return toPlace;

			if (!isShippingBinChest || ModEntry.Config.UnrestrictedShippingBin)
			{
				for (int i = 0; i < inventory.Count; i++)
				{
					if (inventory[i] is not null && inventory[i].canStackWith(toPlace))
					{
						toPlace.Stack = inventory[i].addToStack(toPlace);
						if (toPlace.Stack <= 0)
						{
							return null;
						}
					}
				}
			}
			for (int i = 0; i < inventory.Count; i++)
			{
				if (inventory[i] is null)
				{
					inventory[i] = toPlace;
					return null;
				}
			}
			return toPlace;
		}

		public static Rectangle GetWidgetRectangle(ChestData chestData, int v)
		{
			return new Rectangle(chestData.menu.xPositionOnScreen + chestData.menu.width + 4, chestData.menu.yPositionOnScreen + v * 33, 32, 32);
		}

		public void ClickWidget(ChestData ChestData, int idx)
		{
			if (ChestData.chest is ShippingBinChest)
			{
				switch (idx)
				{
					case 0:
						open();
						break;
					case 1:
						target();
						break;
				}
			}
			else
			{
				switch (idx)
				{
					case 0:
						open();
						break;
					case 1:
						organize();
						break;
					case 2:
						put();
						break;
					case 3:
						take();
						break;
					case 4:
						rename();
						break;
					case 5:
						target();
						break;
				}
			}

			void open()
			{
				exitThisMenu(false);
				Game1.playSound("bigSelect");
				ChestData.chest.ShowMenu();
			}

			void organize()
			{
				Game1.playSound("Ship");
				ItemGrabMenu.organizeItemsInList(ChestData.chest.Items);
			}

			void put()
			{
				Game1.playSound("stoneStep");
				SwapContents(Game1.player.Items, ChestData);
			}

			void take()
			{
				Game1.playSound("stoneStep");
				SwapContents(ChestData, Game1.player.Items);
			}

			void rename()
			{
				Game1.playSound("bigSelect");
				Rename(ChestData);
			}

			void target()
			{
				if (targetChest?.index == ChestData.index)
				{
					Game1.playSound("bigDeSelect");
					targetChest = null;
				}
				else
				{
					Game1.playSound("bigSelect");
					targetChest = ChestData;
				}
			}
		}

		public void Rename(ChestData ChestData)
		{
			renamingChest = ChestData;
			renameBox.Selected = true;
			renameBox.Text = ChestData.chest.modData.TryGetValue(chestsAnywhereNameKey, out string name) ? name : "";
			if (currentlySnappedComponent is not null)
			{
				focusBottom = true;
				lastTopSnappedCC = currentlySnappedComponent;
				currentlySnappedComponent = renameBoxCC;
				snapCursorToCurrentSnappedComponent();
			}
		}

		private void RenameChest()
		{
			ChestData ChestData = allChestDataList[renamingChest.index];
			GameLocation location = Game1.getLocationFromName(ChestData.location);
			GameLocation parentLocation = location.GetParentLocation();
			string locationDisplayName = location.DisplayName;

			if (parentLocation is not null)
			{
				Building building = parentLocation.getBuildingByName(location.NameOrUniqueName);

				if (building is not null)
				{
					locationDisplayName = TokenParser.ParseText(building.GetData().Name);
				}
			}
			if (location is FarmHouse && ReferenceEquals(ChestData.chest, (location as FarmHouse).fridge.Value))
			{
				if (string.IsNullOrEmpty(renameBox.Text))
				{
					ChestData.chest.modData[chestsAnywhereNameKey] = "";
					ChestData.name = "";
					ChestData.label = $"{locationDisplayName} ({fridgeString})";
				}
				else
				{
					ChestData.chest.modData[chestsAnywhereNameKey] = renameBox.Text;
					ChestData.name = renameBox.Text;
					ChestData.label = $"{ChestData.name} ({fridgeString})";
				}
			}
			else
			{
				if (string.IsNullOrEmpty(renameBox.Text))
				{
					ChestData.chest.modData[chestsAnywhereNameKey] = "";
					ChestData.name = "";
					ChestData.label = $"{locationDisplayName} ({ChestData.tile.X},{ChestData.tile.Y})";
				}
				else
				{
					ChestData.chest.modData[chestsAnywhereNameKey] = renameBox.Text;
					ChestData.name = renameBox.Text;
					ChestData.label = $"{ChestData.name} ({ChestData.tile.X},{ChestData.tile.Y})";
				}
			}
			if (currentSort == Sort.NA || currentSort == Sort.ND)
			{
				SortAllChestDataList();
			}
			renamingChest = null;
			renameBox.Selected = false;
			Game1.playSound("bigSelect");
			if (lastTopSnappedCC is not null)
			{
				focusBottom = false;
				currentlySnappedComponent = lastTopSnappedCC;
				snapCursorToCurrentSnappedComponent();
			}
			PopulateMenus(false);
		}

		private void SortAllChestDataList()
		{
			static int CompareLabels(string labelA, string labelB)
			{
				static (bool, int, int) ExtractCoordinates(string label)
				{
					var match = Regex.Match(label, @"\((\d+),(\d+)\)$");
					if (match.Success)
					{
						int x = int.Parse(match.Groups[1].Value);
						int y = int.Parse(match.Groups[2].Value);
						return (true, x, y);
					}
					return (false, 0, 0);
				}

				var (hasCoordsA, xA, yA) = ExtractCoordinates(labelA);
				var (hasCoordsB, xB, yB) = ExtractCoordinates(labelB);
				string labelWithoutCoordsA = hasCoordsA ? labelA[..labelA.LastIndexOf('(')].Trim() : labelA;
				string labelWithoutCoordsB = hasCoordsB ? labelB[..labelB.LastIndexOf('(')].Trim() : labelB;
				int labelComparison = labelWithoutCoordsA.CompareTo(labelWithoutCoordsB);

				if (labelComparison != 0)
				{
					return labelComparison;
				}
				if (hasCoordsA && hasCoordsB)
				{
					int xComparison = ModEntry.Config.SecondarySortingPriority == "Y" ? yA.CompareTo(yB) : xA.CompareTo(xB);

					if (xComparison != 0)
					{
						return xComparison;
					}
					return ModEntry.Config.SecondarySortingPriority == "Y" ? xA.CompareTo(xB) : yA.CompareTo(yB);
				}
				return hasCoordsA.CompareTo(hasCoordsB);
			}

			allChestDataList.Sort(delegate (ChestData a, ChestData b)
			{
				int result = 0;

				if (a.chest is ShippingBinChest)
					return -1;
				if (b.chest is ShippingBinChest)
					return 1;

				switch (currentSort)
				{
					case Sort.LA:
						result = a.location.CompareTo(b.location);
						if (result == 0)
						{
							result = CompareLabels(a.label, b.label);
						}
						break;
					case Sort.LD:
						result = b.location.CompareTo(a.location);
						if (result == 0)
						{
							result = CompareLabels(b.label, a.label);
						}
						break;
					case Sort.NA:
						result = CompareLabels(a.label, b.label);
						break;
					case Sort.ND:
						result = CompareLabels(b.label, a.label);
						break;
					case Sort.CA:
						result = a.chest.Items.Count.CompareTo(b.chest.Items.Count);
						break;
					case Sort.CD:
						result = b.chest.Items.Count.CompareTo(a.chest.Items.Count);
						break;
					case Sort.IA:
						result = a.chest.Items.Where(i => i is not null).Count().CompareTo(b.chest.Items.Where(i => i is not null).Count());
						break;
					case Sort.ID:
						result = b.chest.Items.Where(i => i is not null).Count().CompareTo(a.chest.Items.Where(i => i is not null).Count());
						break;
					case Sort.CS:
						bool csa = a.chest.modData.ContainsKey(CSIndexKey);
						bool csb = b.chest.modData.ContainsKey(CSIndexKey);

						result = (csa, csb) switch
						{
							(false, false) => 0,
							(true, false) => -1,
							(false, true) => 1,
							(true, true) => int.Parse(a.chest.modData[CSIndexKey]) - int.Parse(b.chest.modData[CSIndexKey])
						};
						if (result == 0)
						{
							result = CompareLabels(a.label, b.label);
						}
						break;
				}
				if (result == 0)
				{
					result = a.originalIndex.CompareTo(b.originalIndex);
				}
				return result;
			});
		}

		private void SetPlayerInventoryNeighbours()
		{
			if (playerInventoryMenu.inventory.Count >= 12)
			{
				playerInventoryMenu.inventory[0].leftNeighborID = 2 * ccMagnitude;
				playerInventoryMenu.inventory[11].rightNeighborID = 4 * ccMagnitude;
				if (playerInventoryMenu.inventory.Count >= 24)
				{
					playerInventoryMenu.inventory[12].leftNeighborID = 2 * ccMagnitude;
					playerInventoryMenu.inventory[23].rightNeighborID = 4 * ccMagnitude + 1;
					if (playerInventoryMenu.inventory.Count >= 36)
					{
						playerInventoryMenu.inventory[24].leftNeighborID = 2 * ccMagnitude;
						playerInventoryMenu.inventory[35].rightNeighborID = 4 * ccMagnitude + 1;
					}
				}
			}
		}

		public virtual void DropHeldItem()
		{
			if (heldItem is not null)
			{
				Game1.playSound("throwDownITem");
				Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.facingDirection.Value, null, -1);
				heldItem = null;
			}
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			if (heldItem != null)
			{
				heldItem = Game1.player.addItemToInventory(heldItem);
			}
			if (heldItem != null)
			{
				DropHeldItem();
			}
			Game1.getFarm().getShippingBin(Game1.player).RemoveEmptySlots();
		}
	}
}
