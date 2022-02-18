using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;

namespace AILib
{
	public static class FleetUtil
	{
		public static FleetAI GetAI(GameControllerScript gc, FleetScript fleet, BlockManager.TraversalProvider tp)
		{
			//int roll = Random.Range(0, 2);
			//switch (roll)
			//{
			//	case 0:
			//		return new DefensiveAI(gc, fleet, tp);
			//	default:
			//		return new OffensiveAI(gc, fleet, tp);
			//}
			return new OffensiveAI(gc, fleet, tp);
		}
	}

	[System.Serializable]
	public abstract class FleetAI
	{
		public StarSystemScript target;
		public List<Vector3> path;

		[SerializeField]
		protected GameControllerScript gc;
		[SerializeField]
		protected FleetScript fleet;
		protected BlockManager.TraversalProvider tp;

		public FleetAI(GameControllerScript gc, FleetScript fleet, BlockManager.TraversalProvider tp)
		{
			this.gc = gc;
			this.fleet = fleet;
			path = new List<Vector3>();
		}

		public Vector3 GetDestination(BlockManager.TraversalProvider tp)
		{
			// If enemies are nearby, attack them
			for (int i = 0; i < fleet.location.connections.Count; i++)
			{
				FleetScript potentialEnemy = fleet.location.connections[i].fleet;
				if (potentialEnemy != null && potentialEnemy.controller != fleet.controller)
				{
					return potentialEnemy.location.transform.position;
				}
			}

			// If nearby systems are empty, use pathfinding.
			if (path.Count < 1)
			{
				var aStarPath = fleet.GetComponent<Seeker>().StartPath(fleet.location.transform.position, target.transform.position);
				aStarPath.traversalProvider = tp;

				aStarPath.BlockUntilCalculated();
				if (aStarPath.error)
				{
					Debug.LogWarningFormat("Fleet {0} failed to find a path to {1}", fleet.name, target.name);
				}
				else
				{
					path = aStarPath.vectorPath;
					Debug.Log("Start path");
					foreach (Vector3 v in path)
					{
						Debug.Log(v.ToString());
					}
					Debug.Log("End path");
					// Remove the first node since that's where the fleet is currently
					if (Vector3.Distance(path[0], fleet.location.transform.position) < 0.5f)
					{
						path.RemoveAt(0);
					}
				}
			}
			Vector3 destination = path[0];
			path.RemoveAt(0);
			return destination;
		}

		public abstract StarSystemScript FindTarget();
	}

	[System.Serializable]
	public class DefensiveAI : FleetAI
	{
		public DefensiveAI(GameControllerScript gc, FleetScript fleet, BlockManager.TraversalProvider tp) : base(gc, fleet, tp)
		{

		}

		public override StarSystemScript FindTarget()
		{
			List<StarSystemScript> imperialSystems = gc.starSystems.FindAll(system => system.controller == Player.Empire);
			if (imperialSystems.Count < 1)
			{
				target = gc.empireCapital;
			}
			else
			{
				target = imperialSystems[Random.Range(0, imperialSystems.Count)];
			}
			return target;
		}
	}

	[System.Serializable]
	public class OffensiveAI : FleetAI
	{
		public OffensiveAI(GameControllerScript gc, FleetScript fleet, BlockManager.TraversalProvider tp) : base(gc, fleet, tp)
		{

		}

		public override StarSystemScript FindTarget()
		{
			List<StarSystemScript> rebelSystems = gc.starSystems.FindAll(system => system.controller == Player.Rebellion);
			if (rebelSystems.Count < 1)
			{
				target = gc.playerCapital;
			}
			else
			{
				target = rebelSystems[Random.Range(0, rebelSystems.Count)];
			}
			return target;
		}
	}
}
