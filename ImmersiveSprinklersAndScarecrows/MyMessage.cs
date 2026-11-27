namespace ImmersiveSprinklersAndScarecrows
{
    internal class MyMessage
    {
        public MyMessage(string location, string which)
        {
            Location = location;
            Which = which;
        }

        public string Location { get; }
        public string Which { get; }
    }
}