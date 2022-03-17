using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CustomSigns
{
    public partial class ModEntry : Mod
    {
        public static void OpenPlacementDialogue()
        {
            if (customSignDataDict.Count == 0)
            {
                SMonitor.Log("No custom sign templates.", LogLevel.Warn);
                return;
            }

            List<Response> responses = new List<Response>();
            foreach(var key in customSignDataDict.Keys)
            {
                responses.Add(new Response(key, key));
            }
            responses.Add(new Response("cancel", SHelper.Translation.Get("cancel")));
            Game1.player.currentLocation.createQuestionDialogue(SHelper.Translation.Get("which-template"), responses.ToArray(), "CS_Choose_Template");
        }

        private static void ReloadSignData()
        {
            customSignDataDict.Clear();
            customSignTypeDict.Clear();
            fontDict.Clear();
            var dict = SHelper.Content.Load<Dictionary<string, CustomSignData>>(dictPath, ContentSource.GameContent);
            if (dict == null)
            {
                SMonitor.Log($"No custom signs found", LogLevel.Debug);
                return;
            }
            foreach (var kvp in dict)
            {
                CustomSignData data = kvp.Value;
                foreach (string type in data.types)
                {
                    if (!customSignTypeDict.ContainsKey(type))
                    {
                        customSignTypeDict.Add(type, new List<string>() { type });
                    }
                    else
                    {
                        customSignTypeDict[type].Add(type);
                    }
                }
                if (data.packID != null && !loadedContentPacks.Contains(data.packID))
                    loadedContentPacks.Add(data.packID);
                data.texture = SHelper.Content.Load<Texture2D>(data.texturePath, ContentSource.GameContent);
                foreach(var text in data.text)
                {
                    if (!fontDict.ContainsKey(text.fontPath))
                        fontDict.Add(text.fontPath, Game1.content.Load<SpriteFont>(text.fontPath));
                }
            }
            customSignDataDict = dict;
        }
    }
}