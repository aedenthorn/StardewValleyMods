using Microsoft.Xna.Framework;
using StardewValley;

namespace HelpWanted
{
    public enum QuestType
    {
        ItemDelivery,
        ResourceCollection,
        SlayMonster,
        Fishing

    }
    public class JsonQuestData
    {
        public Rectangle pinTextureSource = new Rectangle(0, 0, 64, 64);
        public Rectangle padTextureSource = new Rectangle(0, 0, 64, 64);
        public string pinTexturePath;
        public string padTexturePath;
        public Color? pinColor;
        public Color? padColor;
        public string iconPath;
        public Rectangle iconSource = new Rectangle(0, 0, 64, 64);
        public Color? iconColor;
        public float iconScale = 1;
        public Point? iconOffset;
        public QuestInfo quest;
        public float percentChance = 100;
    }
    public class QuestInfo
    {
        public QuestType questType;
        public string item;
        public int number;
        public string questTitle;
        public string questDescription;
        public string target;
        public string targetMessage;
        public string currentObjective;
    }
}