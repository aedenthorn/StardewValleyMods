using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

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

            foreach (var pack in loadedContentPacks)
            {
                var cm = AccessTools.Field(SHelper.ConsoleCommands.GetType(), "CommandManager").GetValue(SHelper.ConsoleCommands);
                var cmd = AccessTools.Method(cm.GetType(), "Get").Invoke(cm, new object[]{ "patch" });
                Action<string, string[]> action = (Action<string, string[]>)AccessTools.Property(cmd.GetType(), "Callback").GetValue(cmd);
                action.Invoke("patch", new string[] { "reload", pack });
            }
            SHelper.GameContent.InvalidateCache(dictPath);
            var dict = SHelper.GameContent.Load<Dictionary<string, CustomSignData>>(dictPath);
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
                data.texture = SHelper.GameContent.Load<Texture2D>(data.texturePath);
                loadedContentPacks.Add(data.packID);
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