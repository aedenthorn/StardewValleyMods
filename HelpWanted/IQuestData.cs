using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Quests;

namespace HelpWanted
{
    public interface IQuestData
    {
        public Texture2D padTexture { get; set; }
        public Rectangle padTextureSource { get; set; }
        public Color padColor { get; set; }
        public Texture2D pinTexture { get; set; }
        public Rectangle pinTextureSource { get; set; }
        public Color pinColor { get; set; }
        public Texture2D icon { get; set; }
        public Rectangle iconSource { get; set; }
        public Color iconColor { get; set; }
        public float iconScale { get; set; }
        public Point iconOffset { get; set; }
        public Quest quest { get; set; }
        public bool acceptable { get; set; }
    }
}