using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemLib;

public class WeaponScreen : MonoBehaviour
{
	public Transform contentTransform;
	public GameObject infocardPrefab;

	private CustomGameController gc;

	private void OnEnable()
	{
		if (gc == null)
		{
			gc = CustomGameController.GetController();
		}

		List<Weapon> weapons = gc.GetWeapons();
		for (int i = 0; i < weapons.Count; i++)
		{
			WeaponInfocard newInfocard = Instantiate(infocardPrefab, contentTransform).GetComponent<WeaponInfocard>();
			newInfocard.Init(weapons[i]);
		}
	}

	private void OnDisable()
	{
		WeaponInfocard[] infocards = contentTransform.GetComponentsInChildren<WeaponInfocard>();
		for (int i = 0; i < infocards.Length; i++)
		{
			Destroy(infocards[i].gameObject);
		}
	}
}
