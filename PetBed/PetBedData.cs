
using Microsoft.Xna.Framework;

namespace PetBed
{
    public class PetBedData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int WakeX { get; set; }
        public int WakeY { get; set; }
        public string FrontTexture { get; set; }
        public string PetTypes { get; set; }
        public bool CanFlip { get; set; } = true;
    }
}