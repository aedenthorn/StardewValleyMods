using StardewModdingAPI;
using StardewValley;
using System;
using System.Linq;

namespace FreeLove
{
    public partial class ModEntry
    {
        public static IKissingAPI kissingAPI;
        public static IBedTweaksAPI bedTweaksAPI;
        public static IChildrenTweaksAPI childrenAPI;
        public static ICustomSpouseRoomsAPI customSpouseRoomsAPI;
        public static IPlannedParenthoodAPI plannedParenthoodAPI;
        public static IContentPatcherAPI contentPatcherAPI;

        public static void LoadModApis()
        {
            kissingAPI = SHelper.ModRegistry.GetApi<IKissingAPI>("aedenthorn.HugsAndKisses");
            bedTweaksAPI = SHelper.ModRegistry.GetApi<IBedTweaksAPI>("aedenthorn.BedTweaks");
            childrenAPI = SHelper.ModRegistry.GetApi<IChildrenTweaksAPI>("aedenthorn.ChildrenTweaks");
            customSpouseRoomsAPI = SHelper.ModRegistry.GetApi<ICustomSpouseRoomsAPI>("aedenthorn.CustomSpouseRooms");
            plannedParenthoodAPI = SHelper.ModRegistry.GetApi<IPlannedParenthoodAPI>("aedenthorn.PlannedParenthood");

            if (kissingAPI != null)
            {
                SMonitor.Log("Kissing API loaded");
            }
            if (bedTweaksAPI != null)
            {
                SMonitor.Log("BedTweaks API loaded");
            }
            if (childrenAPI != null)
            {
                SMonitor.Log("ChildrenTweaks API loaded");
            }
            if (customSpouseRoomsAPI != null)
            {
                SMonitor.Log("CustomSpouseRooms API loaded");
            }
            if (plannedParenthoodAPI != null)
            {
                SMonitor.Log("PlannedParenthood API loaded");
            }
            contentPatcherAPI = SHelper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if(contentPatcherAPI is not null)
            {
                contentPatcherAPI.RegisterToken(context.ModManifest, "PlayerSpouses", () =>
                {
                    Farmer player;

                    if (Context.IsWorldReady)
                        player = Game1.player;
                    else if (SaveGame.loaded?.player != null)
                        player = SaveGame.loaded.player;
                    else
                        return null;

                    var spouses = GetSpouses(player, true).Keys.ToList();
                    spouses.Sort(delegate (string a, string b) {
                        player.friendshipData.TryGetValue(a, out Friendship af);
                        player.friendshipData.TryGetValue(b, out Friendship bf);
                        if (af == null && bf == null)
                            return 0;
                        if (af == null)
                            return -1;
                        if (bf == null)
                            return 1;
                        if (af.WeddingDate == bf.WeddingDate)
                            return 0;
                        return af.WeddingDate > bf.WeddingDate ? -1 : 1;
                    });
                    return spouses.ToArray();
                });
            }
        }
    }
}