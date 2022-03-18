using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ParticleEffects
{
    public interface IParticleEffectAPI
    {
        public void BeginFarmerParticleEffect(long farmerID, string key);
        public void EndFarmerParticleEffect(long farmerID, string key);
        public void BeginNPCParticleEffect(string npc, string key);
        public void EndNPCParticleEffect(string npc, string key);
        public void BeginLocationParticleEffect(string location, int x, int y, string key);
        public void EndLocationParticleEffect(string location, int x, int y, string key);
        public List<string> GetEffectNames();
    }
    public class ParticleEffectsAPI { 

        public void BeginFarmerParticleEffect(long farmerID, string key)
        {
            if(!ModEntry.farmerDict.ContainsKey(farmerID))
                ModEntry.farmerDict.Add(farmerID, new List<string>());
            if(!ModEntry.farmerDict[farmerID].Contains(key))
                ModEntry.farmerDict[farmerID].Add(key);
        }
        public void EndFarmerParticleEffect(long farmerID, string key)
        {
            if (ModEntry.farmerDict.ContainsKey(farmerID))
                ModEntry.farmerDict[farmerID].Remove(key);
        }
        public void BeginNPCParticleEffect(string npc, string key)
        {
            if (!ModEntry.NPCDict.ContainsKey(npc))
                ModEntry.NPCDict.Add(npc, new List<string>());
            if (!ModEntry.NPCDict[npc].Contains(key))
                ModEntry.NPCDict[npc].Add(key);
        }
        public void EndNPCParticleEffect(string npc, string key)
        {
            if (ModEntry.NPCDict.ContainsKey(npc))
                ModEntry.NPCDict[npc].Remove(key);
        }
        public void BeginLocationParticleEffect(string location, int x, int y, string key)
        {
            if (!ModEntry.locationDict.ContainsKey(location))
                ModEntry.locationDict.Add(location, new Dictionary<Point, List<string>>());
            Point tile = new Point(x, y);
            if (!ModEntry.locationDict[location].ContainsKey(tile))
                ModEntry.locationDict[location][tile] = new List<string>();
            if (!ModEntry.locationDict[location][tile].Contains(key))
                ModEntry.locationDict[location][tile].Add(key);
        }
        public void EndLocationParticleEffect(string location, int x, int y, string key)
        {
            Point tile = new Point(x, y);
            if (ModEntry.locationDict.ContainsKey(location) && ModEntry.locationDict[location].ContainsKey(tile))
                ModEntry.locationDict[location][tile].Remove(key);
        }
        public List<string> GetEffectNames()
        {
            return ModEntry.effectDict.Keys.ToList();
        }
    }
}