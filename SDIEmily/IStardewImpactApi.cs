using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDIEmily
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
}