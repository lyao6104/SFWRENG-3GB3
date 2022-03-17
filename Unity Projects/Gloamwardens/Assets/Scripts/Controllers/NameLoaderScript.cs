using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NamesLib;

public class NameLoaderScript : MonoBehaviour
{
	private static bool initialized = false;

	private void Start()
	{
		if (initialized)
		{
			return;
		}

		TextAsset[] nameFiles = Resources.LoadAll("Names", typeof(TextAsset)).Cast<TextAsset>().ToArray();
		foreach (TextAsset file in nameFiles)
		{
			NameList loadedNamelist = JsonUtility.FromJson<NameList>(file.text);
			NamesUtil.LoadNameList(loadedNamelist);
		}
		initialized = true;
	}
}
