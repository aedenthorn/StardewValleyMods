using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Quests;
using StardewValley.SDKs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using static StardewValley.LocationRequest;
using Object = StardewValley.Object;

namespace BuffFramework
{
    public partial class ModEntry
    {
        private static float CheckGlowRate(Buff buff, float rate)
        {
            if (!Config.ModEnabled)
                return rate;
            foreach(var b in buffDict.Values)
            {
                if(b.buffId == buff.which) 
                    return b.glowRate;
            }
            return rate;
        }
        private static void UpdateBuffs()
        {
            buffDict = SHelper.GameContent.Load<Dictionary<string, BuffData>>(dictKey);
            var newBuffList = new List<Buff>();
            foreach(var b in buffDict.Values)
            {
                Buff buff;
                if(b.which > -1)
                {
                    buff = new Buff(b.which);

                }
                else
                {
                    buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, minutesDuration: 1, source: b.source, b.displaySource) { which = b.buffId };
                }

                if (b.farming > -1) { buff.buffAttributes[Buff.farming] = b.farming; }
                if (b.fishing > -1) { buff.buffAttributes[Buff.fishing] = b.fishing; }
                if (b.mining > -1) { buff.buffAttributes[Buff.mining] = b.mining; }
                if (b.digging > -1) { buff.buffAttributes[3] = b.digging; }
                if (b.luck > -1) { buff.buffAttributes[Buff.luck] = b.luck; }
                if (b.foraging > -1) { buff.buffAttributes[Buff.foraging] = b.foraging; }
                if (b.crafting > -1) { buff.buffAttributes[Buff.crafting] = b.crafting; }
                if (b.maxStamina > -1) { buff.buffAttributes[Buff.maxStamina] = b.maxStamina; }
                if (b.magneticRadius > -1) { buff.buffAttributes[Buff.magneticRadius] = b.magneticRadius; }
                if (b.speed > -1) { buff.buffAttributes[Buff.speed] = b.speed; }
                if (b.defense > -1) { buff.buffAttributes[Buff.defense] = b.defense; }
                if (b.attack > -1) { buff.buffAttributes[Buff.attack] = b.attack; }
                
                if (b.sheetIndex > -1) { buff.sheetIndex = b.sheetIndex; }
                
                if(b.glow != null)
                {
                    buff.glow = b.glow.Value;
                }
                if (b.sound != null && !farmerBuffs.Value.Exists(b => b.which == buff.which))
                {
                    try
                    {
                        Game1.playSound(b.sound);
                    }
                    catch { }
                }
                newBuffList.Add(buff);
            }
            farmerBuffs.Value = newBuffList;
        }
    }
}