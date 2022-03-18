using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleEffects
{
    public class ParticleEffectData
    {
        public string key;
        public string type;
        public string name;
        public string movementType = "none";
        public float movementSpeed;
        public float acceleration;
        public float frameSpeed;
        public bool restrictOuter;
        public bool restrictInner;
        public float belowOffset = -1;
        public float aboveOffset = 0.001f;
        public float minRotationRate;
        public float maxRotationRate;
        public float minAlpha = 1;
        public float maxAlpha = 1;
        public int particleWidth;
        public int particleHeight;
        public float fieldOuterRadius;
        public int fieldOuterWidth;
        public int fieldOuterHeight;
        public float fieldInnerRadius;
        public int fieldInnerWidth;
        public int fieldInnerHeight;
        public int fieldOffsetX;
        public int fieldOffsetY = -32;
        public int maxParticles = 1;
        public int minLifespan = 1;
        public int maxLifespan = 1;
        public float minParticleScale = 4;
        public float maxParticleScale = 4;
        public float particleChance = 1;
        public string spriteSheetPath;
        public Texture2D spriteSheet;
    }
}