using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CharacterLib;

public class RaceLoaderScript : MonoBehaviour
{
	[System.Serializable]
	private class RaceJSON
	{
		public string name;
		public int baseHealth, baseMana;
		public int basePhysicalResist, baseMagicalResist;
	}

	[System.Serializable]
	private class RaceJSONList
	{
		public RaceJSON[] races;
	}

	private static bool initialized = false;

	private void Start()
	{
		if (initialized)
		{
			return;
		}

		TextAsset pcRacesFile = Resources.Load<TextAsset>("PlayableRaces");
		TextAsset npcRacesFile = Resources.Load<TextAsset>("EnemyRaces");
		RaceJSONList pcRacesJSON = JsonUtility.FromJson<RaceJSONList>(pcRacesFile.text);
		RaceJSONList npcRacesJSON = JsonUtility.FromJson<RaceJSONList>(npcRacesFile.text);
		Debug.Log("Loaded race files.");
		foreach (RaceJSON raceJSON in npcRacesJSON.races)
		{
			Race loadedRace = new Race()
			{
				name = raceJSON.name,
				baseHealth = raceJSON.baseHealth,
				baseMana = raceJSON.baseMana,
				basePhysResistance = raceJSON.basePhysicalResist,
				baseMagiResistance = raceJSON.baseMagicalResist
			};
			Sprite[] bodyAssets = Resources.LoadAll<Sprite>("Third-Party/Sprites/Body/" + loadedRace.name);
			loadedRace.bodySprites = bodyAssets.ToList();

			RaceUtil.LoadRace(loadedRace, false);
		}
		foreach (RaceJSON raceJSON in pcRacesJSON.races)
		{
			Race loadedRace = new Race()
			{
				name = raceJSON.name,
				baseHealth = raceJSON.baseHealth,
				baseMana = raceJSON.baseMana,
				basePhysResistance = raceJSON.basePhysicalResist,
				baseMagiResistance = raceJSON.baseMagicalResist
			};
			Sprite[] bodyAssets = Resources.LoadAll<Sprite>("Third-Party/Sprites/Body/" + loadedRace.name);
			loadedRace.bodySprites = bodyAssets.ToList();

			RaceUtil.LoadRace(loadedRace);
		}

		RaceUtil.LoadHair();

		initialized = true;
	}
}
