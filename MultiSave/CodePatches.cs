using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using xTile.Dimensions;
using static StardewValley.Menus.LoadGameMenu;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MultiSave
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(LoadGameMenu), new Type[] { })]
        [HarmonyPatch(MethodType.Constructor)]
        public class LoadGameMenu_Patch
        {
            public static void Postfix(LoadGameMenu __instance)
            {
                if (!Config.EnableMod || !(bool)AccessTools.Method(__instance.GetType(), "hasDeleteButtons").Invoke(__instance, new object[] { }))
                    return;

                currentSaveSlot = null;
                currentSaveBackupList.Clear();
                saveBackupList.Clear();

                for (int i = 0; i < 4; i++)
                {
                    __instance.deleteButtons[i].downNeighborID = i + 101;
                    __instance.deleteButtons.Add(new ClickableTextureComponent("", new Rectangle(__instance.xPositionOnScreen + __instance.width - 68 - 4, __instance.yPositionOnScreen + 32 + 4 + (i + 1) * (__instance.height / 4) - 80, 48, 48), "", "", Game1.mouseCursors, new Rectangle(240, 320, 16, 16), 3f, false)
                    {
                        myID = i + 101,
                        region = 901,
                        leftNeighborID = -99998,
                        downNeighborImmutable = true,
                        upNeighborID = i + 100,
                        upNeighborImmutable = true,
                        downNeighborID = ((i < 3) ? -99998 : -1),
                        rightNeighborID = -99998
                    });
                }
                __instance.populateClickableComponentList();
            }
        }
        
        [HarmonyPatch(typeof(SaveFileSlot), nameof(SaveFileSlot.Draw))]
        public class SaveFileSlot_Draw_Patch
        {
            public static void Postfix(SpriteBatch b, int i, SaveFileSlot __instance, LoadGameMenu ___menu)
            {
                if (!Config.EnableMod)
                    return;

                if(___menu is CoopMenu || ___menu is FarmhandMenu)
                {
                    saveBackupList.Clear();
                    currentSaveBackupList.Clear();
                    currentSaveSlot = null;
                    return;
                }

                if (currentSaveSlot is null)
                {
                    var currentItemIndex = (int)AccessTools.Field(typeof(LoadGameMenu), "currentItemIndex").GetValue(___menu);
                    if (saveBackupList.Count > currentItemIndex + i && saveBackupList[currentItemIndex + i].Length > 0 && ___menu.deleteButtons.Count > i + 4)
                        ___menu.deleteButtons[i + 4].draw(b, Color.White * 0.75f, 1f, 0);
                }
                else if(__instance.Farmer?.modData.TryGetValue(backupFolderKey, out string backupFolder) == true)
                {
                    var info = new DirectoryInfo(backupFolder);
                    var date = info.CreationTime;
                    var dateString = date.ToShortDateString() + " " + date.ToShortTimeString();
                    var fontMeasure = Game1.smallFont.MeasureString(dateString);
                    b.DrawString(Game1.smallFont, dateString, new Vector2(___menu.slotButtons[i].bounds.Center.X - fontMeasure.X / 2, ___menu.slotButtons[i].bounds.Y + 16), Color.Black);
                }
            }
        }
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.backButtonPressed))]
        public class TitleMenu_backButtonPressed_Patch
        {
            public static bool Prefix(TitleMenu __instance)
            {
                if (!Config.EnableMod || currentSaveSlot is null || TitleMenu.subMenu is not LoadGameMenu)
                    return true;
                currentSaveSlot = null;
                currentSaveBackupList.Clear();
                saveBackupList.Clear();
                AccessTools.FieldRefAccess<LoadGameMenu, List<MenuSlot>>(TitleMenu.subMenu as LoadGameMenu, "menuSlots").Clear();
                AccessTools.FieldRefAccess<LoadGameMenu, int>(TitleMenu.subMenu as LoadGameMenu, "currentItemIndex") = 0;
                AccessTools.Method(typeof(LoadGameMenu), "startListPopulation").Invoke(TitleMenu.subMenu, new object[] { });
                (TitleMenu.subMenu as LoadGameMenu).UpdateButtons();
                if (Game1.options.snappyMenus && Game1.options.gamepadControls)
                {
                    TitleMenu.subMenu.populateClickableComponentList();
                    TitleMenu.subMenu.snapToDefaultClickableComponent();
                }
                Game1.playSound("bigDeSelect");
                return false;
            }
        }
        [HarmonyPatch(typeof(LoadGameMenu), "update")]
        public class LoadGameMenu_update_Patch
        {
            public static void Postfix(LoadGameMenu __instance, List<MenuSlot> ___menuSlots)
            {
                if (!Config.EnableMod || ___menuSlots is null || __instance.GetType() != typeof(LoadGameMenu))
                    return;
                if(currentSaveSlot is null &&  saveBackupList.Count != ___menuSlots.Count)
                {
                    ReloadSaveBackupList(___menuSlots);
                }
                if(__instance.deleteButtons.Count != 8)
                {
                    ReloadDeleteButtons(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(LoadGameMenu), "deleteFile")]
        public class LoadGameMenu_deleteFile_Patch
        {
            public static bool Prefix(LoadGameMenu __instance, int which, List<MenuSlot> ___menuSlots)
            {
                if (!Config.EnableMod || currentSaveSlot is null)
                    return true;
                SaveFileSlot slot = ___menuSlots[which] as SaveFileSlot;
                if (slot == null)
                {
                    return false;
                }
                string fullFilePath = slot.Farmer.modData[backupFolderKey];
                if (Directory.Exists(fullFilePath))
                {
                    Directory.Delete(fullFilePath, true);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(LoadGameMenu), nameof(LoadGameMenu.receiveLeftClick))]
        public class LoadGameMenu_receiveLeftClick_Patch
        {
            public static bool Prefix(LoadGameMenu __instance, int x, int y, List<MenuSlot> ___menuSlots, ref int ___selected, ref int ___selectedForDelete, ref int ___currentItemIndex, ref int ___timerToLoad, ref bool ___loading, ref bool ___deleting)
            {
                if (!Config.EnableMod || ___timerToLoad > 0 || ___loading || ___deleting || __instance.deleteConfirmationScreen)
                    return true;
                if(currentSaveSlot is not null)
                {
                    if (___selected == -1)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (__instance.deleteButtons[i].containsPoint(x, y) && i < ___menuSlots.Count && !__instance.deleteConfirmationScreen)
                            {
                                __instance.deleteConfirmationScreen = true;
                                Game1.playSound("drumkit6");
                                ___selectedForDelete = ___currentItemIndex + i;
                                if (Game1.options.snappyMenus && Game1.options.gamepadControls)
                                {
                                    __instance.currentlySnappedComponent = __instance.getComponentWithID(803);
                                    __instance.snapCursorToCurrentSnappedComponent();
                                }
                                return false;
                            }
                        }
                    }
                    if (!__instance.deleteConfirmationScreen)
                    {
                        for (int j = 0; j < __instance.slotButtons.Count; j++)
                        {
                            if (__instance.slotButtons[j].containsPoint(x, y) && j < ___menuSlots.Count)
                            {
                                SaveFileSlot menu_save_slot = ___menuSlots[___currentItemIndex + j] as LoadGameMenu.SaveFileSlot;
                                if (menu_save_slot != null && menu_save_slot.versionComparison < 0)
                                {
                                    menu_save_slot.redTimer = Game1.currentGameTime.TotalGameTime.TotalSeconds + 1.0;
                                    Game1.playSound("cancel");
                                }
                                else
                                {
                                    Game1.playSound("select");
                                    ___timerToLoad = ___menuSlots[___currentItemIndex + j].ActivateDelay;
                                    if (___timerToLoad > 0)
                                    {
                                        ___loading = true;
                                        ___selected = ___currentItemIndex + j;
                                        return false;
                                    }
                                    ___menuSlots[___currentItemIndex + j].Activate();
                                    return false;
                                }
                            }
                        }
                    }
                    ___currentItemIndex = Math.Max(0, Math.Min(___menuSlots.Count - 4, ___currentItemIndex));
                    return false;
                }
                for (int i = 0; i < 4; i++)
                {
                    if(__instance.deleteButtons.Count > i + 4 && __instance.deleteButtons[i + 4].containsPoint(x, y))
                    {
                        var backups = GetBackups(((SaveFileSlot)___menuSlots[___currentItemIndex + i]).Farmer.slotName);
                        if (!backups.Any())
                            return false;
                        Game1.playSound("bigSelect");
                        currentSaveSlot = ((SaveFileSlot)___menuSlots[___currentItemIndex + i]);
                        ___menuSlots.Clear();
                        var files = GetSaveSlots(currentSaveSlot.Farmer.slotName, backups);
                        ___menuSlots.AddRange(from file in files select new SaveFileSlot(__instance, file));
                        
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(LoadGameMenu), nameof(LoadGameMenu.performHoverAction))]
        public class LoadGameMenu_performHoverAction_Patch
        {
            public static bool Prefix(LoadGameMenu __instance, int x, int y, List<MenuSlot> ___menuSlots, ref int ___selected, ref int ___selectedForDelete, ref int ___currentItemIndex, ref int ___timerToLoad, ref bool ___loading, ref bool ___deleting, ref string ___hoverText)
            {
                if (!Config.EnableMod || __instance.deleteConfirmationScreen)
                    return true;

                for (int i = 0; i < 4; i++)
                {
                    if (__instance.deleteButtons.Count > i + 4 && __instance.deleteButtons[i + 4].containsPoint(x, y))
                    {
                        if (currentSaveSlot is not null || saveBackupList.Count <= ___currentItemIndex + i || saveBackupList[___currentItemIndex + i].Length <= 0)
                        {
                            ___hoverText = "";
                            return false;
                        }
                        __instance.deleteButtons[i + 4].tryHover(x, y, 0.2f);
                        if (__instance.deleteButtons[i + 4].containsPoint(x, y))
                        {
                            ___hoverText = string.Format(saveBackupList[___currentItemIndex + i].Length == 1 ? SHelper.Translation.Get("1-save") : SHelper.Translation.Get("x-saves"), saveBackupList[___currentItemIndex + i].Length);
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(SaveFileSlot), nameof(SaveFileSlot.Activate))]
        public class SaveFileSlot_Activate_Patch
        {
            public static bool Prefix(SaveFileSlot __instance)
            {
                if (!Config.EnableMod || currentSaveSlot is null)
                    return true;
                var newFolder = Path.Combine(Constants.SavesPath, currentSaveSlot.Farmer.slotName, GetFolderName(currentSaveSlot.Farmer));
                var backupFolderPath = __instance.Farmer.modData[backupFolderKey];
                var currentFolderPath = Path.Combine(Constants.SavesPath, currentSaveSlot.Farmer.slotName);
                var tempDir = Path.Combine(Constants.SavesPath, currentSaveSlot.Farmer.slotName + "_tmp");

                if (!Directory.Exists(backupFolderPath))
                {
                    SMonitor.Log($"Backup folder path {backupFolderPath} not found. Something is really, really, really wrong.", LogLevel.Error);
                    return false;
                }
                if (Directory.Exists(tempDir))
                {
                    SMonitor.Log($"Backup temp folder {tempDir} still exists; you must restore it or delete it.", LogLevel.Error);
                    return false;
                }

                CopyFolder(new DirectoryInfo(currentFolderPath), Directory.CreateDirectory(tempDir));
                TransferFiles(currentFolderPath, newFolder, true, true);
                TransferFiles(backupFolderPath, currentFolderPath, true, false);
                Directory.Delete(backupFolderPath, true);
                Directory.Delete(tempDir, true);
                return true;
            }
        }
    }
}