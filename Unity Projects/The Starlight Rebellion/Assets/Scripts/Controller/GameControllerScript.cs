using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using Pathfinding;

public enum Player { Rebellion, Empire };

public class GameControllerScript : MonoBehaviour
{
	public GameObject nodePrefab;
	public List<StarSystemScript> starSystems;
	public CinemachineTargetGroup systemsTG;

	public StarSystemScript playerCapital, empireCapital;

	private string empireName;

	private AstarData astarData;
	private PointGraph astarGraph;

	private OptionsScript gameOptions;

	private void Start()
	{
		gameOptions = GameObject.FindGameObjectWithTag("Options").GetComponent<OptionsScript>();

		GenerateMap();
	}

	private void GenerateMap()
	{
		astarData = AstarPath.active.data;
		astarGraph = astarData.AddGraph(typeof(PointGraph)) as PointGraph;
		astarGraph.use2DPhysics = true;

		AstarPath.active.Scan(astarGraph);

		// Generate the map and add nodes and connections to the A* graph
		AstarPath.active.AddWorkItem(new AstarWorkItem(ctx =>
		{
			List<PointNode> nodes = new List<PointNode>();
			List<Vector2> positions = StarGraph.GenerateVertices(gameOptions.numSystems, gameOptions.minDistance, new Vector2Int(40, 20), 100);
			List<(int, int)> edges = StarGraph.GenerateEdges(gameOptions.minHyperlanes, gameOptions.maxHyperlanes, positions);
			for (int i = 0; i < positions.Count; i++)
			{
				GameObject newStar = Instantiate(nodePrefab, positions[i], Quaternion.identity);
				starSystems.Add(newStar.GetComponent<StarSystemScript>());
				starSystems[i].SetPlayer(Player.Empire);
				systemsTG.AddMember(newStar.transform, 1, 1);

				nodes.Add(astarGraph.AddNode((Int3)(Vector3)positions[i]));
			}

			for (int i = 0; i < edges.Count; i++)
			{
				(int v, int w) edge = edges[i];
				starSystems[edge.v].connections.Add(starSystems[edge.w]);
				starSystems[edge.w].connections.Add(starSystems[edge.v]);
				// Debug.DrawLine(positions[edge.v], positions[edge.w], Color.green, 30);

				nodes[edge.v].AddConnection(nodes[edge.w], 1);
				nodes[edge.w].AddConnection(nodes[edge.v], 1);
			}
		}));

		AstarPath.active.FlushWorkItems();

		StartCoroutine(MapPostInit());
	}

	private IEnumerator MapPostInit()
	{
		yield return new WaitForEndOfFrame();

		// Set up player capital
		starSystems[0].SetPlayer(Player.Rebellion);
		starSystems[0].SetName("Starlight");
		starSystems[0].SetCapital();
		playerCapital = starSystems[0];

		// Set up empire capital, which shouldn't be directly connected to the player's.
		// Starting in the upper half of star systems should make it less likely for such
		// connections to exist.
		int empireIndex = starSystems.Count / 2;
		for (int i = empireIndex; i < starSystems.Count; i++)
		{
			if (!starSystems[i].connections.Contains(playerCapital))
			{
				empireIndex = i;
				break;
			}
		}
		starSystems[empireIndex].SetPlayer(Player.Empire);
		starSystems[empireIndex].SetName("Shadow's Reach");
		starSystems[empireIndex].SetCapital();
		empireCapital = starSystems[empireIndex];

		// Set up hyperlane graphics
		for (int i = 0; i < starSystems.Count; i++)
		{
			starSystems[i].GenerateHyperlanes();
		}

		// Game options are no longer needed
		Destroy(gameOptions.gameObject);
		gameOptions = null;
	}

	public string GetPlayerName(Player player)
	{
		if (player == Player.Rebellion)
		{
			return "The Starlight Rebellion";
		}
		else
		{
			return empireName;
		}
	}

	public void ToMainMenu()
	{
		SceneManager.LoadScene("MenuScene");
	}

	public void EndTurn()
	{
		// TODO
	}
}
