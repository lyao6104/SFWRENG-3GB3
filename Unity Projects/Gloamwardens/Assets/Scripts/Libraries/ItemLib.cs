using System.Collections;
using System.Collections.Generic;
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
	}

	[System.Serializable]
	public class Weapon
	{
		public WeaponClass type;
		public int baseDamage, baseSpeed;
		public int manaCost;
		public bool isMagical, isTaunting;
	}

	[System.Serializable]
	public class Armour
	{
		public ArmourClass type;
		public int healthBonus, manaBonus;
		public int physicalArmour, magicalArmour;
	}
}