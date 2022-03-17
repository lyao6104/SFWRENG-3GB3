using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ItemLib;

public class ItemLoaderScript : MonoBehaviour
{
	[System.Serializable]
	private class WeaponJSON
	{
		public string name;
		public string description;
		public List<string> tags;
		public string slot;
		public int rarity;
		public string iconPath, spritePath;

		public string type;
		public int baseDamage, baseSpeed, baseRange;
		public int manaCost;
		public bool isMagical, isTaunting;

		public Weapon Build()
		{
			return new Weapon()
			{
				name = name,
				description = description,
				tags = tags,
				slot = ItemUtil.StringToES(slot),
				rarity = rarity,
				icon = Resources.Load<Sprite>(iconPath),
				sprite = Resources.Load<Sprite>(spritePath),
				type = ItemUtil.StringToWC(type),
				baseDamage = baseDamage,
				baseSpeed = baseSpeed,
				baseRange = baseRange,
				manaCost = manaCost,
				isMagical = isMagical,
				isTaunting = isTaunting
			};
		}
	}

	[System.Serializable]
	private class WeaponJSONList
	{
		public List<WeaponJSON> weapons;
	}

	[System.Serializable]
	private class ArmourJSON
	{
		public string name;
		public string description;
		public List<string> tags;
		public string slot;
		public int rarity;
		public string iconPath, spritePath;

		public string type;
		public int healthBonus, manaBonus;
		public int physicalArmour, magicalArmour;

		public Armour Build()
		{
			return new Armour()
			{
				name = name,
				description = description,
				tags = tags,
				slot = ItemUtil.StringToES(slot),
				rarity = rarity,
				icon = Resources.Load<Sprite>(iconPath),
				sprite = Resources.Load<Sprite>(spritePath),
				type = ItemUtil.StringToAC(type),
				healthBonus = healthBonus,
				manaBonus = manaBonus,
				physicalArmour = physicalArmour,
				magicalArmour = magicalArmour
			};
		}
	}

	[System.Serializable]
	private class ArmourJSONList
	{
		public List<ArmourJSON> armour;
	}

	private static bool initialized = false;

	private void Start()
	{
		if (initialized)
		{
			return;
		}
		TextAsset weaponsFile = Resources.Load<TextAsset>("Items/Weapons");
		TextAsset armourFile = Resources.Load<TextAsset>("Items/Armour");
		WeaponJSONList weaponJSONs = JsonUtility.FromJson<WeaponJSONList>(weaponsFile.text);
		ArmourJSONList armourJSONs = JsonUtility.FromJson<ArmourJSONList>(armourFile.text);
		Debug.Log("Loaded item files");
		foreach (ArmourJSON armourJSON in armourJSONs.armour)
		{
			ItemUtil.LoadArmour(armourJSON.Build());
		}
		foreach (WeaponJSON weaponJSON in weaponJSONs.weapons)
		{
			ItemUtil.LoadWeapon(weaponJSON.Build());
		}

		initialized = true;
	}
}
