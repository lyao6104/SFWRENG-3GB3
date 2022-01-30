using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleLightScript : MonoBehaviour
{
	public float lightAmount = 0.1f, bloomIntensity = 3;

	private void Start()
	{
		// Randomize colour at game start.
		Color baseColour = Color.HSVToRGB(Random.value, 1, 1);
		Material mat = GetComponent<SpriteRenderer>().material;
		mat.SetColor("_Colour", baseColour * bloomIntensity);
		GetComponent<SpriteRenderer>().sharedMaterial = mat;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			PlayerScript potentialPlayer = collision.GetComponent<PlayerScript>();
			if (potentialPlayer != null)
			{
				potentialPlayer.CollectLight(lightAmount);
				Destroy(gameObject);
			}
		}
	}
}
