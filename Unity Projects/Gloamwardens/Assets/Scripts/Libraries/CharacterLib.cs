using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ItemLib;
using NamesLib;

namespace CharacterLib
{
	[System.Serializable]
	public class Character
	{
		public string name;
		public int health, maxHealth = 100;
		public int mana, maxMana = 100;
		public Race race;

		// Graphics
		public Sprite bodySprite, hairSprite;

		// Combat
		public int basePhysResistance;
		public int baseMagiResistance;
		public CombatClass combatClass;

		// Equipment
		public Weapon weapon;
		public Armour torso;
		public Armour legs;

		public Character(Race desiredRace = null)
		{

			if (desiredRace == null)
			{
				race = RaceUtil.GetPlayableRace();
				Debug.Log(race.name);
			}
			else
			{
				race = desiredRace;
			}
			name = NamesUtil.GetRaceName(race);

			maxHealth = race.baseHealth;
			health = maxHealth;

			maxMana = race.baseMana;
			mana = maxMana;

			bodySprite = race.bodySprites[Random.Range(0, race.bodySprites.Count)];

			basePhysResistance = race.basePhysResistance;
			baseMagiResistance = race.baseMagiResistance;
			combatClass = ClassUtil.GetPlayableClass();
		}

		public void LevelUp()
		{
			if (!combatClass.LevelUp())
			{
				return;
			}

			maxHealth += combatClass.healthIncPerLevel;
			maxMana += combatClass.manaIncPerLevel;
			health = maxHealth;
			mana = maxMana;
		}

		public virtual GameObject SpawnGameObject()
		{
			GameObject spawnedChar = new GameObject(name);
			SpriteRenderer spr = spawnedChar.AddComponent<SpriteRenderer>();
			spr.sprite = bodySprite;

			return spawnedChar;
		}
	}

	[System.Serializable]
	public class CombatClass
	{
		public string name;
		public int level = 1;
		public int curEXP = 0;
		public int levelUpEXP = 100;
		public float levelUpEXPMultiplier = 1.5f;
		public Sprite icon;

		public int healthIncPerLevel = 5;
		public int manaIncPerLevel = 5;

		public int freeSkillPoints = 0;
		public List<CombatSkill> unlockedSkills = new List<CombatSkill>();
		public List<List<CombatSkill>> potentialSkills = new List<List<CombatSkill>>();
		public List<Attack> attacks = new List<Attack>();
		public List<ArmourClass> allowedArmour = new List<ArmourClass>();
		public List<WeaponClass> allowedWeapons = new List<WeaponClass>();

		public bool LevelUp()
		{
			if (curEXP < levelUpEXP)
			{
				Debug.LogWarning("Attempted to level up without required EXP.");
				return false;
			}

			curEXP = 0;
			levelUpEXP = Mathf.RoundToInt(levelUpEXP * levelUpEXPMultiplier);
			level++;
			freeSkillPoints++;
			return true;
		}

		public bool UnlockSkill(int skillLevel, string skillName)
		{
			if (level < skillLevel || skillName == "")
			{
				return false;
			}

			if (potentialSkills.Count < skillLevel - 1)
			{
				return false;
			}

			CombatSkill toUnlock = potentialSkills[skillLevel - 1]
				.Find(skill => skill.name == skillName);
			if (toUnlock == null)
			{
				return false;
			}
			else
			{
				unlockedSkills.Add(toUnlock);
				return true;
			}
		}
	}

	[System.Serializable]
	public class CombatSkill
	{
		public string name;
		public string desc;
		public int level = 1;
		public Sprite icon;

		public List<Attack> newAttacks;
		public int healthBonus, manaBonus;
		public int physRBonus, magiRBonus;
	}

	[System.Serializable]
	public class Attack
	{
		public string name;
		public int rangeBonus;
		public float damageMultiplier = 1;
		public bool isMagical = false;
		// Cooldown is how long before THIS attack is usable again,
		// duration is how long before OTHER attacks can be made.
		public float cooldown, duration;

		public Sprite projectileSprite;
		public float projectileSize;
		public float projectileSpeed;
	}

	[System.Serializable]
	public class Race
	{
		public string name;
		public List<Sprite> bodySprites;

		public int baseHealth, baseMana;
		public int basePhysResistance, baseMagiResistance;

		// Names should be handled by NamesLib
	}
	
	public static class ClassUtil
	{
		private static Dictionary<string, CombatClass> playableClasses = new Dictionary<string, CombatClass>();
		private static Dictionary<string, CombatClass> enemyClasses = new Dictionary<string, CombatClass>();

		public static void LoadClass(CombatClass toLoad, bool playable = true)
		{
			if (playable)
			{
				playableClasses[toLoad.name] = toLoad;
			}
			else
			{
				enemyClasses[toLoad.name] = toLoad;
			}
			Debug.LogFormat("Loaded the {0} class.", toLoad.name);
		}

		public static CombatClass GetPlayableClass()
		{
			return playableClasses.Values.ToArray()[Random.Range(0, playableClasses.Count)];
		}
	}

	public static class RaceUtil
	{
		private static Dictionary<string, Race> playableRaces = new Dictionary<string, Race>();
		private static Dictionary<string, Race> enemyRaces = new Dictionary<string, Race>();
		private static List<Sprite> hairSprites = new List<Sprite>();

		public static void LoadRace(Race toLoad, bool playable = true)
		{
			if (playable)
			{
				playableRaces[toLoad.name] = toLoad;
			}
			else
			{
				enemyRaces[toLoad.name] = toLoad;
			}
			Debug.LogFormat("Loaded the {0} race.", toLoad.name);
		}

		public static void LoadHair()
		{
			if (hairSprites.Count > 0)
			{
				Debug.Log("Hair has already been loaded.");
				return;
			}

			Sprite[] hairAssets = Resources.LoadAll<Sprite>("Third-Party/Sprites/Hair");
			hairSprites.AddRange(hairAssets);
		}

		public static Sprite GetHair()
		{
			return hairSprites[Random.Range(0, hairSprites.Count)];
		}

		public static Race GetPlayableRace()
		{
			return playableRaces.Values.ToArray()[Random.Range(0, playableRaces.Count)];
		}
	}
}