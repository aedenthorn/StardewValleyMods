using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultipleSpouses
{
	public static class UIPatches
	{
		private static IMonitor Monitor;

		// call this method from your Entry class
		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}
		public static void SocialPage_drawNPCSlot(SocialPage __instance, int i, List<string> ___kidsNames, ref Dictionary<string, string> ___npcNames)
		{
			try
			{

				string name = __instance.names[i] as string;
				if (___kidsNames.Contains(name))
				{
					if (___npcNames[name].EndsWith(")"))
					{
						___npcNames[name] = string.Join(" ", ___npcNames[name].Split(' ').Reverse().Skip(1).Reverse());
					}
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(SocialPage_drawNPCSlot)}:\n{ex}", LogLevel.Error);
			}
		}
		
		public static void DialogueBox_Prefix(ref List<string> dialogues)
		{
			try
			{
				if (dialogues == null || dialogues.Count < 2)
					return;

				if (dialogues[1] == Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1826"))
                {
					List<string> newDialogues = new List<string>()
					{
						dialogues[0]
					};



					List<NPC> spouses = Misc.GetSpouses(Game1.player,1).Values.OrderBy(o => Game1.player.friendshipData[o.Name].Points).Reverse().Take(4).ToList();

					List<int> which = new List<int>{ 0, 1, 2, 3 };

					Misc.ShuffleList(ref which);

					List<int> myWhich = new List<int>(which).Take(spouses.Count).ToList();

					for(int i = 0; i < spouses.Count; i++)
                    {
                        switch (which[i])
                        {
							case 0:
								newDialogues.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1827", spouses[i].displayName));
								break;
							case 1:
								newDialogues.Add(((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1832") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1834")) + " " + ((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1837", spouses[i].displayName[0]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1838", spouses[i].displayName[0])));
								break;
							case 2:
								newDialogues.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1843", spouses[i].displayName));
								break;
							case 3:
								newDialogues.Add(((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1831") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1833")) + " " + ((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1837", spouses[i].displayName[0]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1838", spouses[i].displayName[i])));
								break;
                        }
                    }
					dialogues = new List<string>(newDialogues);
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(DialogueBox_Prefix)}:\n{ex}", LogLevel.Error);
			}
		}
	}
}