using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Quests;

namespace FarmerCommissions
{
    public class QuestData : IQuestData
    {
        public Texture2D padTexture { get; set; }
        public Rectangle padTextureSource { get; set; }
        public Color padColor{ get; set; }
        public Texture2D pinTexture{ get; set; }
        public Rectangle pinTextureSource{ get; set; }
        public Color pinColor{ get; set; }
        public Texture2D icon{ get; set; }
        public Color iconColor { get; set; }
        public Rectangle iconSource{ get; set; }
        public float iconScale { get; set; } = 1f;
        public Point iconOffset { get; set; } = new Point(0, 0);
        public Quest quest{ get; set; }
        public bool acceptable { get; set; } = true;
    }
}