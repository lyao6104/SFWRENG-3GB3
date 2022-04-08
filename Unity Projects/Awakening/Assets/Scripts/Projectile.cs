using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemLib;

[System.Serializable]
public struct ProjectileData
{
	public Sprite sprite;
	public float speed, impulse;
	public float lifespan;
}

public class Projectile : MonoBehaviour
{
	private Weapon attackData;
	private Transform targetTransform;
	private bool initialized = false;
	private float timeInitialized = 0;
	private float previousAngle = 0;
	private const float directionCheckInterval = 0.5f;

	private SpriteRenderer spr;
	private Rigidbody2D rb;

	public void Init(Weapon attackingWeapon, Transform target)
	{
		spr = GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody2D>();

		targetTransform = target;
		attackData = attackingWeapon;

		spr.sprite = attackData.projectileData.sprite;

		// Apply an impulse, offset slightly to account for target velocity
		rb.SetRotation(TargetAngle());
		Vector2 direction = transform.up;
		if (target != null)
		{
			float distance = Vector2.Distance(targetTransform.position, rb.position);
			float predictedTime = Mathf.Clamp(distance / attackData.projectileData.speed, 0.25f, 2f);
			Vector2 predictedLocation = (Vector2)targetTransform.position +
				targetTransform.GetComponent<Rigidbody2D>().velocity * predictedTime;

			direction = (predictedLocation - rb.position).normalized;
		}
		Vector2 force = direction * attackData.projectileData.impulse;
		rb.AddForce(force);

		previousAngle = rb.rotation;
		InvokeRepeating(nameof(ValidateDirection), 0, directionCheckInterval);

		timeInitialized = Time.time;
		initialized = true;
	}

	private void FixedUpdate()
	{
		if (!initialized)
		{
			return;
		}

		if (targetTransform == null || Time.time - timeInitialized > attackData.projectileData.lifespan)
		{
			Clear();
		}

		rb.SetRotation(TargetAngle());
		Vector2 direction = transform.up;
		if (targetTransform != null)
		{
			direction = ((Vector2)targetTransform.transform.position - rb.position).normalized;
		}
		Vector2 force = attackData.projectileData.speed * Time.deltaTime * direction;

		rb.AddForce(force);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!initialized || collision.gameObject != targetTransform.gameObject)
		{
			return;
		}

		IAttackable target = collision.GetComponent<Enemy>();
		if (target == null)
		{
			target = collision.GetComponent<Hero>();
		}
		if (target == null)
		{
			return;
		}

		target.TakeDamage(attackData);
		Clear();
	}

	private float TargetAngle()
	{
		Vector2 dirVector;
		if (targetTransform == null)
		{
			dirVector = transform.up;
		}
		else
		{
			dirVector = ((Vector2)targetTransform.transform.position - rb.position).normalized;
		}
		float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg;
		return angle - 90;
	}

	private void ValidateDirection()
	{
		if (Mathf.DeltaAngle(rb.rotation, previousAngle) > 50)
		{
			Clear();
		}
		else
		{
			previousAngle = rb.rotation;
		}
	}

	private void Clear()
	{
		CancelInvoke();
		Destroy(gameObject);
	}
}
