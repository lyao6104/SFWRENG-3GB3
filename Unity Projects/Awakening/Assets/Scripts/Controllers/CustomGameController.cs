using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ItemLib;
using RPGM.UI;
using TMPro;

public class CustomGameController : MonoBehaviour
{
	public float itemGatherTime = 5f;
	private bool gameOver = false;
	private Weapon equippedWeapon;
	private List<Weapon> availableWeapons = new List<Weapon>();

	public GameObject equipmentPanel, craftingPanel, gameOverPanel;
	public TMP_Text gameOverTitle, gameOverDesc;

	private static CustomGameController instance;

	private void Start()
	{
		instance = this;

		Enemy.ResetCount();
	}

	private void Update()
	{
		if (!gameOver)
		{
			if (Input.GetButtonDown("Toggle Equipment"))
			{
				craftingPanel.SetActive(false);
				equipmentPanel.SetActive(!equipmentPanel.activeSelf);
			}
			else if (Input.GetButtonDown("Toggle Crafting"))
			{
				equipmentPanel.SetActive(false);
				craftingPanel.SetActive(!craftingPanel.activeSelf);
			}
		}
		if (Input.GetButtonDown("Cancel"))
		{
			ToMainMenu();
		}
	}

	public static CustomGameController GetController()
	{
		if (instance == null)
		{
			instance = GameObject.FindGameObjectWithTag("GameController").GetComponent<CustomGameController>();
		}
		return instance;
	}

	public bool IsEquipped(Weapon weapon)
	{
		return equippedWeapon == weapon;
	}

	public bool EquipWeapon(Weapon toEquip)
	{
		if (HasWeapon(toEquip))
		{
			equippedWeapon = toEquip;
		}
		Debug.Log($"Equipped a {equippedWeapon.name}");
		return HasWeapon(toEquip);
		
	}

	public bool HasWeapon(Weapon weapon)
	{
		for (int i = 0; i < availableWeapons.Count; i++)
		{
			if (availableWeapons[i] == weapon)
			{
				equippedWeapon = weapon;
				return true;
			}
		}
		return false;
	}

	public void AddWeapon(Weapon weapon)
	{
		availableWeapons.Add(weapon);
	}

	public Weapon GetEquippedWeapon()
	{
		return equippedWeapon;
	}

	public List<Weapon> GetWeapons()
	{
		return new List<Weapon>(availableWeapons);
	}

	public void TriggerGameOver(bool victory)
	{
		if (gameOver)
		{
			return;
		}

		if (victory)
		{
			gameOverTitle.text = "Victory!";
			gameOverDesc.text = "All the enemies in this cave have been defeated, and the hero returns to the surface.\n\nWhat has changed in their absence...?";
		}
		else
		{
			gameOverTitle.text = "Game Over!";
			gameOverDesc.text = string.Format("Your hero has been defeated, and this cave becomes their tomb...\n\nThe game ended with {0} enemies remaining.",
				Enemy.GetCount());
		}
		gameOverPanel.SetActive(true);
		gameOver = true;
	}

	public void ToMainMenu()
	{
		SceneManager.LoadScene("MenuScene");
	}

	public bool IsGameOver()
	{
		return gameOver;
	}
}
