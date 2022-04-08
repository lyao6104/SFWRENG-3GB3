using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ItemLib;

public class Hero : MonoBehaviour, IAttackable
{
	public GameObject projectilePrefab, targetOverlayPrefab;

	public int maxHealth;
	public float attackCooldown = 5;
	public float healthRegenInterval = 2.5f;

	private float lastAttackTime = float.MinValue;
	private int health;
	private readonly ArmourType armourType = ArmourType.Unarmoured;
	
	private List<Enemy> potentialTargets = new List<Enemy>();
	private Enemy target;
	private int targetIndex = -1;
	private GameObject targetOverlay;

	private CustomGameController gc;
	private Slider healthBar;

	private void Start()
	{
		gc = CustomGameController.GetController();
		healthBar = GetComponentInChildren<Slider>();

		health = maxHealth;
		healthBar.value = (float)health / maxHealth;
		InvokeRepeating(nameof(HealthRegenTick), 0, healthRegenInterval);
	}

	private void Update()
	{
		if (gc.IsGameOver())
		{
			return;
		}

		if (Input.GetButtonDown("Target") && potentialTargets.Count > 0)
		{
			if (targetIndex < 0)
			{
				targetIndex = 0;
				target = potentialTargets[targetIndex];
			}
			else
			{
				int iDelta = (int)Mathf.Sign(Input.GetAxis("Target"));
				targetIndex = (targetIndex + iDelta) % potentialTargets.Count;
				target = potentialTargets[targetIndex];
			}
			if (targetOverlay != null)
			{
				Destroy(targetOverlay);
			}
			targetOverlay = Instantiate(targetOverlayPrefab, target.transform);
		}
		else if (Input.GetButtonDown("Attack") && CanAttack())
		{
			lastAttackTime = Time.time;
			Projectile newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<Projectile>();
			newProjectile.Init(gc.GetEquippedWeapon(), target.transform);
		}
	}

	private bool CanAttack()
	{
		Weapon weapon = gc.GetEquippedWeapon();
		return target != null && Time.time - lastAttackTime > attackCooldown && weapon != null && weapon.name.Length > 0;
	}

	private void HealthRegenTick()
	{
		health = Mathf.Clamp(health + 1, 0, maxHealth);
		healthBar.value = (float)health / maxHealth;
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
			gc.TriggerGameOver(false);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Enemy potentialTarget = collision.GetComponent<Enemy>();
		if (potentialTarget != null && !potentialTargets.Contains(potentialTarget))
		{
			potentialTargets.Add(potentialTarget);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		Enemy potentialTarget = collision.GetComponent<Enemy>();
		if (potentialTarget != null && potentialTargets.Contains(potentialTarget))
		{
			potentialTargets.Remove(potentialTarget);
			if (potentialTarget == target)
			{
				target = null;
				targetIndex = -1;
				Destroy(targetOverlay);
			}
			else
			{
				targetIndex = potentialTargets.FindIndex(x => x == target);
			}
		}
	}
}
