using System.Collections.Generic;

namespace StardewValleyOrigins
{
    public class WorldStateData
    {
        public List<string> npcs = new List<string>();
        public List<string> events = new List<string>();
        public List<string> mail = new List<string>();
        public List<int> mapPoints = new List<int>();
        public bool shippingBin;
        public bool bus;
        public bool minecarts;
        public bool marniesLivestock;
        public bool townBoard;
        public bool blacksmith;
        public bool farmHouse;
        public bool specialOrdersBoard;
        public bool linusCampfire;
    }
}