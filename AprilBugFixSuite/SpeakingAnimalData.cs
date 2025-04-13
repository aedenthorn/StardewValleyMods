using Microsoft.Xna.Framework;
using StardewValley;

namespace AprilBugFixSuite
{
	public class SpeakingAnimalData
	{
		public float atextAboveHeadAlpha;
		public string atextAboveHead;
		public float atextAboveHeadPreTimer;
		public int atextAboveHeadTimer;
		public int atextAboveHeadStyle;
		public Color atextAboveHeadColor;
		public float bTextAboveHeadAlpha;
		public string bTextAboveHead;
		public float bTextAboveHeadPreTimer;
		public int bTextAboveHeadTimer;
		public int bTextAboveHeadStyle;
		public Color bTextAboveHeadColor;
		public long aID;
		public long bID;

		internal static string[][] animalSpeech = new string[][]
		{
			new string[]
			{
				ModEntry.SHelper.Translation.Get("animal-speech-1-1"),
				ModEntry.SHelper.Translation.Get("animal-speech-1-2")
			},
			new string[]
			{
				ModEntry.SHelper.Translation.Get("animal-speech-2-1"),
				ModEntry.SHelper.Translation.Get("animal-speech-2-2")
			},
			new string[]
			{
				ModEntry.SHelper.Translation.Get("animal-speech-3-1"),
				ModEntry.SHelper.Translation.Get("animal-speech-3-2")
			},
			new string[]
			{
				ModEntry.SHelper.Translation.Get("animal-speech-4-1"),
				ModEntry.SHelper.Translation.Get("animal-speech-4-2")
			},
			new string[]
			{
				ModEntry.SHelper.Translation.Get("animal-speech-5-1"),
				ModEntry.SHelper.Translation.Get("animal-speech-5-2")
			},
			new string[]
			{
				ModEntry.SHelper.Translation.Get("animal-speech-6-1"),
				ModEntry.SHelper.Translation.Get("animal-speech-6-2")
			},
			new string[]
			{
				ModEntry.SHelper.Translation.Get("animal-speech-7-1"),
				ModEntry.SHelper.Translation.Get("animal-speech-7-2")
			},
			new string[]
			{
				ModEntry.SHelper.Translation.Get("animal-speech-8-1"),
				ModEntry.SHelper.Translation.Get("animal-speech-8-2")
			},
			new string[]
			{
				ModEntry.SHelper.Translation.Get("animal-speech-9-1"),
				ModEntry.SHelper.Translation.Get("animal-speech-9-2")
			},
			new string[]
			{
				ModEntry.SHelper.Translation.Get("animal-speech-10-1"),
				ModEntry.SHelper.Translation.Get("animal-speech-10-2")
			}
		};

		public SpeakingAnimalData(FarmAnimal a, FarmAnimal b)
		{
			string[] strings = animalSpeech[Game1.random.Next(animalSpeech.Length)];

			aID = a.myID.Value;
			atextAboveHeadAlpha = 0f;
			atextAboveHeadPreTimer = 0;
			atextAboveHeadTimer = 3000;
			atextAboveHeadStyle = -1;
			atextAboveHeadColor = Color.Black;
			atextAboveHead = strings[0];
			bID = b.myID.Value;
			bTextAboveHeadAlpha = 0f;
			bTextAboveHeadPreTimer = 2000 + Game1.random.Next(500);
			bTextAboveHeadTimer = 3000;
			bTextAboveHeadStyle = -1;
			bTextAboveHeadColor = Color.Black;
			bTextAboveHead = strings[1];
		}
	}
}
