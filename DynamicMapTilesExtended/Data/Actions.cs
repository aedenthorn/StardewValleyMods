using HarmonyLib;

namespace DMT.Data
{
    public static class Actions
    {
        public const string ActionKey = "DMT/action";
        public const string AddLayerKey = "DMT/addLayer";
        public const string AddQuestKey = "DMT/addQuest";
        public const string AddTilesheetKey = "DMT/addTilesheet";
        public const string AnimationKey = "DMT/animation";
        public const string AnimationOffKey = "DMT/animationOff";
        public const string AppearanceKey = "DMT/appearance";
        public const string BarrierKey = "DMT/barrier";
        public const string BuffKey = "DMT/buff";
        public const string ChangeIndexKey = "DMT/changeIndex";
        public const string ChangeMultipleIndexKey = "DMT/changeMultipleIndex";
        public const string ChangeMultiplePropertiesKey = "DMT/changeMultipleProperties";
        public const string ChangePropertiesKey = "DMT/changeProperties";
        public const string ChestKey = "DMT/chest";
        public const string EmoteKey = "DMT/emote";
        public const string EventKey = "DMT/event";
        public const string EventStringKey = "DMT/eventString";
        public const string ExplodeKey = "DMT/explode";
        public const string ExplosionKey = "DMT/explosion";
        public const string FertilizeKey = "DMT/fertilize";
        public const string FriendsKey = "DMT/friends";
        public const string GiveKey = "DMT/give";
        public const string GrowCropKey = "DMT/growCrop";
        public const string HealthKey = "DMT/health";
        public const string HealthPerSecondKey = "DMT/healthPerSecond";
        public const string HealthPerSecondContKey = "DMT/healthPerSecondContinuous";
        public const string InvalidateKey = "DMT/invalidate";
        public const string KillCropKey = "DMT/killCrop";
        public const string LetterKey = "DMT/letter";
        public const string MailKey = "DMT/mail";
        public const string MailBoxKey = "DMT/mailbox";
        public const string MailRemoveKey = "DMT/mailRemove";
        public const string MakeoverKey = "DMT/makeover";
        public const string MakeoverGenderedKey = "DMT/makeoverGendered";
        public const string MessageKey = "DMT/message";
        public const string MonsterKey = "DMT/monster";
        public const string MoveKey = "DMT/move";
        public const string MusicKey = "DMT/music";
        public const string ObjectKey = "DMT/object";
        public const string PushKey = "DMT/push";
        public const string PushableKey = "DMT/pushable";
        public const string PushAlsoKey = "DMT/pushAlso";
        public const string PushOthersKey = "DMT/pushOthers";
        public const string RemoveQuestKey = "DMT/removeQuest";
        public const string SetCropKey = "DMT/setCrop";
        public const string SetModDataKey = "DMT/modData";
        public const string SoundKey = "DMT/sound";
        public const string SlipperyKey = "DMT/slippery";
        public const string SpeedKey = "DMT/speed";
        public const string StaminaKey = "DMT/stamina";
        public const string StaminaPerSecondContKey = "DMT/staminaPerSecondContinuous";
        public const string StaminaPerSecondKey = "DMT/staminaPerSecond";
        public const string TakeKey = "DMT/take";
        public const string TeleportKey = "DMT/teleport";
        public const string TeleportTileKey = "DMT/teleportTile";
        public const string TerrainFeatureKey = "DMT/terrainFeature";
        public const string TransmogKey = "DMT/transmog";
        public const string TransmogGenderedKey = "DMT/transmogGendered";
        public const string TriggerActionKey = "DMT/triggerAction";
        public const string WarpKey = "DMT/warp";
        public const string WardrobeKey = "DMT/wardrobe";
        public const string WardrobeGenderedKey = "DMT/wardrobeGendered";

        private static List<string?> allActions;

        public static List<string?> AllActions
        {
            get
            {
                if (allActions == null)
                {
                    allActions = new();
                    foreach (var t in AccessTools.GetDeclaredFields(typeof(Actions)))
                    {
                        if(t.IsLiteral)
                            allActions.Add((string?)typeof(Actions).GetField(t.Name)?.GetValue(null));
                    }
                }
                return allActions;
            }
        }

        public static readonly HashSet<string> ModKeys = [];
    }
}
