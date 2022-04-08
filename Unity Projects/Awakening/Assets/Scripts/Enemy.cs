using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;
using ItemLib;

public class Enemy : MonoBehaviour, IAttackable
{
	public GameObject projectilePrefab;

	public List<Vector2> patrolPoints;
	public float attackCooldown;
	public Weapon weapon;
	public ArmourType armourType;
	public int maxHealth = 10, speed;

	private int health;
	private float lastAttackTime = float.MinValue;

	private Transform target;
	private int patrolIndex = 0;
	private Rigidbody2D rb;
	private CustomGameController gc;
	private Slider healthBar;

	private const float pathfindingInterval = 2.5f;
	private const float nextWaypointDistance = 1f;
	private Path path;
	private Seeker seeker;
	private int curWaypoint = 0;

	private static int count = 0;
	private static int enemiesTargetingPlayer = 0;
	private static bool combatMusicPlaying = false;

	private void Start()
	{
		gc = CustomGameController.GetController();
		rb = GetComponent<Rigidbody2D>();
		seeker = GetComponent<Seeker>();
		healthBar = GetComponentInChildren<Slider>();

		health = maxHealth;
		healthBar.value = (float)health / maxHealth;
		InvokeRepeating(nameof(FindPath), 0, pathfindingInterval);
		count++;
	}

	private void FixedUpdate()
	{
		MoveUnit();

		Attack();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			target = collision.transform;
			enemiesTargetingPlayer++;

			if (!combatMusicPlaying)
			{
				AudioController ac = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioController>();
				ac.PlayNextTrack();
				combatMusicPlaying = true;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			target = null;
			enemiesTargetingPlayer--;

			Invoke(nameof(TryStopCombatMusic), 10);
		}
	}

	public void TakeDamage(Weapon attackingWeapon)
	{
		if (attackingWeapon == null)
		{
			return;
		}

		health -= ItemUtil.AdjustDamage(attackingWeapon, armourType);
		healthBar.value = (float)health / maxHealth;
		if (health < 1)
		{
			Clear();
		}
	}

	public void Clear()
	{
		count--;
		if (count < 1)
		{
			gc.TriggerGameOver(true);
		}

		CancelInvoke();
		Destroy(gameObject);
	}

	private void FindPath()
	{
		if (patrolPoints[patrolIndex] == null)
		{
			return;
		}

		if (path != null)
		{
			ClearPathfindingPenalties(path);
		}
		seeker.StartPath(rb.position, patrolPoints[patrolIndex], OnPathComplete);
	}

	private void OnPathComplete(Path p)
	{
		if (!p.error)
		{
			path = p;
			foreach (Vector2 nodePos in path.vectorPath)
			{
				var curNode = AstarPath.active.GetNearest(nodePos).node;
				curNode.Penalty += 1500;
			}
			curWaypoint = 0;
		}
	}

	private void ClearPathfindingPenalties(Path toClear)
	{
		if (toClear == null)
		{
			return;
		}

		foreach (Vector2 nodePos in toClear.vectorPath)
		{
			var curNode = AstarPath.active.GetNearest(nodePos).node;
			curNode.Penalty = 0;
		}
	}

	private void MoveUnit()
	{
		if (path == null || patrolPoints.Count < 1)
		{
			return;
		}

		if (curWaypoint >= path.vectorPath.Count)
		{
			patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
			return;
		}
		else
		{
			// Rotate to face target
			//rb.SetRotation(TargetAngle());
			if (rb.velocity.x >= 1e-6)
			{
				GetComponent<SpriteRenderer>().flipX = true;
			}
			else
			{
				GetComponent<SpriteRenderer>().flipX = false;
			}

			Vector2 direction = ((Vector2)path.vectorPath[curWaypoint] - rb.position).normalized;
			Vector2 force = speed * Time.deltaTime * direction;

			rb.AddForce(force);
		}

		float distance = ((Vector2)path.vectorPath[curWaypoint] - rb.position).magnitude;

		if (distance < nextWaypointDistance)
		{
			curWaypoint++;
		}
	}

	private void Attack()
	{
		if (target == null || Time.time - lastAttackTime < attackCooldown)
		{
			return;
		}

		lastAttackTime = Time.time;
		Projectile newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<Projectile>();
		newProjectile.Init(weapon, target);
	}

	public static void ResetCount()
	{
		count = 0;
		enemiesTargetingPlayer = 0;
		combatMusicPlaying = false;
	}

	public static int GetCount()
	{
		return count;
	}

	public static bool IsInCombat()
	{
		return enemiesTargetingPlayer > 0;
	}

	private void TryStopCombatMusic()
	{
		if (!IsInCombat())
		{
			AudioController ac = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioController>();
			ac.PlayNextTrack();
			combatMusicPlaying = false;
		}
	}
}
