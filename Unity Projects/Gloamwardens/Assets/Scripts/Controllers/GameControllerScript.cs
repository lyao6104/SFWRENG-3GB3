using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CharacterLib;
using TMPro;

public class GameControllerScript : MonoBehaviour
{
	public CameraScript cameraScript;
	public Collider2D cameraBounds;

	public Button nextWaveButton;
	private int enemyCount = 0;

	public int remainingLives = 20;
	public GameObject enemyTarget, projectilePrefab, adventurerPrefab;
	public List<EnemySpawnScript> enemySpawns;
	public List<PartyScript> parties;
	public List<Character> availableAdventurers;
	private const int advPoolCapacity = 2 * PartyScript.partySize;

	public GameObject partyCreationPanel;

	private void Start()
	{
		cameraScript.SetBounds(cameraBounds.bounds);

		for (int i = availableAdventurers.Count; i < advPoolCapacity; i++)
		{
			availableAdventurers.Add(CharUtil.GetPlayableCharacter());
		}
	}

	public bool IsInCombat()
	{
		return enemyCount < 1;
	}

	public void OnEnemySpawn()
	{
		enemyCount++;
	}

	public void OnEnemyDeath()
	{
		enemyCount = Mathf.Max(0, enemyCount - 1);
		if (enemyCount < 1)
		{
			nextWaveButton.interactable = true;
			for (int i = availableAdventurers.Count; i < advPoolCapacity; i++)
			{
				availableAdventurers.Add(CharUtil.GetPlayableCharacter());
			}
		}
	}

	public void NextWave()
	{
		nextWaveButton.interactable = false;

		foreach (EnemySpawnScript enemySpawn in enemySpawns)
		{
			enemySpawn.SpawnWave();
		}
	}

	public void LoseLife()
	{
		remainingLives--;
		if (remainingLives < 1)
		{
			// Game over
		}
	}

	public void CreateParty()
	{
		partyCreationPanel.SetActive(true);
	}

	public void TogglePartyView()
	{

	}

	public void ToMainMenu()
	{

	}

	public static GameControllerScript GetController()
	{
		return GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
	}
}
