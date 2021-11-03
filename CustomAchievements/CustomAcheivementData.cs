using Microsoft.Xna.Framework;

namespace CustomAchievements
{
    public class CustomAcheivementData
    {
        public string ID;
        public string name;
        public string description;
        public string iconPath = "";
        public Rectangle? iconRect;
        public bool drawFace = true;
        public bool achieved = false;
    }
}