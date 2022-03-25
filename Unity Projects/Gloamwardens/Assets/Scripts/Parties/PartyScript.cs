using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterLib;

public class PartyScript : MonoBehaviour
{
	public bool IsDeployed { get; private set; } = false;

	public const int partySize = 4;
	private Character[] adventurers = new Character[partySize];
	// The array below is just for the GameObject scripts representing the characters.
	private AdventurerScript[] adventurerScripts = new AdventurerScript[partySize];

	public bool AssignAdventurer(Character adventurer)
	{
		for (int i = 0; i < adventurers.Length; i++)
		{
			if (adventurers[i] == null)
			{
				adventurers[i] = adventurer;
				return true;
			}
		}
		return false;
	}

	public void Deploy(Vector2 position)
	{
		transform.position = position;
		for (int i = 0; i < adventurers.Length; i++)
		{
			adventurerScripts[i] = Instantiate(GameControllerScript.GetController().adventurerPrefab,
				transform.position, Quaternion.identity, transform).GetComponent<AdventurerScript>();
			adventurerScripts[i].Deploy(this, adventurers[i]);
		}
		IsDeployed = true;
	}

	public void Retreat()
	{
		IsDeployed = false;
		for (int i = 0; i < adventurers.Length; i++)
		{
			adventurerScripts[i].Clear();
			adventurerScripts[i] = null;
		}
	}

	public Character[] GetCharacters()
	{
		Character[] characters = new Character[adventurers.Length];
		adventurers.CopyTo(characters, 0);
		return characters;
	}

	public AdventurerScript[] GetPartyMembers(AdventurerScript member)
	{
		AdventurerScript[] members = new AdventurerScript[3];
		int i = 0;
		for (int j = 0; j < adventurers.Length; j++)
		{
			if (i > 2)
			{
				return new AdventurerScript[0];
			}

			if (adventurerScripts[j] != member)
			{
				members[i++] = adventurerScripts[j];
			}
		}
		return members;
	}

	public void RemoveAdventurer(AdventurerScript toRemove)
	{
		for (int i = 0; i < adventurers.Length; i++)
		{
			if (adventurerScripts[i] == toRemove)
			{
				adventurers[i] = null;
				adventurerScripts[i] = null;
			}
		}

		for (int i = 0; i < adventurers.Length; i++)
		{
			if (adventurers[i] != null)
			{
				return;
			}
		}
		GameControllerScript.GetController().parties.Remove(this);
		Destroy(gameObject);
	}

	public void UpdateCharacterData(AdventurerScript adventurer)
	{
		for (int i = 0; i < adventurers.Length; i++)
		{
			if (adventurerScripts[i] == adventurer)
			{
				adventurers[i] = adventurer.characterData;
			}
		}
	}
}
