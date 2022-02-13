using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsScript : MonoBehaviour
{
	public int numSystems = 10;
	public int minHyperlanes = 1, maxHyperlanes = 3;
	public float minDistance = 5;

	private void Start()
	{
		DontDestroyOnLoad(this);
	}
}
