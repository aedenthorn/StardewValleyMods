using Microsoft.Xna.Framework;

namespace CustomSpouseRooms
{
    public class SpouseRoomData
    {
        public string name;
        public int upgradeLevel = -1;
        public int templateIndex = -1;
        public string templateName;
        public Point startPos = new Point(-1,-1);
        public string shellType;
        public bool islandFarmHouse;
        public Point spousePosOffset = new Point(4, 5);
    }
}