using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using Object = StardewValley.Object;

namespace Familiars
{
    public class FamiliarsPatches
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

        public static bool Object_performObjectDropInAction_Prefix(Object __instance, Item dropInItem, ref bool __result, bool probe, Farmer who)
        {
            if (__instance.isTemporarilyInvisible)
            {
                return true;
            }
            if (!(dropInItem is Object))
            {
                return true;
            }
            Object dropIn = dropInItem as Object;

            if (__instance.name.Equals("Slime Incubator"))
            {
                if (__instance.heldObject.Value == null && dropIn.name.Contains("Familiar Egg"))
                {
                    __instance.heldObject.Value = new Object(dropIn.parentSheetIndex, 1, false, -1, 0);
                    if (!probe)
                    {
                        who.currentLocation.playSound("coin", NetAudio.SoundContext.Default);
                        __instance.minutesUntilReady.Value = Config.FamiliarHatchMinutes;
                        if (who.professions.Contains(2))
                        {
                            __instance.minutesUntilReady.Value /= 2;
                        }
                        int num = __instance.ParentSheetIndex;
                        __instance.ParentSheetIndex = num + 1;
                    }
                    __result = true;
                    return false;
                }
            }
            else if (__instance.name.Equals("Slime Egg-Press"))
            {
                if (__instance.heldObject.Value == null && !probe && dropIn.parentSheetIndex == 767 && dropIn.Stack >= 100)
                {
                    dropIn.Stack -= 100;
                    if (dropIn.Stack <= 0)
                    {
                        who.removeItemFromInventory(dropIn);
                    }
                    who.currentLocation.playSound("batScreech", NetAudio.SoundContext.Default);
                    DelayedAction.playSoundAfterDelay("bubbles", 50, null, -1);
                    ModEntry.mp.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
                    {
                            new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, __instance.tileLocation.Value * 64f + new Vector2(0f, -160f), false, false, (__instance.tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Lime, 1f, 0f, 0f, 0f, false)
                            {
                                alphaFade = 0.005f
                            }
                    });
                    __instance.heldObject.Value = new Object(ModEntry.BatFamiliarEgg, 1, false, -1, 0);
                    __instance.minutesUntilReady.Value = Config.FamiliarEggMinutes;
                    __result = true;
                    return false;
                }
                else if (__instance.heldObject.Value == null && probe && dropIn.parentSheetIndex == 767 && dropIn.Stack >= 100)
                {
                    __instance.heldObject.Value = new Object();
                    __result = true;
                    return false;
                }
                else if (__instance.heldObject.Value == null && !probe && dropIn.parentSheetIndex == ModEntry.ButterflyDust && dropIn.Stack >= 100)
                {
                    dropIn.Stack -= 100;
                    if (dropIn.Stack <= 0)
                    {
                        who.removeItemFromInventory(dropIn);
                    }
                    who.currentLocation.playSound("yoba", NetAudio.SoundContext.Default);
                    DelayedAction.playSoundAfterDelay("bubbles", 50, null, -1);
                    ModEntry.mp.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
                    {
                            new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, __instance.tileLocation.Value * 64f + new Vector2(0f, -160f), false, false, (__instance.tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Lime, 1f, 0f, 0f, 0f, false)
                            {
                                alphaFade = 0.005f
                            }
                    });
                    __instance.heldObject.Value = new Object(ModEntry.ButterflyFamiliarEgg, 1, false, -1, 0);
                    __instance.minutesUntilReady.Value = Config.FamiliarEggMinutes;
                    __result = true;
                    return false;
                }
                else if (__instance.heldObject.Value == null && probe && dropIn.parentSheetIndex == ModEntry.ButterflyDust && dropIn.Stack >= 100)
                {
                    __instance.heldObject.Value = new Object();
                    __result = true;
                    return false;
                }
            }
            return true;
        }
        
        public static void Object_DayUpdate_Postfix(Object __instance, GameLocation location)
        {
            if (__instance.name.Equals("Slime Incubator") && __instance.minutesUntilReady <= 0 && __instance.heldObject.Value != null)
            {
                Vector2 v = new Vector2((float)((int)__instance.tileLocation.X), (float)((int)__instance.tileLocation.Y + 1)) * 64f;
                string name = __instance.heldObject.Value.name;

                if (!name.EndsWith("Familiar Egg"))
                    return;

                Monitor.Log($"Hatched a familiar from {name}");
                long owner = __instance.owner;
                if (owner == 0)
                    owner = Game1.MasterPlayer.UniqueMultiplayerID;
                Familiar familiar = null;

                switch (name)
                {
                    case "Dino Familiar Egg":
                        familiar = new DinoFamiliar(v, owner);
                        break;
                    case "Dust Sprite Familiar Egg":
                        familiar = new DustSpriteFamiliar(v, owner);
                        break;
                    case "Bat Familiar Egg":
                        familiar = new BatFamiliar(v, owner);
                        break;
                    case "Junimo Familiar Egg":
                        familiar = new JunimoFamiliar(v, owner);
                        break;
                    case "Butterfly Familiar Egg":
                        familiar = new ButterflyFamiliar(v, owner);
                        break;
                }

                if (familiar != null)
                {
                    Game1.showGlobalMessage(string.Format(Helper.Translation.Get("familiar-hatched"), Helper.Translation.Get(familiar.Name)));
                    familiar.setTilePosition((int)__instance.tileLocation.X, (int)__instance.tileLocation.Y + 1);
                    location.characters.Add(familiar);
                    __instance.heldObject.Value = null;
                    __instance.ParentSheetIndex = 156;
                    __instance.minutesUntilReady.Value = -1;
                }
            }
        }


        public static void Object_minutesElapsed_Postfix(Object __instance, int minutes, GameLocation environment)
        {
            if (__instance.name.Equals("Slime Incubator") && __instance.heldObject?.Value?.name?.EndsWith("Familiar Egg") == true && __instance.minutesUntilReady <= 0)
            {
                Vector2 v = new Vector2((float)((int)__instance.tileLocation.X), (float)((int)__instance.tileLocation.Y + 1)) * 64f;
                string name = __instance.heldObject.Value.name;

                if (!name.EndsWith("Familiar Egg"))
                    return;

                Familiar familiar = null;

                Monitor.Log($"Hatched a familiar from {__instance.heldObject.Value.name} time {Game1.timeOfDay} owner {__instance.owner}");

                long owner = __instance.owner;
                if (owner == 0)
                    owner = Game1.MasterPlayer.UniqueMultiplayerID;

                switch (__instance.heldObject.Value.name)
                {
                    case "Dino Familiar Egg":
                        familiar = new DinoFamiliar(v, owner);
                        break;
                    case "Dust Sprite Familiar Egg":
                        familiar = new DustSpriteFamiliar(v, owner);
                        break;
                    case "Bat Familiar Egg":
                        familiar = new BatFamiliar(v, owner);
                        break;
                    case "Junimo Familiar Egg":
                        familiar = new JunimoFamiliar(v, owner);
                        break;
                    case "Butterfly Familiar Egg":
                        familiar = new ButterflyFamiliar(v, owner);
                        break;
                }

                if (familiar != null)
                {
                    Game1.showGlobalMessage(string.Format(Helper.Translation.Get("familiar-hatched"), Helper.Translation.Get(familiar.Name)));
                    familiar.setTilePosition((int)__instance.tileLocation.X, (int)__instance.tileLocation.Y + 1);
                    environment.characters.Add(familiar);
                    __instance.heldObject.Value = null;
                    __instance.ParentSheetIndex = 156;
                    __instance.minutesUntilReady.Value = -1;
                }
            }
        }

        public static void GameLocation_drawAboveFrontLayer_Postfix(GameLocation __instance, SpriteBatch b)
        {
            foreach (Character c in __instance.characters)
            {
                if (c is Familiar)
                {
                    (c as Familiar).drawAboveAllLayers(b);
                }
            }
        }

        public static void GameLocation_performTouchAction_Postfix(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            if (Game1.eventUp)
            {
                return;
            }
            try
            {
                string text = fullActionString.Split(' ')[0];
                if (text == "legendarySword" && Game1.player.ActiveObject != null && Utility.IsNormalObjectAtParentSheetIndex(Game1.player.ActiveObject, 107))
                {
                    Game1.player.Halt();
                    Game1.player.faceDirection(2);
                    Game1.player.showCarrying();
                    Game1.player.jitterStrength = 1f;
                    Game1.pauseThenDoFunction(7000, new Game1.afterFadeFunction(FamiliarsUtils.getDinoEgg));
                    Game1.changeMusicTrack("none", false, Game1.MusicContext.Event);
                    __instance.playSound("crit", NetAudio.SoundContext.Default);
                    Game1.screenGlowOnce(new Color(30, 0, 150), true, 0.01f, 0.999f);
                    DelayedAction.playSoundAfterDelay("stardrop", 1500, null, -1);
                    Game1.screenOverlayTempSprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), 500, Color.White, 10, 2000, ""));
                    Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate ()
                    {
                        Game1.stopMusicTrack(Game1.MusicContext.Event);
                    }));
                }
            }
            catch
            {

            }
        }
        public static void Utility_checkForCharacterInteractionAtTile_Postfix(Vector2 tileLocation, Farmer who, ref bool __result)
        {
            if (!(who.currentLocation is SlimeHutch))
                return;

            NPC character = Game1.currentLocation.isCharacterAtTile(tileLocation);
            if (character != null && character is Familiar && (character as Familiar).ownerId.Equals(who))
            {
                Game1.mouseCursor = 4;
                __result = true;
            }
        }
        
        public static bool NPC_isVillager_Prefix(NPC __instance, ref bool __result)
        {
            if(__instance is Familiar)
            {
                __result = false;
                return false;
            }
            return true;
        }
        public static bool NPC_checkAction_Prefix(NPC __instance, ref bool __result)
        {
            if(__instance is Familiar)
            {
                ModEntry.SMonitor.Log("Clicking on familiar");
                __result = false;
                return false;
            }
            return true;
        }
        public static bool Character_checkForFootstep_Prefix(Character __instance)
        {
            if(__instance is Familiar)
            {
                return false;
            }
            return true;
        }
        public static bool Bush_shake_Prefix(Vector2 tileLocation, bool doEvenIfStillShaking, float ___maxShake)
        {
            if (!ModEntry.receivedJunimoEggToday && (___maxShake == 0f || doEvenIfStillShaking) && Game1.player.currentLocation.Name == "Town" && tileLocation.X == 20f && tileLocation.Y == 8f && Game1.player.mailReceived.Contains("junimoPlush") && Game1.dayOfMonth == 28 && Game1.timeOfDay == 1200)
            {
                ModEntry.SMonitor.Log("shaking junimo bush");
                ModEntry.receivedJunimoEggToday = true;
                Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Object(ModEntry.JunimoFamiliarEgg, 1), null);
                return false;
            } 
            return true;
        }

        public static void GameLocation_isCharacterAtTile_Postfix(ref NPC __result)
        {
            if (__result is Familiar)
                __result = null;
        }
        public static void GameLocation_updateCharacters_Prefix(GameLocation __instance)
        {
            foreach (Character c in __instance.characters)
            {
                if (c is Familiar)
                {
                    c.forceUpdateTimer = 1;
                }
            }
        }
        public static void GameLocation_drawCharacters_Prefix(GameLocation __instance, SpriteBatch b)
        {
            if (Game1.eventUp)
            {
                for (int i = 0; i < __instance.characters.Count; i++)
                {
                    if (__instance.characters[i] != null && __instance.characters[i] is Familiar)
                    {
                        __instance.characters[i].draw(b);
                    }
                }
            }
        }
        public static bool AnimalHouse_incubator_Prefix(AnimalHouse __instance)
        {
            if (__instance.incubatingEgg.Y <= 0 && Game1.player.ActiveObject != null && Game1.player.ActiveObject.name.Contains("Familiar Egg"))
            {
                Monitor.Log($"Tried adding familiar egg to incubator");
                return false;
            }
            return true;
        }
        public static void Utility_isThereAFarmerOrCharacterWithinDistance_Postfix(ref Character __result)
        {
            if (__result is Familiar)
                __result = null;
        }

    }
}