using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace ObjectTriggers
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
}
