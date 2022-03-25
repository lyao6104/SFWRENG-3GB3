using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterLib;
using TMPro;

public class SkillViewScript : MonoBehaviour
{
	public TMP_Text nameLabel, skillpointsLabel;
	public Transform contentTransform;
	public GameObject skillCardPrefab;

	private GameControllerScript gc;
	private Character character;
	private bool initialized = false;

	public void Init(Character character)
	{
		if (gc == null)
		{
			gc = GameControllerScript.GetController();
		}

		this.character = character;
		nameLabel.text = string.Format("{0}'s Skills", character.name);
		for (int i = 0; i < character.combatClass.potentialSkills.Count; i++)
		{
			for (int j = 0; j < character.combatClass.potentialSkills[i].Count; j++)
			{
				GameObject newCard = Instantiate(skillCardPrefab, contentTransform);
				newCard.GetComponent<SkillCardScript>().Init(character.combatClass.potentialSkills[i][j], character);
			}
		}

		initialized = true;
	}

	private void OnDisable()
	{
		initialized = false;

		SkillCardScript[] skillCards = contentTransform.GetComponentsInChildren<SkillCardScript>();
		for(int i = 0; i < skillCards.Length; i++)
		{
			Destroy(skillCards[i].gameObject);
		}
	}

	private void FixedUpdate()
	{
		if (!initialized)
		{
			return;
		}

		skillpointsLabel.text = string.Format("Free Skillpoints: {0}", character.combatClass.freeSkillPoints);
	}
}
