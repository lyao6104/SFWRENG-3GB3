using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

	public GameObject mainUI, gameOverPanel;
	public GameObject partyCreationPanel, partyViewPanel;
	public GameObject skillPanel;

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
		return enemyCount > 0;
	}

	public void OnEnemySpawn()
	{
		enemyCount++;
	}

	public void OnEnemyDeath()
	{
		enemyCount = Mathf.Max(0, enemyCount - 1);
		if (!IsInCombat())
		{
			nextWaveButton.interactable = true;
			GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioControllerScript>().PlayNextTrack();
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
		GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioControllerScript>().PlayNextTrack();
	}

	public void LoseLife()
	{
		remainingLives--;
		if (remainingLives < 1)
		{
			mainUI.SetActive(false);
			gameOverPanel.SetActive(true);
		}
	}

	public void CreateParty()
	{
		partyCreationPanel.SetActive(true);
		partyViewPanel.SetActive(false);
	}

	public void DeployParty(PartyScript party)
	{
		if (!parties.Contains(party) || party.IsDeployed)
		{
			return;
		}

		partyCreationPanel.SetActive(false);
		partyViewPanel.SetActive(false);
		GetComponent<SelectorScript>().StartDeploying(party);
	}

	public bool IsPlayerControlled(Character character)
	{
		for (int i = 0; i < parties.Count; i++)
		{
			Character[] members = parties[i].GetCharacters();
			for (int j = 0; j < members.Length; j++)
			{
				if (members[j] == character)
				{
					return true;
				}
			}
		}
		return false;
	}

	public AdventurerScript GetAssociatedAdventurer(Character character)
	{
		for (int i = 0; i < parties.Count; i++)
		{
			AdventurerScript[] members = parties[i].GetAdventurers();
			for (int j = 0; j < members.Length; j++)
			{
				if (members[j].characterData == character)
				{
					return members[i];
				}
			}
		}
		return null;
	}

	public void TogglePartyView()
	{
		partyViewPanel.SetActive(!partyViewPanel.activeSelf);
	}

	public bool ToggleSkillView()
	{
		skillPanel.SetActive(!skillPanel.activeSelf);
		return skillPanel.activeSelf;
	}

	public void ToMainMenu()
	{
		SceneManager.LoadScene("MenuScene");
	}

	public static GameControllerScript GetController()
	{
		return GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
	}
}
