using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CharacterLib;
using TMPro;

public class PartyCreationScript : MonoBehaviour
{
	public List<Character> selectedAdventurers = new List<Character>(PartyScript.partySize);
	public List<Button> slots = new List<Button>(PartyScript.partySize);
	public GameObject selectAdventurerPrefab;
	public Transform contentTransform;

	private GameControllerScript gc;
	private Character selectedAdventurer;

	private void OnEnable()
	{
		if (gc == null)
		{
			gc = GameControllerScript.GetController();
		}

		for (int i = 0; i < gc.availableAdventurers.Count; i++)
		{
			GameObject newCard = Instantiate(selectAdventurerPrefab, contentTransform);
			newCard.GetComponentInChildren<TMP_Text>().text = string.Format("{0}: Level {1} {2}", gc.availableAdventurers[i].name,
				gc.availableAdventurers[i].combatClass.level, gc.availableAdventurers[i].combatClass.name);
			// Basically, disable the clicked button to signify that it's been selected, and re-enable the other buttons.
			int index = i; // Needed since i is not local to this scope.
			newCard.GetComponent<Button>().onClick.AddListener(() =>
			{
				selectedAdventurer = gc.availableAdventurers[index];
				Button[] buttons = contentTransform.GetComponentsInChildren<Button>();
				for (int j = 0; j < buttons.Length; j++)
				{
					if (index == j)
					{
						buttons[j].interactable = false;
					}
					else
					{
						buttons[j].interactable = true;
					}
				}
			});
		}

		for (int i = 0; i < selectedAdventurers.Count; i++)
		{
			selectedAdventurers[i] = null;
		}
	}

	public void AssignToSlot(int index)
	{
		if (selectedAdventurer == null || selectedAdventurers.Contains(selectedAdventurer))
		{
			return;
		}

		selectedAdventurers[index] = selectedAdventurer;
		slots[index].GetComponentInChildren<TMP_Text>().text = string.Format("Slot {0}: {1}", index + 1, selectedAdventurer.name);
	}

	public void CreateParty()
	{
		// It's assumed that an empty name is a null character.
		if (selectedAdventurers.Any(x => x == null || x.name == ""))
		{
			return;
		}

		GameObject partyGO = new GameObject("Adventurer Party");
		// Just some arbitrarily far position so it doesn't interfere with deployment.
		partyGO.transform.position = new Vector3(1000, 1000);
		PartyScript newParty = partyGO.AddComponent<PartyScript>();
		for (int i = 0; i < selectedAdventurers.Count; i++)
		{
			newParty.AssignAdventurer(selectedAdventurers[i]);
		}
		CircleCollider2D collider = partyGO.AddComponent<CircleCollider2D>();
		collider.isTrigger = true;
		collider.radius = 2.5f;
		partyGO.layer = LayerMask.NameToLayer("Party");
		gc.parties.Add(newParty);
		gc.availableAdventurers.RemoveAll(x => selectedAdventurers.Contains(x));
		Cancel();
	}

	public void Cancel()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			slots[i].GetComponentInChildren<TMP_Text>().text = string.Format("Slot {0}", i + 1);
		}
		Button[] potentialAdventurers = contentTransform.GetComponentsInChildren<Button>();
		for (int i = 0; i < potentialAdventurers.Length; i++)
		{
			Destroy(potentialAdventurers[i].gameObject);
		}
		gc.TogglePartyView();
		gameObject.SetActive(false);
	}
}
