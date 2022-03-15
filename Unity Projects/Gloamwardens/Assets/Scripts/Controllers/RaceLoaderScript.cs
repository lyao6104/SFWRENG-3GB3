using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CharacterLib;

public class RaceLoaderScript : MonoBehaviour
{
	private void Start()
	{
		TextAsset[] raceFiles = Resources.LoadAll("Races", typeof(TextAsset)).Cast<TextAsset>().ToArray();
		foreach (TextAsset file in raceFiles)
		{
			//SpeciesGenders genderList = JsonUtility.FromJson<SpeciesGenders>(file.text);
			//GendersUtil.LoadSpeciesGenders(genderList);
		}
	}
}
