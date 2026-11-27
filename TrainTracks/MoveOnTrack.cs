using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace TrainTracks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool[][] trackTileDataDict = new bool[][]
        {
            new bool[] { true, false, true, false },
            new bool[] { false, true, false, true },
            new bool[] { true, false, true, false },
            new bool[] { false, true, false, true },
            new bool[] { false, true, true, false },
            new bool[] { false, false, true, true }, // 5
            new bool[] { true, false, false, true },
            new bool[] { true, true, false, false},
            new bool[] { false, false, true, false },
            new bool[] { true, false, false, false }, 
            new bool[] { false, false, false, true }, // 10
            new bool[] { false, true, false, false },
            new bool[] { true, true, false, true },
            new bool[] { false, true, true, true },
            new bool[] { true, false, true, true }, 
            new bool[] { true, true, true, false }, // 15
            new bool[] { true, true, true, true }
        };
        private TerrainFeature currentFeature;
        private int trackIndex;

        private void MoveOnTrack()
        {
            if (Helper.Input.IsDown(Config.SpeedUpKey) || Helper.Input.IsSuppressed(Config.SpeedUpKey))
            {
                currentSpeed = Math.Min(Config.MaxSpeed, currentSpeed + 0.5f);
                Helper.Input.Suppress(Config.SpeedUpKey);
            }
            else if (Helper.Input.IsDown(Config.SlowDownKey) || Helper.Input.IsSuppressed(Config.SlowDownKey))
            {
                currentSpeed = Math.Max(0, currentSpeed - 0.5f);
                Helper.Input.Suppress(Config.SlowDownKey);
            }
            Game1.player.canMove = false;
            if (canWarp && Game1.currentLocation.Name != lastLocation)
            {
                Monitor.Log($"Moved to new map, disallowing warp");
                canWarp = false;
            }
            for (int i = 0; i < currentSpeed; i++)
            {
                Vector2 offset = GetOffset(Game1.player.FacingDirection);
                var tilePos = new Vector2((int)((Game1.player.Position.X + offset.X) / 64), (int)((Game1.player.Position.Y + offset.Y) / 64));
                
                if (tilePos != lastTile)
                {
                    if (lastLocation != Game1.currentLocation.Name)
                    {
                        Monitor.Log($"Moved to new tile in new map, setting last map to new map");
                        lastLocation = Game1.currentLocation.Name;
                    }
                    else if (!canWarp)
                    {
                        Monitor.Log($"Moved to second tile in new map, allowing warp");
                        canWarp = true;
                    }
                    lastTile = tilePos;
                    turnedThisTile = false;
                    currentFeature = null;
                    if (!Game1.player.currentLocation.terrainFeatures.TryGetValue(tilePos, out TerrainFeature feature) || feature is not Flooring || !feature.modData.TryGetValue(trackKey, out string indexString) || !int.TryParse(indexString, out trackIndex))
                    {
                        Monitor.Log($"Off track", StardewModdingAPI.LogLevel.Warn);
                        return;
                    }
                    if(feature.modData.TryGetValue(speedDataKey, out string speedString) && int.TryParse(speedString, out int speed))
                    {
                        currentSpeed = speed;
                        Monitor.Log($"Tile set speed to {speed}");
                    }
                    currentFeature = feature;
                }
                if (canWarp)
                {
                    Warp warp = Game1.player.currentLocation.isCollidingWithWarp(Game1.player.GetBoundingBox(), Game1.player);
                    if (warp != null)
                    {
                        Monitor.Log($"warping to {warp.TargetName}");
                        Game1.player.warpFarmer(warp);
                        return;
                    }
                }
                if (currentFeature == null)
                    return;

                int facing = Game1.player.FacingDirection;

                Vector2 pos = new Vector2((float)Math.Round(Game1.player.Position.X), (float)Math.Round(Game1.player.Position.Y));
                Vector2 tPos = currentFeature.currentTileLocation * 64;
                int dir = -1;

                GetDirection(ref pos, tPos, ref dir, ref facing, currentFeature, trackTileDataDict[trackIndex]);

                Vector2 move = Vector2.Zero;
                switch (dir)
                {
                    case 0:
                        move = new Vector2(0, -1);
                        break;
                    case 1:
                        move = new Vector2(1, 0);
                        break;
                    case 2:
                        move = new Vector2(0, 1);
                        break;
                    case 3:
                        move = new Vector2(-1, 0);
                        break;
                }
                Game1.player.Position = pos + move;
                Game1.player.FacingDirection = facing;
            }
        }

        private void GetDirection(ref Vector2 pos, Vector2 tPos, ref int dir, ref int facing, TerrainFeature feature, bool[] trackData)
        {
            int leftTurn = facing == 0 ? 3 : facing - 1;
            int rightTurn = (facing + 1) % 4;

            // check forced turn

            if (!turnedThisTile && feature.modData.TryGetValue(switchDataKey, out string switchString))
            {
                int currentSwitch = -1;

                SwitchData switchData = new(switchString);
                int switchIndex = 0;
                if (switchData.trackSwitches[facing] != null)
                {
                    if (feature.modData.TryGetValue(currentSwitchKey, out string switchIndexString) && int.TryParse(switchIndexString, out switchIndex) && switchIndex < switchData.trackSwitches[facing].Length)
                    {
                        //Monitor.Log($"got switch {switchIndex}");
                        currentSwitch = switchData.trackSwitches[facing][switchIndex];
                    }
                    else
                    {
                        //Monitor.Log($"no switch index");
                        currentSwitch = switchData.trackSwitches[facing][0];
                    }
                }
                if (currentSwitch >= 0 && currentSwitch < trackData.Length && trackData[currentSwitch] && ReachedTurnPos(pos, tPos, currentSwitch, facing))
                {
                    feature.modData[currentSwitchKey] = ((switchIndex + 1) % switchData.trackSwitches[facing].Length) + "";
                    //Monitor.Log($"Turning on switch {switchIndex}/{switchData.trackSwitches[facing].Length}: {currentSwitch}; next switch {Game1.currentLocation.terrainFeatures[feature.currentTileLocation].modData[currentSwitchKey]}");
                    SetDir(ref pos, tPos, ref facing, ref dir, currentSwitch);
                    turnedThisTile = true;
                    return;
                }
            }

            // check input turn

            if (!turnedThisTile && Helper.Input.IsDown(Config.TurnLeftKey))
            {
                if (trackData[leftTurn] && ReachedTurnPos(pos, tPos, leftTurn, facing))
                {
                    SetDir(ref pos, tPos, ref facing, ref dir, leftTurn);
                    turnedThisTile = true;
                    return;
                }
            }
            if (!turnedThisTile && Helper.Input.IsDown(Config.TurnRightKey))
            {
                if (trackData[rightTurn] && ReachedTurnPos(pos, tPos, rightTurn, facing))
                {
                    SetDir(ref pos, tPos, ref facing, ref dir, rightTurn);
                    turnedThisTile = true;
                    return;
                }
            }

            // not turning


            // check straight

            if(trackData[facing])
            {
                SetDir(ref pos, tPos, ref facing, ref dir, facing);
                return;
            }

            // check corner

            if(trackData[leftTurn] && !trackData[rightTurn] && ReachedTurnPos(pos, tPos, leftTurn, facing))
            { 
                SetDir(ref pos, tPos, ref facing, ref dir, leftTurn);
                return;
            }
            
            if(!trackData[leftTurn] && trackData[rightTurn] && ReachedTurnPos(pos, tPos, rightTurn, facing))
            { 
                SetDir(ref pos, tPos, ref facing, ref dir, rightTurn);
                return;
            }

            // still entering

            if (!ReachedTurnPos(pos, tPos, leftTurn, facing) && !ReachedTurnPos(pos, tPos, rightTurn, facing))
            {
                SetDir(ref pos, tPos, ref facing, ref dir, facing);
                return;
            }

            // else stop

        }

        private void SetDir(ref Vector2 pos, Vector2 tPos, ref int facing, ref int dir, int newDir)
        {
            switch (newDir)
            {
                case 0:
                case 2:
                    pos.X = tPos.X - GetOffset(newDir).X;
                    break;
                case 1:
                case 3:
                    pos.Y = tPos.Y - GetOffset(newDir).Y;
                    break;
            }
            facing = newDir;
            dir = newDir;
        }

        private bool ReachedTurnPos(Vector2 pos, Vector2 tPos, int newDir, int facing)
        {
            switch (facing)
            {
                case 0:
                    return pos.Y <= tPos.Y - GetOffset(newDir).Y;
                case 1:
                    return pos.X >= tPos.X - GetOffset(newDir).X;
                case 2:
                    return pos.Y >= tPos.Y - GetOffset(newDir).Y;
                case 3:
                    return pos.X <= tPos.X - GetOffset(newDir).X;
            }
            return false;
        }
    }
}