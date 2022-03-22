using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnScript : MonoBehaviour
{
	[System.Serializable]
	public class EnemyWaveEntry
	{
		public EnemyArchetypeSO archetype;
		public int count;
	}

	[System.Serializable]
	public class EnemyWave
	{
		public List<EnemyWaveEntry> enemies;
	}

	public GameObject enemyPrefab;
	public List<EnemyWave> waves;

	private GameControllerScript gc;

	private void Start()
	{
		gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
	}

	public void SpawnWave()
	{
		EnemyWave nextWave = waves[0];
		waves.RemoveAt(0);

		foreach (EnemyWaveEntry enemyType in nextWave.enemies)
		{
			for (int i = 0; i < enemyType.count; i++)
			{
				EnemyScript newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity).GetComponent<EnemyScript>();
				newEnemy.Spawn(enemyType.archetype);
			}
		}
	}
}
