using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.FarmAnimals;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Linq;
using Object = StardewValley.Object;

namespace AdvancedAutoGrabber
{
	public partial class ModEntry
    {

        public static void RegisterLocationTriggers(GameLocation location)
        {
            location.Animals.OnValueAdded -= Animals_OnValueAdded;
            location.Animals.OnValueAdded += Animals_OnValueAdded;
        }

        public static void Objects_OnValueAdded(Vector2 key, Object value)
        {
            if (value?.QualifiedItemId == "(BC)165")
            {

                TriggerAutoGrabber(value);
            }
        }

        public static void Animals_OnValueAdded(long key, FarmAnimal a)
        {
            if ( a?.currentProduce.Value == null || !a.isAdult() || a.GetHarvestType() is null || a.GetHarvestType().GetValueOrDefault() == FarmAnimalHarvestType.DigUp)
                return;
            if (a?.currentLocation?.Objects?.Pairs is null)
                return;
            foreach (var kvp in a.currentLocation.Objects.Pairs)
            {
                if (kvp.Value?.QualifiedItemId == "(BC)165")
                {
                    TriggerAutoGrabber(kvp.Value);
                }
            }
        }

        public static void TriggerAutoGrabber(Object grabber)
        {
            Chest gchest = grabber.heldObject.Value as Chest;
            if (gchest != null)
            {
                bool grabbed = false;
                foreach (var animal in grabber.Location.Animals.Values.Where(a => a.currentProduce.Value != null && a.isAdult() && a.GetHarvestType().GetValueOrDefault() != FarmAnimalHarvestType.DigUp && (Config.GrabRange < 0 || Vector2.Distance(a.Tile, grabber.TileLocation) <= Config.GrabRange)))
                {
                    bool grab = TryGrab(animal, grabber, gchest);
                    if (!grabbed)
                        grabbed = grab;
                }
                if (Config.SendToChests && grabbed)
                {
                    SendToChests(grabber, gchest);
                }
            }
        }

        private static void SendToChests(Object grabber, Chest gchest)
        {
            for (int i = gchest.Items.Count - 1; i >= 0; i--)
            {
                var item = gchest.Items[i];
                if (item is not Object obj)
                    continue;
                var chests = grabber.Location.Objects.Values.OfType<Chest>().Where(c => Config.ChestRange < 0 || Vector2.Distance(c.TileLocation, grabber.TileLocation) <= Config.ChestRange).ToList();
                if (Config.IncludeBuildingChests)
                {
                    foreach(var b in grabber.Location.buildings)
                    {
                        if (!b.HasIndoors())
                            continue;
                        chests.AddRange(b.GetIndoors().Objects.Values.OfType<Chest>());
                    }
                }
                foreach (var chest in chests)
                {
                    if (chest.playerChest.Value && chest.GetItemsForPlayer().ContainsId(obj.QualifiedItemId) && chest.addItem(obj) == null)
                    {
                        gchest.GetItemsForPlayer().Remove(obj);
                    }
                }
            }
        }

        public static bool TryGrab(FarmAnimal animal, Object grabber, Chest chest)
        {
            Object produce = ItemRegistry.Create<Object>("(O)" + animal.currentProduce.Value, 1, 0, false);
            produce.CanBeSetDown = false;
            produce.Quality = animal.produceQuality.Value;
            if (animal.hasEatenAnimalCracker.Value)
            {
                produce.Stack = 2;
            }
            if ((!grabber.modData.TryGetValue(limitKey, out var itemId) || IsSameAnimal(produce.ItemId, itemId)) && chest.addItem(produce) == null)
            {
                animal.HandleStatsOnProduceCollected(produce, (uint)produce.Stack);
                animal.currentProduce.Value = null;
                animal.ReloadTextureIfNeeded(false);
                grabber.showNextIndex.Value = true;
                SMonitor.Log($"Auto-grabbed {produce.Name} at {grabber.TileLocation} from {animal.displayName} at {animal.Tile}");
                return true;
            }
            return false;
        }

        public static bool TryGrab(Object grabber, Chest chest, Object produce)
        {
            Random r = Utility.CreateDaySaveRandom(produce.TileLocation.X, produce.TileLocation.Y * 777f, 0.0);
            produce.Quality = produce.Location.GetHarvestSpawnedObjectQuality(Game1.MasterPlayer, produce.isForage(), produce.TileLocation, r);

            return produce.HasTypeObject() && (!grabber.modData.TryGetValue(limitKey, out var itemId) || IsSameAnimal(produce.ItemId, itemId)) && chest.addItem(produce) == null;
        }

        public static bool IsSameAnimal(string produceId, string itemId)
        {
            return Game1.farmAnimalData.Values.Any(d => (d.ProduceItemIds.Any(p => p.ItemId == produceId) || d.DeluxeProduceItemIds.Any(p => p.ItemId == produceId)) && (d.ProduceItemIds.Any(p => p.ItemId == itemId) || d.DeluxeProduceItemIds.Any(p => p.ItemId == itemId)));
        }

    }
}
