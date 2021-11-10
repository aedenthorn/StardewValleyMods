using Microsoft.Xna.Framework;

namespace CustomSpouseRooms
{
    public class SpouseRoomData
    {
        public string name;
        public int upgradeLevel;
        public int templateIndex = -1;
        public string templateName;
        public Point startPos = new Point(-1,-1);
        public string shellType;
        public Point spousePosOffset = new Point(4, 5);
    }
}