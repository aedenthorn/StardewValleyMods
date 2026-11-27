
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sickhead.Engine.Util;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using static StardewValley.Menus.CharacterCustomization;
using static StardewValley.Menus.LoadGameMenu;
using static StardewValley.Minigames.TargetGame;
using Object = StardewValley.Object;

namespace MultiSave
{
    public partial class ModEntry
    {

        private static void SaveBackup()
        {
            if (Constants.CurrentSavePath is null)
                return;
            var path = Path.Combine(Constants.CurrentSavePath, $"{folderPrefix}_{Game1.year}_{Game1.currentSeason}_{Game1.dayOfMonth}");
            TransferFiles(Constants.CurrentSavePath, path, false, true);
        }
        private static void TransferFiles(string sourcePath, string targetPath, bool move, bool backup)
        {
            if(backup && Directory.Exists(targetPath))
            {
                int num = 1;
                while (Directory.Exists(targetPath + "_" + num))
                {
                    num++;
                }
                SMonitor.Log($"Target {targetPath} exists, redirecting to {targetPath + "_" + num}");
                targetPath += "_" + num;
            }
            SMonitor.Log($"Transferring from {sourcePath} to {targetPath}");
            var source = new DirectoryInfo(sourcePath);
            var target = Directory.CreateDirectory(targetPath);
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (Path.GetFileName(dir.Name).StartsWith(folderPrefix))
                    continue;
                if (move)
                {
                    dir.MoveTo(Path.Combine(target.FullName, dir.Name));
                }
                else
                    CopyFolder(dir, target.CreateSubdirectory(dir.Name));
            }
            foreach (FileInfo file in source.GetFiles())
            {
                if (move)
                {
                    file.MoveTo(Path.Combine(target.FullName, file.Name));
                }
                else
                    file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }

        public static void CopyFolder(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFolder(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }
        private static void ReloadSaveBackupList(List<MenuSlot> menuSlots)
        {
            saveBackupList.Clear();
            foreach(var slot in menuSlots)
            {
                saveBackupList.Add((slot as SaveFileSlot)?.Farmer?.slotName is not null ? GetBackups((slot as SaveFileSlot).Farmer.slotName) : new string[0]);
            }
        }
        private static void ReloadDeleteButtons(LoadGameMenu __instance)
        {
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
        }

        public static string[] GetBackups(string slotName)
        {
            var folder = Path.Combine(Constants.SavesPath, slotName);
            var list = Directory.GetDirectories(folder);
            return list.Where(d => Path.GetFileName(d).StartsWith(folderPrefix + "_")).ToArray();
        }

        private static string GetFolderName(Farmer farmer)
        {
            return $"{folderPrefix}_{farmer.yearForSaveGame}_{(seasons.Length > farmer.seasonForSaveGame.Value && farmer.seasonForSaveGame.Value > -1 ? seasons[farmer.seasonForSaveGame.Value] : farmer.seasonForSaveGame.Value)}_{farmer.dayOfMonthForSaveGame}";
        }

        private static List<Farmer> GetSaveSlots(string slotName, string[] backups)
        {
            List<Farmer> results = new();
            foreach (var backupFolderPath in backups)
            {
                string saveName = slotName;
                string pathToFile = Path.Combine(backupFolderPath, "SaveGameInfo");
                var saveFile = Path.Combine(backupFolderPath, saveName);
                if (File.Exists(saveFile))
                {
                    Farmer f = null;
                    try
                    {
                        using (FileStream stream = File.OpenRead(pathToFile))
                        {
                            f = (Farmer)SaveGame.farmerSerializer.Deserialize(stream);
                            SaveGame.loadDataToFarmer(f);
                            f.slotName = saveName;
                            f.modData[backupFolderKey] = backupFolderPath;
                            results.Add(f);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception occured trying to access file '{0}'", pathToFile);
                        Console.WriteLine(ex.GetBaseException().ToString());
                        if (f != null)
                        {
                            f.unload();
                        }
                    }
                }
            }
            results.Sort();
            return results;
        }
    }
}