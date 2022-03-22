using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseScript : MonoBehaviour
{
	private GameControllerScript gc;

	private void Start()
	{
		gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy"))
		{
			gc.LoseLife();
			collision.GetComponent<EnemyScript>().Clear();
		}
	}
}
