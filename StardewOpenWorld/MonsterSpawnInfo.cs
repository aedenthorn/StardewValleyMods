using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StardewOpenWorld
{
    public class MonsterSpawnInfo
    {
        public double Chance = 1;
        public Rectangle? SpawnRange;
        public float Difficulty;
        public List<MonsterInfo> Monsters = new();
    }

    public class MonsterInfo
    {
        public string Type;
        public int Min;
        public int Max;
        public double Chance = 1;
        public int MinLevel;
        public int MaxLevel;
    }
    public class MonsterSpawn
    {
        public string Type;
        public int Level;
    }
}