using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerScript : MonoBehaviour
{
	public float speed, jumpForce, longJumpMultiplier;
	public float lightDecayRate = 0.05f, minLight = 0, maxLight = 1.5f, initialLight = 1, psLightThreshold = 1.25f;
	public float jumpCost = 0.1f;
	public InputAction moveAction;

	private GameControllerScript gc;
	private Rigidbody2D rb;
	private Light2D lightComponent;
	private ParticleSystem ps;
	private SpriteRenderer sr;

	private Vector2 movement;
	private bool jumpReady = true;
	private float curLight;

	private Color baseColour;
	private float baseColourIntensity = 4, minColourIntensity = 1, maxColourIntensity = 5;

	private void Start()
	{
		gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
		rb = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
		moveAction.Enable();

		// Randomize colour at game start.
		baseColour = Color.HSVToRGB(Random.value, 1, 1);
		Material mat = sr.material;
		mat.SetColor("_Colour", baseColour * GetColourIntensity(initialLight));
		sr.sharedMaterial = mat;

		// Light
		curLight = initialLight;
		ps = GetComponent<ParticleSystem>();
		lightComponent = GetComponent<Light2D>();
		ps.GetComponent<Renderer>().sharedMaterial = mat;
		ps.Stop();
	}

	private void Update()
	{
		curLight -= lightDecayRate * Time.deltaTime;

		// Update visuals to match curLight
		lightComponent.intensity = curLight;
		sr.sharedMaterial.SetColor("_Colour", baseColour * GetColourIntensity(curLight));

		// Enable/disable particles depending on curLight
		if (curLight >= psLightThreshold && ps.isStopped)
		{
			ps.Play();
		}
		else if (curLight < psLightThreshold && ps.isPlaying)
		{
			ps.Stop();
		}

		// Light-related game overs
		if (curLight <= minLight)
		{
			gc.GameOver("your light has been extinguished...");
		}
		else if (curLight >= maxLight)
		{
			gc.GameOver("the light inside you became too much to bear...");
		}
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
		if (!gc.initialized)
		{
			return;
		}

		if (context.started)
		{
			// Checks both jumpReady and vertical velocity to avoid some edge cases.
			if (jumpReady && Mathf.Abs(rb.velocity.y) < 1e-5)
			{
				rb.AddForce(Vector2.up * jumpForce);
				curLight -= jumpCost;
			}

			//Debug.Log(context.duration);
		}
		if (context.performed)
		{
			if (context.interaction is HoldInteraction && jumpReady)
			{
				rb.AddForce(Vector2.up * jumpForce * longJumpMultiplier);
				curLight -= jumpCost * longJumpMultiplier;
				Debug.Log("Player performed extended jump.");
			}

			jumpReady = false;
		}
	}

	/// <summary>
	/// Adds the given amount to the player's current light store.
	/// </summary>
	/// <param name="amount">How much light was collected.</param>
	public void CollectLight(float amount)
	{
		curLight += amount;
	}

	private float GetColourIntensity(float lightValue)
	{
		return Mathf.Clamp(baseColourIntensity * lightValue, minColourIntensity, maxColourIntensity);
	}
}
