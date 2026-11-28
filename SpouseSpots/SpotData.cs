using Microsoft.Xna.Framework;

namespace SpouseSpots
{
    public class SpotData
    {
        public int Weight = 1;
        public Point Tile;
        public int StartTime = 600;
        public int EndTime = 1500;
        public int StayTime = 100;
        public int Facing = 2;
        public string Dialogue;
        public string Animation;
        public int MinFarmHouseLevel = -1;
        public int MaxFarmHouseLevel = int.MaxValue;
    }
}