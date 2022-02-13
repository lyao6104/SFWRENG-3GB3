/* Date: May 18, 2021
 * Name: L. Yao
 * Desc: Collection of utilities for getting names.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NamesLib
{
	[System.Serializable]
	public class WeightedString
	{
		public string value;
		public int weight;
	}

	// Unity can't serialize nested Lists
	[System.Serializable]
	public class NameSegmentList
	{
		public string segmentType;
		public List<string> segments;

		public string this[int key]
		{
			get
			{
				return segments[key];
			}
			set
			{
				segments[key] = value;
			}
		}

		public int Count { get { return segments.Count; } }
	}

	[System.Serializable]
	public class NameList
	{
		public string listName; // This is mainly used for readability purposes.
		public List<NameSegmentList> nameSegments;
		public List<WeightedString> nameFormats;
	}

	public static class NamesUtil
	{
		private static Dictionary<string, NameList> loadedNamelists = new Dictionary<string, NameList>();

		public static void LoadNameList(NameList toLoad)
		{
			loadedNamelists.Add(toLoad.listName, toLoad);
			Debug.Log("Loaded NameList " + toLoad.listName);
		}

		public static string GetName(string listName)
		{
			NameList nameList = loadedNamelists[listName];

			// Choose a format
			string chosenFormat = nameList.nameFormats[nameList.nameFormats.Count - 1].value;
			float formatTotalWeights = 0;
			foreach (WeightedString format in nameList.nameFormats)
			{
				formatTotalWeights += format.weight;
			}
			float formatValue = Random.value * formatTotalWeights;
			for (int i = 0; i < nameList.nameFormats.Count; i++)
			{
				if (formatValue < nameList.nameFormats[i].weight)
				{
					chosenFormat = nameList.nameFormats[i].value;
					break;
				}
				else
				{
					formatValue -= nameList.nameFormats[i].weight;
				}
			}

			// Generate a name according to the chosen format
			string generatedName = chosenFormat;
			for (int i = 0; i < nameList.nameSegments.Count; i++)
			{
				string segmentCode = "{" + nameList.nameSegments[i].segmentType + "}";
				int pos = generatedName.IndexOf(segmentCode);
				while (pos > -1)
				{
					string generatedSegment = nameList.nameSegments[i][Random.Range(0, nameList.nameSegments[i].Count)];
					generatedName = generatedName.Substring(0, pos) + generatedSegment + generatedName.Substring(pos + segmentCode.Length);
					pos = generatedName.IndexOf(segmentCode);
				}
			}

			return generatedName;
		}

		public static string GetSystemName()
		{
			return GetName("SystemNames");
		}

		public static string GetFleetName(bool imperial = false)
		{
			return GetName(imperial ? "FleetNames_I" : "FleetNames_R");
		}
	}
}
