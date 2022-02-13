using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NamesLib;
using TMPro;

public class StarSystemScript : MonoBehaviour
{
	// Gameplay
	public List<StarSystemScript> connections;
	public bool isCapital;
	public Player controller;
	// We also want a list (or single reference) of fleets stationed here.

	// Graphics
	public GameObject hyperlanePrefab;
	public Material empireMat, rebellionMat;

	// UI
	public TMP_Text nameLabel;

	private void Start()
	{
		name = NamesUtil.GetSystemName();
		nameLabel.text = name;
	}

	public void SetName(string newName)
	{
		name = newName;
		nameLabel.text = name;
	}

	public void SetCapital()
	{
		isCapital = true;
	}

	public void SetPlayer(Player newController)
	{
		controller = newController;
		GetComponent<SpriteRenderer>().material = controller == Player.Empire ? empireMat : rebellionMat;
	}

	public void GenerateHyperlanes()
	{
		for (int i = 0; i < connections.Count; i++)
		{
			GameObject newLane = Instantiate(hyperlanePrefab, transform);
			HyperlaneScript laneScript = newLane.GetComponent<HyperlaneScript>();
			laneScript.Init(transform.position, connections[i].transform.position);
			laneScript.Deselect();
		}
	}
}
