using System.Collections.Generic;

namespace DialogueTrees
{
    public class CharacterDataModel
    {
        public List<CharacterData> Changes;

        public CharacterDataModel(string name)
        {
            Changes = new List<CharacterData>();
            CharacterData cd = new CharacterData();
            cd.Action = "EditData";
            cd.Target = $"Characters/Dialogue/{name}";
            cd.Entries = new Dictionary<string, string>();
            Changes.Add(cd);
        }
    }
    public class CharacterData
    {
        public string Action;
        public string Target;
        public Dictionary<string, string> Entries;
    }
}