using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterLib;

public class ProjectileScript : MonoBehaviour
{
	private SpriteRenderer spr;
	private Rigidbody2D rb;

	private Transform target, attacker;
	private int damage;
	private bool isMagical;
	private Attack baseAttack;
	private bool initialized = false;

	// Object persists for this long after hitting for melee attacks,
	// since otherwise the sprites are never visible.
	private const float meleeDisplayTime = 0.5f;
	private bool isMelee = false;

	public void Init(Transform target, Transform attacker, int damage, bool isMagical, bool isMelee, Attack baseAttack)
	{
		spr = GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody2D>();

		this.target = target;
		this.attacker = attacker;

		this.damage = damage;
		this.isMagical = isMagical;
		this.isMelee = isMelee;

		this.baseAttack = baseAttack;

		spr.sprite = baseAttack.projectileSprite;
		transform.localScale = Vector3.one * baseAttack.projectileSize;

		// Apply an impulse
		rb.SetRotation(TargetAngle());
		Vector2 direction = transform.forward;
		if (target != null)
		{
			direction = ((Vector2)target.transform.position - rb.position).normalized;
		}
		Vector2 force = direction * 100;
		rb.AddForce(force);

		initialized = true;
	}

	private void FixedUpdate()
	{
		if (!initialized)
		{
			return;
		}

		if (target == null)
		{
			Destroy(gameObject);
		}

		rb.SetRotation(TargetAngle());
		Vector2 direction = transform.forward;
		if (target != null)
		{
			direction = ((Vector2)target.transform.position - rb.position).normalized;
		}
		Vector2 force = baseAttack.projectileSpeed * Time.deltaTime * direction;

		rb.AddForce(force);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!initialized || collision.gameObject != target.gameObject)
		{
			return;
		}

		if (collision.CompareTag("Adventurer"))
		{
			AdventurerScript adventurer = collision.GetComponent<AdventurerScript>();

			if (damage < 0)
			{
				adventurer.BeHealed(-damage);
			}
			else
			{
				adventurer.TakeDamage(damage, isMagical);
			}
		}
		else if (collision.CompareTag("Enemy"))
		{
			EnemyScript enemy = collision.GetComponent<EnemyScript>();
			AdventurerScript attackingAdventurer = attacker.GetComponent<AdventurerScript>();
			enemy.TakeDamage(damage, isMagical, attackingAdventurer);
		}
		StartCoroutine(Clear());
	}

	private float TargetAngle()
	{
		Vector2 dirVector;
		if (target == null)
		{
			dirVector = transform.forward;
		}
		else
		{
			dirVector = ((Vector2)target.transform.position - rb.position).normalized;
		}
		float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg;
		return angle - 90;
	}

	private IEnumerator Clear()
	{
		initialized = false;

		if (isMelee)
		{
			yield return new WaitForSecondsRealtime(meleeDisplayTime);
		}

		Destroy(gameObject);
	}
}
