using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextTilesetZoneScript : MonoBehaviour
{
	private GameControllerScript gc;

	private void Start()
	{
		gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			Debug.Log("Spawning new tileset...");
			gc.SpawnTileset();
		}
	}
}
