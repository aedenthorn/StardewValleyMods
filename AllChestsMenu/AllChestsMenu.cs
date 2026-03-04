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
	/// <summary>
	/// Represents an item that can be consolidated from multiple chests
	/// </summary>
	public class ConsolidationItem
	{
		public Item SampleItem { get; set; }
		public int TotalStack { get; set; }
		public List<ChestData> ChestsContaining { get; set; }
		public List<ChestData> ChestsWithSpace { get; set; }
	}

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

		private void CalculateResponsiveLayout()
		{
			int viewportWidth = Game1.graphics?.GraphicsDevice?.Viewport.Width ?? Game1.uiViewport.Width;
			int viewportHeight = Game1.graphics?.GraphicsDevice?.Viewport.Height ?? Game1.uiViewport.Height;
			effectiveViewportWidth = Math.Min(Game1.uiViewport.Width, viewportWidth);
			effectiveViewportHeight = Math.Min(Game1.uiViewport.Height, viewportHeight);
			compactLayout = effectiveViewportWidth < 1250 || effectiveViewportHeight < 820;

			int availableWidth = effectiveViewportWidth - borderWidth * 4;
			int minChestWidth = 64 * 12 + 32; // Largura mínima para um baú

			// Determinar número de colunas baseado no espaço disponível
			numberOfChestColumns = (availableWidth >= minChestWidth * 2 + 64) ? 2 : 1;

			// Recalcular dimensões (Adiciona 64px de largura extra para as bordas laterais)
			int chestAreaWidth = numberOfChestColumns * (64 * 12 + 64) + borderWidth * 2 + 64;
			width = Math.Min(chestAreaWidth, effectiveViewportWidth - borderWidth * 2);
			xPositionOnScreen = (effectiveViewportWidth - width) / 2;
		}

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
		public ClickableTextureComponent clearFiltersButton;
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
        public Rectangle scrollbarRect;
        public Rectangle scrollbarBack;
        public string whichLocation;
		public string[] widgetText;
		public Dictionary<string, string> sortNames = new();
		public ChestData targetChest;
		public ChestData renamingChest;
		public TextBox renameBox;
		public ClickableTextureComponent okButton;
		public ClickableTextureComponent storeAlikeButton;
		public ClickableTextureComponent consolidateButton;
		public ClickableTextureComponent sortAllButton;
		public string filterString;
		public string nameString;

		// New filter fields
		public TextBox itemNameText;
		public TextBox chestLabelText;
		public TextBox itemDescriptionText;
		public ClickableComponent chestLabelCC;
		public ClickableComponent itemNameCC;
		public ClickableComponent itemDescCC;
		public ClickableComponent locationDropdownCC;
		public List<ClickableComponent> locationOptionsCCList = new();
		private bool locationDropdownOpen = false;
		public List<ClickableComponent> filterCheckboxCCList = new();
		public List<ClickableComponent> chestHeaders = new();

		// Filter tracking for real-time updates
		private string lastItemNameFilter = "";
		private string lastItemDescriptionFilter = "";

		// Responsive layout fields
		internal int numberOfChestColumns = 2;
		private readonly Color[] chestBgColors = new Color[]
		{
			new Color(0, 0, 0, 30),    // Semi-transparent gray
			new Color(0, 0, 0, 15)     // Lighter semi-transparent gray
		};

		public int filterAreaHeight;  // Altura da área de filtros
		public int filtersY;          // Posição Y dos filtros
		public int buttonsY;          // Posição Y dos botões
		public int buttonsHeight;     // Altura da área de botões
		public int inventoryHeight;   // Altura do inventário
		public string fridgeString;
		public string sortString;
		
		private bool gamepadShift = false;

		public int clickpos;
        public bool draggingScrollbar;
		public int scrollbarWidth;
        private bool scrolling;
        private int totalHeight;
		public int bottomAreaHeight; // To store for drawing
		private int locationOptionBaseId => 6 * ccMagnitude;
		private int effectiveViewportWidth;
		private int effectiveViewportHeight;
		private bool compactLayout;
		private float trashLidScale = 4f;

        public AllChestsMenu() : base(0, -borderWidth - 64, Game1.uiViewport.Width, Game1.uiViewport.Height + borderWidth * 2 + 64, false)
		{
			// Calcular layout responsivo
			CalculateResponsiveLayout();

			currentSort = ModEntry.Config.CurrentSort;
			int actionButtonSize = compactLayout ? 48 : 64;
			int actionButtonGap = compactLayout ? 6 : 8;
			int actionButtonScale = compactLayout ? 3 : 4;
			trashLidScale = compactLayout ? 3f : 4f;

			// Layout claro: área de baús (topo) e área de inventário/filtros (fundo)
			inventoryHeight = 64 * 3;  // Altura do inventário

			// Filters stack height: 4 filtros + labels + espaçamento
			int filterStackHeight = compactLayout ? 320 : 380;
			int verticalPadding = compactLayout ? 40 : 64;
			bottomAreaHeight = Math.Max(inventoryHeight + verticalPadding, filterStackHeight) + borderWidth + verticalPadding;
			cutoff = effectiveViewportHeight - bottomAreaHeight;  // Y onde a área de baús termina
            
            // Centraliza o conjunto (inventário + botões) empurrado pra direita se necessário
            int smallFilterWidth = compactLayout ? 104 : 120;
            int filterXStart = xPositionOnScreen + borderWidth + 24;
            int filtersTotalWidth = filterXStart + smallFilterWidth + 16;

			int availableVerticalSpace = effectiveViewportHeight - cutoff;
			int inventoryY = cutoff + (availableVerticalSpace - inventoryHeight) / 2;
			widgetText = new string[]{
				ModEntry.SHelper.Translation.Get("open"),
				Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"),
				ModEntry.SHelper.Translation.Get("put"),
				ModEntry.SHelper.Translation.Get("take"),
				ModEntry.SHelper.Translation.Get("rename"),
				ModEntry.SHelper.Translation.Get("target")
			};
			filterString = ModEntry.SHelper.Translation.Get("name"); // "Name" or "Chest" depending on translation
			nameString = ModEntry.SHelper.Translation.Get("name");
			fridgeString = ModEntry.SHelper.Translation.Get("fridge");
			sortString = ModEntry.SHelper.Translation.Get("sort");

			int columns = 12;
			int rows = Math.Min(3, (int)Math.Ceiling((double)Game1.player.Items.Count / columns));
			int capacity = rows * columns;

			// Centraliza o conjunto (inventário + botões)
			int totalBottomWidth = 64 * columns + 32 + (actionButtonSize * 5) + (actionButtonSize + 16); // inventario + espaco + botoes e lixeira
			int startX = (effectiveViewportWidth - totalBottomWidth) / 2;
			startX += 40; // Deslocar para a direita para balancear com os filtros na esqueda
			if (startX < filtersTotalWidth) startX = filtersTotalWidth;

			playerInventoryMenu = new InventoryMenu(
				startX,
				inventoryY,
				false,
				Game1.player.Items,
				null,
				capacity,
				rows);
			SetPlayerInventoryNeighbours();
			organizeButton = new ClickableTextureComponent("", new Rectangle(playerInventoryMenu.xPositionOnScreen + playerInventoryMenu.width + 32, playerInventoryMenu.yPositionOnScreen - (compactLayout ? 8 : 16), actionButtonSize, actionButtonSize), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), actionButtonScale, false)
			{
				myID = 4 * ccMagnitude,
				downNeighborID = 4 * ccMagnitude + 1,
				leftNeighborID = 11,
				rightNeighborID = 4 * ccMagnitude + 1
			};
			storeAlikeButton = new ClickableTextureComponent("", new Rectangle(organizeButton.bounds.X + actionButtonSize + actionButtonGap, organizeButton.bounds.Y, actionButtonSize, actionButtonSize), "", Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"), Game1.mouseCursors, new Rectangle(103, 469, 16, 16), actionButtonScale, false)
			{
				myID = 4 * ccMagnitude + 1,
				downNeighborID = 4 * ccMagnitude + 2,
				leftNeighborID = 4 * ccMagnitude,
				rightNeighborID = 4 * ccMagnitude + 2,
				upNeighborID = 4 * ccMagnitude
			};

			// Consolidate button - merges duplicate items across chests
			string consolidateText = ModEntry.SHelper.Translation.Get("consolidate");
			string consolidateTooltip = ModEntry.SHelper.Translation.Get("consolidate-tooltip");
			consolidateButton = new ClickableTextureComponent("", new Rectangle(storeAlikeButton.bounds.X + actionButtonSize + actionButtonGap, organizeButton.bounds.Y, actionButtonSize, actionButtonSize), "", consolidateTooltip, Game1.mouseCursors, new Rectangle(257, 284, 16, 16), actionButtonScale, false)
			{
				myID = 4 * ccMagnitude + 2,
				downNeighborID = 4 * ccMagnitude + 3,
				leftNeighborID = 4 * ccMagnitude + 1,
				rightNeighborID = 4 * ccMagnitude + 3,
				upNeighborID = 4 * ccMagnitude
			};

			// Sort all button - sorts all items in all chests
			string sortAllText = ModEntry.SHelper.Translation.Get("sort-all");
			string sortAllTooltip = ModEntry.SHelper.Translation.Get("sort-all-tooltip");
			sortAllButton = new ClickableTextureComponent("", new Rectangle(consolidateButton.bounds.X + actionButtonSize + actionButtonGap, organizeButton.bounds.Y, actionButtonSize, actionButtonSize), "", sortAllTooltip, Game1.mouseCursors, new Rectangle(162, 440, 16, 16), actionButtonScale, false)
			{
				myID = 4 * ccMagnitude + 3,
				downNeighborID = 4 * ccMagnitude + 4,
				leftNeighborID = 4 * ccMagnitude + 2,
				rightNeighborID = 5 * ccMagnitude,
				upNeighborID = 4 * ccMagnitude
			};

			// Update trashCan position - moved right of the buttons, aligned vertically
			int trashWidth = compactLayout ? 48 : 64;
			int trashHeight = compactLayout ? 78 : 104;
			trashCan = new ClickableTextureComponent(new Rectangle(sortAllButton.bounds.X + actionButtonSize + 16, playerInventoryMenu.yPositionOnScreen + (compactLayout ? 18 : 28), trashWidth, trashHeight), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), actionButtonScale, false)
			{
				myID = 4 * ccMagnitude + 4,
				leftNeighborID = 4 * ccMagnitude + 3,
				upNeighborID = 4 * ccMagnitude + 3,
				rightNeighborID = 5 * ccMagnitude
			};
			locationTextCC = new ClickableComponent(new Rectangle(0, 0, 0, 0), "")  // Placeholder, será definido depois
			{
				myID = 2 * ccMagnitude,
				upNeighborID = 1 * ccMagnitude,
				rightNeighborID = 0,
				downNeighborID = 2 * ccMagnitude + 1
			};
			renameBoxCC = new ClickableComponent(new Rectangle(0, 0, 0, 0), "")  // Placeholder, será definido depois
			{
				myID = 2 * ccMagnitude + 1,
				upNeighborID = 2 * ccMagnitude,
				rightNeighborID = 2 * ccMagnitude + 2
			};
			okButton = new ClickableTextureComponent(new Rectangle(0, 0, 48, 48), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 0.75f, false)
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
				int sortXStep = compactLayout ? 48 : 64;
				int sortYStart = compactLayout ? 84 : 104;
				int sortYStep = compactLayout ? 32 : 40;
				int sortSize = compactLayout ? 26 : 32;

				sortNames[name] = ModEntry.SHelper.Translation.Get("sort-" + name);
				sortCCList.Add(new ClickableComponent(new Rectangle(organizeButton.bounds.X + (i / 2) * sortXStep, organizeButton.bounds.Y + sortYStart + row * sortYStep, sortSize, sortSize), name, name)
				{
					myID = idx + i,
					leftNeighborID = i > 2 ? idx + i - 2: 4 * ccMagnitude + 1,
					rightNeighborID = i < s.Length - 2 ? idx + i + 2 : -1,
					downNeighborID = row == 0 ? idx + i + 1 : -1,
					upNeighborID = row == 1 ? idx + i - 1 : -1
				});
			}

			// Initialize new filter fields - layout vertical na esquerda
			int filterYStart = effectiveViewportHeight - bottomAreaHeight + borderWidth + (compactLayout ? 52 : 64) + 16;
			int filterSpacing = compactLayout ? 72 : 84;

			// Chest Label filter
			chestLabelText = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = filterXStart,
				Y = filterYStart,
				Width = smallFilterWidth
			};
			chestLabelCC = new ClickableComponent(new Rectangle(chestLabelText.X, chestLabelText.Y, chestLabelText.Width, chestLabelText.Height), "")
			{
				myID = 2 * ccMagnitude + 3,
				upNeighborID = 3 * ccMagnitude + 1,
				downNeighborID = 2 * ccMagnitude + 4,
				rightNeighborID = playerInventoryMenu.inventory[0].myID
			};

			// Item Name filter
			itemNameText = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = filterXStart,
				Y = filterYStart + filterSpacing,
				Width = smallFilterWidth
			};
			itemNameCC = new ClickableComponent(new Rectangle(itemNameText.X, itemNameText.Y, itemNameText.Width, itemNameText.Height), "")
			{
				myID = 2 * ccMagnitude + 4,
				upNeighborID = 2 * ccMagnitude + 3,
				downNeighborID = 2 * ccMagnitude + 6,
				rightNeighborID = playerInventoryMenu.inventory.Count > 12 ? playerInventoryMenu.inventory[12].myID : playerInventoryMenu.inventory[0].myID
			};

			// Item Description filter
			itemDescriptionText = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = filterXStart,
				Y = filterYStart + filterSpacing * 2,
				Width = smallFilterWidth
			};
			itemDescCC = new ClickableComponent(new Rectangle(itemDescriptionText.X, itemDescriptionText.Y, itemDescriptionText.Width, itemDescriptionText.Height), "")
			{
				myID = 2 * ccMagnitude + 6,
				upNeighborID = 2 * ccMagnitude + 4,
				downNeighborID = 2 * ccMagnitude + 7,
				rightNeighborID = playerInventoryMenu.inventory.Count > 24 ? playerInventoryMenu.inventory[24].myID : playerInventoryMenu.inventory[0].myID
			};

			// Location dropdown (vertical na esquerda)
			locationDropdownCC = new ClickableComponent(new Rectangle(filterXStart, filterYStart + filterSpacing * 3, smallFilterWidth, 40), "location", "Location")
			{
				myID = 2 * ccMagnitude + 7,
				upNeighborID = 2 * ccMagnitude + 6,
				downNeighborID = 5 * ccMagnitude,
				rightNeighborID = playerInventoryMenu.inventory.Count > 24 ? playerInventoryMenu.inventory[24].myID : playerInventoryMenu.inventory[0].myID
			};

			// Keep original locationText for backward compatibility
			locationText = chestLabelText;

			// Rename box (for renaming chests) - posicionado fora da tela (escondido)
			renameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = xPositionOnScreen + borderWidth,
				Width = 200,
				Y = -1000  // Escondido fora da tela
			};
			renameBoxCC = new ClickableComponent(new Rectangle(renameBox.X, renameBox.Y, renameBox.Width, renameBox.Height), "")
			{
				myID = 2 * ccMagnitude + 8,
				upNeighborID = 2 * ccMagnitude + 7,
				downNeighborID = 5 * ccMagnitude
			};

			// Filter checkboxes with controller navigation
			int checkboxX = xPositionOnScreen + borderWidth;
			int checkboxSize = 24;

			filterCheckboxCCList.Add(new ClickableComponent(
				new Rectangle(chestLabelText.X - checkboxSize - 4, chestLabelText.Y + 4, checkboxSize, checkboxSize), "chestLabelChk"));
			filterCheckboxCCList.Add(new ClickableComponent(
				new Rectangle(itemNameText.X - checkboxSize - 4, itemNameText.Y + 4, checkboxSize, checkboxSize), "itemNameChk"));
			filterCheckboxCCList.Add(new ClickableComponent(
				new Rectangle(itemDescriptionText.X - checkboxSize - 4, itemDescriptionText.Y + 4, checkboxSize, checkboxSize), "itemDescChk"));
			filterCheckboxCCList.Add(new ClickableComponent(
				new Rectangle(locationDropdownCC.bounds.X - checkboxSize - 4, locationDropdownCC.bounds.Y + 8, checkboxSize, checkboxSize), "locationChk"));

			// Clear filters button - 20px to the right of the top filter, vertically centered with text box
			int clearBtnY = filterYStart - 10;
			int clearBtnX = filterXStart + smallFilterWidth + 20;
			clearFiltersButton = new ClickableTextureComponent(
				"",
				new Rectangle(clearBtnX, clearBtnY, compactLayout ? 40 : 48, compactLayout ? 40 : 48),
				"",
				ModEntry.SHelper.Translation.Get("clear-filters"),
				Game1.mouseCursors,
				new Rectangle(337, 494, 12, 12),  // X icon
				compactLayout ? 3.3f : 4f,
				false
			)
			{
				myID = 4 * ccMagnitude + 5,
				leftNeighborID = 2 * ccMagnitude + 7,
				rightNeighborID = playerInventoryMenu.inventory.Count > 0 ? playerInventoryMenu.inventory[0].myID : -1
			};

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

			// Rebuild location options list whenever menus are populated
			// This ensures the dropdown options are always up-to-date and clickable
			locationOptionsCCList.Clear();
			var uniqueLocations = allChestDataList.GroupBy(c => c.locationDisplayName).Select(g => g.First()).ToList();
			for (int i = 0; i < uniqueLocations.Count; i++)
			{
				string loc = uniqueLocations[i].locationDisplayName;
				string displayLoc = loc;
				locationOptionsCCList.Add(new ClickableComponent(new Rectangle(locationDropdownCC.bounds.X, locationDropdownCC.bounds.Bottom + i * 36, Math.Max(locationDropdownCC.bounds.Width, 300), 36), loc, displayLoc)
				{
					myID = locationOptionBaseId + i
				});
			}
			UpdateLocationOptionBounds();
			RebuildControllerNavigation();

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

			// Método auxiliar para obter a cor do baú real em vez de transparente
			Color GetChestColor(Chest chest)
			{
				if (chest == null)
					return new Color(139, 69, 19, 255);  // Marrom padrão

				// Usar a cor escolhida pelo jogador ou cor padrão
				Color baseColor = chest.playerChoiceColor.Value;
				if (baseColor == Color.Black)
					return new Color(139, 69, 19, 255); // Prevent black chests from being pure black boxes, fallback to standard wood color unless it's a specific black dye (standard chest defaults to black color value initially though default is no dye). Wait, actually default chest color isn't Color.Black, but playerChoiceColor.Value defaults to Color.Black if uncolored. Let's just use it but wait, in SDV uncolored chest has playerChoiceColor.Value == Color.Black. We should check if playerChoiceColor.Value.Equals(Color.Black). Note: default uncolored chest has playerChoiceColor.Value == Color.Black.
				return new Color(baseColor.R, baseColor.G, baseColor.B, (byte)255);
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
						allChestDataList.Add(new ChestData() { chest = fridge, name = fridgeName, location = farmhouse.NameOrUniqueName, locationDisplayName = locationDisplayName, tile = new Vector2(-1, -1), label = label, originalIndex = allChestDataList.Count, index = allChestDataList.Count, chestColor = GetChestColor(fridge) });
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
							allChestDataList.Add(new ChestData() { chest = shippingBin, name = "", location = location.NameOrUniqueName, locationDisplayName = locationDisplayName, tile = new Vector2(building.tileX.Value, building.tileY.Value), label = label, originalIndex = allChestDataList.Count, index = allChestDataList.Count, chestColor = GetChestColor(shippingBin) });
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
					allChestDataList.Add(new ChestData() { chest = shippingBin, name = "", location = location.NameOrUniqueName, locationDisplayName = locationDisplayName, tile = new Vector2(90, 39), label = label, originalIndex = allChestDataList.Count, index = allChestDataList.Count, chestColor = GetChestColor(shippingBin) });
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
					allChestDataList.Add(new ChestData() { chest = chest, name = chestName, location = location.NameOrUniqueName, locationDisplayName = locationDisplayName, tile = new Vector2(kvp.Key.X, kvp.Key.Y), label = label, originalIndex = allChestDataList.Count, index = allChestDataList.Count, chestColor = GetChestColor(chest) });
				}
			}

			SortAllChestDataList();
		}

		private void ResetChestList()
		{
			int menusAlready = 0;
			int locationGaps = 0;
			int rowsAlready = 0;
			bool even = false;
			int oddRows = 0;
			string lastLocation = null;

			chestDataList.Clear();
            inventoryButtons.Clear();
            inventoryCells.Clear();
			if (!allChestDataList.Any())
			{
                populateClickableComponentList();
                return;
            }
            for (int i = 0; i < allChestDataList.Count; i++)
			{
				allChestDataList[i].index = i;

				ChestData chestData = allChestDataList[i];
				
				if (!string.IsNullOrEmpty(whichLocation))
				{
					if (!chestData.label.ToLower().Contains(whichLocation.ToLower().Trim()))
						continue;
				}

				bool hasItemFilter = !string.IsNullOrEmpty(itemNameText.Text) || 
									 !string.IsNullOrEmpty(itemDescriptionText.Text);

				if (hasItemFilter)
				{
					string searchName = itemNameText.Text.ToLower().Trim();
					string searchDesc = itemDescriptionText.Text.ToLower().Trim();

					bool anyItemMatches = chestData.chest.Items.Any(item =>
					{
						if (item == null) return false;
						
						if (!string.IsNullOrEmpty(searchName) && !item.DisplayName.ToLower().Trim().Contains(searchName)) return false;
						if (!string.IsNullOrEmpty(searchDesc) && !item.getDescription().ToLower().Trim().Contains(searchDesc)) return false;
						
						return true;
					});

					if (!anyItemMatches)
						continue;
				}

				if (ModEntry.Config.SelectedLocations.Count > 0 && !ModEntry.Config.SelectedLocations.Contains(chestData.locationDisplayName))
				{
					continue;
				}

                // Location grouping check
                if (lastLocation != null && chestData.locationDisplayName != lastLocation)
                {
                    if (numberOfChestColumns > 1 && even) // In 2-column mode, finish the partial row before starting a new location group
                    {
                        var prevChest = chestDataList[chestDataList.Count - 1];
                        rowsAlready += Math.Max(!prevChest.collapsed ? Math.Max(prevChest.menu.rows, 3) : 0, oddRows);
                        menusAlready++;
                        even = false;
                    }
                    menusAlready++; // Add extra header space for the new location
					locationGaps++;
                }
                else if (lastLocation == null)
                {
                    menusAlready++; // Extra header space for the very first location
                }
                lastLocation = chestData.locationDisplayName;
                chestData.isFirstInLocation = (lastLocation != null && chestDataList.Count > 0 && chestData.locationDisplayName != chestDataList[chestDataList.Count - 1].locationDisplayName) || chestDataList.Count == 0;

				int columns = 12;
				int rows = (int)Math.Ceiling(chestData.chest.GetActualCapacity() / (float)columns);

				int columnIndex = (numberOfChestColumns > 1 && even) ? 1 : 0;
				chestData.menu = new ChestMenu(xPositionOnScreen + borderWidth + 64 + columnIndex * (64 * 13), yPositionOnScreen - scrolled * scrollInterval + borderWidth + 64 + 64 * rowsAlready + 96 * (1 + menusAlready) + 48 * locationGaps, false, chestData.chest.Items, null, chestData.chest.GetActualCapacity(), rows);
				if (chestData.chest is ShippingBinChest && !ModEntry.Config.UnrestrictedShippingBin)
				{
					chestData.menu.highlightMethod = (Item i) => {
						return i == Game1.getFarm().lastItemShipped;
					};
				}
				if (numberOfChestColumns == 1)
				{
					rowsAlready += !chestData.collapsed ? Math.Max(chestData.menu.rows, 3) : 0;
					menusAlready++;
				}
				else
				{
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
				}
				if (chestDataList.Count >= 1000)
				{
					ModEntry.SMonitor.Log("More than 1000 chests. Giving up while we're ahead.", LogLevel.Warn);
					break;
				}
				chestDataList.Add(chestData);
			}
            if (!chestDataList.Any())
			{
                populateClickableComponentList();
                return;
            }

            int lastY = chestDataList[0].menu.yPositionOnScreen;
            int lastHeight = 0;
			totalHeight = 0;
            for (int i = 0; i < chestDataList.Count; i++)
            {
                if (lastY < chestDataList[i].menu.yPositionOnScreen)
                {
                    totalHeight += chestDataList[i].menu.yPositionOnScreen - lastY;
                    lastY = chestDataList[i].menu.yPositionOnScreen;
                }
                if (i >= chestDataList.Count - 2 && lastHeight < chestDataList[i].menu.height)
                {
                    lastHeight = chestDataList[i].menu.height;
                }
            }
            totalHeight += lastHeight;

			chestHeaders.Clear();
			for (int i = 0; i < chestDataList.Count; i++)
			{
				const int columns = 12;
				ChestData chestData = chestDataList[i];
				int count = chestData.menu.inventory.Count;
				int lastCount = i > 0 ? chestDataList[i - 1].menu.inventory.Count : 0;
				int nextCount = i < chestDataList.Count - 1 ? chestDataList[i + 1].menu.inventory.Count : 0;
				int lastLastCount = i > 1 ? chestDataList[i - 2].menu.inventory.Count : 0;
				int nextNextCount = i < chestDataList.Count - 2 ? chestDataList[i + 2].menu.inventory.Count : 0;
				int index = ccMagnitude + i * ccMagnitude / 1000;
				int lastIndex = ccMagnitude + (i - 1) * ccMagnitude / 1000;
				int nextIndex = ccMagnitude + (i + 1) * ccMagnitude / 1000;
				int lastLastIndex = ccMagnitude + (i - 2) * ccMagnitude / 1000;
				int nextNextIndex = ccMagnitude + (i + 2) * ccMagnitude / 1000;

				int headerWidth = numberOfChestColumns == 1 ? width - borderWidth * 2 - 128 : (width - borderWidth * 2 - 128) / 2;
				ClickableComponent headerCC = new ClickableComponent(new Rectangle(chestData.menu.xPositionOnScreen, chestData.menu.yPositionOnScreen - 48, headerWidth, 48), chestData.index.ToString(), chestData.label)
				{
					myID = index + 5000,
					downNeighborID = index + 0,
					rightNeighborID = numberOfChestColumns == 1 ? -1 : ((i % 2 == 0 && i + 1 < chestDataList.Count) ? ccMagnitude + (i + 1) * ccMagnitude / 1000 + 5000 : -1),
					leftNeighborID = numberOfChestColumns == 1 ? -1 : ((i % 2 == 1) ? ccMagnitude + (i - 1) * ccMagnitude / 1000 + 5000 : -1),
					upNeighborID = numberOfChestColumns == 1 ? (i > 0 ? ccMagnitude + (i - 1) * ccMagnitude / 1000 + 5000 : -1) : ((i > 1) ? ccMagnitude + (i - 2) * ccMagnitude / 1000 + 5000 : -1)
				};
				chestHeaders.Add(headerCC);

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
						if (numberOfChestColumns == 1)
						{
							chestData.menu.inventory[j].downNeighborID = i < chestDataList.Count - 1 ? nextIndex + 5000 : -1;
						}
						else if (i < chestDataList.Count - 2)
						{
							chestData.menu.inventory[j].downNeighborID = nextNextIndex + 5000;
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
						chestData.menu.inventory[j].upNeighborID = index + 5000;
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
							downNeighborID = k < widgetTextLength - 1 ? index + count + k + 5 : (numberOfChestColumns == 1 ? (i < chestDataList.Count - 1 ? nextIndex + nextCount : -1) : (i < chestDataList.Count - 2 ? nextNextIndex + nextNextCount : -1)),
							leftNeighborID = index + 11 + LeftRowIndex * columns - ((LeftRowIndex == (int)Math.Ceiling((double)count / columns) - 1) ? columns - (count % columns) : 0),
							rightNeighborID = numberOfChestColumns == 1 ? -1 : (i < chestDataList.Count - 1 ? nextIndex + RightRowIndex * columns : -1),
							upNeighborID = k > 0 ? index + count + k - 5: (numberOfChestColumns == 1 ? (i > 0 ? lastIndex + lastCount + widgetTextLength - 1 : -1) : (i > 1 ? lastLastIndex + lastLastCount + widgetTextLength - 1: -1))
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
							downNeighborID = j < widgetText.Length - 1 ? index + count + j + 1 : (numberOfChestColumns == 1 ? (i < chestDataList.Count - 1 ? nextIndex + nextCount : -1) : (i < chestDataList.Count - 2 ? nextNextIndex + nextNextCount : -1)),
							leftNeighborID = index + 11 + LeftRowIndex * columns - ((LeftRowIndex == (int)Math.Ceiling((double)count / columns) - 1) ? columns - (count % columns) : 0),
							rightNeighborID = numberOfChestColumns == 1 ? -1 : (i < chestDataList.Count - 1 ? nextIndex + RightRowIndex * columns : -1),
							upNeighborID = j > 0 ? index + count + j - 1: (numberOfChestColumns == 1 ? (i > 0 ? lastIndex + lastCount + widgetText.Length - 1 : -1) : (i > 1 ? lastLastIndex + lastLastCount + widgetText.Length - 1: -1))
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

			b.End();
			Rectangle prevScissor = b.GraphicsDevice.ScissorRectangle;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState { ScissorTestEnable = true });
			
			Rectangle scissor = new Rectangle(xPositionOnScreen, yPositionOnScreen, width, cutoff - yPositionOnScreen);
			b.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(scissor, b.GraphicsDevice.Viewport.Bounds);

			int locationZebraIndex = 0;
			canScroll = (chestDataList.Count > 0 && chestDataList[^1].menu.yPositionOnScreen + Math.Max(chestDataList[^1].menu.rows, 3) * 64 + borderWidth > cutoff) || (chestDataList.Count > 1 && chestDataList[^2].menu.yPositionOnScreen + Math.Max(chestDataList[^2].menu.rows, 3) * 64 + borderWidth > cutoff);
			for (int i = 0; i < chestDataList.Count; i++)
			{
				ChestData chestData = chestDataList[i];
				if (chestData.isFirstInLocation) locationZebraIndex++;

				if (canScroll && chestData.menu.yPositionOnScreen - 48 > cutoff + 64 * 4)
				{
					break;
				}
				if (i == heldMenu - (chestDataList[i].index - i))
				{
					continue;
				}

				if (!chestData.collapsed)
				{
					// Desenhar apenas os botões/slots (Game1.menuTexture já tem a borda e background)
					Color boxColor = (locationZebraIndex % 2 == 0) ? Color.White : new Color(210, 210, 210);
					IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), chestData.menu.xPositionOnScreen - 24, chestData.menu.yPositionOnScreen - 20, chestData.menu.width + 48, chestData.menu.height + 40, boxColor, 1f, false);
					// Desenhar Overlay de cor do baú sobre os quadrados
					Rectangle bgRect = new Rectangle(
						chestData.menu.xPositionOnScreen,
						chestData.menu.yPositionOnScreen,
						chestData.menu.width,
						chestData.menu.height
					);
					b.Draw(Game1.staminaRect, bgRect, chestData.chestColor * 0.45f);

					chestData.menu.draw(b);
					for (int j = 0; j < chestData.inventoryButtons.Count; j++)
					{
						chestData.inventoryButtons[j].draw(b, targetChest?.index != chestData.index && j == chestData.inventoryButtons.Count - 1 ? Color.Purple : Color.White, 1);
					}
				}
                if (chestData.isFirstInLocation)
                {
					if (i > 0)
					{
						// Draw separator line centered in the new 48px gap
						b.Draw(
							Game1.staminaRect, 
							new Rectangle(
								xPositionOnScreen + borderWidth + 24,
								chestData.menu.yPositionOnScreen - 64 * 2 - 20,
								width - borderWidth * 2 - 48,
								4
							), 
							new Color(50, 30, 0) * 0.5f
						);
					}

                    // Draw location header
                    SpriteText.drawString(b, chestData.locationDisplayName, chestData.menu.xPositionOnScreen, chestData.menu.yPositionOnScreen - 64 * 2 - 4);
                }
				SpriteText.drawString(b, chestData.label, chestData.menu.xPositionOnScreen, chestData.menu.yPositionOnScreen - 64);
			}

			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			b.GraphicsDevice.ScissorRectangle = prevScissor;
			if(chestDataList.Count > 0)
			{
                //scrollbar
                int scrollbarSize = cutoff - 12;
                int barHeight = (int)Math.Round((float)scrollbarSize * (float)scrollbarSize / (float)totalHeight);
                int barOffset = (int)Math.Round(scrolled * scrollInterval * ((float)scrollbarSize / totalHeight));
                scrollbarBack = new Rectangle(xPositionOnScreen + width - borderWidth - scrollbarWidth + 8, yPositionOnScreen + borderWidth + 76, scrollbarWidth, scrollbarSize);
                scrollbarRect = new Rectangle(xPositionOnScreen + width - borderWidth - scrollbarWidth + 8, yPositionOnScreen + borderWidth + 76 + barOffset, scrollbarWidth, barHeight);
                if (scrollbarBack.Contains(Game1.getMousePosition()) || scrolling)
                {
                    scrollbarWidth = Math.Min(24, Math.Max(12, scrollbarWidth + 1));
                }
                else
                {
                    scrollbarWidth = Math.Max(12, Math.Min(24, scrollbarWidth - 1));
                }
                b.Draw(Game1.staminaRect, scrollbarBack, Color.Gray);
                b.Draw(Game1.staminaRect, scrollbarRect, Color.Orange);
                SpriteText.drawString(b, barOffset+"", 16, 16);
                SpriteText.drawString(b, scrollbarSize+"", 16, 64);
            }

			Game1.drawDialogueBox(xPositionOnScreen, cutoff - (borderWidth / 2), width, bottomAreaHeight + borderWidth + (borderWidth / 2), false, true, null, false, true);
			playerInventoryMenu.draw(b);
			b.DrawString(Game1.smallFont, filterString, new Vector2(locationText.X, locationText.Y - 29), Game1.textColor);
			locationText.Draw(b);

			// Draw additional filter fields
			// Item Name filter
			b.DrawString(Game1.smallFont, ModEntry.SHelper.Translation.Get("filter-item-name"), new Vector2(itemNameText.X, itemNameText.Y - 29), Game1.textColor);
			itemNameText.Draw(b);

			// Item Description filter
			b.DrawString(Game1.smallFont, ModEntry.SHelper.Translation.Get("filter-item-description"), new Vector2(itemDescriptionText.X, itemDescriptionText.Y - 29), Game1.textColor);
			itemDescriptionText.Draw(b);

			// Location dropdown
			string locationLabelText = ModEntry.Config.SelectedLocations.Count > 0
				? $"{ModEntry.Config.SelectedLocations.Count} {ModEntry.SHelper.Translation.Get("filter-location-select")}"
				: ModEntry.SHelper.Translation.Get("category-all");
			Game1.spriteBatch.Draw(Game1.menuTexture, locationDropdownCC.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
			b.DrawString(Game1.smallFont, "Loc", new Vector2(locationDropdownCC.bounds.X, locationDropdownCC.bounds.Y - 29), Game1.textColor);
			b.DrawString(Game1.smallFont, locationLabelText, new Vector2(locationDropdownCC.bounds.X + 8, locationDropdownCC.bounds.Y + 8), Game1.textColor);
			// Dropdown arrow
			b.Draw(Game1.mouseCursors, new Vector2(locationDropdownCC.bounds.Right - 20, locationDropdownCC.bounds.Y + locationDropdownCC.bounds.Height / 2 - 4),
				new Rectangle(345, 494, 10, 6), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.87f);

			if (renamingChest is not null)
			{
				SpriteText.drawString(b, nameString, renameBox.X + 16, renameBox.Y - 48);
				renameBox.Draw(b);
				okButton.draw(b);
			}
			SpriteText.drawString(b, sortString, organizeButton.bounds.X, organizeButton.bounds.Y + 62);
			foreach (ClickableComponent cc in sortCCList)
			{
				b.DrawString(Game1.smallFont, cc.label, cc.bounds.Location.ToVector2() + new Vector2(-1, 1), currentSort.ToString() == cc.label ? Color.Green : Color.Black);
				b.DrawString(Game1.smallFont, cc.label, cc.bounds.Location.ToVector2(), currentSort.ToString() == cc.label ? Color.LightGreen : Color.White);
			}
			trashCan.draw(b);
			organizeButton.draw(b);
			storeAlikeButton.draw(b);
			consolidateButton.draw(b);
			sortAllButton.draw(b);
			clearFiltersButton.draw(b);
			float lidOffsetX = compactLayout ? 44f : 60f;
			float lidOffsetY = compactLayout ? 30f : 40f;
			b.Draw(Game1.mouseCursors, new Vector2(trashCan.bounds.X + lidOffsetX, trashCan.bounds.Y + lidOffsetY), new Rectangle?(new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10)), Color.White, trashCanLidRotation, new Vector2(16f, 10f), trashLidScale, SpriteEffects.None, 0.86f);
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

			if (locationDropdownOpen)
			{
				int listHeight = locationOptionsCCList.Count * 36;
				if (listHeight > 0)
				{
					UpdateLocationOptionBounds();
					int startY = locationOptionsCCList[0].bounds.Y - 8;
					int dropdownWidth = Math.Max(locationDropdownCC.bounds.Width, 300);
					IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), locationDropdownCC.bounds.X, startY, dropdownWidth, listHeight + 16, Color.White * 0.95f, 1f, false);
					for (int i = 0; i < locationOptionsCCList.Count; i++)
					{
						var cc = locationOptionsCCList[i];
						if (cc.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
						{
							b.Draw(Game1.staminaRect, cc.bounds, Color.Wheat);
						}
						
						bool isSelected = ModEntry.Config.SelectedLocations.Contains(cc.name);
						Color textColor = isSelected ? Color.Green : Game1.textColor;
						string displayTxt = (isSelected ? "[X] " : "[ ] ") + cc.label;
						b.DrawString(Game1.smallFont, displayTxt, new Vector2(cc.bounds.X + 8, cc.bounds.Y + 4), textColor);
					}
				}
			}

			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			emergencyShutDown();
			Game1.activeClickableMenu = new AllChestsMenu();
		}
        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (scrolling)
            {
                float scrollbarSize = cutoff - 12;
				float scale = scrollbarSize / totalHeight;
                int barHeight = (int)Math.Round((float)scrollbarSize * (float)scrollbarSize / (float)totalHeight);
                int pos = (int)Math.Round(Math.Min(totalHeight, Math.Max(0,(float)(y - clickpos - yPositionOnScreen + borderWidth + 76))) / (scrollInterval *  scale));
				if(pos != scrolled)
				{
					if (pos > scrolled && !canScroll)
						return;
					scrolled = pos;
                    PopulateMenus(false);
                }
            }
        }
        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
			scrolling = false;
        }
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (locationDropdownOpen)
			{
				foreach (var cc in locationOptionsCCList)
				{
					if (cc.containsPoint(x, y))
					{
						if (ModEntry.Config.SelectedLocations.Contains(cc.name))
							ModEntry.Config.SelectedLocations.Remove(cc.name);
						else
							ModEntry.Config.SelectedLocations.Add(cc.name);
						
						ModEntry.SHelper.WriteConfig(ModEntry.Config);
						Game1.playSound("drumkit6");
						PopulateMenus(false);
						RebuildControllerNavigation();
						populateClickableComponentList();
						return;
					}
				}
				locationDropdownOpen = false;
				RebuildControllerNavigation();
				populateClickableComponentList();
				if (locationDropdownCC.bounds.Contains(x, y))
				{
					Game1.playSound("drumkit6");
					return;
				}
			}
			
			if (locationDropdownCC.bounds.Contains(x, y))
			{
				locationDropdownOpen = true;
				UpdateLocationOptionBounds();
				RebuildControllerNavigation();
				populateClickableComponentList();
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && locationOptionsCCList.Count > 0)
				{
					currentlySnappedComponent = locationOptionsCCList[0];
					snapCursorToCurrentSnappedComponent();
				}
				Game1.playSound("shwip");
				return;
			}

			Item held = heldItem;
			Rectangle rect;

			renameBox.Selected = new Rectangle(renameBox.X, renameBox.Y, renameBox.Width, renameBox.Height).Contains(x,y);
			locationText.Selected = new Rectangle(locationText.X, locationText.Y, locationText.Width, locationText.Height).Contains(x,y);
			itemNameText.Selected = new Rectangle(itemNameText.X, itemNameText.Y, itemNameText.Width, itemNameText.Height).Contains(x,y);
			itemDescriptionText.Selected = new Rectangle(itemDescriptionText.X, itemDescriptionText.Y, itemDescriptionText.Width, itemDescriptionText.Height).Contains(x,y);
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
				itemNameText.Update();
				itemDescriptionText.Update();

				if (ModEntry.Config.EnableControllerKeyboard && Game1.options.gamepadControls)
				{
					if (renamingChest is not null && renameBox.Selected)
						Game1.showTextEntry(renameBox);
					if (locationText.Selected)
						Game1.showTextEntry(locationText);
					if (itemNameText.Selected)
						Game1.showTextEntry(itemNameText);
					if (itemDescriptionText.Selected)
						Game1.showTextEntry(itemDescriptionText);
				}

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
				if (consolidateButton.containsPoint(x, y))
				{
					OpenConsolidationMenu();
					return;
				}
				if (sortAllButton.containsPoint(x, y))
				{
					SortAllChests();
					return;
				}
				if (clearFiltersButton.containsPoint(x, y))
				{
					ClearAllFilters();
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
				if(scrollbarRect.Contains(x, y))
				{
					clickpos = y - yPositionOnScreen + borderWidth + 76;
                    scrolling = true;
				}
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
							if (ModEntry.SHelper.Input.IsDown(ModEntry.Config.ModKey) || gamepadShift)
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
			// Don't close menu if typing in a text box
			bool isTypingInFilter = locationText.Selected || itemNameText.Selected || itemDescriptionText.Selected || (renamingChest is not null && renameBox.Selected);
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose() && !isTypingInFilter)
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

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.B || b == Buttons.Start)
			{
				if (renameBox.Selected || locationText.Selected || itemNameText.Selected || itemDescriptionText.Selected)
				{
					renameBox.Selected = false;
					locationText.Selected = false;
					itemNameText.Selected = false;
					itemDescriptionText.Selected = false;
					Game1.keyboardDispatcher.Subscriber = null;
					return;
				}
			}

			if (b == Buttons.X)
			{
				if (currentlySnappedComponent != null && currentlySnappedComponent.myID >= ccMagnitude)
				{
					int id = currentlySnappedComponent.myID;
					int chestIndex = (id - ccMagnitude) / (ccMagnitude / 1000);
					int localId = id - ccMagnitude - (chestIndex * (ccMagnitude / 1000));
					
					if (localId == 5000)
					{
						if (heldMenu == -1 && chestIndex >= 0 && chestIndex < chestDataList.Count)
						{
							heldMenu = chestDataList[chestIndex].index;
							Game1.playSound("bigSelect");
							return;
						}
					}
				}
				
				// Simulate a Left Click on the item while injecting a faux-shift
				// We use getMouseX and getMouseY so it works even if snapping is broken (virtual mouse)
				gamepadShift = true;
				receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
				gamepadShift = false;
				return;
			}

			if (b == Buttons.A && currentlySnappedComponent is not null)
			{
				receiveLeftClick(currentlySnappedComponent.bounds.Center.X, currentlySnappedComponent.bounds.Center.Y);
				return;
			}
			base.receiveGamePadButton(b);
		}

		public override void populateClickableComponentList()
		{
			base.populateClickableComponentList();
			allClickableComponents = new List<ClickableComponent>();

			allClickableComponents.AddRange(inventoryCells);
			allClickableComponents.AddRange(inventoryButtons);
			allClickableComponents.AddRange(chestHeaders);
			allClickableComponents.AddRange(playerInventoryMenu.inventory);
			allClickableComponents.AddRange(sortCCList);
			allClickableComponents.Add(organizeButton);
			allClickableComponents.Add(storeAlikeButton);
			allClickableComponents.Add(consolidateButton);
			allClickableComponents.Add(sortAllButton);
			allClickableComponents.Add(trashCan);
			allClickableComponents.Add(clearFiltersButton);
			allClickableComponents.Add(chestLabelCC);
			allClickableComponents.Add(itemNameCC);
			allClickableComponents.Add(itemDescCC);
			allClickableComponents.Add(locationDropdownCC);

			if (locationDropdownOpen)
				allClickableComponents.AddRange(locationOptionsCCList);

			if (renamingChest is not null)
			{
				allClickableComponents.Add(renameBoxCC);
				allClickableComponents.Add(okButton);
			}
		}

		public override void setUpForGamePadMode()
		{
			base.setUpForGamePadMode();
			if (this.allClickableComponents == null)
			{
				this.populateClickableComponentList();
			}
			this.snapToDefaultClickableComponent();
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
			if (renamingChest != null)
			{
				return;
			}

			// Only update location filter if it's currently selected
			if (locationText.Selected && whichLocation?.ToLower() != locationText.Text.ToLower())
			{
				whichLocation = locationText.Text;
				scrolled = 0;
				PopulateMenus(false);
				lastTopSnappedCC = getComponentWithID(ccMagnitude);
			}

			// Update item name filter when changed
			string currentItemNameFilter = itemNameText.Text?.ToLower().Trim() ?? "";
			if (currentItemNameFilter != lastItemNameFilter)
			{
				lastItemNameFilter = currentItemNameFilter;
				scrolled = 0;
				PopulateMenus(false);
			}

			// Update item description filter when changed
			string currentItemDescFilter = itemDescriptionText.Text?.ToLower().Trim() ?? "";
			if (currentItemDescFilter != lastItemDescriptionFilter)
			{
				lastItemDescriptionFilter = currentItemDescFilter;
				scrolled = 0;
				PopulateMenus(false);
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
				consolidateButton.tryHover(x, y, 0.1f);
				if (consolidateButton.containsPoint(x, y))
				{
					hoverText = consolidateButton.hoverText;
					return;
				}
				sortAllButton.tryHover(x, y, 0.1f);
				if (sortAllButton.containsPoint(x, y))
				{
					hoverText = sortAllButton.hoverText;
					return;
				}
				clearFiltersButton.tryHover(x, y, 0.1f);
				if (clearFiltersButton.containsPoint(x, y))
				{
					hoverText = clearFiltersButton.hoverText;
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
							result = b.chest.GetActualCapacity().CompareTo(a.chest.GetActualCapacity()); // Sort by capacity descending
						}
						// Tie-breaker: Group by color as well
						if (result == 0)
						{
							result = a.chestColor.PackedValue.CompareTo(b.chestColor.PackedValue);
						}
						if (result == 0)
						{
							result = CompareLabels(a.label, b.label);
						}
						break;
					case Sort.LD:
						result = b.location.CompareTo(a.location);
						if (result == 0)
						{
							result = b.chest.GetActualCapacity().CompareTo(a.chest.GetActualCapacity()); // Sort by capacity descending
						}
						// Tie-breaker: Group by color as well
						if (result == 0)
						{
							result = a.chestColor.PackedValue.CompareTo(b.chestColor.PackedValue); // Still sort color similarly
						}
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
			int leftTop = chestLabelCC?.myID ?? 2 * ccMagnitude;
			int leftMiddle = itemNameCC?.myID ?? 2 * ccMagnitude;
			int leftBottom = itemDescCC?.myID ?? 2 * ccMagnitude;

			if (playerInventoryMenu.inventory.Count >= 12)
			{
				playerInventoryMenu.inventory[0].leftNeighborID = leftTop;
				playerInventoryMenu.inventory[11].rightNeighborID = 4 * ccMagnitude;
				if (playerInventoryMenu.inventory.Count >= 24)
				{
					playerInventoryMenu.inventory[12].leftNeighborID = leftMiddle;
					playerInventoryMenu.inventory[23].rightNeighborID = 4 * ccMagnitude + 1;
					if (playerInventoryMenu.inventory.Count >= 36)
					{
						playerInventoryMenu.inventory[24].leftNeighborID = leftBottom;
						playerInventoryMenu.inventory[35].rightNeighborID = 4 * ccMagnitude + 1;
					}
				}
			}
		}

		private void RebuildControllerNavigation()
		{
			SetPlayerInventoryNeighbours();

			chestLabelCC.upNeighborID = -1;
			chestLabelCC.downNeighborID = itemNameCC.myID;
			chestLabelCC.rightNeighborID = playerInventoryMenu.inventory.Count > 0 ? playerInventoryMenu.inventory[0].myID : -1;

			itemNameCC.upNeighborID = chestLabelCC.myID;
			itemNameCC.downNeighborID = itemDescCC.myID;
			itemNameCC.rightNeighborID = playerInventoryMenu.inventory.Count > 12 ? playerInventoryMenu.inventory[12].myID : chestLabelCC.rightNeighborID;

			itemDescCC.upNeighborID = itemNameCC.myID;
			itemDescCC.downNeighborID = locationDropdownCC.myID;
			itemDescCC.rightNeighborID = playerInventoryMenu.inventory.Count > 24 ? playerInventoryMenu.inventory[24].myID : chestLabelCC.rightNeighborID;

			locationDropdownCC.upNeighborID = itemDescCC.myID;
			locationDropdownCC.downNeighborID = (locationDropdownOpen && locationOptionsCCList.Count > 0) ? locationOptionsCCList[0].myID : clearFiltersButton.myID;
			locationDropdownCC.rightNeighborID = itemDescCC.rightNeighborID;

			clearFiltersButton.leftNeighborID = locationDropdownCC.myID;
			clearFiltersButton.rightNeighborID = chestLabelCC.rightNeighborID;
			clearFiltersButton.upNeighborID = locationDropdownCC.myID;

			for (int i = 0; i < locationOptionsCCList.Count; i++)
			{
				ClickableComponent cc = locationOptionsCCList[i];
				cc.upNeighborID = i == 0 ? locationDropdownCC.myID : locationOptionsCCList[i - 1].myID;
				cc.downNeighborID = i == locationOptionsCCList.Count - 1 ? clearFiltersButton.myID : locationOptionsCCList[i + 1].myID;
				cc.leftNeighborID = locationDropdownCC.myID;
				cc.rightNeighborID = locationDropdownCC.rightNeighborID;
			}
		}

		private void UpdateLocationOptionBounds()
		{
			int listHeight = locationOptionsCCList.Count * 36;
			if (listHeight <= 0)
				return;

			int startY = locationDropdownCC.bounds.Bottom;
			if (startY + listHeight > Game1.uiViewport.Height)
			{
				startY = locationDropdownCC.bounds.Y - listHeight;
			}

			for (int i = 0; i < locationOptionsCCList.Count; i++)
			{
				locationOptionsCCList[i].bounds.Y = startY + 8 + i * 36;
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

		/// <summary>
		/// Creates a unique key for an item considering type and quality
		/// </summary>
		private string GetItemKey(Item item)
		{
			string key = item.ItemId;
			if (item is StardewValley.Object obj && obj.Quality > 0)
				key += $"_q{obj.Quality}";
			return key;
		}

		/// <summary>
		/// Counts free slots in a chest
		/// </summary>
		private int CountFreeSlots(ChestData chestData)
		{
			if (chestData?.chest == null) return 0;
			return chestData.chest.Items.Count(i => i == null);
		}

		/// <summary>
		/// Finds items that exist in multiple chests
		/// </summary>
		private List<ConsolidationItem> FindDuplicateItems()
		{
			var duplicates = new List<ConsolidationItem>();
			var itemGroups = new Dictionary<string, List<ChestData>>();

			// Group items by type across all chests
			foreach (var chestData in allChestDataList)
			{
				if (chestData?.chest == null) continue;

				foreach (var item in chestData.chest.Items)
				{
					if (item == null) continue;

					string key = GetItemKey(item);
					if (!itemGroups.ContainsKey(key))
						itemGroups[key] = new List<ChestData>();

					if (!itemGroups[key].Contains(chestData))
						itemGroups[key].Add(chestData);
				}
			}

			// Find items that exist in multiple chests
			foreach (var kvp in itemGroups)
			{
				var chestsContainingItem = kvp.Value;
				if (chestsContainingItem.Count < 2) continue; // Skip if only in one chest

				// Get a sample item and calculate total stack
				Item sampleItem = null;
				int totalStack = 0;

				foreach (var chestData in chestsContainingItem)
				{
					foreach (var item in chestData.chest.Items)
					{
						if (item != null && GetItemKey(item) == kvp.Key)
						{
							if (sampleItem == null)
								sampleItem = item;
							totalStack += item.Stack;
						}
					}
				}

				if (sampleItem == null) continue;

				// Calculate if consolidation is possible (enough space in one chest)
				int maxStackSize = sampleItem.maximumStackSize();
				int minSlotsNeeded = (int)Math.Ceiling((double)totalStack / maxStackSize);

				var chestsWithSpace = chestsContainingItem
					.Where(c => CountFreeSlots(c) >= minSlotsNeeded)
					.ToList();

				if (chestsWithSpace.Any())
				{
					duplicates.Add(new ConsolidationItem
					{
						SampleItem = sampleItem,
						TotalStack = totalStack,
						ChestsContaining = chestsContainingItem,
						ChestsWithSpace = chestsWithSpace
					});
				}
			}

			return duplicates;
		}

		/// <summary>
		/// Opens the consolidation menu to merge duplicate items
		/// </summary>
		private void OpenConsolidationMenu()
		{
			var duplicates = FindDuplicateItems();
			if (duplicates.Count == 0)
			{
				Game1.showRedMessage(ModEntry.SHelper.Translation.Get("no-duplicates"));
				return;
			}
			Game1.activeClickableMenu = new ConsolidationMenu(duplicates, this);
		}

		/// <summary>
		/// Consolidates an item from multiple chests into a destination chest
		/// </summary>
		internal void ConsolidateItem(ConsolidationItem item, ChestData destination)
		{
			if (item?.SampleItem == null || destination?.chest == null) return;

			// Check space
			int totalStack = item.TotalStack;
			int maxStackSize = item.SampleItem.maximumStackSize();
			int slotsNeeded = (int)Math.Ceiling((double)totalStack / maxStackSize);

			if (CountFreeSlots(destination) < slotsNeeded)
			{
				Game1.showRedMessage(ModEntry.SHelper.Translation.Get("not-enough-space"));
				return;
			}

			// Collect all items from source chests
			var allItems = new List<Item>();
			foreach (var chest in item.ChestsContaining)
			{
				if (chest?.chest == null) continue;
				for (int i = chest.chest.Items.Count - 1; i >= 0; i--)
				{
					var itemInChest = chest.chest.Items[i];
					if (itemInChest != null && itemInChest.canStackWith(item.SampleItem))
					{
						allItems.Add(itemInChest);
						chest.chest.Items[i] = null;
					}
				}
			}

			// Move to destination chest
			foreach (var itemToMove in allItems)
			{
				AddItemToInventory(destination.chest.Items, itemToMove);
			}

			Game1.playSound("bigSelect");
			PopulateMenus(false);
		}

		/// <summary>
		/// Sorts all items in all chests using the same algorithm as the individual organize button
		/// </summary>
		private void SortAllChests()
		{
			foreach (var chestData in allChestDataList)
			{
				if (chestData?.chest == null) continue;
				ItemGrabMenu.organizeItemsInList(chestData.chest.Items);
			}
			Game1.playSound("Ship");
		}

		/// <summary>
		/// Clears all filters and resets the chest list
		/// </summary>
		private void ClearAllFilters()
		{
			Game1.playSound("bigDeSelect");
			itemNameText.Text = "";
			itemDescriptionText.Text = "";
			locationText.Text = "";
			whichLocation = "";
			ModEntry.Config.SelectedLocations.Clear();
			ModEntry.SHelper.WriteConfig(ModEntry.Config);
			scrolled = 0;
			PopulateMenus(false);
		}

		/// <summary>
		/// Helper method to add item to inventory with stacking
		/// </summary>
		private void AddItemToInventory(IList<Item> inventory, Item item)
		{
			// Try to stack with existing items
			foreach (var invItem in inventory)
			{
				if (invItem != null && invItem.canStackWith(item))
				{
					int spaceLeft = invItem.maximumStackSize() - invItem.Stack;
					int toAdd = Math.Min(spaceLeft, item.Stack);
					invItem.Stack += toAdd;
					item.Stack -= toAdd;
					if (item.Stack <= 0)
						return;
				}
			}

			// Find empty slot
			for (int i = 0; i < inventory.Count; i++)
			{
				if (inventory[i] == null)
				{
					inventory[i] = item;
					return;
				}
			}
		}
	}
}
