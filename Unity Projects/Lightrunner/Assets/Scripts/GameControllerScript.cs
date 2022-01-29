using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerScript : MonoBehaviour
{
	public GameObject initialTileset;
	public List<GameObject> tilesets;
	public float baseHeight = 0, minHeight = -2, maxHeight = 2, tileHeight = 1;
	public int minGap = 1, maxGap = 3;
	public float spawnX = 11.5f;
	public float initialSpeed, maxSpeed, secondsToMaxSpeed = 180;
	public float speed, acceleration;
	public GameObject playerPrefab;
	public Vector3 startPos;
	public bool initialized;
	public Canvas mainMenu;

	private float startTime = 0;
	private float lastHeight = 0;

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		if (initialized)
		{
			if (speed < maxSpeed)
			{
				speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, 0, maxSpeed);
			}
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
		GameObject newTile = Instantiate(initialTileset, new Vector3(-8, 0, 0), Quaternion.identity);
		TilesetScript tilesetScript = newTile.GetComponent<TilesetScript>();
		tilesetScript.Initialize(speed, maxSpeed, Time.time, acceleration);
		spawnX = tilesetScript.length / 2 + 1;

		// Setup player
		Instantiate(playerPrefab, startPos, Quaternion.identity);

		mainMenu.enabled = false;
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
	public void GameOver()
	{
		// TODO this should load up a menu where your stats are shown before returning to the main menu.

		initialized = false;

		GameObject[] activeTilesets = GameObject.FindGameObjectsWithTag("Tileset");
		GameObject player = GameObject.FindGameObjectWithTag("Player");

		Destroy(player);
		for(int i = 0; i < activeTilesets.Length; i++)
		{
			Destroy(activeTilesets[i]);
		}

		speed = initialSpeed;
		mainMenu.enabled = true;
	}

	/// <summary>
	/// Spawns a new tileset with a possible gap and variance in height.
	/// </summary>
	public void SpawnTileset()
	{
		int i;
		TilesetScript tilesetScript;
		do
		{
			i = Random.Range(0, tilesets.Count);
			tilesetScript = tilesets[i].GetComponent<TilesetScript>();
		} while (speed < tilesetScript.minSpeedToSpawn);
		
		int gap = Random.Range(minGap, maxGap + 1);

		// Determine the height of the new tileset
		int heightSign = Random.Range(-1, 2);
		float dHeight = heightSign * tileHeight;
		float height = Mathf.Clamp(lastHeight + dHeight, minHeight, maxHeight);
		lastHeight = height;

		GameObject newTile = Instantiate(tilesets[i], new Vector3(spawnX + gap, height, 0), Quaternion.identity);
		tilesetScript = newTile.GetComponent<TilesetScript>();
		tilesetScript.Initialize(speed, maxSpeed, Time.time, acceleration);
		spawnX = tilesetScript.length / 2 + 1;
		Debug.Log(string.Format("Spawned new tileset with gap of {0} and height of {1}.", gap, height));
	}
}
