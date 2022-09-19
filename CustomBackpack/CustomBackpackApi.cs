namespace CustomBackpack
{
    public interface ICustomBackpackApi
    {
        public bool SetPlayerSlots(int slots, bool force);
    }
    public class CustomBackpackApi
    {
        public bool SetPlayerSlots(int slots, bool force)
        {
            return ModEntry.SetPlayerSlots(slots, force);
        }
    }
}