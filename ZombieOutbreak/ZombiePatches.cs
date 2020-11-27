using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.CodeDom;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace ZombieOutbreak
{
    internal class ZombiePatches
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }
        public static void NPC_draw_prefix(NPC __instance)
        {
            if (ModEntry.zombieTextures.ContainsKey(__instance.name) && ModEntry.zombieTextures[__instance.name] != null)
            {
                helper.Reflection.GetField<Texture2D>(__instance.sprite.Value, "spriteTexture").SetValue(ModEntry.zombieTextures[__instance.Name]);
            }
        }
        public static void Farmer_draw_prefix(Farmer __instance)
        {
            if (ModEntry.playerZombies.ContainsKey(__instance.UniqueMultiplayerID) && ModEntry.playerZombies[__instance.uniqueMultiplayerID] != null)
            {
                Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, ModEntry.playerZombies[__instance.uniqueMultiplayerID].Width, ModEntry.playerZombies[__instance.uniqueMultiplayerID].Height);
                Color[] data = new Color[ModEntry.playerZombies[__instance.uniqueMultiplayerID].Width * ModEntry.playerZombies[__instance.uniqueMultiplayerID].Height];
                ModEntry.playerZombies[__instance.uniqueMultiplayerID].GetData(data);
                texture.SetData(data);
                helper.Reflection.GetField<Texture2D>(__instance.FarmerRenderer, "baseTexture").SetValue(texture);
            }
        }

        public static void DialogueBox_drawPortrait_prefix(DialogueBox __instance, Dialogue ___characterDialogue)
        {
            if (ModEntry.zombieTextures.ContainsKey(___characterDialogue.speaker.Name))
            {
                ___characterDialogue.speaker.Portrait = ModEntry.zombiePortraits[___characterDialogue.speaker.Name];
            }
        }
        public static void DialogueBox_complex_prefix(ref List<Response> responses)
        {
            if (ModEntry.playerZombies.ContainsKey(Game1.player.uniqueMultiplayerID))
            {
                for(int i = 0; i < responses.Count; i++)
                {
                    Utils.MakeZombieSpeak(ref responses[i].responseText, true);
                }
            }
        }

        internal static void Farmer_eatObject_prefix(Farmer __instance, Object o)
        {
            if(o.Name == "Zombie Cure")
            {
                ModEntry.curedFarmers.Add(__instance.uniqueMultiplayerID);
                if (ModEntry.playerZombies.ContainsKey(__instance.uniqueMultiplayerID))
                {
                    monitor.Log($"zombie farmer {__instance.Name} ate zombie cure");
                    Utils.RemoveZombiePlayer(__instance.uniqueMultiplayerID);
                }
            }
        }

        public static void Dialogue_prefix(Dialogue __instance, ref string masterString)
        {
            if (ModEntry.zombieTextures.ContainsKey(__instance.speaker.Name))
            {
                Utils.MakeZombieSpeak(ref masterString);
            }
        }

        public static void NPC_showTextAboveHead_Prefix(NPC __instance, ref string Text)
        {
            try
            {
                if (ModEntry.zombieTextures.ContainsKey(__instance.Name))
                {
                    Utils.MakeZombieSpeak(ref Text, true);
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed in {nameof(NPC_showTextAboveHead_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void NPC_getHi_Postfix(NPC __instance, ref string __result)
        {
            try
            {
                if (ModEntry.zombieTextures.ContainsKey(__instance.Name))
                {
                    Utils.MakeZombieSpeak(ref __result, true);
                }
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed in {nameof(NPC_getHi_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void setUpShopOwner_postfix(ShopMenu __instance, string who)
        {
            if(who != null && ModEntry.zombieTextures.ContainsKey(who))
            {
                Utils.MakeZombieSpeak(ref __instance.potraitPersonDialogue);
            }
        }

        internal static bool NPC_tryToReceiveActiveObject_prefix(NPC __instance, Farmer who)
        {
            if (who.ActiveObject.Name == "Zombie Cure" && ModEntry.zombieTextures.ContainsKey(__instance.Name))
            {
                monitor.Log($"Gave Zombie Cure to {__instance.Name}");
                who.currentLocation.playSound("slimedead", NetAudio.SoundContext.Default);
                Utils.RemoveZombie(__instance.Name);
                __instance.CurrentDialogue.Clear();
                __instance.CurrentDialogue.Push(new Dialogue(helper.Translation.Get("cured"), __instance));
                return false;
            }
            return true;
        }
    }
}