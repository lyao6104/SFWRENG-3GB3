using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AILib;
using NamesLib;
using Pathfinding;

public class FleetScript : MonoBehaviour
{
    public Player controller;
    public StarSystemScript location;

	public bool hasMoved = false;

	private GameControllerScript gc;
	[SerializeField]
	private FleetAI empireAI;
	private static BlockManager.TraversalProvider traversalProvider;
	private static List<SingleNodeBlocker> obstacles = new List<SingleNodeBlocker>();

	private static int rebelCasualties = 0;

	public static int GetRebelCasualties()
	{
		return rebelCasualties;
	}

	public static void ResetRebelCasualties()
	{
		rebelCasualties = 0;
	}

	private void Start()
	{
		gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerScript>();
		BlockManager blockManager = GameObject.FindGameObjectWithTag("Block Manager").GetComponent<BlockManager>();
		traversalProvider = new BlockManager.TraversalProvider(blockManager, BlockManager.BlockMode.AllExceptSelector, obstacles);
	}

	public void Init()
	{
		name = NamesUtil.GetFleetName(controller == Player.Empire);
	}

	public void EndTurn()
	{
		hasMoved = false;
		if (controller == Player.Empire)
		{
			if (empireAI == null)
			{
				empireAI = FleetUtil.GetAI(gc, this, traversalProvider);
				obstacles.Add(gameObject.AddComponent<SingleNodeBlocker>());
				gameObject.AddComponent<Seeker>();
				
			}
			if (empireAI.target == null || empireAI.target == location)
			{
				empireAI.FindTarget();
			}
			Move(empireAI.GetDestination(traversalProvider));
		}
	}

	public bool Move(Vector3 destination)
	{
		for (int i = 0; i < location.connections.Count; i++)
		{
			if (Vector3.Distance(location.connections[i].transform.position, destination) < 0.5f)
			{
				return Move(location.connections[i]);
			}
		}
		Debug.LogErrorFormat("{0} is an invalid destination for \"{1}\"", destination.ToString(), name);
		return false;
	}

	public bool Move(StarSystemScript destination)
	{
		if (hasMoved)
		{
			return false;
		}
		else
		{
			hasMoved = true;
			Debug.LogFormat("{0} moving from {1} to {2}", name, location.name, destination.name);
		}

		if (location.connections.Contains(destination))
		{
			if (destination.fleet != null)
			{
				if (destination.fleet.controller == controller)
				{
					Debug.LogFormat("Cancelling move; Star \"{0}\" already has a friendly fleet.", destination.name);
					hasMoved = false;
					return false;
				}
				else
				{
					destination.fleet.KillFleet();
				}
			}
			
			location.fleet = null;
			location.UpdateUI();
			destination.fleet = this;
			destination.UpdateUI();
			location = destination;

			if (destination.controller != controller)
			{
				destination.Conquer(controller);
				if (controller == Player.Rebellion && ++gc.liberatedSystems % 3 == 0)
				{
					// New rebel fleets shouldn't move on their first turn.
					gc.SpawnFleet(controller, destination, false);
				}
			}

			return true;
		}
		else
		{
			Debug.LogErrorFormat("Star \"{0}\" is an invalid destination for \"{1}\"", destination.name, name);
			hasMoved = false;
			return false;
		}
	}

	public void KillFleet()
	{
		if (controller == Player.Rebellion)
		{
			rebelCasualties++;
		}
		location.fleet = null;
		location = null;
		gc.fleets.Remove(this);
		Destroy(gameObject);
	}
}
