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
	public FleetScript fleet;
	// We also want a list (or single reference) of fleets stationed here.

	// Graphics
	public GameObject hyperlanePrefab;
	public Material empireMat, rebellionMat;
	private List<HyperlaneScript> hyperlanes;

	// UI
	public TMP_Text nameLabel;
	public GameObject fleetIndicator; // UI element indicating whether a fleet is present.
	public Color empireFleetColour, rebelFleetReadyColour, rebelFleetMovedColour;

	private void Start()
	{
		hyperlanes = new List<HyperlaneScript>();

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

	public void Conquer(Player conquerer)
	{
		SetPlayer(conquerer);
		GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>().SystemConquered(this);
	}

	public void GenerateHyperlanes()
	{
		for (int i = 0; i < connections.Count; i++)
		{
			GameObject newLane = Instantiate(hyperlanePrefab, transform);
			HyperlaneScript laneScript = newLane.GetComponent<HyperlaneScript>();
			laneScript.Init(transform.position, connections[i].transform.position);
			laneScript.Deselect();
			hyperlanes.Add(laneScript);
		}
	}

	public void UpdateUI()
	{
		fleetIndicator.SetActive(fleet != null);
		if (fleet != null)
		{
			TMP_Text fleetIndicatorText = fleetIndicator.GetComponent<TMP_Text>();
			if (fleet.controller == Player.Empire)
			{
				fleetIndicatorText.color = empireFleetColour;
			}
			else if (fleet.hasMoved)
			{
				fleetIndicatorText.color = rebelFleetMovedColour;
			}
			else
			{
				fleetIndicatorText.color = rebelFleetReadyColour;
			}
		}
	}

	public void Select()
	{
		for (int i = 0; i < hyperlanes.Count; i++)
		{
			hyperlanes[i].Select();
		}
	}

	public void Deselect()
	{
		for (int i = 0; i < hyperlanes.Count; i++)
		{
			hyperlanes[i].Deselect();
		}
	}
}
