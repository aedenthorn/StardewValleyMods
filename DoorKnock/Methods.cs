using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Minigames;
using StardewValley.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;

namespace DoorKnock
{
    public partial class ModEntry
    {
        public static void KnockInteriorDoor(Vector2 doorTile, string[] action)
        {
            PlayKnockSound(doorTile);

            var up = Game1.player.FacingDirection == 0;
            Vector2 answerTile = doorTile + (up ? new Vector2(0, -1) : new Vector2(0, 1));
            NPC npc;
            if (action.Length > 2)
            {
                List<NPC> inRoom = new();
                foreach (var name in action.Skip(1))
                {
                    var n = Game1.currentLocation.getCharacterFromName(name);
                    if (n != null && (n.controller == null || n.doingEndOfRouteAnimation.Value) && IsInRoom(n, answerTile, new List<Vector2>()))
                    {
                        inRoom.Add(n);
                    }
                }
                if (!inRoom.Any())
                    return;
                npc = Game1.random.ChooseFrom(inRoom);
            }
            else
            {
                npc = Game1.currentLocation.getCharacterFromName(action[1]);
                if (npc == null || (npc.controller != null && !npc.doingEndOfRouteAnimation.Value) || !IsInRoom(npc, answerTile, new List<Vector2>()))
                    return;
            }
            SMonitor.Log($"Starting to answer interior door: {npc.Name} at {npc.currentLocation.Name} {npc.Tile} for door at {doorTile}");
            npc.modData[answerPointKey] = $"{doorTile.X},{doorTile.Y},{(up ? 2 : 0)}";
            AddDelay(npc.Name, Config.AnswerDelay);
        }

        public static void KnockExteriorDoor(Vector2 doorTile, string[] action)
        {
            PlayKnockSound(doorTile);

            if (action.Length < 8 || action[0] != "LockedDoorWarp")
                return;

            var interior = Game1.getLocationFromName(action[3]);
            if(interior == null) 
                return;

            NPC npc = interior.getCharacterFromName(action[6]);
            if (npc == null || npc.controller != null)
                return;

            SMonitor.Log($"Starting to answer exterior door: {npc.Name} at {npc.currentLocation.Name} {npc.Tile} for door at {Game1.currentLocation.Name} {doorTile}");

            npc.modData[answerPointKey] = $"{doorTile.X},{doorTile.Y},2,{Game1.currentLocation.NameOrUniqueName}";
            AddDelay(npc.Name, Config.AnswerDelay);


        }
        public static void PlayKnockSound(Vector2 doorTile)
        {
            SMonitor.Log($"Playing knock sound for door at: {Game1.currentLocation.Name} {doorTile}");

            int delay = 1;
            for (int i = 0; i < Config.KnockNumber; i++)
            {
                DelayedAction.playSoundAfterDelay(Config.KnockSound, delay, Game1.currentLocation, doorTile);
                delay += Config.KnockInterval;
            }
            foreach (var k in Config.KnockButton.Keybinds)
            {
                foreach (var b in k.Buttons)
                {
                    SHelper.Input.Suppress(b);
                }
            }

        }
        public static void DoneDelaying(NPC npc)
        {
            var ps = npc.modData[answerPointKey].Split(',');
            var exterior = ps.Length == 4;
            npc.modData.Remove(answerPointKey);
            if (npc.isSleeping.Value && !Config.WakeWhenSleeping)
            {
                SMonitor.Log($"Not answering door: {npc.Name} is sleeping");
                return;
            }
            if (npc.controller != null)
            {
                if (npc.doingEndOfRouteAnimation.Value)
                {
                    npc.modData[animationKey] = npc.endOfRouteBehaviorName.Value;
                }
                else
                {
                    SMonitor.Log($"Not answering door: {npc.Name} is busy");
                    return;
                }
            }

            SMonitor.Log($"Answering after delay: {npc.Name} at {npc.currentLocation.Name} {npc.Tile} going to {(exterior ? ps[3] : npc.currentLocation.Name)} {ps[0]},{ps[1]}");

            var schedule = npc.pathfindToNextScheduleLocation("knock", npc.currentLocation.NameOrUniqueName, npc.TilePoint.X, npc.TilePoint.Y, exterior ? ps[3] : npc.currentLocation.NameOrUniqueName, int.Parse(ps[0]), int.Parse(ps[1]) + (exterior ? 1 : 0), int.Parse(ps[2]), null, null);
            AccessTools.Method(typeof(NPC), "prepareToDisembarkOnNewSchedulePath").Invoke(npc, Array.Empty<object>());
            npc.modData[returnPointKey] = $"{npc.TilePoint.X},{npc.TilePoint.Y},{npc.FacingDirection}{(exterior ? $",{npc.currentLocation.NameOrUniqueName}":"")}";
            npc.controller = new PathFindController(schedule.route, npc, Utility.getGameLocationOfCharacter(npc))
            {
                finalFacingDirection = schedule.facingDirection,
                endBehaviorFunction = new PathFindController.endBehavior(WaitBehaviour),
            };
            if (exterior)
            {
                farmerController.Value = Game1.player.Position;
            }
        }
       public static void WaitBehaviour(Character c, GameLocation l)
        {
            NPC npc = c as NPC;
            //var doorTile = c.TilePoint + new Point(0, c.FacingDirection == 0 ? -1 : 1);
            //c.currentLocation.openDoor(new xTile.Dimensions.Location(c.TilePoint.X, c.TilePoint.Y), true);
            AddReturn(npc.Name, Config.WaitTime);
        }


        public static void DoneWaiting(NPC npc)
        {
            var ps = npc.modData[returnPointKey].Split(',');
            var exterior = ps.Length == 4;
            npc.modData.Remove(returnPointKey);
            if (npc.controller != null)
            {
                SMonitor.Log($"Not returning after answer: {npc.Name} is busy");
                return;
            }
            var dest = exterior ? ps[3] : npc.currentLocation.Name;
            SMonitor.Log($"Returning after answer: {npc.Name} going to {dest} {ps[0]},{ps[1]}");
            var schedule = npc.pathfindToNextScheduleLocation("knock", npc.currentLocation.Name, npc.TilePoint.X, npc.TilePoint.Y, dest, int.Parse(ps[0]), int.Parse(ps[1]), int.Parse(ps[2]), null, null);
            npc.controller = new PathFindController(schedule.route, npc, Utility.getGameLocationOfCharacter(npc))
            {
                finalFacingDirection = int.Parse(ps[2]),
                endBehaviorFunction = new PathFindController.endBehavior(ReturnedBehaviour)
            };

        }


        public static void ReturnedBehaviour(Character c, GameLocation l)
        {
            NPC npc = c as NPC;
            SMonitor.Log($"Returned to pos: {npc.Name} is at {npc.currentLocation.Name}, {npc.Tile}");

            if (npc.modData.TryGetValue(animationKey, out var str))
            {
                SMonitor.Log($"Playing animation: {npc.Name} is restarting {str}");
                AccessTools.FieldRefAccess<NPC, string>(npc, "loadedEndOfRouteBehavior") = npc.endOfRouteBehaviorName.Value;
                PathFindController.endBehavior behavior = (PathFindController.endBehavior)AccessTools.Method(typeof(NPC), "getRouteEndBehaviorFunction").Invoke(npc, new object[] { npc.endOfRouteBehaviorName.Value, null });
                behavior.Invoke(c, l);
            }
            //npc.faceTowardFarmerForPeriod(3000, 100, false, Game1.player);
        }


        public static bool IsInRoom(NPC npc, Vector2 tile, List<Vector2> tiles)
        {
            if (npc.Tile == tile)
                return true;
            if (Game1.currentLocation.getTileIndexAt((int)tile.X, (int)tile.Y, "Buildings") >= 0)
                return false;
            tiles.Add(tile);
            foreach (var t in Utility.getAdjacentTileLocationsArray(tile))
            {
                if (tiles.Contains(t) || !Game1.currentLocation.isTileOnMap(t))
                    continue;
                if (IsInRoom(npc, t, tiles))
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddReturn(string name, int waitTime)
        {
            if (Game1.IsMasterGame)
                returnDict[name] = waitTime;
            else
                SendMessage(name, waitTime, "Return");
        }
        public static void AddDelay(string name, int waitTime)
        {
            if (Game1.IsMasterGame)
                delayDict[name] = waitTime;
            else
                SendMessage(name, waitTime, "Delay");
        }
        public static void SendMessage(string name, int delay, string type)
        {
            MyMessage message = new MyMessage(name, delay); // create your own class with the data to send
            SHelper.Multiplayer.SendMessage(message, type);
        }
    }
}