using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerScript : MonoBehaviour
{
	public GameObject initialTileset;
	public List<GameObject> tilesets;
	public float baseHeight = 0, minHeight = -2, maxHeight = 2, tileHeight = 1;
	public int minGap = 1, maxGap = 3, spawnX = 10;
	public float initialSpeed, maxSpeed, secondsToMaxSpeed = 180;
	public float speed, acceleration;

	private float startTime = 0;
	private float lastHeight = 0;

	private void Start()
	{
		speed = initialSpeed;
		acceleration = (maxSpeed - initialSpeed) / secondsToMaxSpeed;
		GameObject newTile = Instantiate(initialTileset, new Vector3(-8, 0, 0), Quaternion.identity);
		newTile.GetComponent<TilesetScript>().Initialize(speed, maxSpeed, Time.time, acceleration);
	}

	private void FixedUpdate()
	{
		if (speed < maxSpeed)
		{
			speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, 0, maxSpeed);
		}
	}

	public void SpawnTileset()
	{
		int i = Random.Range(0, tilesets.Count);
		
		int gap = Random.Range(minGap, maxGap + 1);

		// Determine the height of the new tileset
		int heightSign = Random.Range(-1, 2);
		float dHeight = heightSign * tileHeight;
		float height = Mathf.Clamp(lastHeight + dHeight, minHeight, maxHeight);
		lastHeight = height;

		GameObject newTile = Instantiate(tilesets[i], new Vector3(spawnX + gap, height, 0), Quaternion.identity);
		newTile.GetComponent<TilesetScript>().Initialize(speed, maxSpeed, Time.time, acceleration);
		Debug.Log(string.Format("Spawned new tileset with gap of {0} and height of {1}.", gap, height));
	}
}
