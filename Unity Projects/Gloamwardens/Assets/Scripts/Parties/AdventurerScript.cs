using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterLib;
using Pathfinding;

public class AdventurerScript : MonoBehaviour
{
	public Character characterData;
	public int threat;

	public float healthRegenTime = 1, manaRegenTime = 1;

	private PartyScript party;
	private AdventurerScript[] partyMembers = new AdventurerScript[3];
	[SerializeField]
	private TargetingCriteria targetingCriterion = TargetingCriteria.Closest;

	// 0 -> Leggings, 1 -> Chestpiece, 2 -> Weapon, 3 -> Hair
	private GameObject[] graphicsObjects = new GameObject[4];

	private GameControllerScript gc;
	private Rigidbody2D rb;
	private Transform targetTransform;
	private List<EnemyScript> potentialTargets = new List<EnemyScript>();
	private const float threatDecayInterval = 0.5f;

	private const float pathfindingInterval = 2.5f;
	private const float targetingInterval = 2.5f;
	private const float nextWaypointDistance = 3f;
	private const float adventurerSpeed = 50;
	private Path path;
	private Seeker seeker;
	private int curWaypoint = 0;

	private float nextAttackTime = 0;

	private void FixedUpdate()
	{
		MoveUnit();

		Attack();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy"))
		{
			potentialTargets.Add(collision.GetComponent<EnemyScript>());
			FindTarget();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Enemy"))
		{
			potentialTargets.Remove(collision.GetComponent<EnemyScript>());
			FindTarget();
		}
	}

	public TargetingCriteria GetTargetPriority()
	{
		return targetingCriterion;
	}

	public void PrioritizeTarget(TargetingCriteria criterion)
	{
		targetingCriterion = criterion;
		FindTarget();
	}

	private void FindTarget()
	{
		Transform oldTarget = targetTransform;

		if (potentialTargets.Count > 0)
		{
			potentialTargets.RemoveAll(x => x == null);

			potentialTargets.Sort(CompareTarget);
			targetTransform = potentialTargets[0].transform;
		}
		else
		{
			targetTransform = party.transform;
		}

		if (targetTransform != oldTarget)
		{
			FindPath();
		}
	}

	private int CompareTarget(EnemyScript a, EnemyScript b)
	{
		switch (targetingCriterion)
		{
			case TargetingCriteria.Closest:
				float distanceA = Vector2.Distance(transform.position, a.transform.position);
				float distanceB = Vector2.Distance(transform.position, b.transform.position);
				return distanceA.CompareTo(distanceB);				
			case TargetingCriteria.MostHealthy:
				float ratioA = a.characterData.health / a.characterData.GetCharacterMaxHealth();
				float ratioB = b.characterData.health / b.characterData.GetCharacterMaxHealth();
				return -ratioA.CompareTo(ratioB);
			case TargetingCriteria.LeastHealthy:
				ratioA = a.characterData.health / a.characterData.GetCharacterMaxHealth();
				ratioB = b.characterData.health / b.characterData.GetCharacterMaxHealth();
				return ratioA.CompareTo(ratioB);
			// Default criterion is MostThreat
			default:
				return -a.GetThreat().CompareTo(b.GetThreat());
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
		if (targetTransform.gameObject != party.gameObject && characterData.weapon.baseRange > 1e-6 && distanceFromTarget < characterData.weapon.baseRange - 1)
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
			Vector2 force = adventurerSpeed * Time.deltaTime * direction;

			rb.AddForce(force);
		}

		float distance = ((Vector2)path.vectorPath[curWaypoint] - rb.position).magnitude;

		if (distance < nextWaypointDistance)
		{
			curWaypoint++;
		}
	}

	private Transform GetHealingTarget()
	{
		List<AdventurerScript> potentials = new List<AdventurerScript>(partyMembers)
		{
			this
		};
		//for (int i = 0; i < partyMembers.Length; i++)
		//{
		//	Debug.Log(partyMembers[i].characterData.name);
		//	potentials.Add(partyMembers[i]);
		//}
		//foreach (AdventurerScript script in potentials)
		//{
		//	Debug.Log(script.characterData.name);
		//}
		potentials.Sort(CompareAdventurerHealth);
		//Debug.Log(potentials[0].characterData.name);
		return potentials[0].transform;
	}

	private int CompareAdventurerHealth(AdventurerScript a, AdventurerScript b)
	{
		float ratioA = a.characterData.health / a.characterData.GetCharacterMaxHealth();
		float ratioB = b.characterData.health / b.characterData.GetCharacterMaxHealth();
		int val = ratioA.CompareTo(ratioB);
		//Debug.LogFormat("{3}: {0}, {4}:{1}, V: {2}", ratioA, ratioB, val, a.characterData.name, b.characterData.name);
		return val;
	}

	private void Attack()
	{
		//if (isAttacking)
		//{
		//	return;
		//}
		//isAttacking = true;

		EnemyScript target = targetTransform.GetComponent<EnemyScript>();
		if (Time.time < nextAttackTime)
		{
			//isAttacking = false;
			return;
		}

		for (int i = 0; i < characterData.combatClass.attacks.Count; i++)
		{
			Attack potentialAttack = characterData.combatClass.attacks[i];
			bool isHealing = potentialAttack.damageMultiplier < 0;
			if (!isHealing && target == null)
			{
				continue;
			}
			Transform atkTarget = isHealing ? GetHealingTarget() : targetTransform;
			if (isHealing)
			{
				AdventurerScript targetAdv = atkTarget.GetComponent<AdventurerScript>();
				if (targetAdv != null && targetAdv.characterData.health >= targetAdv.characterData.GetCharacterMaxHealth())
				{
					continue;
				}
			}

			bool inRange = characterData.weapon.baseRange + potentialAttack.rangeBonus >=
				Vector2.Distance(transform.position, atkTarget.position);
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
			// Threat generated is damage, plus 2 times damage if isTaunting
			threat += damage * (characterData.weapon.isTaunting ? 3 : 1);

			if (isHealing)
			{
				atkTarget.GetComponent<AdventurerScript>().BeHealed(Mathf.Abs(damage));
			}
			else
			{
				GameObject newProjectile = Instantiate(gc.projectilePrefab, transform.position, Quaternion.identity);
				newProjectile.GetComponent<ProjectileScript>()
					.Init(atkTarget, transform, damage, isMagical, isMelee, potentialAttack);
				newProjectile.layer = LayerMask.NameToLayer("Ignore Raycast");
			}

			nextAttackTime = characterData.combatClass.attacks[i].SignalAttack();
			//isAttacking = false;
			return;
		}
		//isAttacking = false;
	}

	private void SpawnGraphic(Sprite sprite, int index)
	{
		if (graphicsObjects[index] == null)
		{
			graphicsObjects[index] = new GameObject(name + "'s Graphic");
			graphicsObjects[index].transform.SetParent(transform, false);
		}
		SpriteRenderer spr = graphicsObjects[index].GetComponent<SpriteRenderer>();
		if (spr == null)
		{
			spr = graphicsObjects[index].AddComponent<SpriteRenderer>();
			spr.sortingOrder = index + 1;
			spr.sortingLayerName = "Units";
		}

		spr.sprite = sprite;
	}

	public void BeHealed(int amount)
	{
		characterData.health = Mathf.Min(characterData.GetCharacterMaxHealth(), characterData.health + amount);
	}

	private void RegenHealth()
	{
		BeHealed(1);
	}

	private void RegenMana()
	{
		characterData.mana = Mathf.Min(characterData.GetCharacterMaxMana(), characterData.mana + 1);
	}

	private void DecayThreat()
	{
		threat = Mathf.Max(0, threat - 1);
	}

	public void GainEXP(int amount)
	{
		characterData.combatClass.GainEXP(amount);
	}

	private void Kill()
	{
		// Remove from parties, etc.
		party.RemoveAdventurer(this);
		party = null;

		Clear();
	}

	public void Clear()
	{
		if (party != null)
		{
			party.UpdateCharacterData(this);
		}

		CancelInvoke();
		Destroy(gameObject);
	}

	public void TakeDamage(int baseDamage, bool isMagical)
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
			Kill();
		}
	}

	public void UnlockSkill(int skillLevel, string skillName)
	{
		characterData.combatClass.UnlockSkill(skillLevel, skillName);
	}

	public void Deploy(PartyScript party, Character character)
	{
		this.party = party;
		StartCoroutine(GetPartyMembers());

		characterData = character;
		name = characterData.name;
		//characterData.hairSprite = RaceUtil.GetHair();
		//characterData.weapon = ItemUtil.GetWeapon();
		//characterData.torso = ItemUtil.GetArmour(new ArmourClass[] { ArmourClass.LightArmour }, EquipmentSlot.Torso);
		//characterData.legs = ItemUtil.GetArmour(ArmourClass.LightArmour, EquipmentSlot.Legs);

		gc = GameControllerScript.GetController();
		seeker = GetComponent<Seeker>();
		rb = GetComponent<Rigidbody2D>();

		GetComponent<SpriteRenderer>().sprite = characterData.bodySprite;
		SpawnGraphic(characterData.legs.sprite, 0);
		SpawnGraphic(characterData.torso.sprite, 1);
		SpawnGraphic(characterData.weapon.sprite, 2);
		SpawnGraphic(characterData.hairSprite, 3);

		InvokeRepeating(nameof(FindTarget), 0, targetingInterval);
		InvokeRepeating(nameof(FindPath), 0, pathfindingInterval);
		InvokeRepeating(nameof(RegenHealth), 0, healthRegenTime);
		InvokeRepeating(nameof(RegenMana), 0, manaRegenTime);
		InvokeRepeating(nameof(DecayThreat), 0, threatDecayInterval);
	}

	public void Retreat()
	{
		party.Retreat();
	}

	private IEnumerator GetPartyMembers()
	{
		yield return new WaitForEndOfFrame();

		partyMembers = party.GetPartyMembers(this);
	}
}
