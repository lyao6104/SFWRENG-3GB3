using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ItemLib;
using NamesLib;

namespace CharacterLib
{
	[System.Serializable]
	public enum TargetingCriteria { Closest, MostThreat, MostHealthy, LeastHealthy };

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

		public void LevelUp(bool force = false)
		{
			if (!combatClass.LevelUp(force))
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

		// Currently just returns weapon range.
		public float GetCharacterRange()
		{
			return weapon.baseRange;
		}

		// Currently just returns weapon damage.
		public int GetCharacterDamage()
		{
			return weapon.baseDamage;
		}

		public int GetCharacterPhysicalResistance()
		{
			int skillResistance = combatClass.unlockedSkills.Aggregate(0, (a, b) => a + b.physRBonus);
			return basePhysResistance + torso.physicalArmour + legs.physicalArmour + skillResistance;
		}

		public int GetCharacterMagicalResistance()
		{
			int skillResistance = combatClass.unlockedSkills.Aggregate(0, (a, b) => a + b.magiRBonus);
			return baseMagiResistance + torso.magicalArmour + legs.magicalArmour + skillResistance;
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

		public void GainEXP(int amount)
		{
			curEXP += amount;
			if (curEXP > levelUpEXP)
			{
				LevelUp();
			}
		}

		public bool LevelUp(bool force = false)
		{
			if (curEXP < levelUpEXP && !force)
			{
				Debug.LogWarning("Attempted to level up without required EXP.");
				return false;
			}

			curEXP = Mathf.Max(0, curEXP - levelUpEXP);
			levelUpEXP = Mathf.RoundToInt(levelUpEXP * levelUpEXPMultiplier);
			level++;
			freeSkillPoints++;
			return true;
		}

		// Skill point management should be done elsewhere.
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
				attacks.AddRange(toUnlock.newAttacks);
				return true;
			}
		}

		// Unlocks all skills for the current level.
		// Mainly used by enemies.
		public void UnlockCurrentSkills()
		{
			for (int i = 0; i < potentialSkills[level - 1].Count; i++)
			{
				UnlockSkill(level, potentialSkills[level - 1][i].name);
			}
		}

		public CombatClass GetCopy()
		{
			return new CombatClass()
			{
				name = name,
				level = level,
				curEXP = curEXP,
				levelUpEXP = levelUpEXP,
				levelUpEXPMultiplier = levelUpEXPMultiplier,
				icon = icon,
				healthIncPerLevel = healthIncPerLevel,
				manaIncPerLevel = manaIncPerLevel,
				freeSkillPoints = freeSkillPoints,
				unlockedSkills = new List<CombatSkill>(unlockedSkills),
				potentialSkills = potentialSkills,
				attacks = attacks.Select(attack => attack.GetCopy()).ToList(),
				allowedArmour = allowedArmour,
				allowedWeapons = allowedWeapons
			};
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
		public float rangeBonus;
		public float damageMultiplier = 1;
		public float manaCostMultiplier = 1;
		public bool isMagical = false;
		// Cooldown is how long before THIS attack is usable again,
		// duration is how long before OTHER attacks can be made.
		public float cooldown, duration;
		private float lastUsed = int.MinValue;

		public Sprite projectileSprite;
		public float projectileSize;
		public float projectileSpeed;

		public bool CanAttack()
		{
			//Debug.LogFormat("Last used: {0}, Cooldown: {1}", lastUsed, cooldown);
			return Time.time - lastUsed >= cooldown;
		}

		/// <summary>
		/// Assuming this attack isn't on cooldown, updates cooldown values
		/// to "signal" that the attack has been made.
		/// </summary>
		/// <returns>Time at which further attacks can be made after this attack's duration elapses.</returns>
		public float SignalAttack()
		{
			lastUsed = Time.time;
			return Time.time + duration;
		}

		public Attack GetCopy()
		{
			return new Attack()
			{
				name = name,
				rangeBonus = rangeBonus,
				damageMultiplier = damageMultiplier,
				manaCostMultiplier = manaCostMultiplier,
				isMagical = isMagical,
				cooldown = cooldown,
				duration = duration,
				lastUsed = int.MinValue,
				projectileSprite = projectileSprite,
				projectileSize = projectileSize,
				projectileSpeed = projectileSpeed
			};
		}
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
			return playableClasses.Values.ToArray()[Random.Range(0, playableClasses.Count)].GetCopy();
		}

		public static CombatClass GetEnemyClass(string name)
		{
			if (enemyClasses.ContainsKey(name))
			{
				return enemyClasses[name].GetCopy();
			}
			else
			{
				return null;
			}
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

		public static Race GetEnemyRace()
		{
			return enemyRaces.Values.ToArray()[Random.Range(0, enemyRaces.Count)];
		}

		public static Race GetEnemyRace(string name)
		{
			if (enemyRaces.ContainsKey(name))
			{
				return enemyRaces[name];
			}
			else
			{
				return null;
			}
		}
	}

	public static class CharUtil
	{
		public static Character GetPlayableCharacter()
		{
			Character newChar = new Character
			{
				hairSprite = RaceUtil.GetHair()
			};
			newChar.weapon = ItemUtil.GetWeapon(newChar.combatClass.allowedWeapons.ToArray(), 0, 0);
			newChar.torso = ItemUtil.GetArmour(newChar.combatClass.allowedArmour.ToArray(), EquipmentSlot.Torso, 0, 0);
			newChar.legs = ItemUtil.GetArmour(newChar.combatClass.allowedArmour.ToArray(), EquipmentSlot.Legs, 0, 0);

			return newChar;
		}
	}
}