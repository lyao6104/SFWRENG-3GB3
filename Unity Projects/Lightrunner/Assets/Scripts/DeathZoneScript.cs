using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZoneScript : MonoBehaviour
{
	private GameControllerScript gc;

	private void Start()
	{
		gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Debug.Log(string.Format("Collided with {0}", collision.name));

		if (collision.tag == "Player")
		{
			gc.GameOver("you fell off the platform.");
		}
	}
}
