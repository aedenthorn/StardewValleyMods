namespace BuffFramework
{
    public interface IBuffFrameworkAPI
    {
        public void UpdateBuffs();
    }

    public class BuffFrameworkAPI : IBuffFrameworkAPI
    {
        public void UpdateBuffs()
        {
            ModEntry.UpdateBuffs();
        }
    }
}