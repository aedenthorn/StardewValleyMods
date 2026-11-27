using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CustomMonsters
{
    public class MonsterData
    {
        public string MonsterId { get; set; }
        public string Type { get; set; }
        public string Parameters { get; set; } = "position";
        public int Level { get; set; }
        public int Facing { get; set; }
        public string Color { get; set; }
        public List<ColorData> Colors { get; set; }
        public string Name { get; set; }
        public bool Switch { get; set; }
        public string Sprite { get; set; }

        public float Scale { get; set; } = -1;
        public int Damage { get; set; } = -1;
        public bool HardMode { get; set; }
        public int Health { get; set; } = -1;
        public int Speed { get; set; } = -1;
        public int Experience { get; set; } = -1;
        public int Resilience { get; set; } = -1;
        public int MissChance { get; set; } = -1;
        public double Jitteriness { get; set; } = -1;
        public int Slipperiness { get; set; } = -1;
        public int LightType { get; set; } = -1;
        public bool? HideShadow { get; set; }

        public int PlayerThreshold { get; set; } = -1;
        public string Despawn { get; set; }

        public List<CustomDropData> Drops { get; set; }
        public bool ClearDrops { get; set; } = true;

        public string MoveSound { get; set; }
        public string MoveSound2 { get; set; }
        public string SpawnSound { get; set; }
        public string DamageSound { get; set; }
        public string DamageSound2 { get; set; }
        public string DeathSound { get; set; }
        public string DeathSound2 { get; set; }
        public string ContactSound { get; set; }

        public string ProjectileSound { get; set; }
        public string ProjectileSound2 { get; set; }
        public int? ProjectileIndex { get; set; }
        public string ProjectileDebuff { get; set; }
        public Rectangle? ProjectileSource { get; set; }
        public string ProjectileSprite { get; set; }
        public float? ProjectileScale { get; set; }
        public int? ProjectileDamage { get; set; }
        public int? ProjectileRange { get; set; }

        public List<DungeonReplaceData> MineSpawns { get; set; }
        public List<DungeonReplaceData> VolcanoSpawns { get; set; }
        public List<MonsterSpawnData> LocationSpawns { get; set; }

        //Angry Roger specific
        public string SprinkleColor { get; set; }


        // Bat specific
        public bool CanLunge { get; set; } = false;
        public bool CursedDoll { get; set; } = false;
        public bool HauntedSkull { get; set; } = false;
        public int SpeedMin { get; set; } = -1;
        public int SpeedMax { get; set; } = -1;
        public float? ExtraVelocity { get; set; } = null;

        // BigSlime specific
        public List<CustomDropData> HeldItem { get; set; }

        // bug specific
        public string ArmorSound { get; set; }
        public bool? Armored { get; set; }

        // GreenSlime specific
        public List<StackData> Stacks { get; set; }
        public int? ChildhoodLength { get; set; }
        public bool? Prismatic { get; set; }
        public string ContactDebuff { get; set; }

        // Mummy specific
        public string CrumbleSound { get; set; }
        public string UncrumbleSound { get; set; }
        public int? ReviveTimer { get; set; }

        // RockCrab specific
        public int? WaiterChance { get; set; }
        public string ShellSound { get; set; }
        public string BreakSound { get; set; }
        public bool StickBug { get; set; }

        // Serpent specific
        public int? MinSegment { get; set; }
        public int? MaxSegment { get; set; }


        // Shooter specific
        public int? ProjectileSpeed { get; set; }
        public int? ShotsPerFire { get; set; }
        public float? AimTime { get; set; }
        public float? BurstTime { get; set; }
        public float? AimEndTime { get; set; }
        public int? DesiredDistance { get; set; }
        public int? FireRange { get; set; }

        // Spiker specific
        public bool? Vulnerable { get; set; }

        // SquidKid specific
        public int? ProjectileCount { get; set; }
        public float? ProjectileTimer { get; set; }

    }


    public class StackData
    {
        public int Chance { get; set; }
        public int MinStacks { get; set; }
        public int MaxStacks { get; set; }
    }

    public class DungeonReplaceData
    {
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public int MinDifficulty { get; set; } = -1;
        public int MaxDifficulty { get; set; } = -1;
        public int Chance { get; set; }
        public List<string> Types { get; set; }
    }
    public enum RespawnPeriod
    {
        Daily,
        Hourly,
        TenMinutes
    }
    public class MonsterSpawnData
    {
        public string Location { get; set; }
        public int MinSpawn { get; set; }
        public int MaxSpawn { get; set; }
        public int MaxAmount { get; set; } = -1;
        public Point MinTile { get; set; }
        public Point MaxTile { get; set; }
        public RespawnPeriod Respawn { get; set; } = RespawnPeriod.Daily;
    }

    public class CustomDropData
    {
        public string ItemId { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public int Quality { get; set; }
        public int Chance { get; set; } = 100;
    }

    public class ColorData
    {
        public string Color { get; set; }
        public ColorChannel R { get; set; }
        public ColorChannel G { get; set; }
        public ColorChannel B { get; set; }
        public int Chance { get; set; }

    }

    public class ColorChannel
    {
        public byte Min { get; set; }
        public byte Max { get; set; }
    }
}