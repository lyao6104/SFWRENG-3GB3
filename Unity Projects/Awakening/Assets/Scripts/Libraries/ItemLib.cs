using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGM.Core;
using RPGM.Gameplay;
using RPGM.UI;

namespace ItemLib
{
	[System.Serializable]
	public class Recipe
	{
		
		public Weapon product;
		// The RPG kit is weird so these actually need to be on the map somewhere
		public List<InventoryItem> materials;

		public bool Craft()
		{
			GameModel model = Schedule.GetModel<GameModel>();

			for (int i = 0; i < materials.Count; i++)
			{
				if (!model.HasInventoryItem(materials[i].name, materials[i].count))
				{
					return false;
				}
			}

			CustomGameController.GetController().AddWeapon(product);
			for (int i = 0; i < materials.Count; i++)
			{
				model.RemoveInventoryItem(materials[i], materials[i].count);
			}
			MessageBar.Show($"{product.name} acquired");
			return true;
		}
	}

	[System.Serializable]
	public enum DamageType { Blunt, Piercing, Magical }

	[System.Serializable]
	public enum ArmourType { Unarmoured, Armoured, Magical, Incorporeal }

	[System.Serializable]
	public class Weapon
	{
		public string name;
		public DamageType type;
		public int damage;
		public Sprite icon;
		public ProjectileData projectileData;
	}

	public static class ItemUtil
	{
		public static int AdjustDamage(Weapon weapon, ArmourType armour)
		{
			switch (weapon.type)
			{
				case DamageType.Blunt:
					switch (armour)
					{
						case ArmourType.Armoured:
							return weapon.damage / 2;
						case ArmourType.Incorporeal:
							return 0;
						default:
							break;
					}
					break;
				case DamageType.Piercing:
					switch (armour)
					{
						case ArmourType.Armoured:
							return weapon.damage * 2;
						case ArmourType.Incorporeal:
							return 0;
						default:
							break;
					}
					break;
				case DamageType.Magical:
					switch (armour)
					{
						case ArmourType.Magical:
							return weapon.damage / 2;
						case ArmourType.Incorporeal:
							return weapon.damage * 2;
						default:
							break;
					}
					break;
				default:
					break;
			}
			return weapon.damage;
		}
	}
}