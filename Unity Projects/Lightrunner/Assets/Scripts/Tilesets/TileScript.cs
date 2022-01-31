using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
	private BoxCollider2D bc;
	private GameControllerScript gc;

	private void Start()
	{
		bc = GetComponent<BoxCollider2D>();
		gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		ContactPoint2D[] contacts = new ContactPoint2D[2];
		bc.GetContacts(contacts);
		foreach (ContactPoint2D contact in contacts)
		{
			// Skip if the contact is (0, 0)
			if (contact.normal == Vector2.zero)
			{
				continue;
			}

			//Debug.Log(contact.normal);
			float collisionAngle = Mathf.Atan2(contact.normal.y, contact.normal.x) * Mathf.Rad2Deg;
			//Debug.Log(Mathf.Rad2Deg * collisionAngle);

			// If the collision is between -10 and 10 degrees, that's a game over.
			if (-10 <= collisionAngle && collisionAngle <= 10)
			{
				gc.GameOver("you ran into an obstacle.");
			}
			// Otherwise, refresh the player's jump
			else
			{
				PlayerScript potentialPlayer = collision.gameObject.GetComponent<PlayerScript>();
				if (potentialPlayer != null)
				{
					potentialPlayer.RefreshJump();
				}
			}
		}
	}

	/// <summary>
	/// Set the material of this tile.
	/// </summary>
	/// <param name="mat">Material to be assigned.</param>
	public void SetMaterial(Material mat)
	{
		// Get sprite renderer and set material.
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sharedMaterial = mat;

		// Change the base colour of the sprite to a desaturated version of the material's emissive colour.
		Color colour = mat.GetColor("_Colour");
		float h;
		Color.RGBToHSV(colour, out h, out _, out _);
		colour = Color.HSVToRGB(h, 0.5f, 1);
		spriteRenderer.color = colour;
		//Debug.Log(string.Format("Colour set to {0}", colour));
	}
}
