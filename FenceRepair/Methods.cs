using StardewValley;
using StardewValley.Audio;
using StardewValley.Objects;
using System;
using System.Linq;

namespace FenceRepair
{
    public partial class ModEntry
    {
        public static bool showToggled;
        public static bool IsShowing()
        {
            return Config.ModEnabled && ((Config.ToggleShow && showToggled) || (!Config.ToggleShow && SHelper.Input.IsDown(Config.ShowHealthKey)));
        }
        public static bool TryRepairFence(GameLocation location, Fence fence, Farmer who, bool probe)
        {
            if (fence.health.Value >= fence.maxHealth.Value)
                return false;
            if (Config.RequireMats)
            {
                Item sample = null;
                var recipe = new CraftingRecipe(fence.Name, false);
                if (recipe.name != fence.Name || recipe.recipeList.Count != 1)
                {
                    return false;
                }
                var kvp = recipe.recipeList.First();
                int amount = kvp.Value;
                if (CraftingRecipe.ItemMatchesForCrafting(who.ActiveItem, kvp.Key))
                {
                    int count = Math.Min(amount, who.ActiveItem.Stack);
                    amount -= count;
                    if (!probe)
                    {
                        if (sample is null)
                            sample = who.ActiveItem;
                        who.ActiveItem.Stack -= count;
                    }
                }
                if (!probe && amount > 0 && Config.TakeMatsFromInventory)
                {
                    for (int i = 0; i < who.Items.Count; i++)
                    {
                        if (i == who.CurrentToolIndex)
                            continue;
                        var item = who.Items[i];
                        if (CraftingRecipe.ItemMatchesForCrafting(item, kvp.Key))
                        {
                            int count = Math.Min(amount, item.Stack);
                            amount -= count;
                            if (!probe)
                            {
                                if (sample is null)
                                    sample = item;
                                who.Items[i] = item.ConsumeStack(count);
                            }
                        }
                        if (amount <= 0)
                            break;
                    }
                }
                if (!probe && amount > 0 && Config.TakeMatsFromChests)
                {
                    foreach (var o in who.currentLocation.Objects.Values)
                    {
                        if (o is not Chest c)
                            continue;
                        for (int i = 0; i < c.Items.Count; i++)
                        {
                            var item = c.Items[i];
                            if (CraftingRecipe.ItemMatchesForCrafting(item, kvp.Key))
                            {
                                int count = Math.Min(amount, item.Stack);
                                amount -= count;
                                if (!probe)
                                {
                                    if (sample is null)
                                        sample = item;

                                    c.Items[i] = item.ConsumeStack(count);
                                }
                            }
                            if (amount <= 0)
                                break;
                        }
                    }
                }
                if (amount > 0)
                {
                    return false;
                }
                if (!probe && sample != null)
                {
                    who.ShowItemReceivedHudMessage(sample, kvp.Value);
                }
            }
            if (!probe)
            {

                string repair_sound = fence.GetRepairSound();
                if (!string.IsNullOrEmpty(repair_sound))
                {
                    location.playSound(repair_sound, null, null, SoundContext.Default);
                }
                fence.repairQueued.Value = true;
            }
            return true;
        }
    }
}