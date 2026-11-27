using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarmCaveFramework
{
    public class CaveChoice
    {
        public string id;
        public string choice = "";
        public string description;
        public List<CaveObject> objects = new List<CaveObject>();
        public List<CaveResource> resources = new List<CaveResource>();
        public int resourceChance = 66;
        public List<CaveAnimation> animations = new List<CaveAnimation>();
        public List<CavePeriodicEffect> periodics = new List<CavePeriodicEffect>();
        public Color ambientLight = Color.White;
    }

    public class CavePeriodicEffect
    {
        public List<CaveSound> randomSounds = new List<CaveSound>();
        public List<CaveSound> repeatedSounds = new List<CaveSound>();
        public List<CaveAnimation> animations = new List<CaveAnimation>();
        public List<string> specials = new List<string>();
        public float chance = 0.2f;
    }
    public class CaveSound
    {
        public string id;
        public float chance;
        public int count;
        public int pitch;
        public int delayMult;
        public int delayAdd;
    }

    public class CaveObject
    {
        public string id;
        public int X;
        public int Y;
    }
    public class CaveResource
    {
        public string id;
        public int weight;
        public int min = 1;
        public int max = 1;
    }
    public class CaveAnimation
    {
        public int index;
        public int X;
        public int Y;
        public string sourceFile;
        public int sourceX;
        public int sourceY;
        public int width;
        public int height;
        public float interval;
        public int length;
        public int loops;
        public bool flicker;
        public bool flipped;
        public float alphaFade;
        public Color color = Color.White;
        public int delay;
        public float scale;
        public float scaleChange;
        public float rotation;
        public float rotationChange;
        public bool light;
        public float lightRadius;
        public float loopTIme;
        public float range;
        public float motionX;
        public float motionY;
        public bool bottom;
        public bool right;
        public bool randomX;
        public bool randomY;
    }
}