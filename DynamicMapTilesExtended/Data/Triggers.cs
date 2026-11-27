namespace DMT.Data
{
    public static class Triggers
    {
        public const string Action = "Action";
        public const string CropPlanted = "CropPlanted{0}";
        public const string CropGrown = "CropGrown{0}";
        public const string CropHarvest = "CropHarvest{0}";
        public const string EnterLocation = "Enter";
        public const string Explode = "Explode";
        public const string Load = "Load";
        public const string MonsterSlain = "MonsterSlain{0}";
        public const string ObjectClicked = "ObjectClicked{0}";
        public const string ObjectPlaced = "ObjectPlaced{0}";
        public const string ObjectRemoved = "ObjectRemoved{0}";
        public const string PushTile = "Push";
        public const string PushedTile = "Pushed";
        public const string StepOff = "Off";
        public const string StepOn = "On";
        public const string StepOffNPC = "OffNPC";
        public const string StepOnNPC = "OnNPC";
        public const string TalkToNPC = "Talk{0}";
        public const string UseItem = "Item{0}";
        public const string UseTool = "Tool{0}";
        public const string Mount = "Mount{0}";
        public const string Dismount = "Dismount{0}";

        public const string CropPlantedRegex = @"CropPlanted(\([A-z]{1,}\)){0,1}";
        public const string CropGrownRegex = @"CropGrown(\([A-z]{1,}\)){0,1}";
        public const string CropHarvestRegex = @"CropHarvest(\([A-z]{1,}\)){0,1}";
        public const string DismountRegex = @"Dismount(\([A-z]{1,}\)){0,1}";
        public const string MonsterSlainRegex = @"MonsterSlain(\([A-z]{1,}\)){0,1}";
        public const string MountRegex = @"Mount(\([A-z]{1,}\)){0,1}";
        public const string ObjectPlacedRegex = @"ObjectPlaced(\(\([A-z]+\)[A-z0-9]+\))?";
        public const string ObjectRemovedRegex = @"ObjectRemoved(\(\([A-z]+\)[A-z0-9]+\))?";
        public const string ObjectClickedRegex = @"ObjectClicked(\(\([A-z]+\)[A-z0-9]+\))?";
        public const string TalkToNPCRegex = @"Talk(\([A-z]{1,}\)){0,1}";
        public const string UseItemRegex = @"Item(\([\(\)_A-z0-9]{1,}(\-[0-9]{1,}){0,2}\)){0,1}"; //Item(\([\(\)_A-z0-9]{1,}((\-[0-9]{1,}){0,1}(\-[0-9]{1,}(\+{0,1}|\-{0,1}){0,1}){0,1}){0,1}\)){0,1}
        public const string UseToolRegex = @"Tool(\([A-z]{1,}\)){0,1}";

        public static readonly HashSet<string> Regexes = [
            Action,
            CropPlantedRegex,
            CropGrownRegex,
            CropHarvestRegex,
            EnterLocation,
            Explode,
            Load,
            MonsterSlainRegex,
            ObjectClickedRegex,
            ObjectPlacedRegex,
            ObjectRemovedRegex,
            PushTile,
            PushedTile,
            StepOff,
            StepOn,
            StepOffNPC,
            StepOnNPC,
            TalkToNPCRegex,
            UseItemRegex,
            UseToolRegex,
            MountRegex,
            DismountRegex
        ];

        public static readonly HashSet<string> GlobalTriggers = [
            EnterLocation,
            TalkToNPCRegex,
            MonsterSlainRegex,
            MountRegex,
            DismountRegex
        ];
    }
}
