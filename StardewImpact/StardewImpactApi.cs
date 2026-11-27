using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewImpact
{
    public interface IStardewImpactApi
    {
        public bool IsCharacterAvailable(string name);
        public string[] GetCharacterNames(bool available);
        public ICharacterData GetCharacter(string name, bool available);
        public bool GetCharacterBool(string name, out ICharacterData character, bool available);
        public void AddCharacter(string name, Color characterColor, float energyPerSkill, float energyPerHit, float burstEnergyCost, float skillCooldoown, float burstCooldoown, string portraitPath, string spritePath, string skillIconPath, string burstIconPath, List<Action<string, Farmer>> skillEvent, List<Action<string, Farmer>> burstEvent);
        public void AddSkillEvent(string name, Action<string, Farmer> action);
        public void AddBurstEvent(string name, Action<string, Farmer> action);
    }
    public class StardewImpactApi : IStardewImpactApi
    {
        public bool IsCharacterAvailable(string name)
        {
            return ModEntry.GetAvailableCharacters().ContainsKey(name);
        }
        public string[] GetCharacterNames(bool available)
        {
            return (available ? ModEntry.GetAvailableCharacters() : ModEntry.characterDict).Keys.ToArray();
        }
        public ICharacterData GetCharacter(string name, bool available = false)
        {
            if(!(available ? ModEntry.GetAvailableCharacters() : ModEntry.characterDict).TryGetValue(name, out CharacterData data))
            {
                if (ModEntry.SMonitor is not null)
                    ModEntry.SMonitor.Log($"Error getting character {name}: character data not found", StardewModdingAPI.LogLevel.Error);
                return null;
            }
            return data;
        }
        public bool GetCharacterBool(string name, out ICharacterData character, bool available = false)
        {
            bool result = (available ? ModEntry.GetAvailableCharacters() : ModEntry.characterDict).TryGetValue(name, out CharacterData data);
            character = data;
            return result;
        }
        public void AddCharacter(string name, Color characterColor, float energyPerSkill, float energyPerHit, float burstEnergyCost, float skillCooldoown, float burstCooldoown, string portraitPath, string spritePath, string skillIconPath, string burstIconPath, List<Action<string, Farmer>> skillEvent, List<Action<string, Farmer>> burstEvent)
        {
            ModEntry.characterDict[name] = new CharacterData(name, characterColor, energyPerSkill, energyPerHit, burstEnergyCost, skillCooldoown, burstCooldoown, portraitPath, spritePath, skillIconPath, burstIconPath, skillEvent, burstEvent);
            ModEntry.LoadTextures(name);
            if(ModEntry.SMonitor is not null)
                ModEntry.SMonitor.Log($"Added character {name}", StardewModdingAPI.LogLevel.Debug);
        }
        public void AddSkillEvent(string name, Action<string, Farmer> action)
        {
            if (!ModEntry.characterDict.TryGetValue(name, out CharacterData data))
            {
                if (ModEntry.SMonitor is not null)
                    ModEntry.SMonitor.Log($"Error adding skill event for {name}: character data not found", StardewModdingAPI.LogLevel.Error);
                return;
            }
            data.SkillEvent.Add(action);
        }
        public void AddBurstEvent(string name, Action<string, Farmer> action)
        {
            if (!ModEntry.characterDict.TryGetValue(name, out CharacterData data))
            {
                if (ModEntry.SMonitor is not null)
                    ModEntry.SMonitor.Log($"Error adding burst event for {name}: character data not found", StardewModdingAPI.LogLevel.Error);
                return;
            }
            data.BurstEvent.Add(action);
        }
    }
}