using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CharacterLib;
using ItemLib;

public class ClassLoaderScript : MonoBehaviour
{
	[System.Serializable]
	private class AttackJSON
	{
		public string name;
		public int rangeBonus;
		public float damageMultiplier;
		public float manaCostMultiplier;
		public bool isMagical;
		public float cooldown, duration;

		public string projectileSpritePath;
		public float projectileSize;
		public float projectileSpeed;

		public Attack Build()
		{
			return new Attack()
			{
				name = name,
				rangeBonus = rangeBonus,
				damageMultiplier = damageMultiplier,
				manaCostMultiplier = manaCostMultiplier,
				cooldown = cooldown,
				duration = duration,
				isMagical = isMagical,

				projectileSprite = Resources.Load<Sprite>(projectileSpritePath),
				projectileSize = projectileSize,
				projectileSpeed = projectileSpeed
			};
		}
	}

	[System.Serializable]
	private class PotentialSkillEntry
	{
		public int level;
		public CombatSkillJSON skill;

		public CombatSkill Build()
		{
			return new CombatSkill()
			{
				name = skill.name,
				desc = skill.desc,
				level = level,
				icon = Resources.Load<Sprite>(skill.iconPath),
				newAttacks = skill.newAttacks.Select(attack => attack.Build())
					.ToList(),
				healthBonus = skill.healthBonus,
				manaBonus = skill.manaBonus,
				physRBonus = skill.physicalResistanceBonus,
				magiRBonus = skill.magicResistanceBonus
			};
		}
	}

	[System.Serializable]
	private class CombatSkillJSON
	{
		public string name;
		public string desc;
		public string iconPath;

		public List<AttackJSON> newAttacks;
		public int healthBonus, manaBonus;
		public int physicalResistanceBonus, magicResistanceBonus;
	}

	[System.Serializable]
	private class CombatClassJSON
	{
		public string name;
		public int levelUpEXP;
		public float levelUpEXPMultiplier;
		public string iconPath;

		public int healthPerLevel, manaPerLevel;
		public List<PotentialSkillEntry> potentialSkills;
		public AttackJSON baseAttack;
		public List<string> allowedArmour;
		public List<string> allowedWeapons;

		public CombatClass Build()
		{
			int maxLevel = 0;
			foreach (PotentialSkillEntry entry in potentialSkills)
			{
				if (maxLevel < entry.level)
				{
					maxLevel = entry.level;
				}
			}
			List<List<CombatSkill>> builtSkills = new List<List<CombatSkill>>();
			for (int i = 0; i < maxLevel; i++)
			{
				builtSkills.Add(new List<CombatSkill>());
			}
			foreach (PotentialSkillEntry entry in potentialSkills)
			{
				CombatSkill builtSkill = entry.Build();
				builtSkills[entry.level - 1].Add(builtSkill);
			}

			return new CombatClass()
			{
				name = name,
				level = 1,
				curEXP = 0,
				levelUpEXP = levelUpEXP,
				levelUpEXPMultiplier = levelUpEXPMultiplier,
				icon = Resources.Load<Sprite>(iconPath),
				healthIncPerLevel = healthPerLevel,
				manaIncPerLevel = manaPerLevel,
				freeSkillPoints = 0,
				unlockedSkills = new List<CombatSkill>(),
				potentialSkills = builtSkills,
				attacks = new List<Attack>() { baseAttack.Build() },
				allowedArmour = allowedArmour.Select(s => ItemUtil.StringToAC(s)).ToList(),
				allowedWeapons = allowedWeapons.Select(s => ItemUtil.StringToWC(s)).ToList()
			};
		}
	}

	private class ClassJSONList
	{
		public CombatClassJSON[] classes;
	}

	private static bool initialized = false;

	private void Start()
	{
		if (initialized)
		{
			return;
		}
		TextAsset enemyClassFile = Resources.Load<TextAsset>("EnemyClasses");
		TextAsset playableClassFile = Resources.Load<TextAsset>("PlayableClasses");
		ClassJSONList enemyClasses = JsonUtility.FromJson<ClassJSONList>(enemyClassFile.text);
		ClassJSONList playableClasses = JsonUtility.FromJson<ClassJSONList>(playableClassFile.text);
		Debug.Log("Loaded combat class files");
		foreach (CombatClassJSON combatClassJSON in enemyClasses.classes)
		{
			ClassUtil.LoadClass(combatClassJSON.Build(), false);
		}
		foreach (CombatClassJSON combatClassJSON in playableClasses.classes)
		{
			ClassUtil.LoadClass(combatClassJSON.Build());
		}
		
		initialized = true;
	}
}
