using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CharacterLib;
using TMPro;

public class InfocardScript : MonoBehaviour
{
	public TMP_Text nameLabel, healthLabel, manaLabel, levelLabel;

	private GameControllerScript gc;
	private Character selected;
	private bool initialized = false;

	public void Init(Character selected)
	{
		if (gc == null)
		{
			gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
		}
		
		this.selected = selected;
		initialized = true;
	}

	private void OnDisable()
	{
		initialized = false;
		selected = null;
	}

	private void FixedUpdate()
	{
		if (!initialized)
		{
			return;
		}

		nameLabel.text = selected.name;
		healthLabel.text = string.Format("Health: {0} / {1}", selected.health, selected.maxHealth);
		manaLabel.text = string.Format("Mana: {0} / {1}", selected.mana, selected.maxMana);
		levelLabel.text = string.Format("Level {0}: {1} / {2} exp", selected.combatClass.level,
			selected.combatClass.curEXP, selected.combatClass.levelUpEXP);
	}
}
