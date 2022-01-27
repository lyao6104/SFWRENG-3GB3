using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesetScript : MonoBehaviour
{
	// Speed is in units per second.
	public float speed = 2;
	private float startTime, maxSpeed, acceleration;
	// Deletion zones with colliders appear to require rigidbodies,
	// so old tilesets will be cleaned up like this instead.
	private float lifespan = 20;

	public void Initialize(float speed, float maxSpeed, float startTime, float acceleration)
	{
		this.speed = speed;
		this.maxSpeed = maxSpeed;
		this.startTime = startTime;
		this.acceleration = acceleration;
	}

	private void FixedUpdate()
	{
		transform.position += Vector3.left * speed * Time.deltaTime;

		if (speed < maxSpeed)
		{
			speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, 0, maxSpeed);
		}

		if (Time.time - startTime > lifespan)
		{
			Destroy(gameObject);
		}
	}
}
