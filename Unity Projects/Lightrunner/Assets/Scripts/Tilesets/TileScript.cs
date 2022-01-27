using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
	private BoxCollider2D bc;

	private void Start()
	{
		bc = GetComponent<BoxCollider2D>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		ContactPoint2D[] contacts = new ContactPoint2D[2];
		bc.GetContacts(contacts);
		foreach (ContactPoint2D contact in contacts)
		{
			//Debug.Log(contact.normal);
			float collisionAngle = Mathf.Atan2(contact.normal.y, contact.normal.x) * Mathf.Rad2Deg;
			//Debug.Log(Mathf.Rad2Deg * collisionAngle);

			// Refresh the player's jump if they've landed on top of a tile
			// (i.e. collision angle is less than -45 degrees).
			if (collisionAngle <= -45)
			{
				PlayerScript potentialPlayer = collision.gameObject.GetComponent<PlayerScript>();
				if (potentialPlayer != null)
				{
					potentialPlayer.RefreshJump();
				}
			}
		}
	}
}
