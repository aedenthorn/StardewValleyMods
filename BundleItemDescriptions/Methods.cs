using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using System;

namespace BundleItemDescriptions
{
    public partial class ModEntry
    {
        public static void AdjustText(JunimoNoteMenu menu)
        {
            for (int i = 0; i < menu.ingredientList.Count; i++)
            {
                BundleIngredientDescription ingredient = menu.currentPageBundle.ingredients[i];
                ItemMetadata metadata = ItemRegistry.GetMetadata(ingredient.id);
                if (((metadata != null) ? metadata.TypeIdentifier : null) == "(O)")
                {
                    ParsedItemData parsedOrErrorData = metadata.GetParsedOrErrorData();
                    Texture2D texture = parsedOrErrorData.GetTexture();
                    Rectangle sourceRect = parsedOrErrorData.GetSourceRect(0, null);
                    Item item = ((ingredient.preservesId != null) ? Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack) : ItemRegistry.Create(ingredient.id, ingredient.stack, ingredient.quality, false));
                    menu.ingredientList[i].hoverText = $"{item.DisplayName}^{item.getDescription()}";
                }
            }
        }
    }
}