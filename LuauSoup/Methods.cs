using HarmonyLib;
using StardewValley;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace LuauSoup
{
    public partial class ModEntry
    {
        public static string GetDescription(Item hoveredItem)
        {
            var desc = hoveredItem.getDescription();
            if(!Config.EnableMod || Game1.season != Season.Summer || Game1.dayOfMonth != 11 || Game1.currentLocation.mapPath.Value != "Maps\\Beach-Luau" || !Config.ShowItemInfo || hoveredItem is not Object o || !Utility.highlightLuauSoupItems(hoveredItem))
                return desc;
            int itemLevel = 5;
            int friendship = 0;
            var dict = SHelper.GameContent.Load<Dictionary<string, SoupIngredientData>>(dictPath);


            if (dict.TryGetValue(hoveredItem.QualifiedItemId, out var data) || dict.TryGetValue(hoveredItem.ItemId, out data))
            {
                if (data.friendship != null)
                {
                   friendship = data.friendship.Value;
                }
                if (data.itemLevel != null)
                {
                    itemLevel = data.itemLevel.Value;
                }
            }
            else if (Event.IsItemMayorShorts(hoveredItem))
            {
                itemLevel = 6;
            }
            else
            {
                itemLevel = GetItemLevel(o);
                friendship = GetFriendshipForLevel(itemLevel);
            }
            try
            {
                return desc + "\n" + Game1.parseText(string.Format(SHelper.Translation.Get("item-info"), GetLevelString(itemLevel), (friendship > 0 ? "+" : "") + friendship), Game1.smallFont, (int)AccessTools.Method(typeof(Item), "getDescriptionWidth").Invoke(hoveredItem, new object[0]));
            }
            catch
            {
                return desc;
            }
        }

        public static string GetLevelString(int itemLevel)
        {
            if (!SHelper.Translation.ContainsKey("friendship-" + itemLevel))
                return SHelper.Translation.Get("friendship-special");
            else
                return SHelper.Translation.Get("friendship-" + itemLevel);
        }

        public static bool MatchesReqs(Object item, List<ReqData> list)
        {
            foreach (ReqData req in list)
            {
                if (
                    (req.minPrice == null || item.Price >= req.minPrice) &&
                    (req.maxPrice == null || item.Price <= req.maxPrice) &&
                    (req.minQuality == null || item.Quality >= req.minQuality) &&
                    (req.maxQuality == null || item.Quality <= req.maxQuality) &&
                    (req.minEdibility == null || item.Edibility >= req.minEdibility) &&
                    (req.maxEdibility == null || item.Edibility <= req.maxEdibility)
                    )
                    return true;
            }
            return false;
        }

        public static int GetItemLevel(Object o)
        {
            int itemLevel = 5;
            if (MatchesReqs(o, Config.ReqsLoved))
            {
                itemLevel = 4;
            }
            else if (MatchesReqs(o, Config.ReqsLiked))
            {
                itemLevel = 3;
            }
            else if (MatchesReqs(o, Config.ReqsNeutral))
            {
                itemLevel = 2;
            }
            else if (MatchesReqs(o, Config.ReqsDisliked))
            {
                itemLevel = 1;
            }
            if (MatchesReqs(o, Config.ReqsHated))
            {
                itemLevel = 0;
            }
            return itemLevel;
        }
        public static int GetFriendshipForLevel(int itemLevel)
        {
            switch (itemLevel)
            {
                case 4:
                    return Config.FriendshipLoved;
                case 3:
                    return Config.FriendshipLiked;
                case 1:
                    return Config.FriendshipDisliked;
                case 0:
                    return Config.FriendshipHated;
            }
            return 0;
        }
    }
}