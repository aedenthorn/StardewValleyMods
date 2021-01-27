using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace Murdercrows
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;

        public static ModConfig Config;
        private Dictionary<string, MurderCrow> murderCrows = new Dictionary<string, MurderCrow>
        {
           {"Scarecrow", new MurderCrow("Scarecrow", 4, 10, 10, 391, "Cowboy_monsterDie","Cowboy_gunshot", true, false)},
           {"Rarecrow", new MurderCrow("Rarecrow", 8, 300, 15, 381, "","", true, false)},
           {"Deluxe Scarecrow", new MurderCrow("Deluxe Scarecrow", 8, 30, 20, 7, "flameSpellHit","flameSpell", false, true)},
           {"Gold Scarecrow", new MurderCrow("Gold Scarecrow", 12, 40, 25, 7, "flameSpellHit", "flameSpell", false, true)},
           {"Iridium Scarecrow", new MurderCrow("Iridium Scarecrow", 8, 300, 35, 7, "", "", false, true)}
        };
        private List<MonsterWave> waves = new List<MonsterWave>()
        {
            new MonsterWave(slimes:10, bigSlimes:1), // 9
            new MonsterWave(slimes:20,bigSlimes:5,bats:2),
            new MonsterWave(slimes:20,bigSlimes:5,bats:5, flies:5),
            new MonsterWave(slimes:20,bigSlimes:5,bats:10, flies:10, dustSpirits:5), // 12
            new MonsterWave(slimes:20,bigSlimes:5,bats:10, flies:10, dustSpirits:10, shadowBrutes: 5, skeletons:5),
            new MonsterWave(slimes:20,bigSlimes:5,bats:10, flies:10, dustSpirits:10, shadowBrutes: 10, skeletons:10, shadowShamans: 5),
            new MonsterWave(slimes:20,bigSlimes:5,bats:10, flies:10, dustSpirits:10, shadowBrutes: 10, skeletons:10, shadowShamans: 10, serpents: 5), // 3
            new MonsterWave(slimes:20,bigSlimes:10,bats:10, flies:10, dustSpirits:10, shadowBrutes: 10, skeletons:10, shadowShamans: 10, serpents: 10, squidKids: 5),
            new MonsterWave(slimes:20,bigSlimes:10,bats:10, flies:10, dustSpirits:10, shadowBrutes: 10, skeletons:10, shadowShamans: 10, serpents: 10, squidKids: 10, dinos:5),
            new MonsterWave(slimes:20,bigSlimes:10,bats:10, flies:10, dustSpirits:10, shadowBrutes: 10, skeletons:10, shadowShamans: 10, serpents: 10, squidKids: 10, dinos:10, skulls:10, dolls: 5), // 6
        };
        private int ticksSinceMorning;
        private float shotVelocity = 10;
        private MonsterWave thisWave;
        private int monstersDeployed;
        private int spotIndex;
        private bool waveStarted;
        private IJsonAssetsApi JsonAssets;
        private int waitingSeconds;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding; 
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            if(Config.EnableMonsterWaves)
                Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (JsonAssets == null)
            {
                Monitor.Log("Can't load Json Assets API for scarecrows");
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets/json-assets"));
            }
        }




        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.player.IsMainPlayer || !(Game1.player.currentLocation is Farm) || thisWave == null)
                return;
            if (waveStarted)
            {
                if (Game1.getFarm().characters.Where(n => n is Monster).Count() >= Config.MaxSimultaneousMonsters)
                {
                    if (waitingSeconds++ > 60)
                        ClearMonsters();
                    return;
                }
                waitingSeconds = 0;
                if (thisWave.totalMonsters() <= 0)
                {
                    ClearMonsters();
                    thisWave = null;
                    waveStarted = false;
                    return;
                }
                Vector2[] spots = GetOpenSpots().ToArray();
                spotIndex = 0;

                if (thisWave.slimes > 0)
                {
                    Game1.getFarm().characters.Add(new GreenSlime(spots[spotIndex++]*Game1.tileSize) { willDestroyObjectsUnderfoot = true });
                    spotIndex %= spots.Length;
                    thisWave.slimes--;
                }
                if (thisWave.bigSlimes > 0)
                {
                    Game1.getFarm().characters.Add(new BigSlime(spots[spotIndex++] * Game1.tileSize, 40) { willDestroyObjectsUnderfoot = true });
                    spotIndex %= spots.Length;
                    thisWave.bigSlimes--;
                }
                if (thisWave.bats > 0)
                {
                    Game1.getFarm().characters.Add(new Bat(spots[spotIndex++] * Game1.tileSize));
                    spotIndex %= spots.Length;
                    thisWave.bats--;
                }
                if (thisWave.flies > 0)
                {
                    Game1.getFarm().characters.Add(new Fly(spots[spotIndex++] * Game1.tileSize));
                    spotIndex %= spots.Length;
                    thisWave.flies--;
                }
                if (thisWave.dustSpirits > 0)
                {
                    Game1.getFarm().characters.Add(new DustSpirit(spots[spotIndex++] * Game1.tileSize) { willDestroyObjectsUnderfoot = true });
                    spotIndex %= spots.Length;
                    thisWave.dustSpirits--;
                }
                if (thisWave.shadowBrutes > 0)
                {
                    Game1.getFarm().characters.Add(new ShadowBrute(spots[spotIndex++] * Game1.tileSize) { willDestroyObjectsUnderfoot = true });
                    spotIndex %= spots.Length;
                    thisWave.shadowBrutes--;
                }
                if (thisWave.skeletons > 0)
                {
                    Game1.getFarm().characters.Add(new Skeleton(spots[spotIndex++] * Game1.tileSize) { willDestroyObjectsUnderfoot = true });
                    spotIndex %= spots.Length;
                    thisWave.skeletons--;
                }
                if (thisWave.shadowShamans > 0)
                {
                    Game1.getFarm().characters.Add(new ShadowShaman(spots[spotIndex++] * Game1.tileSize) { willDestroyObjectsUnderfoot = true });
                    spotIndex %= spots.Length;
                    thisWave.shadowShamans--;
                }
                if (thisWave.serpents > 0)
                {
                    Game1.getFarm().characters.Add(new Serpent(spots[spotIndex++] * Game1.tileSize));
                    spotIndex %= spots.Length;
                    thisWave.serpents--;
                }
                if (thisWave.squidKids > 0)
                {
                    Game1.getFarm().characters.Add(new SquidKid(spots[spotIndex++] * Game1.tileSize));
                    spotIndex %= spots.Length;
                    thisWave.squidKids--;
                }
                if (thisWave.dinos > 0)
                {
                    Game1.getFarm().characters.Add(new DinoMonster(spots[spotIndex++] * Game1.tileSize) { willDestroyObjectsUnderfoot = true });
                    spotIndex %= spots.Length;
                    thisWave.dinos--;
                }
                if (thisWave.skulls > 0)
                {
                    Game1.getFarm().characters.Add(new Bat(spots[spotIndex++] * Game1.tileSize, 77377));
                    spotIndex %= spots.Length;
                    thisWave.skulls--;
                }
                if (thisWave.dolls > 0)
                {
                    Game1.getFarm().characters.Add(new Bat(spots[spotIndex++] * Game1.tileSize, -666));
                    spotIndex %= spots.Length;
                    thisWave.dolls--;
                }
            }
        }

        private IOrderedEnumerable<Vector2> GetOpenSpots()
        {
            Farm farm = Game1.getFarm();
            List<Vector2> clearSpots = new List<Vector2>();
            for (int x = 0; x < farm.map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < farm.map.Layers[0].LayerHeight; y++)
                {
                    Tile tile = farm.map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                    if (Vector2.Distance(Game1.player.getTileLocation(), new Vector2(x, y)) < Config.MaxDistanceSpawn && tile != null && farm.map.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && farm.map.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && !farm.waterTiles[x,y] && !farm.objects.ContainsKey(new Vector2(x, y)))
                    {
                        clearSpots.Add(new Vector2(x, y));
                    }
                }
            }
            return clearSpots.OrderBy(s => Game1.random.NextDouble());
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.player.IsMainPlayer || !(Game1.player.currentLocation is Farm))
                return;

            if (e.NewTime > e.OldTime && e.NewTime > 800 && e.NewTime < 1900)
            {
                if(e.NewTime % 100 == 0)
                    NewWave(e.NewTime / 100 - 9);
                else if(e.NewTime % 100 == 50 && e.NewTime < 1800)
                {
                    Game1.addHUDMessage(new HUDMessage(string.Format(Helper.Translation.Get("prepare"), e.NewTime / 100 - 7), 2));
                    Game1.playSound("Pickup_Coin15");
                }
            }
        }

        private void NewWave(int wave)
        {
            ClearMonsters();
            Game1.getFarm().debris.Clear();
            thisWave = waves[wave];
            monstersDeployed = 0;
            spotIndex = 0;
            waveStarted = true;
        }

        private void ClearMonsters()
        {
            List<NPC> monsters = Game1.getFarm().characters.Where(n => n is Monster).ToList();
            foreach (NPC m in monsters)
            {
                Game1.getFarm().characters.Remove(m);
            }
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            ticksSinceMorning = 0;
        }
        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            ClearMonsters();
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (waveStarted)
                Game1.timeOfDay = 100 * (Game1.timeOfDay / 100);
            if (!Context.IsWorldReady || !Game1.player.IsMainPlayer || !(Game1.player.currentLocation is Farm))
                return;
            Farm farm = Game1.getFarm();

            if (farm == null)
                return;



            IEnumerable<Vector2> monsterPosList = farm.characters.Where(n => n is Monster && (n as Monster).health > 0).Select(m => m.position.Value);

            if (!monsterPosList.Any())
                return;

            foreach(Vector2 pos in monsterPosList)
            {
                KeyValuePair<Vector2, TerrainFeature> kvp = farm.terrainFeatures.Pairs.FirstOrDefault(k => k.Value is HoeDirt && (k.Value as HoeDirt).crop != null && k.Key == pos / Game1.tileSize);
                if(!kvp.Equals(default(KeyValuePair<Vector2, TerrainFeature>)))
                {
                    (farm.terrainFeatures[kvp.Key] as HoeDirt).destroyCrop(kvp.Key, false, farm);
                }
                    
            }

            foreach (KeyValuePair<Vector2, Object> crow in farm.objects.Pairs.Where(s => s.Value.bigCraftable && s.Value.Name.Contains("arecrow")))
            {
                MurderCrow mc = murderCrows[crow.Value.Name];
                IEnumerable<Vector2> monsters = monsterPosList.Where(m => Vector2.Distance(m, crow.Key * Game1.tileSize) < mc.range * Game1.tileSize).OrderBy(m => Vector2.Distance(m, crow.Key));

                if (monsters.Any() && ticksSinceMorning % (1000 / mc.rate) == 0)
                {
                    Vector2 dir = (monsters.Last() - crow.Key * Game1.tileSize);
                    dir.Normalize();

                    if(mc.name == "Rarecrow" || mc.name == "Iridium Scarecrow")
                    {
                        if (ticksSinceMorning % 1000  == 0)
                            farm.playSound("furnace"); 


                        float fire_angle = (float)Math.Atan2(dir.Y, dir.X);
                        if(mc.name == "Iridium Scarecrow")
                            fire_angle += (float)Math.Sin((double)((float)ticksSinceMorning % 180f) * 3.1415926535897931 / 180.0) * 25f;
                        else
                            fire_angle += (float)Math.Sin((double)((float)ticksSinceMorning % 10f) * 3.1415926535897931 / 180.0);

                        Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle), (float)Math.Sin((double)fire_angle));
                        shot_velocity *= 10f;
                        BasicProjectile projectile = new BasicProjectile(mc.damage, 10, 0, 1, 0.196349546f, shot_velocity.X, shot_velocity.Y, crow.Key * Game1.tileSize, "", "", false, true, farm, Game1.MasterPlayer, false, null);
                        projectile.ignoreTravelGracePeriod.Value = true;
                        projectile.maxTravelDistance.Value = mc.range * 64;
                        farm.projectiles.Add(projectile);
                    }
                    else 
                        farm.projectiles.Add(new BasicProjectile(mc.damage, mc.ammoIndex, 0, 0, 0.3f, dir.X * shotVelocity, dir.Y * shotVelocity, crow.Key * Game1.tileSize, mc.hitSound, mc.fireSound, mc.explode, true, farm, Game1.MasterPlayer, mc.useTileSheet));
                }
            }
            ticksSinceMorning++;
        }

    }
}
