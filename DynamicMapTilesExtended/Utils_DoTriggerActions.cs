using DMT.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace DMT
{
    public static partial class Utils
    {
        public static bool DoTriggerActions(Farmer? who, GameLocation location, Point tilePosition, List<(DynamicTileProperty prop, Tile tile)> properties)
        {
            List<string> triggered = new();

            foreach (var item in properties)
            {
                bool found = true;
                try
                {
                    var value = item.prop.Value;
                    var tile = item.tile;
                    switch (item.prop.Key)
                    {
                        case Actions.ActionKey:
                            DoAction(who, value);
                            break;
                        case Actions.AddLayerKey:
                            DoAddLayer(location, value);
                            break;
                        case Actions.AddQuestKey:
                            DoAddQuest(who, value);
                            break;
                        case Actions.AddTilesheetKey:
                            DoAddTileSheet(location, value);
                            break;
                        case Actions.AnimationKey:
                            DoAnimate(location, value, false);
                            break;
                        case Actions.AnimationOffKey:
                            DoAnimate(location, value, true);
                            break;
                        case Actions.AppearanceKey:
                            DoChangeAppearance(who, value);
                            break;
                        case Actions.BuffKey:
                            DoAddBuff(who, value);
                            break;
                        case Actions.ChangeIndexKey:
                            DoChangeIndex(location, value, tile, tilePosition);
                            break;
                        case Actions.ChangeMultipleIndexKey:
                            DoChangeMultipleIndexes(location, value, tile, tilePosition);
                            break;
                        case Actions.ChangePropertiesKey:
                            DoChangeProperties(value, tile);
                            break;
                        case Actions.ChangeMultiplePropertiesKey:
                            DoChangeMultipleProperties(location, value, tile);
                            break;
                        case Actions.ChestKey:
                            DoSpawnChest(location, value);
                            break;
                        case Actions.EmoteKey:
                            DoEmote(who, value);
                            break;
                        case Actions.EventKey:
                            DoPlayEvent(value);
                            break;
                        case Actions.EventStringKey:
                            DoPlayEventString(value);
                            break;
                        case Actions.ExplosionKey:
                            DoExplode(who, location, value, tilePosition);
                            break;
                        case Actions.FertilizeKey:
                            DoFertilize(location, value);
                            break;
                        case Actions.FriendsKey:
                            DoFriendshipChange(who, value);
                            break;
                        case Actions.GrowCropKey:
                            DoGrowCrop(location, value);
                            break;
                        case Actions.GiveKey:
                            DoGive(who, value);
                            break;
                        case Actions.HealthKey:
                            DoUpdateHealth(who, value);
                            break;
                        case Actions.HealthPerSecondKey:
                            DoUpdateHealthPerSecond(who, value);
                            break;
                        case Actions.HealthPerSecondContKey:
                            DoUpdateHealthPerSecondCont(who, value);
                            break;
                        case Actions.InvalidateKey:
                            DoInvalidateAsset(value);
                            break;
                        case Actions.KillCropKey:
                            DoKillCrop(location, value);
                            break;
                        case Actions.LetterKey:
                            DoShowLetter(value);
                            break;
                        case Actions.MailBoxKey:
                            DoAddMailForTomorrow(who, value);
                            break;
                        case Actions.MailKey:
                            DoAddMailflag(who, value);
                            break;
                        case Actions.MailRemoveKey:
                            DoRemoveMailflag(who, value);
                            break;;
                        case Actions.MakeoverKey:
                            DoMakeover(who, value, false, false, false);
                            break;
                        case Actions.MakeoverGenderedKey:
                            DoMakeover(who, value, true, false, false);
                            break;
                        case Actions.MessageKey:
                            DoShowMessage(value);
                            break;
                        case Actions.MonsterKey:
                            DoSpawnMonster(location, value);
                            break;
                        case Actions.MusicKey:
                            DoPlayMusic(value);
                            break;
                        case Actions.ObjectKey:
                            DoSpawnObject(location, value);
                            break;
                        case Actions.PushKey:
                            DoPushTiles(who, tile, tilePosition);
                            break;
                        case Actions.PushOthersKey:
                            DoPushOtherTiles(who, value, tile, tilePosition);
                            break;
                        case Actions.RemoveQuestKey:
                            DoRemoveQuest(who, value);
                            break;
                        case Actions.SetCropKey:
                            DoSetCrop(location, value);
                            break;
                        case Actions.SetModDataKey:
                            DoSetModData(location, value);
                            break;
                        case Actions.StaminaKey:
                            DoUpdateStamina(who, value);
                            break;
                        case Actions.StaminaPerSecondKey:
                            DoUpdateStaminaPerSecond(who, value);
                            break;
                        case Actions.StaminaPerSecondContKey:
                            DoUpdateStaminaPerSecondCont(who, value);
                            break;
                        case Actions.SoundKey:
                            DoPlaySound(location, value);
                            break;
                        case Actions.TakeKey:
                            DoTake(who, value);
                            break;
                        case Actions.TeleportKey:
                            DoTeleport(who, value);
                            break;
                        case Actions.TeleportTileKey:
                            DoTeleportTile(who, value);
                            break;
                        case Actions.TerrainFeatureKey:
                            DoSpawnTerraiFeature(location, value);
                            break;
                        case Actions.TransmogKey:
                            DoMakeover(who, value, false, false, true);
                            break;
                        case Actions.TransmogGenderedKey:
                            DoMakeover(who, value, true, false, true);
                            break;
                        case Actions.TriggerActionKey:
                            DoTriggerAction(who, location, value);
                            break;
                        case Actions.WarpKey:
                            DoWarp(who, value);
                            break;
                        case Actions.WardrobeKey:
                            DoMakeover(who, value, false, true, false);
                            break;
                        case Actions.WardrobeGenderedKey:
                            DoMakeover(who, value, true, true, false);
                            break;
                        default:
                            if (Actions.ModKeys.Contains(item.prop.key))
                            {
                                ModActions[item.prop.Key]?.Invoke(who, value, tile, tilePosition);
                            }
                            else
                                found = false;
                            break;
                    }
                    if (found)
                    {
                        triggered.Add(item.prop.key);
                        if (item.prop.Invalidate != "None" && item.prop.Invalidate != null)
                        {
                            if (item.prop.Invalidate.Equals(Invalidate.OnNewDay + ""))
                            {
                                InvalidateOnNewDay.Add(location);
                            }
                            else if (item.prop.Invalidate.Equals(Invalidate.OnLocationChanged + ""))
                            {
                                InvalidateOnLocationChanged.Add(location);
                            }
                            else if (item.prop.Invalidate.StartsWith(Invalidate.OnTimeChanged + ""))
                            {
                                int time = 10;
                                string timeString = item.prop.Invalidate.Substring((Invalidate.OnTimeChanged + "").Length);
                                if (!string.IsNullOrEmpty(timeString))
                                {
                                    int.TryParse(timeString, out time);
                                }
                                if (InvalidateOnTimeChanged.TryGetValue(location, out int oldTime))
                                {
                                    if (time < oldTime)
                                        InvalidateOnTimeChanged[location] = time;
                                }
                                else
                                {
                                    InvalidateOnTimeChanged[location] = time;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    context.Monitor.Log($"Error while trying to run action {item.prop.Key}", LogLevel.Error);
                    context.Monitor.Log($"{ex}", LogLevel.Error);
                }
            }
            if (triggered.Any())
            {
                //Context.Monitor.Log($"Triggered at {tilePosition} by {who?.displayName ?? "no one"}: {string.Join(',', triggered)}");
                return true;
            }
            return false;
        }

    }
}
