using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Linq;

namespace SDIEmily
{
    public partial class ModEntry
    {

        private void SkillEvent(string name, Farmer farmer)
        {
            var data = sdiAPI.GetCharacter(name, true);
            var target = GetTarget(farmer, Config.MaxSkillRange, Config.MaxSkillDistanceOffLookAxis);
            farmer.currentLocation.playSound("parrot_squawk");
            runningSkills.Add(new RunningSkill(farmer.currentLocation, farmer.Position, target, data.CharacterColor, true, farmer.CurrentTool));
        }

        private void BurstEvent(string name, Farmer farmer)
        {
            var data = sdiAPI.GetCharacter(name, true);
            var target = GetTarget(farmer, Config.MaxBurstRange, Config.MaxBurstDistanceOffLookAxis);
            farmer.currentLocation.playSound("parrot_squawk");
            runningBursts.Add(new RunningBurst(farmer.currentLocation, target, data.CharacterColor, true, farmer.CurrentTool));
        }

        private Vector2 GetTarget(Farmer farmer, float maxRange, float maxDistanceOffAxis)
        {
            var cursorPos = new Vector2(Game1.viewport.Location.X, Game1.viewport.Location.Y) + Game1.getMousePosition().ToVector2();
            var playerPos = farmer.Position;
            var cursorDistance = Vector2.Distance(cursorPos, playerPos);
            var rangeEnd = cursorPos;
            if (cursorDistance > Config.MaxSkillRange)
            {
                rangeEnd = Vector2.Lerp(playerPos, cursorPos, maxRange / cursorDistance);
            }
            var rangeDistance = Vector2.Distance(rangeEnd, playerPos);
            SMonitor.Log($"Checking for monster; player: {playerPos}; cursor {cursorPos}; cursor distance {cursorDistance}; rangeEnd {rangeEnd}; range distance {Vector2.Distance(playerPos, rangeEnd)}");
            for (int i = 0; i < rangeDistance; i++)
            {
                var point = Vector2.Lerp(playerPos, rangeEnd, i / rangeDistance);
                foreach (var m in farmer.currentLocation.characters)
                {
                    var offLine = Vector2.Distance(point, m.Position);
                    if (m is Monster && offLine <= maxDistanceOffAxis)
                    {
                        SMonitor.Log($"returning {m.Name} position {m.Position}");
                        return m.Position;
                    }
                }
            }
            SMonitor.Log($"(no monster found) returning range end {rangeEnd}");
            return rangeEnd;
        }

        private void DealDamage(GameLocation location, Vector2 center, MeleeWeapon weapon, Color damageColor, float damageMult, int radius)
        {
            for (int i = 0; i < location.characters.Count; i++)
            {
                if(location.characters[i] is Monster && !location.characters[i].IsInvisible && !(location.characters[i] as Monster).isInvincible() && Vector2.Distance(location.characters[i].Position, center) <= radius)
                {
                    int damageAmount = (int)(damageMult * GetWeaponDamage(weapon));
                    location.removeDamageDebris(location.characters[i] as Monster);
                    location.debris.Add(new Debris(damageAmount, new Vector2((float)(location.characters[i].GetBoundingBox().Center.X + 16), (float)location.characters[i].GetBoundingBox().Center.Y), new Color(255, 130, 0), 1f, location.characters[i]));
                    (location.characters[i] as Monster).takeDamage(damageAmount, 0, 0, false, 0, Game1.player);
                }
            }
            foreach(var key in location.terrainFeatures.Keys.ToArray())
            {
                if (Vector2.Distance(key * 64, center) > radius)
                    continue;
                TerrainFeature f = location.terrainFeatures[key];
                if (f is HoeDirt)
                {
                    location.terrainFeatures[key].performUseAction(f.currentTileLocation, location);
                }
                else if (f is Grass)
                {
                    location.terrainFeatures[key].performToolAction(weapon, 0, f.currentTileLocation, location);
                    if ((location.terrainFeatures[key] as Grass).numberOfWeeds.Value <= 0)
                        location.terrainFeatures.Remove(key);
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