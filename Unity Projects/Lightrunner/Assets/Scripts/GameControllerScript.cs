using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameControllerScript : MonoBehaviour
{
	public List<GameObject> initialTilesets;
	public List<GameObject> tilesets;
	public float baseHeight = 0, minHeight = -2, maxHeight = 2, tileHeight = 1;
	public int minGap = 1, maxGap = 3;
	public float spawnX = 11.5f, startingSpawnX = -8;
	public float initialSpeed, maxSpeed, secondsToMaxSpeed = 180;
	public float speed, acceleration;
	public GameObject playerPrefab;
	public Vector3 startPos;
	public bool initialized;
	public Canvas mainMenu, gameUI, gameOverUI;
	public TMP_Text timerLabel, gameOverReasonLabel;

	public GameObject bgStarfield, bgCityscape;
	public float bgSpeedMult = 0.05f, bgCitySpeedMult = 0.75f;

	private SpriteRenderer srStarfield, srCityscape;

	private float startTime = 0;
	private int initTilesetCursor = 0;
	private float lastHeight = 0;
	private bool alreadyGameOver = false;

	private void Start()
	{
		srStarfield = bgStarfield.GetComponent<SpriteRenderer>();
		srCityscape = bgCityscape.GetComponent<SpriteRenderer>();
	}

	private void FixedUpdate()
	{
		if (initialized)
		{
			if (speed < maxSpeed)
			{
				speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, 0, maxSpeed);

				srStarfield.material.SetFloat("_ScrollSpeed", speed * bgSpeedMult);
				srCityscape.material.SetFloat("_ScrollSpeed", speed * bgSpeedMult * bgCitySpeedMult);
			}

			timerLabel.text = string.Format("Time Survived: {0:F2}s", Time.time - startTime);
		}
	}

	/// <summary>
	/// Code runs on game start.
	/// </summary>
	public void Play()
	{
		// Setup tiles
		speed = initialSpeed;
		acceleration = (maxSpeed - initialSpeed) / secondsToMaxSpeed;
		//GameObject newTile = Instantiate(initialTilesets[0], new Vector3(-8, 0, 0), Quaternion.identity);
		//TilesetScript tilesetScript = newTile.GetComponent<TilesetScript>();
		//tilesetScript.Initialize(speed, maxSpeed, Time.time, acceleration);
		//spawnX = tilesetScript.length / 2 + 1;
		//initialTilesets.RemoveAt(0);
		SpawnTileset();

		// Setup player
		Instantiate(playerPrefab, startPos, Quaternion.identity);

		startTime = Time.time;
		mainMenu.enabled = false;
		gameUI.enabled = true;
		initialized = true;
	}

	/// <summary>
	/// Exits the game.
	/// </summary>
	public void Quit()
	{
		Application.Quit();
	}	
	
	/// <summary>
	/// Code runs when player dies or returns to the main menu.
	/// </summary>
	public void GameOver(string reason)
	{
		if (alreadyGameOver)
		{
			return;
		}
		else
		{
			alreadyGameOver = true;
		}

		gameUI.enabled = false;
		gameOverUI.enabled = true;
		gameOverReasonLabel.text = string.Format("You survived for {0:F2} seconds, but {1}", Time.time - startTime, reason);
	}

	/// <summary>
	/// Should be called after GameOver when the player returns to the main menu.
	/// </summary>
	public void ToMainMenu()
	{
		gameOverUI.enabled = false;

		initialized = false;
		alreadyGameOver = false;

		// Delete player and any active tiles
		GameObject[] activeTilesets = GameObject.FindGameObjectsWithTag("Tileset");
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		Destroy(player);
		for (int i = 0; i < activeTilesets.Length; i++)
		{
			Destroy(activeTilesets[i]);
		}

		// Reset background
		srStarfield.material.SetFloat("_ScrollSpeed", 0);
		srCityscape.material.SetFloat("_ScrollSpeed", 0);

		// Reset some values
		speed = initialSpeed;
		initTilesetCursor = 0;
		spawnX = startingSpawnX;
		lastHeight = 0;
		mainMenu.enabled = true;
	}

	/// <summary>
	/// Spawns a new tileset with a possible gap and variance in height.
	/// </summary>
	public void SpawnTileset()
	{
		int i;
		GameObject setPrefab;
		TilesetScript tilesetScript;
		int gap = 0;
		float height = 0;
		if (initTilesetCursor >= initialTilesets.Count)
		{
			do
			{
				i = Random.Range(0, tilesets.Count);
				tilesetScript = tilesets[i].GetComponent<TilesetScript>();
			} while (speed < tilesetScript.minSpeedToSpawn);
			setPrefab = tilesets[i];

			// Determine the gap and height of the new tileset
			gap = Random.Range(minGap, maxGap + 1);

			int heightSign = Random.Range(-1, 2);
			float dHeight = heightSign * tileHeight;
			height = Mathf.Clamp(lastHeight + dHeight, minHeight, maxHeight);
		}
		else
		{
			setPrefab = initialTilesets[initTilesetCursor];
			initTilesetCursor++;
		}
		lastHeight = height;

		GameObject newTile = Instantiate(setPrefab, new Vector3(spawnX + gap, height, 0), Quaternion.identity);
		tilesetScript = newTile.GetComponent<TilesetScript>();
		tilesetScript.Initialize(speed, maxSpeed, Time.time, acceleration);
		spawnX = tilesetScript.length / 2 + 1;
		Debug.Log(string.Format("Spawned new tileset with gap of {0} and height of {1}.", gap, height));
	}
}
