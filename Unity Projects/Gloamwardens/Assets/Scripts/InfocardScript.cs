using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CharacterLib;
using TMPro;

public class InfocardScript : MonoBehaviour
{
	public TMP_Text nameLabel, healthLabel, manaLabel, levelLabel;
	public TMP_Text resistancesLabel, curTargetingLabel;
	public GameObject skillsButton, retreatButton, targetingPrefs;

	private GameControllerScript gc;
	private Character selected;
	private bool initialized = false;

	public void Init(Character selected)
	{
		if (gc == null)
		{
			gc = GameControllerScript.GetController();
		}
		
		this.selected = selected;

		if (!gc.IsPlayerControlled(this.selected))
		{
			skillsButton.SetActive(false);
			retreatButton.SetActive(false);
			targetingPrefs.SetActive(false);
		}
		else
		{
			skillsButton.SetActive(true);
			retreatButton.SetActive(true);
			targetingPrefs.SetActive(true);
			string targetingCriterion;
			TargetingCriteria criterion = gc.GetAssociatedAdventurer(this.selected).GetTargetPriority();
			//Debug.Log(criterion.ToString());
			switch (criterion)
			{
				case TargetingCriteria.Closest:
					targetingCriterion = "Closest";
					break;
				case TargetingCriteria.MostHealthy:
					targetingCriterion = "Most Healthy";
					break;
				case TargetingCriteria.LeastHealthy:
					targetingCriterion = "Least Healthy";
					break;
				default:
					targetingCriterion = "Default";
					break;
			}
			curTargetingLabel.text = string.Format("Current: {0}", targetingCriterion);
		}

		initialized = true;
	}

	public void OnSkillButtonClicked()
	{
		if (gc.ToggleSkillView())
		{
			gc.skillPanel.GetComponent<SkillViewScript>().Init(selected);
		}
	}

	public void OnTargetButtonClicked(int criterion)
	{
		if (!gc.IsPlayerControlled(selected))
		{
			return;
		}

		AdventurerScript adventurer = gc.GetAssociatedAdventurer(selected);
		if (adventurer != null)
		{
			//Debug.Log(adventurer.name);
			//Debug.Log(((TargetingCriteria)criterion).ToString());
			// Enums don't show up in the inspector so this will have to do.
			adventurer.PrioritizeTarget((TargetingCriteria)criterion);
			string targetingCriterion;
			switch((TargetingCriteria)criterion)
			{
				case TargetingCriteria.Closest:
					targetingCriterion = "Closest";
					break;
				case TargetingCriteria.MostHealthy:
					targetingCriterion = "Most Healthy";
					break;
				case TargetingCriteria.LeastHealthy:
					targetingCriterion = "Least Healthy";
					break;
				default:
					targetingCriterion = "Default";
					break;
			}
			curTargetingLabel.text = string.Format("Current: {0}", targetingCriterion);
		}
	}

	public void OnRetreatButtonClicked()
	{
		AdventurerScript adventurer = gc.GetAssociatedAdventurer(selected);
		if (adventurer != null)
		{
			adventurer.Retreat();
			gameObject.SetActive(false);
		}
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

		nameLabel.text = string.Format("{0} - {1}", selected.name, selected.combatClass.name);
		healthLabel.text = string.Format("Health: {0} / {1}", selected.health, selected.GetCharacterMaxHealth());
		manaLabel.text = string.Format("Mana: {0} / {1}", selected.mana, selected.GetCharacterMaxMana());
		levelLabel.text = string.Format("Level {0}: {1} / {2} exp", selected.combatClass.level,
			selected.combatClass.curEXP, selected.combatClass.levelUpEXP);
		resistancesLabel.text = string.Format("Physical Resistance: {0}\nMagical Resistance: {1}",
			selected.GetCharacterPhysicalResistance(), selected.GetCharacterMagicalResistance());

		if (selected == null || selected.health < 1)
		{
			gameObject.SetActive(false);
		}
	}
}
