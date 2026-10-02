namespace SittingAction
{
    public partial class ModEntry
    {

        public static bool SwitchBool(bool value)
        {
            if(!Config.ModEnabled || SHelper.Input.IsDown(Config.SuppressKey) || !value)
                return value;
            return false;
        }
    }
}