# All Chests Menu

A Stardew Valley mod that allows you to access and manage all your chests, fridges, shipping bins, and more from a single, unified menu.

## 🌟 Key Features

- **Global Chest Access:** View and interact with all your storage containers from anywhere in the world.
- **Extensive Container Support:** Access not just standard Chests, but also:
  - Farmhouse Fridges & Mini-Fridges
  - Shipping Bins & Mini-Shipping Bins
  - Junimo Chests
  - Auto-Grabbers
- **Advanced Filtering & Search:** Quickly find exactly what you need with dedicated text filters:
  - **Location Dropdown:** Filter chests by specific map locations (e.g., Farm, House, Greenhouse) via an intuitive dropdown menu.
  - **Chest Name:** Search for custom-named chests.
  - **Item Name:** Search for specific items inside the chests.
  - **Item Description / Category:** Search based on item descriptions or their category.
- **Smart Organization & Quality of Life:**
  - **Consolidate Items:** A powerful button that merges and stacks duplicate items scattered across different chests, saving space automatically.
  - **Sort All (Global):** Sort items across *all* your chests with a single click.
  - **Target System:** Select a specific chest as your "Target" to quickly send items from other chests directly into it.
  - **Custom Sorting:** Sort your list of chests dynamically by Location, Name, Capacity, or Item Count (Ascending/Descending).
  - **Store Alike (Put All / Take All):** Quickly deposit or extract items. Use the **Same-Item Transfer Key** (default: *Left Control*) while clicking to only transfer items that match what's already in the destination.
- **Polished UI:**
  - **Responsive Grid:** The menu adapts dynamically based on your screen resolution, expanding columns when space is available.
  - **Zebra Striping:** Chests are visually grouped by location with alternating background colors and separator lines, making browsing much easier on the eyes.
  - **Clear Filters:** A dedicated "X" button to instantly wipe all active text and location filters.
- **Gamepad Support:** Full controller support with virtual keyboard integration for text filtering. Use the **Switch Button** (default: *Back/Select*) to easily toggle focus between the chest list and your player inventory.
- **Localization (i18n):** Supports multiple languages, including English, French, Russian, and Portuguese (pt-BR).

---

## 🎮 How to Use the Menu

Once the mod is installed, press the configured hotkey (default: **`F2`** or **`I`** depending on config) to open the All Chests Menu.

### Individual Chest Action Buttons
Every chest in the list has a set of buttons on its right side:
- **Location:** The location of the chest in the world.
- **Open:** Opens the standard game menu for that specific chest.
- **Put:** Moves items from your player inventory into this chest. *(Hold `Left Control` to only deposit items the chest already has).*
- **Take:** Takes items from this chest into your player inventory. *(Hold `Left Control` to only take items you already have).*
- **Rename:** Allows you to change the custom name of the chest.
- **Target:** Sets this specific chest as your current "Target". A purple border will appear around it. When you click items in other chests, they will be sent directly to this Target chest instead of your inventory!

### Main Global Action Buttons
Located just above your player inventory, you will find the main action buttons:
1. **Organize (Chest Icon):** Sorts the items within the currently selected chest.
2. **Store Alike (Stack Icon):** Scans your inventory and automatically moves items into chests that already contain items of the same type.
3. **Consolidate (Merge Icon):** Looks through all your chests and merges matching item stacks together to free up slots.
4. **Sort All (Global Organize Icon):** Sorts the items individually inside *every single chest* you own.
5. **Trash Can:** Drag and drop items here to delete them permanently.

### Filtering Menu
On the left side of the screen, you have multiple fields to filter the displayed chests in real-time:
- **Loc (Location):** Click to select one or multiple specific locations from a Dropdown list.
- **Chest Label:** Show only chests that contain this word in their name.
- **Item Name:** Show only chests that contain an item with this exact name.
- **Item Description:** Show only chests that contain an item with this word in its description.
- **Clear Filters (X Button):** Clears all text fields and resets the location dropdown instantly.

---

## ⚙️ Configuration

You can configure this mod by editing the `config.json` file generated in the mod's folder after running the game once, or simply by using the **[Generic Mod Config Menu (GMCM)](https://www.nexusmods.com/stardewvalley/mods/5098)** in-game (Highly Recommended).

### Key Settings:
- **Menu Key:** The default key to open the menu.
- **Limit To Current Location:** If enabled, the menu will only show chests in your current map area instead of globally.
- **Container Toggles:** Individually toggle whether Fridges, Shipping Bins, Junimo Chests, and Auto-Grabbers should appear in the menu.
- **Shipping Bin Access:** Determine if you have unrestricted access to take any item out of the Shipping Bin, or just the last shipped item.
- **Coordinate Sort Order:** Choose how chests in the same location are sorted (Left-to-Right `X` vs Top-to-Bottom `Y`).
- **Modifier Keys:** Customize shortcuts for Same-Item Transfers (`ModKey2`, default: *Left Control*) and standard transfers (`ModKey`, default: *Left Shift*).
- **Controller Switch Button:** Set the gamepad button to jump between your inventory and the chest menu.

---

## 🤝 Compatibility
Works with Stardew Valley 1.6+ and requires **[SMAPI](https://smapi.io/)**.
