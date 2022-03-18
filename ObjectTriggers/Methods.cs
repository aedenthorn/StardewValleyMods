using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace ObjectTriggers
{
    public partial class ModEntry {

        private void CheckTrigger(Farmer farmer, string triggerKey, ObjectTriggerData data)
        {
            foreach (var kvp in farmer.currentLocation.objects.Pairs)
            {
                if (kvp.Value.Name == data.objectID)
                {
                    CheckObjectTrigger(farmer, triggerKey, data, kvp.Key, kvp.Value);
                }
            }

        }

        private void CheckObjectTrigger(Farmer farmer, string triggerKey, ObjectTriggerData data, Vector2 tile, Object obj)
        {
            switch (data.triggerType)
            {
                case "range":
                    if (Vector2.Distance(farmer.Position, tile * 64) < data.radius)
                    {
                        TripTrigger(farmer, triggerKey, data, tile, obj);
                    }
                    else
                        ResetTrigger(farmer, triggerKey, data, tile, obj);
                    break;
            }
        }

        private void ResetTrigger(object tripper, string triggerKey, ObjectTriggerData data, Vector2 tile, Object obj)
        {
            switch (data.tripperType)
            {
                case "farmer":
                    var id = (tripper as Farmer).UniqueMultiplayerID;
                    if (farmerTrippingDict.ContainsKey(id))
                    {
                        for(int i = 0; i < farmerTrippingDict[id].Count; i++)
                        {
                            if (farmerTrippingDict[id][i].TriggerKey == triggerKey && farmerTrippingDict[id][i].Tile == tile)
                            {
                                switch (objectTriggerDataDict[triggerKey].triggerEffectType)
                                {
                                    case "particle":
                                        particleEffectAPI.EndFarmerParticleEffect((tripper as Farmer).UniqueMultiplayerID, data.triggerEffectName);
                                        break;
                                }
                                farmerTrippingDict[id].RemoveAt(i);
                                return;
                            }

                        }
                    }
                    break;
            }

        }

        private void TripTrigger(object tripper, string triggerKey, ObjectTriggerData data, Vector2 tile, Object obj)
        {
            switch (data.triggerEffectType)
            {
                case "particle":
                    if (particleEffectAPI != null && particleEffectAPI.GetEffectNames().Contains(data.triggerEffectName))
                    {

                        if (data.targetTripper)
                        {
                            switch (data.tripperType)
                            {
                                case "farmer":
                                    var id = (tripper as Farmer).UniqueMultiplayerID;
                                    if (!farmerTrippingDict.ContainsKey(id))
                                        farmerTrippingDict.Add(id, new List<ObjectTriggerInstance>());
                                    foreach(var oti in farmerTrippingDict[id])
                                    {
                                        if (oti.TriggerKey == triggerKey && oti.Tile == tile)
                                            return;
                                    }
                                    farmerTrippingDict[id].Add(new ObjectTriggerInstance(triggerKey, tile));
                                    particleEffectAPI.BeginFarmerParticleEffect((tripper as Farmer).UniqueMultiplayerID, data.triggerEffectName);
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

    }
}