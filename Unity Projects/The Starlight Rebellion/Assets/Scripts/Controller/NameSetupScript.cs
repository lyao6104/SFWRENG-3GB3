/* Date: May 18, 2021
 * Name: L. Yao
 * Desc: Reads and initializes names.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NamesLib;

public class NameSetupScript : MonoBehaviour
{
	private void Start()
	{
		// Uses names from https://www.fantasynamegenerators.com/
		var starsFile = Resources.Load<TextAsset>("Names/SystemNames");
		var empiresFile = Resources.Load<TextAsset>("Names/EmpireNames");
		var empireFleetsFile = Resources.Load<TextAsset>("Names/EmpireFleetNames");
		var rebelFleetsFile = Resources.Load<TextAsset>("Names/RebelFleetNames");

		NameList starsList = JsonUtility.FromJson<NameList>(starsFile.text);
		NameList empiresList = JsonUtility.FromJson<NameList>(empiresFile.text);
		NameList empireFleetsList = JsonUtility.FromJson<NameList>(empireFleetsFile.text);
		NameList rebelFleetsList = JsonUtility.FromJson<NameList>(rebelFleetsFile.text);

		NamesUtil.LoadNameList(starsList);
		NamesUtil.LoadNameList(empiresList);
		NamesUtil.LoadNameList(empireFleetsList);
		NamesUtil.LoadNameList(rebelFleetsList);
	}
}
