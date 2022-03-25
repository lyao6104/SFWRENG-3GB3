using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartySelectionPanelScript : MonoBehaviour
{
	public GameObject partyInfocardPrefab;
	public Transform contentTransform;

	private GameControllerScript gc;

	private void OnEnable()
	{
		if (gc == null)
		{
			gc = GameControllerScript.GetController();
		}

		for (int i = 0; i < gc.parties.Count; i++)
		{
			GameObject newCard = Instantiate(partyInfocardPrefab, contentTransform);
			newCard.GetComponent<PartyInfocardScript>().Init(gc.parties[i]);
		}
	}

	private void OnDisable()
	{
		PartyInfocardScript[] infocards = contentTransform.gameObject.GetComponentsInChildren<PartyInfocardScript>();
		for (int i = 0; i < infocards.Length; i++)
		{
			Destroy(infocards[i].gameObject);
		}
	}
}
