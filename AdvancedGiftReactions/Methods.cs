namespace AdvancedGiftReactions
{
    public partial class ModEntry
    {
        public static void ModifyGiftTaste(ref int taste, int num)
        {
            switch (taste)
            {
                case 0:
                    if (Config.LovedToLiked > 0 && num >= Config.LovedToLiked)
                    {
                        taste = 2;
                        ModifyGiftTaste(ref taste, num - Config.LovedToLiked);
                    }
                    break;
                case 2:
                    if (Config.LikedToNeutral > 0 && num >= Config.LikedToNeutral)
                    {
                        taste = 8;
                        ModifyGiftTaste(ref taste, num - Config.LikedToNeutral);
                    }
                    break;
                case 8:
                    if (Config.NeutralToDisliked > 0 && num >= Config.NeutralToDisliked)
                    {
                        taste = 4;
                        ModifyGiftTaste(ref taste, num - Config.NeutralToDisliked);
                    }
                    break;
                case 4:
                    if (Config.DislikedToHated > 0 && num >= Config.DislikedToHated)
                    {
                        taste = 6;
                    }
                    break;
            }
        }
        public static void IncreaseGiftTaste(ref int taste)
        {
            switch (taste)
            {
                case 2:
                    taste = 0;
                    break;
                case 8:
                    taste = 2;
                    break;
                case 4:
                    taste = 8;
                    break;
                case 6:
                    taste = 4;
                    break;
            }
        }
    }
}