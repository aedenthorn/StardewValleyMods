using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace StardewImpact
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || Game1.player.CurrentTool is not MeleeWeapon)
                return;
            if (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
            {
                List<Rectangle> rects = GetCharacterRectangles();
                for (int i = 0; i < rects.Count; i++)
                {
                    if (rects[i].Contains(Game1.getMousePosition()))
                    {
                        PressedButton(i + 1, e.Button == SButton.MouseLeft);
                        Helper.Input.Suppress(e.Button);
                        return;
                    }
                }
            }
            else if (e.Button == Config.Button1 && Game1.player.modData.ContainsKey(slotPrefix + 1) && GetAvailableCharacters().Count > 0)
            {
                Game1.currentLocation.playSound("bigSelect");
                Game1.player.modData[currentSlotKey] = "1";
                Helper.Input.Suppress(e.Button);
            }
            else if (e.Button == Config.Button2 && Game1.player.modData.ContainsKey(slotPrefix + 2) && GetAvailableCharacters().Count > 1)
            {
                Game1.currentLocation.playSound("bigSelect");
                Game1.player.modData[currentSlotKey] = "2";
                Helper.Input.Suppress(e.Button);
            }
            else if (e.Button == Config.Button3 && Game1.player.modData.ContainsKey(slotPrefix + 3) && GetAvailableCharacters().Count > 2)
            {
                Game1.currentLocation.playSound("bigSelect");
                Game1.player.modData[currentSlotKey] = "3";
                Helper.Input.Suppress(e.Button);
            }
            else if (e.Button == Config.Button4 && Game1.player.modData.ContainsKey(slotPrefix + 4) && GetAvailableCharacters().Count > 3)
            {
                Game1.currentLocation.playSound("bigSelect");
                Game1.player.modData[currentSlotKey] = "4";
                Helper.Input.Suppress(e.Button);
            }
            else if (e.Button == Config.Button5 && Game1.player.modData.ContainsKey(currentSlotKey))
            {
                Game1.currentLocation.playSound("bigDeSelect");
                Game1.player.modData.Remove(currentSlotKey);
                Helper.Input.Suppress(e.Button);
            }
            else if (e.Button == Config.SkillButton && Game1.player.modData.ContainsKey(currentSlotKey))
            {
                Helper.Input.Suppress(e.Button);
                CharacterData data = GetCurrentCharacter();
                if (data.skillCooldownValue > 0)
                {
                    Monitor.Log($"Skill on cooldown {data.skillCooldownValue}");
                    return;
                }
                Monitor.Log($"invoking {data.skillEvent.Count} skill events");
                characterDict[data.name].currentEnergy = Math.Min(characterDict[data.name].burstEnergyCost, characterDict[data.name].currentEnergy + characterDict[data.name].energyPerSkill);
                characterDict[data.name].skillCooldownValue = characterDict[data.name].skillCooldown;
                foreach (var a in data.skillEvent)
                {
                    a.Invoke(data.name, Game1.player);
                }
            }
            else if (e.Button == Config.BurstButton && Game1.player.modData.ContainsKey(currentSlotKey))
            {
                Helper.Input.Suppress(e.Button);
                CharacterData data = GetCurrentCharacter();
                if (data.burstCooldownValue > 0)
                {
                    Monitor.Log($"Burst on cooldown {data.burstCooldownValue}");
                    return;
                }
                else if(data.currentEnergy < data.burstEnergyCost)
                {
                    Monitor.Log($"Not enough energy for burst");
                    return;
                }
                Monitor.Log($"invoking {data.burstEvent.Count} burst events");
                characterDict[data.name].currentEnergy = 0;
                characterDict[data.name].burstCooldownValue = characterDict[data.name].burstCooldown;
                foreach (var a in data.burstEvent)
                {
                    a.Invoke(data.name, Game1.player);
                }
            }
        }

    }
}