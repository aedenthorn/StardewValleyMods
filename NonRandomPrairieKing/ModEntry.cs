using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace NonRandomPrairieKing
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;
        public static IMonitor SMonitor;
        public static Random myRand;
        public static int[] hundred;
        public static int[] hundred1;
        public static int[] hundred2;
        public static int[] hundred3;
        public static int[] hundred4;
        public static int[] hundred5;
        public static int[] hundred6;
        public static int[] hundred7;
        public static int[] twoNine;
        public static int[] twoNine1;
        public static int[] twoNine2;
        public static int[] sixSeven;
        public static int[] hundred9;
        public static int[] hundred8;
        public static int hundredIndex1 = 0;
        public static int hundredIndex2 = 0;
        public static int hundredIndex3 = 0;
        public static int hundredIndex4 = 0;
        public static int hundredIndex5 = 0;
        public static int hundredIndex6 = 0;
        public static int hundredIndex7 = 0;
        public static int twoNineIndex1 = 0;
        public static int twoNineIndex2 = 0;
        public static int hundredIndex9 = 0;
        public static int hundredIndex8 = 0;
        public static int sixSevenIndex = 0;
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;

            myRand = new Random();
            hundred = Enumerable.Range(0, 100).ToArray();
            twoNine = Enumerable.Range(2, 8).ToArray();

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(AbigailGame), nameof(AbigailGame.updateBullets)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(AbigailGame_updateBullets_Prefix))
               //transpiler: new HarmonyMethod(typeof(ModEntry), nameof(AbigailGame_updateBullets_Transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(AbigailGame), nameof(AbigailGame.reset)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(AbigailGame_reset_Prefix))
            );


            harmony.Patch(
               original: AccessTools.Method(typeof(AbigailGame.CowboyMonster), nameof(AbigailGame.CowboyMonster.getLootDrop)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(AbigailGame_CowboyMonster_getLootDrop_Prefix))
            );
        }


        public static void AbigailGame_reset_Prefix()
        {
            hundred1 = hundred.OrderBy(x => myRand.Next()).ToArray();
            hundred2 = hundred.OrderBy(x => myRand.Next()).ToArray();
            hundred3 = hundred.OrderBy(x => myRand.Next()).ToArray();
            hundred4 = hundred.OrderBy(x => myRand.Next()).ToArray();
            hundred5 = hundred.OrderBy(x => myRand.Next()).ToArray();
            hundred6 = hundred.OrderBy(x => myRand.Next()).ToArray();
            hundred7 = hundred.OrderBy(x => myRand.Next()).ToArray();
            hundred8 = hundred.OrderBy(x => myRand.Next()).ToArray();
            hundred9 = hundred.OrderBy(x => myRand.Next()).ToArray();
            sixSeven = hundred.OrderBy(x => myRand.Next()).ToArray();
            twoNine1 = twoNine.OrderBy(x => myRand.Next()).ToArray();
            twoNine2 = twoNine.OrderBy(x => myRand.Next()).ToArray();
        }
        public static bool AbigailGame_CowboyMonster_getLootDrop_Prefix(AbigailGame.CowboyMonster __instance, ref int __result)
        {
            if (__instance.type == 6 && __instance.special)
            {
                __result = -1;
                return false;
            }
            hundredIndex9++;
            hundredIndex9 %= 100;
            if (hundred9[hundredIndex9] < 5)
            {
                if (__instance.type != 0)
                {
                    if(hundred1[hundredIndex1] < 10)
                    {
                        __result = 1;
                        return false;
                    }
                    hundredIndex1++;
                    hundredIndex1 %= 100;
                }

                if (hundred2[hundredIndex2] < 1)
                    __result = 1;
                else
                    __result = 0;
                hundredIndex2++;
                hundredIndex2 %= 100;
                return false; 
            }
            hundredIndex8++;
            hundredIndex8 %= 100;
            if (hundred8[hundredIndex8] >= 5)
            {
                __result = -1;
                return false;
            }
            hundredIndex3++;
            hundredIndex3 %= 100;
            if (hundred3[hundredIndex3] < 15)
            {
                sixSevenIndex++;
                sixSevenIndex %= 100;
                __result = sixSeven[sixSevenIndex] < 50 ? 6 : 7;
                return false;
            }
            hundredIndex4++;
            hundredIndex4 %= 100;
            if (hundred4[hundredIndex4] < 7)
                __result = 10;
            else
            {
                twoNineIndex1++;
                twoNineIndex1 %= 8;
                int loot = twoNine1[twoNineIndex1];
                hundredIndex5++;
                hundredIndex5 %= 100;
                if (loot == 5 && hundred5[hundredIndex5] < 40)
                {
                    twoNineIndex2++;
                    twoNineIndex2 %= 8;
                    loot = twoNine2[twoNineIndex2];
                }
                __result = loot;
            }
            return false;
        }

        public static bool AbigailGame_updateBullets_Prefix(AbigailGame __instance, GameTime time, ICue ___outlawSong)
        {
            for (int i = __instance.bullets.Count - 1; i >= 0; i--)
            {
                AbigailGame.CowboyBullet cowboyBullet = __instance.bullets[i];
                cowboyBullet.position.X = cowboyBullet.position.X + __instance.bullets[i].motion.X;
                AbigailGame.CowboyBullet cowboyBullet2 = __instance.bullets[i];
                cowboyBullet2.position.Y = cowboyBullet2.position.Y + __instance.bullets[i].motion.Y;
                if (__instance.bullets[i].position.X <= 0 || __instance.bullets[i].position.Y <= 0 || __instance.bullets[i].position.X >= 768 || __instance.bullets[i].position.Y >= 768)
                {
                    __instance.bullets.RemoveAt(i);
                }
                else if (AbigailGame.map[__instance.bullets[i].position.X / 16 / 3, __instance.bullets[i].position.Y / 16 / 3] == 7)
                {
                    __instance.bullets.RemoveAt(i);
                }
                else
                {
                    int j = AbigailGame.monsters.Count - 1;
                    while (j >= 0)
                    {
                        if (AbigailGame.monsters[j].position.Intersects(new Rectangle(__instance.bullets[i].position.X, __instance.bullets[i].position.Y, 12, 12)))
                        {
                            int monsterhealth = AbigailGame.monsters[j].health;
                            int monsterAfterDamageHealth;
                            if (AbigailGame.monsters[j].takeDamage(__instance.bullets[i].damage))
                            {
                                monsterAfterDamageHealth = AbigailGame.monsters[j].health;
                                AbigailGame.addGuts(AbigailGame.monsters[j].position.Location, AbigailGame.monsters[j].type);
                                int loot = AbigailGame.monsters[j].getLootDrop();
                                hundredIndex6++;
                                hundredIndex6 %= 100;
                                if (__instance.whichRound == 1 && hundred6[hundredIndex6] < 50)
                                {
                                    loot = -1;
                                }
                                if (__instance.whichRound > 0 && (loot == 5 || loot == 8))
                                {
                                    hundredIndex7++;
                                    hundredIndex7 %= 100;
                                    if(hundred7[hundredIndex7] < 40)
                                        loot = -1;
                                }
                                if (loot != -1 && AbigailGame.whichWave != 12)
                                {
                                    AbigailGame.powerups.Add(new AbigailGame.CowboyPowerup(loot, AbigailGame.monsters[j].position.Location, __instance.lootDuration));
                                }
                                if (AbigailGame.shootoutLevel)
                                {
                                    if (AbigailGame.whichWave == 12 && AbigailGame.monsters[j].type == -2)
                                    {
                                        Game1.playSound("cowboy_explosion");
                                        AbigailGame.powerups.Add(new AbigailGame.CowboyPowerup(-3, new Point(8 * AbigailGame.TileSize, 10 * AbigailGame.TileSize), 9999999));
                                        __instance.noPickUpBox = new Rectangle(8 * AbigailGame.TileSize, 10 * AbigailGame.TileSize, AbigailGame.TileSize, AbigailGame.TileSize);
                                        if (___outlawSong != null && ___outlawSong.IsPlaying)
                                        {
                                            ___outlawSong.Stop(AudioStopOptions.Immediate);
                                        }
                                        AbigailGame.screenFlash = 200;
                                        for (int k = 0; k < 30; k++)
                                        {
                                            AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(512, 1696, 16, 16), 70f, 6, 0, new Vector2((float)(AbigailGame.monsters[j].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), (float)(AbigailGame.monsters[j].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize))) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
                                            {
                                                delayBeforeAnimationStart = k * 75
                                            });
                                            if (k % 4 == 0)
                                            {
                                                AbigailGame.addGuts(new Point(AbigailGame.monsters[j].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize), AbigailGame.monsters[j].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), 7);
                                            }
                                            if (k % 4 == 0)
                                            {
                                                AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2((float)(AbigailGame.monsters[j].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), (float)(AbigailGame.monsters[j].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize))) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
                                                {
                                                    delayBeforeAnimationStart = k * 75
                                                });
                                            }
                                            if (k % 3 == 0)
                                            {
                                                AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(544, 1728, 16, 16), 100f, 4, 0, new Vector2((float)(AbigailGame.monsters[j].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), (float)(AbigailGame.monsters[j].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize))) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
                                                {
                                                    delayBeforeAnimationStart = k * 75
                                                });
                                            }
                                        }
                                    }
                                    else if (AbigailGame.whichWave != 12)
                                    {
                                        AbigailGame.powerups.Add(new AbigailGame.CowboyPowerup((AbigailGame.world == 0) ? -1 : -2, new Point(8 * AbigailGame.TileSize, 10 * AbigailGame.TileSize), 9999999));
                                        if (___outlawSong != null && ___outlawSong.IsPlaying)
                                        {
                                            ___outlawSong.Stop(AudioStopOptions.Immediate);
                                        }
                                        AbigailGame.map[8, 8] = 10;
                                        AbigailGame.screenFlash = 200;
                                        for (int l = 0; l < 15; l++)
                                        {
                                            AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2((float)(AbigailGame.monsters[j].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), (float)(AbigailGame.monsters[j].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize))) + AbigailGame.topLeftScreenCoordinate + new Vector2((float)(AbigailGame.TileSize / 2), (float)(AbigailGame.TileSize / 2)), false, false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, true)
                                            {
                                                delayBeforeAnimationStart = l * 75
                                            });
                                        }
                                    }
                                }
                                AbigailGame.monsters.RemoveAt(j);
                                Game1.playSound("Cowboy_monsterDie");
                            }
                            else
                            {
                                monsterAfterDamageHealth = AbigailGame.monsters[j].health;
                            }
                            __instance.bullets[i].damage -= monsterhealth - monsterAfterDamageHealth;
                            if (__instance.bullets[i].damage <= 0)
                            {
                                __instance.bullets.RemoveAt(i);
                                break;
                            }
                            break;
                        }
                        else
                        {
                            j--;
                        }
                    }
                }
            }
            for (int m = AbigailGame.enemyBullets.Count - 1; m >= 0; m--)
            {
                AbigailGame.CowboyBullet cowboyBullet3 = AbigailGame.enemyBullets[m];
                cowboyBullet3.position.X = cowboyBullet3.position.X + AbigailGame.enemyBullets[m].motion.X;
                AbigailGame.CowboyBullet cowboyBullet4 = AbigailGame.enemyBullets[m];
                cowboyBullet4.position.Y = cowboyBullet4.position.Y + AbigailGame.enemyBullets[m].motion.Y;
                if (AbigailGame.enemyBullets[m].position.X <= 0 || AbigailGame.enemyBullets[m].position.Y <= 0 || AbigailGame.enemyBullets[m].position.X >= 762 || AbigailGame.enemyBullets[m].position.Y >= 762)
                {
                    AbigailGame.enemyBullets.RemoveAt(m);
                }
                else if (AbigailGame.map[(AbigailGame.enemyBullets[m].position.X + 6) / 16 / 3, (AbigailGame.enemyBullets[m].position.Y + 6) / 16 / 3] == 7)
                {
                    AbigailGame.enemyBullets.RemoveAt(m);
                }
                else if (AbigailGame.playerInvincibleTimer <= 0 && AbigailGame.deathTimer <= 0f && __instance.playerBoundingBox.Intersects(new Rectangle(AbigailGame.enemyBullets[m].position.X, AbigailGame.enemyBullets[m].position.Y, 15, 15)))
                {
                    __instance.playerDie();
                    return false;
                }
            }
            return false;
        }

        public static IEnumerable<CodeInstruction> AbigailGame_updateBullets_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Stloc_S)
                {
                    SMonitor.Log(""+codes[i].operand);

                }
                if (codes[i].opcode == OpCodes.Stloc_S && codes[i].operand.ToString() == "System.Int32 (4)"  && codes[i-1].opcode == OpCodes.Ldc_I4_M1)
                {
                    SMonitor.Log($"got loot opcode!");
                    codes[i - 1] = codes[i];
                }
            }

            return codes.AsEnumerable();
        }
    }    
}
