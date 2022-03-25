using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CharacterLib;
using TMPro;

public class PartyInfocardScript : MonoBehaviour
{
	public TMP_Text membersLabel;
	public Button deployButton, disbandButton;

	private PartyScript party;
	private GameControllerScript gc;

	public void Init(PartyScript party)
	{
		gc = GameControllerScript.GetController();
		this.party = party;

		string labelText = "";
		Character[] characters = party.GetCharacters();
		foreach (Character character in characters)
		{
			if (character == null)
			{
				continue;
			}

			labelText += string.Format("- {0}: Level {1} {2}\n", character.name,
				character.combatClass.level, character.combatClass.name);
		}
		// There will be an extra newline character at the end
		if (labelText[labelText.Length - 1] == '\n')
		{
			labelText = labelText.Substring(0, labelText.Length - 1);
		}
		membersLabel.text = labelText;

		deployButton.onClick.AddListener(OnDeploy);
		disbandButton.onClick.AddListener(OnDisband);

		if (party.IsDeployed)
		{
			deployButton.interactable = false;
		}
	}

	public void OnDeploy()
	{
		gc.DeployParty(party);
	}

	public void OnDisband()
	{
		gc.parties.Remove(party);
		gc.availableAdventurers.AddRange(party.GetCharacters().Where(x => x != null));
		Destroy(party.gameObject);
		Destroy(gameObject);
	}
}
