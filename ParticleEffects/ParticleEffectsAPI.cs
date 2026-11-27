using Microsoft.Xna.Framework;
using System;
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
        public void BeginScreenParticleEffect(string name, int x, int y, string key)
        {
            if (!ModEntry.effectDict.TryGetValue(key, out ParticleEffectData template))
                return;
            Point position = new Point(x, y);
            if (!ModEntry.screenDict.ContainsKey(position))
                ModEntry.screenDict[position] = new List<ParticleEffectData>();
            if (!ModEntry.screenDict[position].Exists(d => d.key == key))
            {
                ParticleEffectData ped = ModEntry.CloneParticleEffect(key, "screen", name, x, y, template);
                ModEntry.screenDict[position].Add(ped);

            }
        }
        public void EndScreenParticleEffect(string location, int x, int y, string key)
        {
            Point position = new Point(x, y);
            if (ModEntry.screenDict.ContainsKey(position))
            {
                for (int i = ModEntry.screenDict[position].Count - 1; i >= 0; i--)
                {
                    if (ModEntry.screenDict[position][i].key == key)
                        ModEntry.screenDict[position].RemoveAt(i);
                }
            }
        }
        public void BeginLocationParticleEffect(string location, int x, int y, string key)
        {
            if (!ModEntry.effectDict.TryGetValue(key, out ParticleEffectData template))
                return;
            if (!ModEntry.locationDict.ContainsKey(location))
                ModEntry.locationDict.Add(location, new Dictionary<Point, List<ParticleEffectData>>());
            Point position = new Point(x, y);
            if (!ModEntry.locationDict[location].ContainsKey(position))
                ModEntry.locationDict[location][position] = new List<ParticleEffectData>();
            if (!ModEntry.locationDict[location][position].Exists(d => d.key == key))
            {
                ParticleEffectData ped = ModEntry.CloneParticleEffect(key, "location", location, x, y, template);
                ModEntry.locationDict[location][position].Add(ped);

            }
        }
        public void EndLocationParticleEffect(string location, int x, int y, string key)
        {
            Point position = new Point(x, y);
            if (ModEntry.locationDict.ContainsKey(location) && ModEntry.locationDict[location].ContainsKey(position))
            {
                for(int i = ModEntry.locationDict[location][position].Count - 1; i >= 0; i--)
                {
                    if (ModEntry.locationDict[location][position][i].key == key)
                        ModEntry.locationDict[location][position].RemoveAt(i);
                }
            }
        }
        public List<string> GetEffectNames()
        {
            return ModEntry.effectDict.Keys.ToList();
        }
    }
}