using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ParticleEffects
{
    public class EntityParticleData
    {
        public Dictionary<string, List<ParticleData>> particleDict  = new Dictionary<string,List<ParticleData>>();
    }

    public class ParticleData
    {
        public Vector2 direction;
        public int age;
        public float rotation;
        public float rotationRate;
        public int lifespan;
        public float scale;
        public float alpha;
        public int option = -1;
        public Vector2 position;
    }
}