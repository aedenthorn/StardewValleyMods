using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Web;

namespace LogUploader
{
    public partial class ModEntry
    {
        public static void OnGameUpdating_Postfix(object __instance)
        {
            if (!Config.ModEnabled)
                return;
            try
            {
                if (sending)
                {
                    SMonitor.Log("sending log");
                    sending = false;
                    object lm = AccessTools.Field(__instance.GetType(), "LogManager").GetValue(__instance);
                    SendLog(lm);
                }
            }
            catch(Exception ex)
            {
                SMonitor.Log(ex.ToString(), StardewModdingAPI.LogLevel.Error);
            }
        }

        public static void LogFatalLaunchError_Postfix(object __instance)
        {
            if (!Config.ModEnabled || !Config.SendOnFatalError)
                return;
            SMonitor.Log("sending log");
            sending = false;
            SendLog(__instance);
        }
    }
}