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
		// Load rift names. Uses names from https://www.fantasynamegenerators.com/
		var starsFile = Resources.Load<TextAsset>("Names/SystemNames");
		NameList starsList = JsonUtility.FromJson<NameList>(starsFile.text);
		//Debug.Log(JsonUtility.ToJson(riftList));
		NamesUtil.LoadNameList(starsList);
	}
}
