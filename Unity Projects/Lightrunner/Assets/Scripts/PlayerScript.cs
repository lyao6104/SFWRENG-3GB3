using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerScript : MonoBehaviour
{
	public float speed, jumpForce, longJumpMultiplier;
	public InputAction moveAction;

	private Rigidbody2D rb;
	private Vector2 movement;
	private bool jumpReady = true;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		moveAction.Enable();

		// Randomize colour at game start.
		Color colour = Color.HSVToRGB(Random.value, 1, 1) * 4;
		Material mat = GetComponent<SpriteRenderer>().material;
		mat.SetColor("_Colour", colour);
		GetComponent<SpriteRenderer>().sharedMaterial = mat;
	}

	private void FixedUpdate()
	{
		movement = moveAction.ReadValue<Vector2>();
		rb.AddForce(speed * movement);
	}

	/// <summary>
	/// Called by TileScript to allow players to jump again.
	/// </summary>
	public void RefreshJump()
	{
		jumpReady = true;
	}

	/// <summary>
	/// Makes the player jump upwards. Players can jump if they're on the ground.
	/// The jump height will be increased if the button is held down rather than tapped.
	/// </summary>
	public void Jump(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			// Checks both jumpReady and vertical velocity to avoid some edge cases.
			if (jumpReady && Mathf.Abs(rb.velocity.y) < 1e-5)
			{
				rb.AddForce(Vector2.up * jumpForce);
			}

			//Debug.Log(context.duration);
		}
		if (context.performed)
		{
			if (context.interaction is HoldInteraction && jumpReady)
			{
				rb.AddForce(Vector2.up * jumpForce * longJumpMultiplier);
				Debug.Log("Player performed extended jump.");
			}

			jumpReady = false;
		}
	}
}
