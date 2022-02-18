using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfocardScript : MonoBehaviour
{
	public TMP_Text systemLabel;
	public TMP_Text controllerLabel, fleetLabel;

	private GameControllerScript gc;

	public void Init(StarSystemScript selected)
	{
		if (gc == null)
		{
			gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
		}
		systemLabel.text = selected.name;
		controllerLabel.text = string.Format("Controlled By: {0}", gc.GetPlayerName(selected.controller));
		if (selected.fleet == null)
		{
			fleetLabel.text = "Fleet: None";
		}
		else
		{
			fleetLabel.text = string.Format("Fleet: {0}", selected.fleet.name);
		}
	}
}
