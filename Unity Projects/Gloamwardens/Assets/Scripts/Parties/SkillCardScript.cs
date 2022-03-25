using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CharacterLib;
using TMPro;

public class SkillCardScript : MonoBehaviour
{
	public Image skillImage;
	public TMP_Text nameLabel, descLabel, effectsLabel;

	private GameControllerScript gc;
	private CombatSkill skill;
	private Character character;

	public void Init(CombatSkill skill, Character character)
	{
		if (gc == null)
		{
			gc = GameControllerScript.GetController();
		}

		this.skill = skill;
		this.character = character;

		skillImage.sprite = this.skill.icon;
		if (character.combatClass.unlockedSkills.Contains(this.skill))
		{
			skillImage.color = Color.white;
		}
		else
		{
			skillImage.color = Color.black;
		}

		nameLabel.text = skill.name;
		descLabel.text = skill.desc;
		List<string> attackSnippets = new List<string>();
		foreach(Attack atk in skill.newAttacks)
		{
			attackSnippets.Add(string.Format("New Attack: <i>{0}</i>", atk.name));
		}
		effectsLabel.text = string.Format("Level {0} Skill.", skill.level);
		if (attackSnippets.Count > 0)
		{
			effectsLabel.text += string.Format(" {0}.", string.Join(" ", attackSnippets));
		}
		if (skill.healthBonus > 0)
		{
			effectsLabel.text += string.Format(" +{0} Health.", skill.healthBonus);
		}
		if (skill.manaBonus > 0)
		{
			effectsLabel.text += string.Format(" +{0} Mana.", skill.manaBonus);
		}
		if (skill.physRBonus > 0)
		{
			effectsLabel.text += string.Format(" +{0} Physical Resistance.", skill.physRBonus);
		}
		if (skill.magiRBonus > 0)
		{
			effectsLabel.text += string.Format(" +{0} Magical Resistance.", skill.magiRBonus);
		}
	}

	public void AttemptUnlock()
	{
		if (character.combatClass.unlockedSkills.Contains(skill) || character.combatClass.freeSkillPoints < 1)
		{
			return;
		}

		character.combatClass.unlockedSkills.Add(skill);
		character.combatClass.freeSkillPoints--;
		skillImage.color = Color.white;
	}
}
