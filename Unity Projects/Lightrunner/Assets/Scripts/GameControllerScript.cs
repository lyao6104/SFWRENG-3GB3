using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class MusicSettings
{
	public AudioClip menuMusic;
	public List<AudioClip> playlist;
}

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
	public Canvas mainMenu, gameUI, gameOverUI;
	public Canvas mmButtons, mmCredits;
	public TMP_Text timerLabel, gameOverReasonLabel, returnMenuLabel;

	public GameObject bgStarfield, bgCityscape;
	public float bgSpeedMult = 0.05f, bgCitySpeedMult = 0.75f;

	public MusicSettings musicSettings;

	private AudioSource audioSource;
	private SpriteRenderer srStarfield, srCityscape;

	private float startTime = 0;
	private int initTilesetCursor = 0;
	private float lastHeight = 0;
	private bool alreadyGameOver = false;
	private bool initialized;
	private bool viewingCredits = false;
	private float maxVolume = 1;

	private void Start()
	{
		srStarfield = bgStarfield.GetComponent<SpriteRenderer>();
		srCityscape = bgCityscape.GetComponent<SpriteRenderer>();

		audioSource = GetComponent<AudioSource>();
		audioSource.clip = musicSettings.menuMusic;
		audioSource.Play();
		maxVolume = audioSource.volume; // Avoids requiring another public field in the script.
	}

	private void FixedUpdate()
	{
		if (initialized)
		{
			if (speed < maxSpeed)
			{
				speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, 0, maxSpeed);

				srStarfield.material.SetFloat("_ScrollSpeed", speed * bgSpeedMult);
				srCityscape.material.SetFloat("_ScrollSpeed", speed * bgCitySpeedMult);
			}

			timerLabel.text = string.Format("Time Survived: {0:F2}s", Time.time - startTime);

			if (!audioSource.isPlaying)
			{
				PlayNextTrack();
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
		SpawnTileset();

		// Setup player
		Instantiate(playerPrefab, startPos, Quaternion.identity);


		startTime = Time.time;
		srStarfield.material.SetFloat("_StartTime", startTime);
		srCityscape.material.SetFloat("_StartTime", startTime);
		mainMenu.enabled = false;
		mmButtons.enabled = false;
		gameUI.enabled = true;
		initialized = true;
		PlayNextTrack();
	}

	/// <summary>
	/// Exits the game.
	/// </summary>
	public void Quit()
	{
		Application.Quit();
	}

	/// <summary>
	/// Toggles viewing of credits while on the main menu
	/// </summary>
	public void ToggleCredits()
	{
		if (!mainMenu.enabled)
		{
			return;
		}

		viewingCredits = !viewingCredits;
		mmButtons.enabled = !viewingCredits;
		mmCredits.enabled = viewingCredits;
	}

	/// <summary>
	/// Code runs when player dies or returns to the main menu.
	/// </summary>
	public void GameOver(string reason, bool immediately = false)
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
		if (immediately)
		{
			ToMainMenu();
		}
	}

	/// Exists because apparently Unity events can't have two parameters.
	public void ToMainMenuImmediately()
	{
		GameOver("you wanted to stop playing.", true);
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
		mmButtons.enabled = true;

		StartCoroutine(FadeToTrack(musicSettings.menuMusic, 3));
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

	private void PlayNextTrack()
	{
		int i = 0;
		do
		{
			i = Random.Range(0, musicSettings.playlist.Count);
		} while (musicSettings.playlist[i] == audioSource.clip);

		StartCoroutine(FadeToTrack(musicSettings.playlist[i], 3));
	}

	private IEnumerator FadeToTrack(AudioClip nextTrack, float duration)
	{
		float t = 1;
		
		// Fade out and stop.
		while (t > 0)
		{
			audioSource.volume = Mathf.Clamp01(Mathf.Lerp(0, maxVolume, t));
			t -= Time.deltaTime / duration;
			yield return new WaitForEndOfFrame();
		}
		audioSource.Stop();

		// Swap to next track and fade in.
		audioSource.clip = nextTrack;
		audioSource.Play();
		while (t < 1)
		{
			audioSource.volume = Mathf.Clamp01(Mathf.Lerp(0, maxVolume, t));
			t += Time.deltaTime / duration;
			yield return new WaitForEndOfFrame();
		}
	}

	public IEnumerator FadeInMenuLabel(float duration)
	{
		Color vertexColour = returnMenuLabel.color;

		returnMenuLabel.text = "Returning to menu...";
		float t = 0;
		do
		{
			vertexColour.a = Mathf.Lerp(0, 1, t);
			returnMenuLabel.color = vertexColour;
			t += Time.deltaTime / duration;
			yield return new WaitForEndOfFrame();
		} while (vertexColour.a < 1);

		returnMenuLabel.text = "Press ESC or Tap to Exit";
		ToMainMenuImmediately();
	}

	public void ResetMenuLabel()
	{
		returnMenuLabel.text = "Press ESC or Tap to Exit";
		Color baseColour = returnMenuLabel.color;
		baseColour.a = 1;
		returnMenuLabel.color = baseColour;
	}

	public bool PlayerCanAct()
	{
		return !alreadyGameOver && initialized;
	}
}
