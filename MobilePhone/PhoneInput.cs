using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MobilePhone
{
    public class PhoneInput 
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }

        public static void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.activeClickableMenu == null && ModEntry.phoneOpen && e.Button == SButton.Escape)
            {
                PhoneUtils.TogglePhone();
                Helper.Input.Suppress(SButton.Escape);
                return;
            }

            if (e.Button == Config.OpenPhoneKey)
            {
                PhoneUtils.TogglePhone();
                return;
            }
            if(e.Button == Config.RotatePhoneKey)
            {
                PhoneUtils.RotatePhone();
                return;
            }

            if (e.Button == SButton.MouseLeft && ModEntry.inCall && Game1.activeClickableMenu?.GetType() == typeof(DialogueBox) && Game1.player.currentLocation?.lastQuestionKey?.StartsWith("PhoneApp_InCall_") == true)
            {
                IClickableMenu menu = Game1.activeClickableMenu;
                int resp = (int)typeof(DialogueBox).GetField("selectedResponse", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);
                List<Response> resps = (List<Response>)typeof(DialogueBox).GetField("responses", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);

                if (resp >= 0 && resps != null && resp < resps.Count && resps[resp] != null)
                {
                    Helper.Input.Suppress(SButton.MouseLeft);
                    Game1.player.currentLocation.lastQuestionKey = "";
                    Monitor.Log($"clicked on response {resps[resp].responseKey} calling npc: {ModEntry.callingNPC} inCall {ModEntry.inCall}");
                    MobilePhoneCall.CallDialogueAnswer(resps[resp].responseKey, ModEntry.callingNPC);

                    return;
                }
            }

            Point mousePos = Game1.getMousePosition();

            if (!ModEntry.phoneOpen)
            {
                if (e.Button == SButton.MouseLeft && Game1.displayHUD && Config.ShowPhoneIcon && new Rectangle((int)ModEntry.phoneIconPosition.X, (int)ModEntry.phoneIconPosition.Y, Config.PhoneIconWidth, Config.PhoneIconHeight).Contains(mousePos))
                {
                    Helper.Input.Suppress(SButton.MouseLeft);
                    ModEntry.clickingPhoneIcon = true;
                    ModEntry.draggingPhoneIcon = false;
                    ModEntry.lastMousePosition = mousePos;
                }
                return;
            }

            if (e.Button == SButton.MouseLeft)
            {
                if (Game1.activeClickableMenu == null && !ModEntry.inCall && !ModEntry.appRunning && !ModEntry.phoneRect.Contains(mousePos))
                {
                    Monitor.Log($"pressing mouse outside of opened phone");
                    Helper.Input.Suppress(SButton.MouseLeft);
                    PhoneUtils.TogglePhone();
                    return;
                }

                if (ModEntry.phoneRect.Contains(mousePos) && !ModEntry.screenRect.Contains(mousePos))
                {
                    Monitor.Log($"pressing mouse key on phone border");
                    Helper.Input.Suppress(SButton.MouseLeft);
                    ModEntry.clicking = true;
                    ModEntry.draggingPhone = true;
                    ModEntry.lastMousePosition = mousePos;
                    return;
                }


                if(ModEntry.callingNPC != null && ModEntry.screenRect.Contains(mousePos))
                {
                    Monitor.Log($"pressing mouse key in phone while calling");
                    Helper.Input.Suppress(SButton.MouseLeft);

                    ModEntry.clicking = true;
                    return;
                }

                if (!ModEntry.appRunning && ModEntry.screenRect.Contains(mousePos))
                {
                    Monitor.Log($"pressing mouse key in phone");
                    Helper.Input.Suppress(SButton.MouseLeft);

                    ModEntry.clicking = true;
                    ModEntry.lastMousePosition = mousePos;
                    for (int i = 0; i < ModEntry.appOrder.Count; i++)
                    {
                        Vector2 pos = PhoneUtils.GetAppPos(i);
                        Rectangle r = new Rectangle((int)pos.X, (int)pos.Y, Config.IconWidth, Config.IconHeight);
                        if (r.Contains(mousePos))
                        {
                            ModEntry.clickingApp = i;
                            ModEntry.clickingTicks = 0;
                            break;
                        }
                    }
                }
            }

        }

        public static void PressKey(MobileApp app)
        {
            if(app.closePhone)
                PhoneUtils.TogglePhone(false);

            if (!Enum.TryParse(app.keyPress, out SButton keyPress))
            {
                Monitor.Log($"Error on app invoke: {app.keyPress} isn't a valid key", LogLevel.Error);
                return;
            }

            // get SMAPI's input handler
            object input = typeof(Game1).GetField("input", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null)
               ?? throw new InvalidOperationException("Can't find 'Game1.input' field.");

            // get OverrideButton method
            var method = input.GetType().GetMethod("OverrideButton")
               ?? throw new InvalidOperationException("Can't find 'OverrideButton' method on SMAPI's input class.");

            // call method
            // The arguments are the button to override, and whether to mark the button pressed (true) or raised (false)
            method.Invoke(input, new object[] { keyPress, true });
            return;
        }

    }
}
