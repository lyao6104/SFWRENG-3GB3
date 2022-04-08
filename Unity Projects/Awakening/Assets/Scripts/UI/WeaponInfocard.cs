using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ItemLib;
using TMPro;

public class WeaponInfocard : MonoBehaviour
{
	public TMP_Text nameLabel, typeLabel, dmgLabel;
	public Image iconImage;
	public Color equippedColour;

	private Weapon weapon;
	private CustomGameController gc;
	private static UnityEvent clickedEvent;

	public void Init(Weapon weapon)
	{
		gc = CustomGameController.GetController();
		this.weapon = weapon;
		if (clickedEvent == null)
		{
			clickedEvent = new UnityEvent();
		}
		clickedEvent.AddListener(OnClicked);

		nameLabel.text = this.weapon.name;
		typeLabel.text = this.weapon.type.ToString();
		dmgLabel.text = string.Format("{0} Damage", this.weapon.damage);
		iconImage.sprite = this.weapon.icon;

		// Run OnClicked just so the currently equipped weapon starts out darkened
		OnClicked();
	}

	// Runs on the infocard that was clicked
	public void ButtonClicked()
	{
		if (weapon == null || weapon.name.Length < 1)
		{
			return;
		}

		gc.EquipWeapon(weapon);
		clickedEvent.Invoke();
	}

	// Runs for every infocard when one is clicked
	private void OnClicked()
	{
		if (gc.IsEquipped(weapon))
		{
			GetComponent<Image>().color = equippedColour;
		}
		else
		{
			GetComponent<Image>().color = Color.white;
		}
	}
}
