using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Object = StardewValley.Object;

namespace SDIEmily
{
    public partial class ModEntry
    {
        private void SkillEvent(string name, Farmer farmer)
        {
            var cursorPos = Game1.getMousePosition().ToVector2();
            var playerPos = Game1.GlobalToLocal(farmer.Position);
            var cursorDistance = Vector2.Distance(cursorPos, playerPos);
            var rangeEnd = Vector2.Lerp(playerPos, cursorPos, Config.MaxSkillRange / cursorDistance);
            SMonitor.Log($"Checking for monster for emily skill; player: {playerPos}; cursor {cursorPos}; cursor distance {cursorDistance}; rangeEnd {rangeEnd}; range distance {Vector2.Distance(playerPos, rangeEnd)}");
            for (int i = 0; i < Config.MaxSkillRange; i++)
            {
                var point = Vector2.Lerp(playerPos, rangeEnd, i / (float)Config.MaxSkillRange);
                foreach (var m in farmer.currentLocation.characters)
                {
                    var offLine = Vector2.Distance(point, m.Position);
                    if (m is Monster && offLine <= Config.MaxSkillDistanceOffLookAxis)
                    {
                        SMonitor.Log($"Triggering Emily skill on {m.Name} at {m.Position}");
                        farmer.currentLocation.TemporarySprites.Add(new EmilySkillSprite(m.Position));
                        return;
                    }
                }
            }
            SMonitor.Log($"Triggering Emily skill (no monster found) at range end {rangeEnd}");
            farmer.currentLocation.TemporarySprites.Add(new EmilySkillSprite(rangeEnd));
        }


        private void BurstEvent(string name, Farmer farmer)
        {
        }
    }
}