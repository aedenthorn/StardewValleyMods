using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace QuickChestColor
{
    public partial class ModEntry
    {
        private bool IncrementColor(bool inc)
        {
            Chest? chest = GetChest();
            if (chest is null)
                return false;
            int which = DiscreteColorPicker.getSelectionFromColor(chest.playerChoiceColor.Value);
            if (SHelper.Input.IsDown(Config.ModKey) ^ Config.HalfByDefault)
            {
                if (chest.modData.ContainsKey(modKey))
                {
                    if(inc)
                        which = (which + 1) % 21;
                    chest.modData.Remove(modKey);
                }
                else
                {
                    chest.modData[modKey] = "T";
                    if(!inc)
                        which = (which == 0 ? 20 : which - 1);
                }
            }
            else
            {
                if (inc)
                    which = (which + 1) % 21;
                else
                {
                    which = (which == 0 ? 20 : which - 1);
                }
            }
            chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(which);
            Game1.playSound("shiny4", null);
            return true;
        }


        private bool CopyColor()
        {
            Chest? chest = GetChest();
            if (chest is null)
                return false;
            copyColor.Value = chest.playerChoiceColor.Value;
            copyHalf.Value = chest.modData.ContainsKey(modKey);
            Game1.playSound("bigSelect", null);
            return true;
        }
        private bool PasteColor()
        {
            Chest? chest = GetChest();
            if (chest is null)
                return false;
            chest.playerChoiceColor.Value = copyColor.Value;
            if(copyHalf.Value)
                chest.modData.Add(modKey, "T");
            else
                chest.modData.Remove(modKey);
            Game1.playSound("bigDeSelect", null);
            return true;
        }
        private static Color GetPlayerChoiceColor(Color value, Chest chest)
        {
            if (!Config.ModEnabled || !chest.modData.ContainsKey(modKey) || chest.playerChoiceColor.Value == Color.Black)
                return value;
            int which = DiscreteColorPicker.getSelectionFromColor(chest.playerChoiceColor.Value);
            which = (which + 1) % 21;
            Color nextColor = DiscreteColorPicker.getColorFromSelection(which);
            Color mixedColor = Color.Lerp(chest.playerChoiceColor.Value, nextColor, 0.5f);
            return mixedColor;
        }
        private Chest? GetChest()
        {
            if (!Game1.player.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out var obj) || obj is not Chest)
            {
                if ((Game1.getMousePosition().Y + Game1.viewport.Y) % 64 < 32 ||  !Game1.player.currentLocation.Objects.TryGetValue(Game1.currentCursorTile + new Vector2(0, 1), out obj) || obj is not Chest)
                {
                    return null;
                }
            }
            Chest chest = (Chest)obj;
            if ((chest.SpecialChestType != Chest.SpecialChestTypes.None && chest.SpecialChestType != Chest.SpecialChestTypes.BigChest) || chest.fridge.Value)
            {
                return null;
            }
            return chest;
        }
    }
}