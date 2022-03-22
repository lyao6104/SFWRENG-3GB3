using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterLib;
using ItemLib;

[CreateAssetMenu(fileName = "NewEnemyArchetype", menuName = "Scriptable Objects/Enemy Archetype")]
public class EnemyArchetypeSO : ScriptableObject
{
	// Used for building character
	public string race;
	public string className;
	public int level;
	public Weapon weapon;

	// Actually used by enemy script
	public float lootChance = 0.05f;
	public float minRarityPerLevel = 0.1f;
	public float maxRarityPerLevel = 0.2f;
	public float speed;

	public Character Build()
	{
		Character newEnemy = new Character(RaceUtil.GetEnemyRace(race))
		{
			combatClass = ClassUtil.GetEnemyClass(className),
			weapon = weapon
		};
		for (int i = newEnemy.combatClass.level; i < level; i++)
		{
			newEnemy.LevelUp(true);
			newEnemy.combatClass.UnlockCurrentSkills();
		}
		return newEnemy;
	}
}
