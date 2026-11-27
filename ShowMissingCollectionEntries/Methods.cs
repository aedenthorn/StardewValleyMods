using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using System;

namespace ShowMissingCollectionEntries
{
    public partial class ModEntry
    {
        public static bool AchievementAchieved(bool value)
        {
            if (!Config.ModEnabled || !Config.ShowMissingAchievementNames)
                return value;
            return true;
        }
        public static Color ModifyColor(Color color, IClickableMenu iPage)
        {
            if (!Config.ModEnabled)
                return color;
            if(iPage is CollectionsPage page)
            {
                if (!Config.ShowMissingItemIcons || page.currentTab == 5)
                    return color;
                if (page.currentTab == 4)
                {
                    return Config.MissingRecipeTint;
                }
                return Color.White;
            }
            else if(iPage is PowersTab tab)
            {
                if (!Config.ShowMissingPowerIcons)
                    return color;
                return Color.White;
            }
            return color;
        }
        public static bool AllowName(bool value, PowersTab tab)
        {
            if (value || Config.ModEnabled || !Config.ShowMissingPowerNames)
                return value;
            return value;
        }

        private void ReloadCollections()
        {
            if (!Config.ModEnabled || Game1.activeClickableMenu is not GameMenu gm)
                return;
            for(int i = 0; i < gm.pages.Count; i++)
            {
                if (gm.pages[i] is CollectionsPage cp)
                {
                    gm.pages[i] = new CollectionsPage(cp.xPositionOnScreen, cp.yPositionOnScreen, cp.width, cp.height);
                    if(i == gm.currentTab)
                    {
                        gm.pages[i].populateClickableComponentList();
                        gm.AddTabsToClickableComponents(gm.pages[i]);
                    }
                }
            }
        }

    }
}