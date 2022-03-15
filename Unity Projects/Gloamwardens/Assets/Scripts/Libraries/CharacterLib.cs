using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemLib;
using NamesLib;

namespace CharacterLib
{
	[System.Serializable]
	public class Character
	{
		public string name;
		public int health, maxHealth = 100;
		public int mana, maxMana = 100;
		public Race race;

		// Equipment
		public Weapon weapon;
		public Armour torso;
		public Armour legs;

		public Character()
		{

		}
	}

	[System.Serializable]
	public class Race
	{
		public string name;
		public List<Sprite> bodySprites;

		// Names should be handled by NamesLib
	}

	public static class RaceUtil
	{
		private static Dictionary<string, Race> loadedRaces = new Dictionary<string, Race>();
		private static List<Sprite> hairSprites = new List<Sprite>();

		public static void LoadRace(Race toLoad)
		{
			loadedRaces[toLoad.name] = toLoad;
			Debug.LogFormat("Loaded the {0} race.", toLoad.name);
		}

		public static void LoadHair()
		{
			if (hairSprites.Count > 0)
			{
				Debug.Log("Hair has already been loaded.");
				return;
			}

			Sprite[] hairAssets = Resources.LoadAll<Sprite>("Third-Party/Sprites/Hair");
			hairSprites.AddRange(hairAssets);
		}
	}
}