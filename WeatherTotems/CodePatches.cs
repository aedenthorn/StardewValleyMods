using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace WeatherTotems
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), "rainTotem")]
        public class Object_rainTotem_Patch
        {
            public static bool Prefix(Object __instance, Farmer who)
            {
                if (!Config.ModEnabled)
                    return true;

                SMonitor.Log("Showing weather totem dialogue");
                Game1.currentLocation.createQuestionDialogue(
                    SHelper.Translation.Get("which-totem"), 
                    new Response[]
                    {
                        new Response("Cloudy", SHelper.Translation.Get("cloudy")),
                        new Response("Rain", SHelper.Translation.Get("rain")),
                        new Response("Thunder", SHelper.Translation.Get("thunder")),
                        new Response("Sunny", SHelper.Translation.Get("sunny")),
                        new Response("Snow", SHelper.Translation.Get("snow")),
                        new Response("Cancel", SHelper.Translation.Get("cancel"))
                    },
                    "WeatherTotem");
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
        public class GameLocation_answerDialogueAction_Patch
        {
            public static bool Prefix(string questionAndAnswer, string[] questionParams, ref bool __result)
            {
                if (!Config.ModEnabled || !questionAndAnswer.StartsWith("WeatherTotem_"))
                    return true;
                var which = questionAndAnswer.Substring("WeatherTotem_".Length);
                int weather = 0; // sunny
                string sound = Config.SunnySound;
                switch (which)
                {
                    case "Sunny":
                        break;
                    case "Rain":
                        weather = 1;
                        sound = Config.RainSound;
                        break;
                    case "Thunder":
                        weather = 3;
                        sound = Config.ThunderSound;
                        break;
                    case "Cloudy":
                        weather = 2;
                        sound = Config.CloudySound;
                        break;
                    case "Snow":
                        sound = Config.SnowSound;
                        weather = 5;
                        break;
                    default:
                        __result = true;
                        return false;
                }
                GameLocation.LocationContext location_context = Game1.currentLocation.GetLocationContext();
                if (location_context == GameLocation.LocationContext.Default)
                {
                    if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
                    {
                        Game1.netWorldState.Value.WeatherForTomorrow = (Game1.weatherForTomorrow = weather);
                        Game1.pauseThenMessage(2000, SHelper.Translation.Get("message-"+which.ToLower()), false);
                    }
                }
                else
                {
                    Game1.netWorldState.Value.GetWeatherForLocation(location_context).weatherForTomorrow.Value = weather;
                    Game1.pauseThenMessage(2000, SHelper.Translation.Get("message-" + which.ToLower()), false);
                }
                Game1.screenGlow = false;
                if(!string.IsNullOrEmpty(Config.InvokeSound))
                    Game1.player.currentLocation.playSound(Config.InvokeSound, NetAudio.SoundContext.Default);
                Game1.player.canMove = false;
                Game1.screenGlowOnce(Color.SlateBlue, false, 0.005f, 0.3f);
                Game1.player.faceDirection(2);
                Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[]
                {
                new FarmerSprite.AnimationFrame(57, 2000, false, false, new AnimatedSprite.endOfAnimationBehavior(Farmer.canMoveNow), true)
                }, null);
                var mp = (Multiplayer)AccessTools.Field(typeof(Game1), "multiplayer").GetValue(null);
                for (int i = 0; i < 6; i++)
                {
                    mp.broadcastSprites(Game1.player.currentLocation, new TemporaryAnimatedSprite[]
                    {
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -128f), false, false, 1f, 0.01f, Color.White * 0.8f, 2f, 0.01f, 0f, 0f, false)
                    {
                        motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -2f),
                        delayBeforeAnimationStart = i * 200
                    }
                    });
                    mp.broadcastSprites(Game1.player.currentLocation, new TemporaryAnimatedSprite[]
                    {
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -128f), false, false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f, false)
                    {
                        motion = new Vector2((float)Game1.random.Next(-30, -10) / 10f, -1f),
                        delayBeforeAnimationStart = 100 + i * 200
                    }
                    });
                    mp.broadcastSprites(Game1.player.currentLocation, new TemporaryAnimatedSprite[]
                    {
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -128f), false, false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f, false)
                    {
                        motion = new Vector2((float)Game1.random.Next(10, 30) / 10f, -1f),
                        delayBeforeAnimationStart = 200 + i * 200
                    }
                    });
                }
                mp.broadcastSprites(Game1.player.currentLocation, new TemporaryAnimatedSprite[]
                {
                new TemporaryAnimatedSprite(681, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), false, false, false, 0f)
                {
                    motion = new Vector2(0f, -7f),
                    acceleration = new Vector2(0f, 0.1f),
                    scaleChange = 0.015f,
                    alpha = 1f,
                    alphaFade = 0.0075f,
                    shakeIntensity = 1f,
                    initialPosition = Game1.player.Position + new Vector2(0f, -96f),
                    xPeriodic = true,
                    xPeriodicLoopTime = 1000f,
                    xPeriodicRange = 4f,
                    layerDepth = 1f
                }
                });
                if(!string.IsNullOrEmpty(sound))
                    DelayedAction.playSoundAfterDelay(sound, 2000, null, -1);
                SMonitor.Log($"Set tomorrow's weather to {which}");

                __result = true;
                return false;
            }
        }

    }
}