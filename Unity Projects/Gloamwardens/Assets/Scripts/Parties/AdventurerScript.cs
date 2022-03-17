using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterLib;
using ItemLib;

public class AdventurerScript : MonoBehaviour
{
	public Character characterData;

	// 0 -> Leggings, 1 -> Chestpiece, 2 -> Weapon, 3 -> Hair
	private GameObject[] graphicsObjects = new GameObject[4];

	private void Start()
	{
		characterData = new Character();
		name = characterData.name;
		characterData.hairSprite = RaceUtil.GetHair();
		characterData.weapon = ItemUtil.GetWeapon();
		characterData.torso = ItemUtil.GetArmour(ArmourClass.LightArmour, EquipmentSlot.Torso);
		characterData.legs = ItemUtil.GetArmour(ArmourClass.LightArmour, EquipmentSlot.Legs);

		GetComponent<SpriteRenderer>().sprite = characterData.bodySprite;
		SpawnGraphic(characterData.legs.sprite, 0);
		SpawnGraphic(characterData.torso.sprite, 1);
		SpawnGraphic(characterData.weapon.sprite, 2);
		SpawnGraphic(characterData.hairSprite, 3);
	}

	private void SpawnGraphic(Sprite sprite, int index)
	{
		if (graphicsObjects[index] == null)
		{
			graphicsObjects[index] = new GameObject(name + "'s Graphic");
			graphicsObjects[index].transform.SetParent(transform, false);
		}
		SpriteRenderer spr = graphicsObjects[index].GetComponent<SpriteRenderer>();
		if (spr == null)
		{
			spr = graphicsObjects[index].AddComponent<SpriteRenderer>();
			spr.sortingOrder = index + 1;
			spr.sortingLayerName = "Units";
		}

		spr.sprite = sprite;
	}
}
