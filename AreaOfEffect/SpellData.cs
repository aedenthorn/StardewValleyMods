using Microsoft.Xna.Framework;
using Newtonsoft.Json.Converters;
using StardewValley;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Weapons;
using System.Collections.Generic;

namespace AreaOfEffect
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum SpellDirection
    {
        Up, 
        UpRight, 
        Right,
        DownRight,
        Down, 
        DownLeft, 
        Left,
        UpLeft,
        None
    }

    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum AOEType
    {
        Circle,
        Square,
        Line
    }
    
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum SpriteType
    {
        Animation,
        Balloon,
        Butterfly,
        EvilRabbit,
        Explosion,
        Fire,
        Fountain,
        Heart,
        Ice,
        Lightning,
        Object,
        Poof,
        Texture,
        Sparkle,
        Smoking,

    }
    
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum SpellAffectedType
    {
        Animal,
        Building,
        Crop,
        Farmer,
        FruitTree,
        Grass,
        HoeDirt,
        Horse,
        Monster,
        NPC,
        Object,
        Pet,
        ResourceClump,
        Stone,
        Tile,
        Tree,
        Twig,
        Weed,

    }

    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum SpellEffectType
    {
        Buff, // Farmer, Monster
        Burn, // tile, tf, crop, object, rc
        Custom, // all except tile
        Damage, // monster, farmer
        Destroy, //rc, obj, tf
        Explode, // tile
        Fertilize, // hd
        Freeze, // Monster, Farmer
        Grow, // grass, tree, fruittree, crop
        Heal, // Farmer
        Harvest, // crop, animal, grass, fruittree, object
        Invincible, // farmer
        Index,
        Light, // Farmer, Tile, Monster
        ModData,
        Pet, // Pet, Animal
        Plant,
        Property,
        TileSheet,
        Tool, // Tile, TF, RC, Obj, crop
        Transform,
        Water, // Tile, HoeDirt, building
    }

    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum FieldChangeType
    {
        Set,
        Toggle,
        Subtract,
        Add,
        Prefix,
        Multiply,
        Divide,
        Replace
    }
    public class SpellToolData
    {
        public int MaxCharges { get; set; }
        public string RechargeItem { get; set; }
        public int RechargeAmount { get; set; } = 1;
        public int MaxDistance { get; set; }
        public string RechargeSound { get; set; }
        public List<string> Spells { get; set; }
        public Color ChargesColor { get; set; } = Color.White;
        public Color ChargesBackColor { get; set; } = Color.DarkGray;
        public Color AuraColor { get; set; } = Color.White;
    }
    public class SpellData
    {
        public string DisplayName { get; set; }
        public List<SpellDirection> Sequence { get; set; }
        public string SetSound { get; set; }
        public List<SpellCastData> SpellLevels { get; set; }
        public bool AddTutorial { get; set; } = true;
    }
    public class SpellCastData
    {
        public string CastSound { get; set; }
        public string TriggerAction { get; set; }
        public string TriggerSound { get; set; }
        public int Charges { get; set; } = 1;
        public AOEType AreaType { get; set; } = AOEType.Circle;
        public int Radius { get; set; }
        public List<string> Buffs { get; set; } = new();
        public List<SpellProjectileData> Projectiles { get; set; }
        public List<SpriteData> Sprites { get; set; } = new();
        public List<SpellEffect> Effects { get; set; } = new();
    }

    public class SpriteData
    {
        public SpriteType Type { get; set; }
        public bool PerTile { get; set; } = true;
        public string Texture { get; set; }
        public Rectangle SourceRect { get; set; }
        public float Alpha { get; set; } = 1;
        public float AlphaFade { get; set; } = -1;
        public Color Color { get; set; } = Color.White;
        public int Index { get; set; } = -1;
        public float Interval { get; set; } = 100;
        public int Length { get; set; }
        public int Loops { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 Acceleration { get; set; }
        public int YStop { get; set; }
        public bool Flicker { get; set; }
        public bool? Flipped { get; set; }
        public bool FlippedRandom { get; set; }
        public bool FlippedVertical { get; set; }
        public Vector2 Motion { get; set; }
        public float Rotation { get; set; }
        public float RotationChange { get; set; }
        public int SourceWidth { get; set; } = -1;
        public int SourceHeight { get; set; } = -1;
        public float LayerDepth { get; set; } = -1;
        public float? Scale { get; set; }
        public float ScaleChange { get; set; }
        public float ScaleChangeChange { get; set; }
        public int Delay { get; set; }
        public int Number { get; set; }
        public bool DrawAbove { get; set; }
        public bool Bounce { get; set; }
    }


    public class SpellEffect
    {
        public string TriggerAction { get; set; }
        public List<SpellAffectedType> Affected { get; set; }
        public List<SpellAffectedType> Unaffected { get; set; }
        public List<SpriteData> Sprites { get; set; } = new();
        public SpellEffectType EffectType { get; set; }
        public FieldChangeType ChangeType { get; set; }
        public int Delay { get; set; }
        public int Seconds { get; set; }
        public bool ReapplyToTile { get; set; } = true;
        public bool AsFarmer { get; set; }
        public bool First { get; set; }
        public bool PerTile { get; set; } = true;
        public string Name { get; set; }
        public object Value { get; set; }
        public float Radius { get; set; }
        public float Multiplier { get; set; }
        public Color Color { get; set; } = Color.White;
    }
}