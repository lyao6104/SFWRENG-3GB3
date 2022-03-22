using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemLib
{
	[System.Serializable]
	public enum EquipmentSlot
	{
		Weapon, Torso, Legs
	}

	[System.Serializable]
	public enum WeaponClass
	{
		LightStaff,
		DarkStaff,
		Bow,
		Crossbow,
		SwordAndShield,
		HammerAndShield,
		LightMelee,
		HeavyMelee
	}

	[System.Serializable]
	public enum ArmourClass
	{
		LightArmour, HeavyArmour
	}

	[System.Serializable]
	public class Equipment
	{
		public string name;
		public string description;
		public List<string> tags;
		public EquipmentSlot slot;
		public int rarity;
		public Sprite icon, sprite;
	}

	[System.Serializable]
	public class Weapon : Equipment
	{
		public WeaponClass type;
		public int baseDamage, baseSpeed;
		public float baseRange;
		public int manaCost;
		public bool isMagical, isTaunting;
	}

	[System.Serializable]
	public class Armour : Equipment
	{
		public ArmourClass type;
		public int healthBonus, manaBonus;
		public int physicalArmour, magicalArmour;
	}

	public static class ItemUtil
	{
		private static Dictionary<string, Weapon> loadedWeapons = new Dictionary<string, Weapon>();
		private static Dictionary<string, Armour> loadedArmour = new Dictionary<string, Armour>();

		public static void LoadWeapon(Weapon toLoad)
		{
			loadedWeapons[toLoad.name] = toLoad;
			Debug.LogFormat("Loaded weapon {0}.", toLoad.name);
		}

		public static void LoadArmour(Armour toLoad)
		{
			loadedArmour[toLoad.name] = toLoad;
			Debug.LogFormat("Loaded armour {0}.", toLoad.name);
		}

		public static Weapon GetWeapon(int minRarity = 0, int maxRarity = 2)
		{
			List<Weapon> validWeapons = loadedWeapons.Values.ToList()
				.FindAll(weapon => minRarity <= weapon.rarity && weapon.rarity <= maxRarity);
			return validWeapons[Random.Range(0, validWeapons.Count)];
		}

		public static Weapon GetWeapon(WeaponClass[] types, int minRarity = 0, int maxRarity = 2)
		{
			List<Weapon> validWeapons = loadedWeapons.Values.ToList()
				.FindAll(weapon => minRarity <= weapon.rarity && weapon.rarity <= maxRarity && types.Contains(weapon.type));
			return validWeapons[Random.Range(0, validWeapons.Count)];
		}

		public static Armour GetArmour(int minRarity = 0, int maxRarity = 2)
		{
			List<Armour> validArmour = loadedArmour.Values.ToList()
				.FindAll(armour => minRarity <= armour.rarity && armour.rarity <= maxRarity);
			return validArmour[Random.Range(0, validArmour.Count)];
		}

		public static Armour GetArmour(ArmourClass[] types, EquipmentSlot slot, int minRarity = 0, int maxRarity = 2)
		{
			if (slot == EquipmentSlot.Weapon)
			{
				Debug.LogError("Tried to get weapon armour");
				slot = EquipmentSlot.Torso;
			}

			List<Armour> validArmour = loadedArmour.Values.ToList()
				.FindAll(armour => minRarity <= armour.rarity && armour.rarity <= maxRarity &&
					types.Contains(armour.type) && armour.slot == slot);
			return validArmour[Random.Range(0, validArmour.Count)];
		}

		public static Equipment GetEquipment(int minRarity = 0, int maxRarity = 2)
		{
			int roll = Random.Range(0, loadedWeapons.Count + loadedArmour.Count);
			if (roll > loadedWeapons.Count)
			{
				return GetArmour(minRarity, maxRarity);
			}
			else
			{
				return GetWeapon(minRarity, maxRarity);
			}
		}

		public static EquipmentSlot StringToES(string s)
		{
			return s.ToLower() switch
			{
				"weapon" => EquipmentSlot.Weapon,
				"torso" => EquipmentSlot.Torso,
				"legs" => EquipmentSlot.Legs,
				_ => throw new UnityException("Cannot convert string to equipment slot."),
			};
		}

		public static ArmourClass StringToAC(string s)
		{
			switch (s.ToLower())
			{
				case "light":
				case "lightarmour":
					return ArmourClass.LightArmour;
				case "heavy":
				case "heavyarmour":
					return ArmourClass.HeavyArmour;
				default:
					throw new UnityException("Cannot convert string to armour class.");
			}
		}

		public static WeaponClass StringToWC(string s)
		{
			return s.ToLower() switch
			{
				"lightstaff" => WeaponClass.LightStaff,
				"darkstaff" => WeaponClass.DarkStaff,
				"bow" => WeaponClass.Bow,
				"crossbow" => WeaponClass.Crossbow,
				"swordandshield" => WeaponClass.SwordAndShield,
				"hammerandshield" => WeaponClass.HammerAndShield,
				"lightmelee" => WeaponClass.LightMelee,
				"heavymelee" => WeaponClass.HeavyMelee,
				_ => throw new UnityException("Cannot convert string to weapon class."),
			};
		}
	}
}