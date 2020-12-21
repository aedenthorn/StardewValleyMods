using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using System;
using System.IO;
using System.Reflection;
using xTile;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class FileIO
    {

        public static IMonitor Monitor;
        public static IModHelper Helper;
        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }

        public static void LoadKissAudio()
        {
            // kiss audio

            string filePath = $"{Helper.DirectoryPath}/assets/kiss.wav";
            Monitor.Log("Kissing audio path: " + filePath);
            if (File.Exists(filePath))
            {
                try
                {
                    Kissing.kissEffect = SoundEffect.FromStream(new FileStream(filePath, FileMode.Open));
                }
                catch(Exception ex)
                {
                    Monitor.Log("Error loading kissing audio: " + ex, LogLevel.Error);
                }
            }
            else
            {
                Monitor.Log("Kissing audio not found at path: " + filePath);
            }
        }

        public static void LoadTMXSpouseRooms()
        {
            try
            {
                Maps.tmxSpouseRooms.Clear();
                var tmxlAPI = Helper.ModRegistry.GetApi("Platonymous.TMXLoader");
                var tmxlAssembly = tmxlAPI?.GetType()?.Assembly;
                var tmxlModType = tmxlAssembly?.GetType("TMXLoader.TMXLoaderMod");
                var tmxlEditorType = tmxlAssembly?.GetType("TMXLoader.TMXAssetEditor");

                if (tmxlModType == null)
                    return;

                var tmxlHelper = Helper.Reflection.GetField<IModHelper>(tmxlModType, "helper").GetValue();
                foreach (var editor in tmxlHelper.Content.AssetEditors)
                {
                    try
                    {
                        if (editor == null)
                            continue;
                        if (!ReferenceEquals(editor.GetType(),tmxlEditorType)) continue;

                        if (editor.GetType().GetField("type", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editor).ToString() != "SpouseRoom") continue;

                        string name = (string)tmxlEditorType.GetField("assetName").GetValue(editor);
                        if (name != "FarmHouse1_marriage") continue;

                        object edit = tmxlEditorType.GetField("edit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editor);
                        string info = (string)edit.GetType().GetProperty("info").GetValue(edit);

                        Map map = Helper.Reflection.GetField<Map>(editor, "newMap").GetValue();
                        if (map != null && !Maps.tmxSpouseRooms.ContainsKey(info))
                        {
                            Monitor.Log("Adding TMX spouse room for " + info, LogLevel.Debug);
                            Maps.tmxSpouseRooms.Add(info, map);
                        }
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log($"Failed getting TMX spouse room data. Exception: {ex}", LogLevel.Debug);
                    }
                }

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed getting TMX spouse room data. Exception: {ex}", LogLevel.Debug);
            }
        }
   }
}