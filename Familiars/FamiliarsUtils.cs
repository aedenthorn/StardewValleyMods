using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace Familiars
{
    class FamiliarsUtils
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;

        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }
        public static Vector2 getAwayFromNPCTrajectory(Microsoft.Xna.Framework.Rectangle monsterBox, NPC who)
        {
            float num = (float)(-(float)(who.GetBoundingBox().Center.X - monsterBox.Center.X));
            float ySlope = (float)(who.GetBoundingBox().Center.Y - monsterBox.Center.Y);
            float total = Math.Abs(num) + Math.Abs(ySlope);
            if (total < 1f)
            {
                total = 5f;
            }
            float x = num / total * (float)(50 + Game1.random.Next(-20, 20));
            ySlope = ySlope / total * (float)(50 + Game1.random.Next(-20, 20));
            return new Vector2(x, ySlope);
        }

        public static bool withinMonsterThreshold(Familiar m1, Monster m2, int threshold)
        {
            if (m1.Equals(m2) || m1.Health <= 0 || m2.Health <= 0 || m2.IsInvisible || m2.isInvincible())
                return false;

            Vector2 m1l = m1.getTileLocation();
            Vector2 m2l = m2.getTileLocation();
            return Math.Abs(m2l.X - m1l.X) <= (float)threshold && Math.Abs(m2l.Y - m1l.Y) <= (float)threshold;
        }

        public static void warpFamiliar(NPC character, GameLocation targetLocation, Vector2 position)
        {
            if (Game1.currentSeason.Equals("winter") && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 17 && targetLocation.name.Equals("Beach"))
            {
                targetLocation = Game1.getLocationFromName("BeachNightMarket");
            }
            if (targetLocation.name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
            {
                targetLocation = Game1.getLocationFromName("Trailer_Big");
                if (position.X == 12f && position.Y == 9f)
                {
                    position.X = 13f;
                    position.Y = 24f;
                }
            }
            if (Game1.IsClient)
            {
                ModEntry.mp.requestCharacterWarp(character, targetLocation, position);
                return;
            }
            if (!targetLocation.characters.Contains(character))
            {
                if(character.currentLocation != null)
                    character.currentLocation.characters.Remove(character);
                targetLocation.addCharacter(character);
            }
            character.position.X = position.X * 64f;
            character.position.Y = position.Y * 64f;
            if (character.currentLocation != null && !character.currentLocation.Equals(targetLocation))
            {
                character.currentLocation.characters.Remove(character);
            }
            character.currentLocation = targetLocation;
        }

        public static bool monstersColliding(Familiar m1, Monster m2)
        {
            if (m1.Equals(m2) || m1.Health <= 0 || m2.Health <= 0 || m2.IsInvisible)
                return false;

            Rectangle m1l = m1.GetBoundingBox();
            Rectangle m2l = m2.GetBoundingBox();
            return m1l.Intersects(m2l);
        }

        public static void monsterDrop(Familiar familiar, Monster monster, Farmer owner)
        {
            IList<int> objects = monster.objectsToDrop;
            if (Game1.player.isWearingRing(526))
            {
                string result = "";
                Game1.content.Load<Dictionary<string, string>>("Data\\Monsters").TryGetValue(monster.Name, out result);
                if (result != null && result.Length > 0)
                {
                    string[] objectsSplit = result.Split(new char[]
                    {
                        '/'
                    })[6].Split(new char[]
                    {
                        ' '
                    });
                    for (int i = 0; i < objectsSplit.Length; i += 2)
                    {
                        if (Game1.random.NextDouble() < Convert.ToDouble(objectsSplit[i + 1]))
                        {
                            objects.Add(Convert.ToInt32(objectsSplit[i]));
                        }
                    }
                }
            }
            if (objects == null || objects.Count == 0)
                return;

            int objectToAdd = objects[Game1.random.Next(objects.Count)];
            if (objectToAdd < 0)
            {
                familiar.currentLocation.debris.Add(Game1.createItemDebris(new StardewValley.Object(Math.Abs(objectToAdd), Game1.random.Next(1, 4)), familiar.position, Game1.random.Next(4)));
            }
            else
            {
                familiar.currentLocation.debris.Add(Game1.createItemDebris(new StardewValley.Object(Math.Abs(objectToAdd), 1), familiar.position, Game1.random.Next(4)));
            }
        }

        internal static void LoadFamiliars()
        {
            FamiliarSaveData fsd = Helper.Data.ReadSaveData<FamiliarSaveData>("familiars") ?? new FamiliarSaveData();

            foreach (FamiliarData f in fsd.dustSpriteFamiliars)
            {
                Monitor.Log($"Got saved Dust Familiar at {f.currentLocation}");
                GameLocation l = null;
                if (Game1.getLocationFromName(f.currentLocation) != null)
                {
                    l = Game1.getLocationFromName(f.currentLocation);
                }
                else
                {
                    l = Game1.getFarm().buildings.FirstOrDefault(b => b.buildingType == "Slime Hutch")?.indoors.Value;
                }
                if (l == null)
                    continue;

                Monitor.Log($"Returning saved Dust Familiar to {l.Name}");
                DustSpriteFamiliar d = new DustSpriteFamiliar(f.position, f.ownerId);
                d.followingOwner = f.followingOwner;
                d.daysOld.Value = f.daysOld;
                d.exp.Value = f.exp;
                d.mainColor = f.mainColor;
                d.redColor = f.redColor;
                d.greenColor = f.greenColor;
                d.blueColor = f.blueColor;
                d.SetScale();
                d.currentLocation = l;
                l.characters.Add(d);
            }
            foreach (FamiliarData f in fsd.dinoFamiliars)
            {
                Monitor.Log($"Got saved Dino Familiar at {f.currentLocation}");

                GameLocation l = null;
                if (Game1.getLocationFromName(f.currentLocation) != null)
                {
                    l = Game1.getLocationFromName(f.currentLocation);
                }
                else
                {
                    l = Game1.getFarm().buildings.FirstOrDefault(b => b.buildingType == "Slime Hutch")?.indoors.Value;
                }
                if (l == null)
                    continue;
                Monitor.Log($"Returning saved Dino Familiar to {l.Name}");
                DinoFamiliar d = new DinoFamiliar(f.position, f.ownerId);
                d.followingOwner = f.followingOwner;
                d.daysOld.Value = f.daysOld;
                d.exp.Value = f.exp;
                d.mainColor = f.mainColor;
                d.redColor = f.redColor;
                d.greenColor = f.greenColor;
                d.blueColor = f.blueColor;
                d.SetScale();
                d.currentLocation = l;
                l.characters.Add(d);
            }
            foreach (FamiliarData f in fsd.batFamiliars)
            {
                Monitor.Log($"Got saved Bat Familiar at {f.currentLocation}");

                GameLocation l = null;
                if (Game1.getLocationFromName(f.currentLocation) != null)
                {
                    l = Game1.getLocationFromName(f.currentLocation);
                }
                else
                {
                    l = Game1.getFarm().buildings.FirstOrDefault(b => b.buildingType == "Slime Hutch")?.indoors.Value;
                }
                if (l == null)
                    continue;
                Monitor.Log($"Returning saved Bat Familiar to {l.Name}");
                BatFamiliar d = new BatFamiliar(f.position, f.ownerId);
                d.followingOwner = f.followingOwner;
                d.daysOld.Value = f.daysOld;
                d.exp.Value = f.exp;
                d.mainColor = f.mainColor;
                d.redColor = f.redColor;
                d.greenColor = f.greenColor;
                d.blueColor = f.blueColor;
                d.SetScale();
                d.currentLocation = l;
                l.characters.Add(d);
            }
            foreach (FamiliarData f in fsd.junimoFamiliars)
            {
                Monitor.Log($"Got saved Junimo Familiar at {f.currentLocation}");

                GameLocation l = null;
                if (Game1.getLocationFromName(f.currentLocation) != null)
                {
                    l = Game1.getLocationFromName(f.currentLocation);
                }
                else
                {
                    l = Game1.getFarm().buildings.FirstOrDefault(b => b.buildingType == "Slime Hutch")?.indoors.Value;
                }
                if (l == null)
                    continue;
                Monitor.Log($"Returning saved Junimo Familiar to {l.Name}");
                JunimoFamiliar d = new JunimoFamiliar(f.position, f.ownerId);
                d.followingOwner = f.followingOwner;
                d.daysOld.Value = f.daysOld;
                d.exp.Value = f.exp;
                d.mainColor = f.mainColor;
                d.redColor = f.redColor;
                d.greenColor = f.greenColor;
                d.blueColor = f.blueColor;
                if(f.color != null && f.color.A == 255)
                {
                    d.color.Value = f.color;
                }
                else
                {
                    d.color.Value = FamiliarsUtils.GetJunimoColor();
                }
                d.SetScale();
                d.currentLocation = l;
                l.characters.Add(d);
            }
            foreach (FamiliarData f in fsd.butterflyFamiliars)
            {
                Monitor.Log($"Got saved Butterfly Familiar at {f.currentLocation}");

                GameLocation l = null;
                if (Game1.getLocationFromName(f.currentLocation) != null)
                {
                    l = Game1.getLocationFromName(f.currentLocation);
                }
                else
                {
                    l = Game1.getFarm().buildings.FirstOrDefault(b => b.buildingType == "Slime Hutch")?.indoors.Value;
                }
                if (l == null)
                    continue;
                Monitor.Log($"Returning saved Butterfly Familiar to {l.Name}");
                ButterflyFamiliar d = new ButterflyFamiliar(f.position, f.ownerId);
                d.followingOwner = f.followingOwner;
                d.daysOld.Value = f.daysOld;
                d.exp.Value = f.exp;
                d.mainColor = f.mainColor;
                d.redColor = f.redColor;
                d.greenColor = f.greenColor;
                d.blueColor = f.blueColor;
                d.baseFrame = f.baseFrame;
                d.SetScale();
                d.currentLocation = l;
                d.baseFrame = f.baseFrame;
                d.reloadSprite();
                l.characters.Add(d);
            }
        }

        public static Color GetJunimoColor()
        {
            if (ModEntry.Config.JunimoColorType.ToLower() == "default")
            {
                switch (Game1.random.Next(8))
                {
                    case 0:
                        return Color.LimeGreen;
                    case 1:
                        return Color.Orange;
                    case 2:
                        return Color.LightGreen;
                    case 3:
                        return Color.Tan;
                    case 4:
                        return Color.GreenYellow;
                    case 5:
                        return Color.LawnGreen;
                    case 6:
                        return Color.PaleGreen;
                    case 7:
                        return Color.Turquoise;
                    default:
                        return Color.LimeGreen;
                }
            }
            else if (ModEntry.Config.JunimoColorType.ToLower() == "random")
            {
                return new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
            }
            else
            {
                return ModEntry.Config.JunimoMainColor;
            }
        }

        public static Texture2D ColorFamiliar(Texture2D texture, Color mainColor, Color redColor, Color greenColor, Color blueColor)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for(int i = 0; i < data.Length; i++)
            {
                if (data[i] == Color.Transparent)
                    continue;

                if (data[i].R == data[i].G && data[i].R == data[i].B && data[i].G == data[i].B)
                {
                    data[i] = new Color((int)(mainColor.R * (data[i].R / 255f)), (int)(mainColor.G * (data[i].G / 255f)), (int)(mainColor.B * (data[i].B / 255f)));
                }
                else if (data[i].R == 255)
                {
                    data[i] = redColor;
                }
                else if (data[i].G == 255)
                {
                    data[i] = greenColor;
                }
                else if (data[i].B == 255)
                {
                    data[i] = blueColor;
                }
            }
            texture.SetData(data);
            return texture;
        }

        public static void getDinoEgg()
        {
            Game1.flashAlpha = 1f;
            Game1.player.holdUpItemThenMessage(new Object(ModEntry.DinoFamiliarEgg, 1), true);
            Game1.player.reduceActiveItemByOne();
            if (!Game1.player.addItemToInventoryBool(new Object(ModEntry.DinoFamiliarEgg, 1), false))
            {
                Game1.createItemDebris(new Object(ModEntry.DinoFamiliarEgg, 1), Game1.player.getStandingPosition(), 1, null, -1);
            }
            Game1.player.jitterStrength = 0f;
            Game1.screenGlowHold = false;
            ModEntry.mp.globalChatInfoMessage("DinoFamiliarEgg", new string[]
            {
                Game1.player.Name
            });
        }
    }
}
