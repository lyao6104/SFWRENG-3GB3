using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerScript : MonoBehaviour
{
	public GameObject nodePrefab;
	public List<StarSystemScript> starSystems;

	private void Start()
	{
		List<Vector2> positions = StarGraph.GenerateVertices(20, 5, new Vector2Int(40, 20), 100);
		List<(int, int)> edges = StarGraph.GenerateEdges(1, 2, positions);
		for (int i = 0; i < positions.Count; i++)
		{
			GameObject newStar = Instantiate(nodePrefab, positions[i], Quaternion.identity);
			starSystems.Add(newStar.GetComponent<StarSystemScript>());
		}

		for (int i = 0; i < edges.Count; i++)
		{
			(int v, int w) edge = edges[i];
			starSystems[edge.v].connections.Add(starSystems[edge.w]);
			starSystems[edge.w].connections.Add(starSystems[edge.v]);
			Debug.DrawLine(positions[edge.v], positions[edge.w], Color.green, 30);
		}
	}
}
