using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Familiars
{
    public class FamiliarSaveData
    {
        public FamiliarSaveData()
        {
        }

        public List<FamiliarData> dustSpriteFamiliars = new List<FamiliarData>();
        public List<FamiliarData> dinoFamiliars = new List<FamiliarData>();
        public List<FamiliarData> batFamiliars = new List<FamiliarData>(); 
        public List<FamiliarData> junimoFamiliars = new List<FamiliarData>(); 
        public List<FamiliarData> butterflyFamiliars = new List<FamiliarData>(); 

    }

    public class FamiliarData
    {
        public int daysOld;
        public int exp;
        public long ownerId;
        public bool followingOwner;
        public Color mainColor;
        public Color redColor;
        public Color greenColor;
        public Color blueColor;
        public string currentLocation;
        public Vector2 position;
        public int baseFrame;
        public Color color;
    }
}