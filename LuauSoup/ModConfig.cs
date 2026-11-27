
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace LuauSoup
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool WarnMessage { get; set; } = true;
        public bool ShowItemInfo { get; set; } = true;
        public bool EveryoneMustContribute { get; set; } = true;
        public int FriendshipLoved { get; set; } = 120;
        public int FriendshipLiked { get; set; } = 60;
        public int FriendshipDisliked { get; set; } = -50;
        public int FriendshipHated { get; set; } = -100;
        public List<ReqData> ReqsLoved { get; set; } = new()
        {
            new ReqData()
            {
                minQuality = 2,
                minPrice = 160
            },
            new ReqData()
            {
                minQuality = 1,
                maxQuality = 1,
                minPrice = 300,
                minEdibility = 11
            }
        };
        public List<ReqData> ReqsLiked { get; set; } = new()
        {
            new ReqData()
            {
                minEdibility = 20
            },
            new ReqData()
            {
                minPrice = 160
            },
            new ReqData()
            {
                minPrice = 70,
                minQuality = 1
            }
        };
        public List<ReqData> ReqsNeutral { get; set; } = new()
        {
            new ReqData()
            {
                minPrice = 21,
                minEdibility = 10
            },
            new ReqData()
            {
                minPrice = 40,
                minEdibility = 5
            }
        };
        public List<ReqData> ReqsDisliked { get; set; } = new()
        {
            new ReqData()
            {
                minEdibility = 0
            }
        };
        public List<ReqData> ReqsHated { get; set; } = new()
        {
            new ReqData()
            {
                minEdibility = -299,
                maxEdibility = -1
            }
        };
    }
}
