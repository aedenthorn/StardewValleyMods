using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace ObjectTriggers
{
    public partial class ModEntry {

        private void CheckTrigger(object tripper, GameLocation location, Vector2 tripperTile, string triggerKey)
        {
            foreach (var kvp in location.objects.Pairs)
            {
                if (kvp.Value.Name == objectTriggerDataDict[triggerKey].objectID)
                {
                    if (CheckObjectTrigger(location, tripperTile, triggerKey, kvp.Key))
                    {
                        TripTrigger(tripper, triggerKey, kvp.Key, kvp.Value);
                    }
                }
            }
        }

        private bool CheckObjectTrigger(GameLocation location, Vector2 tripperTile, string triggerKey, Vector2 objectTile)
        {
            if (!location.objects.ContainsKey(objectTile))
                return false;
            switch (objectTriggerDataDict[triggerKey].triggerType)
            {
                case "range":
                    if (Vector2.Distance(tripperTile * 64, objectTile * 64) < objectTriggerDataDict[triggerKey].radius)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        private void ResetTrigger(object tripper, Vector2 tile, string triggerKey)
        {
            switch (objectTriggerDataDict[triggerKey].tripperType)
            {
                case "farmer":
                    var id = (tripper as Farmer).UniqueMultiplayerID;
                    if (farmerTrippingDict.ContainsKey(id))
                    {
                        for(int i = 0; i < farmerTrippingDict[id].Count; i++)
                        {
                            if (farmerTrippingDict[id][i].triggerKey == triggerKey && farmerTrippingDict[id][i].tilePosition == tile)
                            {
                                Monitor.Log($"Resetting trigger {triggerKey} for farmer {(tripper as Farmer).Name}");
                                farmerTrippingDict[id].RemoveAt(i);
                                switch (objectTriggerDataDict[triggerKey].triggerEffectType)
                                {
                                    case "particle":
                                        if (particleEffectAPI == null)
                                            break;
                                        if (objectTriggerDataDict[triggerKey].targetTripper)
                                            particleEffectAPI.EndFarmerParticleEffect((tripper as Farmer).UniqueMultiplayerID, objectTriggerDataDict[triggerKey].triggerEffectName);
                                        else
                                            particleEffectAPI.EndLocationParticleEffect((tripper as Farmer).currentLocation.Name, (int)tile.X * 64 + 32, (int)tile.Y * 64, objectTriggerDataDict[triggerKey].triggerEffectName);
                                        break;
                                }
                                return;
                            }

                        }
                    }
                    break;
            }

        }

        private void TripTrigger(object tripper, string triggerKey, Vector2 tile, Object obj)
        {
            switch (objectTriggerDataDict[triggerKey].tripperType)
            {
                case "farmer":
                    var id = (tripper as Farmer).UniqueMultiplayerID;
                    if (!farmerTrippingDict.ContainsKey(id))
                        farmerTrippingDict.Add(id, new List<ObjectTriggerInstance>());
                    foreach (var oti in farmerTrippingDict[id])
                    {
                        if (oti.triggerKey == triggerKey && oti.tilePosition == tile)
                            return;
                    }
                    farmerTrippingDict[id].Add(new ObjectTriggerInstance(triggerKey, tile));
                    Monitor.Log($"Tripping trigger {triggerKey} for farmer {(tripper as Farmer).Name}");
                    break;
            }

            switch (objectTriggerDataDict[triggerKey].triggerEffectType)
            {
                case "particle":
                    if (particleEffectAPI != null && particleEffectAPI.GetEffectNames().Contains(objectTriggerDataDict[triggerKey].triggerEffectName))
                    {
                        switch (objectTriggerDataDict[triggerKey].tripperType)
                        {
                            case "farmer":
                                if(objectTriggerDataDict[triggerKey].targetTripper)
                                    particleEffectAPI.BeginFarmerParticleEffect((tripper as Farmer).UniqueMultiplayerID, objectTriggerDataDict[triggerKey].triggerEffectName);
                                else
                                    particleEffectAPI.BeginLocationParticleEffect((tripper as Farmer).currentLocation.Name, (int)tile.X * 64 + 32, (int)tile.Y * 64, objectTriggerDataDict[triggerKey].triggerEffectName);
                                break;
                        }
                    }
                    break;
            }
        }

    }
}