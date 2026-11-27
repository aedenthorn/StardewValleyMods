using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Drawing;

namespace SGStardewDrops
{
    public enum EffectType
    {
        Square,
        Radial,
        Horizontal,
        Vertical,
        Diagonal,
        RemoveSame,
        RandomSame,
        RandomRadial,
        RandomSquare
    }

    public class SDDGameData
    {
        public Dictionary<string, SDDPieceType> Pieces { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<Point, string> InitialPieces { get; set; }
    }

    public class SDDPieceType
    {
        public int Index { get; set; } = -1;
        public string Texture { get; set; }
        public Rectangle SourceRect { get; set; }
        public Color Color { get; set; }
        public List<FrameData> AnimationFrames { get; set; }
        public int Weight { get; set; } = 100;
        public ConnectEffect Effect { get; set; }
        public bool WildCard { get; set; }
        public bool Prismatic { get; set; }
    }

    public class ConnectEffect
    {
        public EffectType Type { get; set; }
    }

    public class FrameData
    {
        public string Texture { get; set; }
        public Rectangle SourceRect { get; set; }
        public Color Color { get; set; }
        public bool Prismatic { get; set; }
    }
}