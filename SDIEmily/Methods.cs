using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
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
            var rangeEnd = cursorPos;
            if (cursorDistance > Config.MaxSkillRange)
            {
                rangeEnd = Vector2.Lerp(playerPos, cursorPos, Config.MaxSkillRange / cursorDistance);
            }
            var rangeDistance = Vector2.Distance(rangeEnd, playerPos);
            SMonitor.Log($"Checking for monster for emily skill; player: {playerPos}; cursor {cursorPos}; cursor distance {cursorDistance}; rangeEnd {rangeEnd}; range distance {Vector2.Distance(playerPos, rangeEnd)}");
            for (int i = 0; i < rangeDistance; i++)
            {
                var point = Vector2.Lerp(playerPos, rangeEnd, i / rangeDistance);
                foreach (var m in farmer.currentLocation.characters)
                {
                    var offLine = Vector2.Distance(point, m.Position);
                    if (m is Monster && offLine <= Config.MaxSkillDistanceOffLookAxis)
                    {
                        SMonitor.Log($"Triggering Emily skill on {m.Name} at {m.Position}");
                        runningSkill = new RunningSkill(farmer.currentLocation, m.Position, true, farmer.CurrentTool);
                        return;
                    }
                }
            }
            SMonitor.Log($"Triggering Emily skill (no monster found) at range end {rangeEnd}");
            runningSkill = new RunningSkill(farmer.currentLocation, rangeEnd, true, farmer.CurrentTool);
        }

        private void BurstEvent(string arg1, Farmer arg2)
        {
            throw new NotImplementedException();
        }


        private void DealDamage(GameLocation location, Vector2 center, MeleeWeapon weapon)
        {
            for(int i = 0; i < location.characters.Count; i++)
            {
                if(location.characters[i] is Monster && Vector2.Distance(Game1.GlobalToLocal(location.characters[i].Position), center) <= Config.SkillRadius)
                {
                    (location.characters[i] as Monster).takeDamage((int)(Config.SkillDamageMult * GetWeaponDamage(weapon)), 0, 0, false, 0, Game1.player);
                }
            }
            foreach(var key in location.terrainFeatures.Keys.ToArray())
            {
                if (Vector2.Distance(Game1.GlobalToLocal(key * 64), center) > Config.SkillRadius)
                    continue;
                TerrainFeature f = location.terrainFeatures[key];
                if (f is HoeDirt)
                {
                    location.terrainFeatures[key].performUseAction(f.currentTileLocation, location);
                }
                else if (f is Grass)
                {
                    location.terrainFeatures[key].performToolAction(weapon, 0, f.currentTileLocation, location);
                }
            }
        }

        private int GetWeaponDamage(MeleeWeapon weapon)
        {
            return Game1.random.Next(weapon.minDamage.Value, weapon.maxDamage.Value + 1);
        }

        private Vector2 GetRandomPointOnCircle(Vector2 center, int radius)
        {
            double angle = Game1.random.Next(360) * Math.PI / 180;
            return new Vector2(center.X + radius * (float)Math.Cos(angle), center.Y + radius * (float)Math.Sin(angle));
        }
    }
}