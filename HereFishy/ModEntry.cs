using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using Object = StardewValley.Object;

namespace HereFishy
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;

        public static ModConfig Config;
        private static SoundEffect fishySound;
        private static SoundEffect weeSound;
        private static List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();
        private static SparklingText sparklingText;
        private static bool caughtDoubleFish;
        private static Farmer lastUser;
        private static int whichFish;
        private static int fishSize;
        private static bool recordSize;
        private static bool perfect;
        private static int fishQuality;
        private static bool fishCaught;
        private static bool bossFish;
        private static int fishDifficulty;
        private static bool canPerfect;
        private static bool hereFishying;


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            try
            {
                fishySound = SoundEffect.FromStream(new FileStream(Path.Combine(Helper.DirectoryPath, "assets", "fishy.wav"), FileMode.Open));
                weeSound = SoundEffect.FromStream(new FileStream(Path.Combine(Helper.DirectoryPath, "assets", "wee.wav"), FileMode.Open));
            }
            catch(Exception ex)
            {
                context.Monitor.Log($"error loading fishy.wav: {ex}");
            }

            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseRight && Context.IsWorldReady && Context.CanPlayerMove && (Game1.player.CurrentTool is FishingRod))
            {
                if (hereFishying)
                {
                    if (canPerfect)
                    {
                        perfect = true;
                        sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"), Color.Yellow, Color.White, false, 0.1, 1500, -1, 500, 1f);
                        Game1.playSound("jingle1");
                    }
                    return;
                }

                try
                {
                    Vector2 mousePos = Game1.currentCursorTile;
                    if (Game1.player.currentLocation.waterTiles != null && Game1.player.currentLocation.waterTiles[(int)mousePos.X, (int)mousePos.Y])
                    {
                        context.Monitor.Log($"here fishy fishy {mousePos.X},{mousePos.Y}");
                        HereFishyFishy(Game1.player, (int)mousePos.X * 64, (int)mousePos.Y * 64);
                    }

                }
                catch
                {
                    context.Monitor.Log($"error getting water tile");
                }
            }

        }

        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            for (int i = animations.Count - 1; i >= 0; i--)
            {
                if (animations[i].update(Game1.currentGameTime))
                {
                    animations.RemoveAt(i);
                }
            }
            if (sparklingText != null && sparklingText.update(Game1.currentGameTime))
            {
                sparklingText = null;
            }
            if (fishCaught)
            {
                lastUser.addItemToInventoryBool(new Object(whichFish, caughtDoubleFish ? 2 : 1, false, -1, fishQuality), false);
                fishCaught = false;
            }
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            for (int i = animations.Count - 1; i >= 0; i--)
            {
                animations[i].draw(e.SpriteBatch, false, 0, 0, 1f);
            }
            if (sparklingText != null && lastUser != null)
            {
                sparklingText.draw(e.SpriteBatch, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-64f, -352f)));
            }

        }

        private static async void HereFishyFishy(Farmer who, int x, int y)
        {
            hereFishying = true;
            if (fishySound != null)
            {
                fishySound.Play();
            }
            who.completelyStopAnimatingOrDoingAction();
            who.jitterStrength = 2f;
            List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>(){
                new FarmerSprite.AnimationFrame(94, 100, false, false, null, false).AddFrameAction(delegate (Farmer f)
                {
                    f.jitterStrength = 2f;
                })
            };
            who.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
            who.FarmerSprite.PauseForSingleAnimation = true;
            who.FarmerSprite.loop = true;
            who.FarmerSprite.loopThisAnimation = true;
            who.Sprite.currentFrame = 94;

            await System.Threading.Tasks.Task.Delay(1793);

            canPerfect = true;
            perfect = false;

            who.synchronizedJump(8f);

            await System.Threading.Tasks.Task.Delay(100);

            canPerfect = false;

            await System.Threading.Tasks.Task.Delay(900);

            who.stopJittering();
            who.completelyStopAnimatingOrDoingAction();
            who.forceCanMove();

            hereFishying = false;

            await System.Threading.Tasks.Task.Delay(Game1.random.Next(500, 1000));

            Object o = who.currentLocation.getFish(0, -1, 1, who, 0, new Vector2(x, y), who.currentLocation.Name);
            if (o == null || o.ParentSheetIndex <= 0)
            {
                o = new Object(Game1.random.Next(167, 173), 1, false, -1, 0);
            }


            int parentSheetIndex = o.ParentSheetIndex;
            animations.Clear();
            float t;
            lastUser = who;
            whichFish = parentSheetIndex;
            Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            string[] datas = null;
            if (data.ContainsKey(whichFish))
            {
                datas = data[whichFish].Split('/');
            }


            bool non_fishable_fish = false;
            if (o is Furniture)
            {
                non_fishable_fish = true;
            }
            else if (Utility.IsNormalObjectAtParentSheetIndex(o, o.ParentSheetIndex) && data.ContainsKey(o.ParentSheetIndex))
            {
                string[] array = data[o.ParentSheetIndex].Split(new char[]
                {
                            '/'
                });
                int difficulty = -1;
                if (!int.TryParse(array[1], out difficulty))
                {
                    non_fishable_fish = true;
                }
            }
            else
            {
                non_fishable_fish = true;
            }


            float fs = 1f;
            int minimumSizeContribution = 1 + who.FishingLevel / 2;
            fs *= (float)Game1.random.Next(minimumSizeContribution, Math.Max(6, minimumSizeContribution)) / 5f;
            fs *= 1.2f;
            fs *= 1f + (float)Game1.random.Next(-10, 11) / 100f;
            fs = Math.Max(0f, Math.Min(1f, fs));
            if(datas != null && !non_fishable_fish)
            {
                try
                {
                    int minFishSize = int.Parse(datas[3]);
                    int maxFishSize = int.Parse(datas[4]);
                    fishSize = (int)((float)minFishSize + (float)(maxFishSize - minFishSize) * fishSize);
                    fishSize++;
                    fishQuality = (((double)fishSize < 0.33) ? 0 : (((double)fishSize < 0.66) ? 1 : 2));
                    if (perfect)
                        fishQuality *= 2;
                }
                catch 
                {
                    context.Monitor.Log($"Error getting fish size from {data[whichFish]}", LogLevel.Error);
                }
            }
            bossFish = FishingRod.isFishBossFish(whichFish);
            caughtDoubleFish = !bossFish && Game1.random.NextDouble() < 0.1 + Game1.player.DailyLuck / 2.0;

            context.Monitor.Log($"pulling fish {whichFish} {fishSize} {who.Name} {x},{y}");

            if (who.IsLocalPlayer)
            {
                if (datas != null && !non_fishable_fish)
                {
                    fishDifficulty = int.Parse(datas[1]);

                }
                else
                    fishDifficulty = 0;
                
                int experience = Math.Max(1, (fishQuality + 1) * 3 + fishDifficulty / 3);
                if (bossFish)
                {
                    experience *= 5;
                }
                
                if(perfect)
                    experience += (int)((float)experience * 1.4f);
                
                who.gainExperience(1, experience);
            }
            if (weeSound != null)
            {
                weeSound.Play();
            }
            if (who.FacingDirection == 1 || who.FacingDirection == 3)
            {
                float distance = Vector2.Distance(new Vector2(x, y), who.Position);
                float gravity = 0.001f;
                float height = 128f - (who.Position.Y - y + 10f);
                double angle = 1.1423973285781066;
                float yVelocity = (float)((double)(distance * gravity) * Math.Tan(angle) / Math.Sqrt((double)(2f * distance * gravity) * Math.Tan(angle) - (double)(2f * gravity * height)));
                if (float.IsNaN(yVelocity))
                {
                    yVelocity = 0.6f;
                }
                float xVelocity = (float)((double)yVelocity * (1.0 / Math.Tan(angle)));
                t = distance / xVelocity;
                animations.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), t, 1, 0, new Vector2(x,y), false, false, y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                {
                    motion = new Vector2((float)((who.FacingDirection == 3) ? -1 : 1) * -xVelocity, -yVelocity),
                    acceleration = new Vector2(0f, gravity),
                    timeBasedMotion = true,
                    endFunction = new TemporaryAnimatedSprite.endBehavior(playerCaughtFishEndFunction),
                    extraInfoForEndBehavior = parentSheetIndex,
                    endSound = "tinyWhip"
                });
                if (caughtDoubleFish)
                {
                    distance = Vector2.Distance(new Vector2(x, y), who.Position);
                    gravity = 0.0008f;
                    height = 128f - (who.Position.Y - y + 10f);
                    angle = 1.1423973285781066;
                    yVelocity = (float)((double)(distance * gravity) * Math.Tan(angle) / Math.Sqrt((double)(2f * distance * gravity) * Math.Tan(angle) - (double)(2f * gravity * height)));
                    if (float.IsNaN(yVelocity))
                    {
                        yVelocity = 0.6f;
                    }
                    xVelocity = (float)((double)yVelocity * (1.0 / Math.Tan(angle)));
                    t = distance / xVelocity;
                    animations.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                    {
                        motion = new Vector2((float)((who.FacingDirection == 3) ? -1 : 1) * -xVelocity, -yVelocity),
                        acceleration = new Vector2(0f, gravity),
                        timeBasedMotion = true,
                        endSound = "fishSlap",
                        Parent = who.currentLocation
                    });
                }
            }
            else
            {
                float distance2 = y - (float)(who.getStandingY() - 64);
                float height2 = Math.Abs(distance2 + 256f + 32f);
                if (who.FacingDirection == 0)
                {
                    height2 += 96f;
                }
                float gravity2 = 0.003f;
                float velocity = (float)Math.Sqrt((double)(2f * gravity2 * height2));
                t = (float)(Math.Sqrt((double)(2f * (height2 - distance2) / gravity2)) + (double)(velocity / gravity2));
                float xVelocity2 = 0f;
                if (t != 0f)
                {
                    xVelocity2 = (who.Position.X - x) / t;
                }
                animations.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                {
                    motion = new Vector2(xVelocity2, -velocity),
                    acceleration = new Vector2(0f, gravity2),
                    timeBasedMotion = true,
                    endFunction = new TemporaryAnimatedSprite.endBehavior(playerCaughtFishEndFunction),
                    extraInfoForEndBehavior = parentSheetIndex,
                    endSound = "tinyWhip"
                });
                if (caughtDoubleFish)
                {
                    distance2 = y - (float)(who.getStandingY() - 64);
                    height2 = Math.Abs(distance2 + 256f + 32f);
                    if (who.FacingDirection == 0)
                    {
                        height2 += 96f;
                    }
                    gravity2 = 0.004f;
                    velocity = (float)Math.Sqrt((double)(2f * gravity2 * height2));
                    t = (float)(Math.Sqrt((double)(2f * (height2 - distance2) / gravity2)) + (double)(velocity / gravity2));
                    xVelocity2 = 0f;
                    if (t != 0f)
                    {
                        xVelocity2 = (who.Position.X - x) / t;
                    }
                    animations.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                    {
                        motion = new Vector2(xVelocity2, -velocity),
                        acceleration = new Vector2(0f, gravity2),
                        timeBasedMotion = true,
                        endSound = "fishSlap",
                        Parent = who.currentLocation
                    });
                }
            }
        }
        public static void playerCaughtFishEndFunction(int extraData)
        {
            context.Monitor.Log($"caught fish end");
            fishCaught = true;
            fishQuality = Game1.random.Next(0, 5);
            if (fishQuality == 3)
            {
                fishQuality = 0;
            }
            lastUser.Halt();
            lastUser.armOffset = Vector2.Zero;

            recordSize = lastUser.caughtFish(whichFish, fishSize, false, caughtDoubleFish ? 2 : 1);
            lastUser.faceDirection(2);
            if (FishingRod.isFishBossFish(whichFish))
            {
                Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14068"));
                string name = Game1.objectInformation[whichFish].Split(new char[]
                {
                    '/'
                })[4];
                context.Helper.Reflection.GetField<Multiplayer>(Game1.game1, "multiplayer").GetValue().globalChatInfoMessage("CaughtLegendaryFish", new string[]
                {
                    Game1.player.Name,
                    name
                });
                return;
            }
            if (recordSize)
            {
                sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14069"), Color.LimeGreen, Color.Azure, false, 0.1, 2500, -1, 500, 1f);
                lastUser.currentLocation.localSound("newRecord");
                return;
            }
            lastUser.currentLocation.localSound("fishSlap");
        }
    }
}
