using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CharacterLib;
using Pathfinding;

public class EnemyScript : MonoBehaviour
{
    public EnemyArchetypeSO archetype;

	public float manaRegenTime = 1;

	public Character characterData;
	private GameControllerScript gc;
	private Rigidbody2D rb;
	private Transform targetTransform;
	private List<AdventurerScript> potentialTargets = new List<AdventurerScript>();

	private GameObject weaponObj;

	private const float pathfindingInterval = 2.5f;
	private const float targetingInterval = 2.5f;
	private const float nextWaypointDistance = 1f;
	private Path path;
	private Seeker seeker;
	private int curWaypoint = 0;

	private float nextAttackTime = 0;
	//private bool isAttacking = false;

	private void Start()
	{
		gc = GameControllerScript.GetController();
		rb = GetComponent<Rigidbody2D>();
		seeker = GetComponent<Seeker>();
	}

	private void FixedUpdate()
	{
		MoveUnit();

		Attack();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Adventurer"))
		{
			potentialTargets.Add(collision.GetComponent<AdventurerScript>());
			FindTarget();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Adventurer"))
		{
			potentialTargets.Remove(collision.GetComponent<AdventurerScript>());
			FindTarget();
		}
	}

	public void Spawn(EnemyArchetypeSO archetype, int bonusLevels)
	{
		gc = GameControllerScript.GetController();

		this.archetype = archetype;
		characterData = archetype.Build(bonusLevels);

		name = characterData.name;
		GetComponent<SpriteRenderer>().sprite = characterData.bodySprite;

		if (characterData.weapon.sprite != null)
		{
			SpawnGraphic(characterData.weapon.sprite);
		}

		InvokeRepeating(nameof(FindTarget), 0, targetingInterval);
		InvokeRepeating(nameof(FindPath), 0, pathfindingInterval);
		InvokeRepeating(nameof(RegenMana), 0, manaRegenTime);

		gc.OnEnemySpawn();
	}

	private void Kill()
	{
		Clear();
	}

	/// <summary>
	/// Clears the unit immediately.
	/// </summary>
	public void Clear()
	{
		gc.OnEnemyDeath();

		CancelInvoke();
		Destroy(gameObject);
	}

	public void TakeDamage(int baseDamage, bool isMagical, AdventurerScript damager)
	{
		int resistance;
		if (isMagical)
		{
			resistance = characterData.GetCharacterMagicalResistance();
		}
		else
		{
			resistance = characterData.GetCharacterPhysicalResistance();
		}
		// Each point of resistance decreases damage by 0.5, with a minimum damage of 1 regardless.
		// Total damage taken is rounded up.
		int damageTaken = Mathf.Clamp(Mathf.CeilToInt(baseDamage - resistance * 0.5f), 1, baseDamage);
		characterData.health = Mathf.Clamp(characterData.health - damageTaken, 0, characterData.GetCharacterMaxHealth());

		if (characterData.health <= 0)
		{
			// Drop loot, etc.
			damager.GainEXP(archetype.killEXPPerLevel * characterData.combatClass.level);

			Kill();
		}
	}

	public int GetThreat()
	{
		const float vitalityWeight = 0.4f;
		// 100 is the "default" amount of health and mana
		float healthValue = characterData.health - 100;
		float manaValue = characterData.mana - 100;

		const float defenceWeight = 0.3f;
		// Let's say 5 is the "default" average resistance.
		float resistanceValue = (characterData.GetCharacterMagicalResistance() +
			characterData.GetCharacterPhysicalResistance()) / 2 - 5;

		const float offenceWeight = 0.3f;
		float approxDPS = 0;
		foreach (Attack attack in characterData.combatClass.attacks)
		{
			approxDPS += characterData.weapon.baseDamage * attack.damageMultiplier / attack.cooldown;
		}

		float threat = (healthValue + manaValue) * vitalityWeight +
			resistanceValue * defenceWeight + approxDPS * offenceWeight;
		return Mathf.RoundToInt(threat);
	}

	private void RegenMana()
	{
		characterData.mana = Mathf.Min(characterData.GetCharacterMaxMana(), characterData.mana + 1);
	}

	private static int CompareAdventurerThreat(AdventurerScript x, AdventurerScript y)
	{
		return -x.threat.CompareTo(y.threat);
	}

	private void FindTarget()
	{
		Transform oldTarget = targetTransform;

		if (potentialTargets.Count > 0)
		{
			potentialTargets.RemoveAll(x => x == null);

			potentialTargets.Sort(CompareAdventurerThreat);
			targetTransform = potentialTargets[0].transform;
		}
		else
		{
			targetTransform = gc.enemyTarget.transform;
		}
		
		if (targetTransform != oldTarget)
		{
			FindPath();
		}
	}

	private void FindPath()
	{
		if (targetTransform == null)
		{
			return;
		}

		if (path != null)
		{
			ClearPathfindingPenalties(path);
		}
		seeker.StartPath(rb.position, targetTransform.position, OnPathComplete);
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
		if (path == null)
		{
			return;
		}
		if (targetTransform == null)
		{
			FindTarget();
			return;
		}

		float distanceFromTarget = ((Vector2)targetTransform.position - rb.position).magnitude;
		if (characterData.weapon.baseRange > 1e-6 && distanceFromTarget < characterData.weapon.baseRange)
		{
			return;
		}

		if (curWaypoint >= path.vectorPath.Count)
		{
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
			Vector2 force = archetype.speed * Time.deltaTime * direction;

			rb.AddForce(force);
		}

		float distance = ((Vector2)path.vectorPath[curWaypoint] - rb.position).magnitude;

		if (distance < nextWaypointDistance)
		{
			curWaypoint++;
		}
	}

	private void SpawnGraphic(Sprite sprite)
	{
		if (weaponObj == null)
		{
			weaponObj = new GameObject(name + "'s Graphic");
			weaponObj.transform.SetParent(transform, false);
		}
		SpriteRenderer spr = weaponObj.GetComponent<SpriteRenderer>();
		if (spr == null)
		{
			spr = weaponObj.AddComponent<SpriteRenderer>();
			spr.sortingOrder = 1;
			spr.sortingLayerName = "Units";
		}

		spr.sprite = sprite;
	}

	private void Attack()
	{
		//if (isAttacking)
		//{
		//	return;
		//}
		//isAttacking = true;

		AdventurerScript target = targetTransform.GetComponent<AdventurerScript>();
		if (target == null || Time.time < nextAttackTime)
		{
			//isAttacking = false;
			return;
		}

		for (int i = 0; i < characterData.combatClass.attacks.Count; i++)
		{
			Attack potentialAttack = characterData.combatClass.attacks[i];
			bool inRange = characterData.weapon.baseRange + potentialAttack.rangeBonus >=
				Vector2.Distance(transform.position, targetTransform.position);
			if (!characterData.combatClass.attacks[i].CanAttack() || !inRange)
			{
				continue;
			}

			int manaCost = Mathf.RoundToInt(characterData.weapon.manaCost * potentialAttack.manaCostMultiplier);
			if (characterData.mana < manaCost)
			{
				continue;
			}
			characterData.mana -= manaCost;
			bool isMagical = characterData.weapon.isMagical || potentialAttack.isMagical;

			// Damage is rounded down.
			int damage = (int)(characterData.GetCharacterDamage() * potentialAttack.damageMultiplier);
			bool isMelee = characterData.weapon.baseRange + potentialAttack.rangeBonus < 2;

			GameObject newProjectile = Instantiate(gc.projectilePrefab, transform.position, Quaternion.identity);
			newProjectile.GetComponent<ProjectileScript>()
				.Init(target.transform, transform, damage, isMagical, isMelee, potentialAttack);
			newProjectile.layer = LayerMask.NameToLayer("Ignore Raycast");

			nextAttackTime = characterData.combatClass.attacks[i].SignalAttack();
			//isAttacking = false;
			return;
		}
		//isAttacking = false;
	}
}
