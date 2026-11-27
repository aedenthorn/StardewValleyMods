using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ImmersiveSprinklersAndScarecrows
{
    public class ImmersiveData
    {
        public bool Sprinkler;
        public string ItemId;
        public bool BigCraftable;
        public bool Nozzle;
        public bool Enricher;
        public string Fertilizer;
        public int FertilizerStack;
        public int Scared;
        public string Hat;
        public Dictionary<string, string> modData = new();
    }
}