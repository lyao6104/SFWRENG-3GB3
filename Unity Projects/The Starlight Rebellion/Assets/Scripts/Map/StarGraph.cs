using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StarGraph
{
	public static List<Vector2> GenerateVertices(int num, float minRadius, Vector2Int gridDimensions, int allowedTries = 50)
	{
		float cellSize = Mathf.Sqrt(minRadius * minRadius / 2f);
		int[,] grid = new int[gridDimensions.x, gridDimensions.y];
		List<Vector2> output = new List<Vector2>();

		for (int i = 0; i < num; i++)
		{
			Vector2 candidate = Vector2.zero;
			bool available;
			int tries = 0;
			do
			{
				float cX = Random.Range(0, gridDimensions.x * cellSize);
				float cY = Random.Range(0, gridDimensions.y * cellSize);
				candidate.Set(cX, cY);
				// Debug.LogFormat("Trying to spawn Node at ({0}, {1})", cX, cY);

				int gridX = (int)(cX / cellSize);
				int gridY = (int)(cY / cellSize);
				if (grid[gridX, gridY] > 0)
				{
					available = false;
					tries++;
					continue;
				}
				else
				{
					grid[gridX, gridY] = i + 1; // Zero is empty, so we shift indices by one.
					available = true;
				}
				// Debug.LogFormat("Candidate is at Grid ({0}, {1})", gridX, gridY);
				for (int x = Mathf.Max(0, gridX - 2); x < Mathf.Min(gridDimensions.x, gridX + 2); x++)
				{
					for (int y = Mathf.Max(0, gridY - 2); y < Mathf.Min(gridDimensions.y, gridY + 2); y++)
					{
						if (grid[x, y] > 0 && grid[x, y] != i + 1)
						{
							available = false;
							tries++;

							// Also clear the cell we marked earlier.
							grid[gridX, gridY] = 0;

							break;
						}
					}

					if (!available)
					{
						break;
					}
				}
			} while (!available && tries < allowedTries);
			if (available)
			{
				output.Add(candidate);
			}
			else
			{
				Debug.LogWarningFormat("Skipping Node {0} as maximum tries were exceeded.", i);
			}
		}

		return output;
	}

	public static List<(int, int)> GenerateEdges(int minEdges, int maxEdges, List<Vector2> vertices)
	{
		// Stored as a hashset to easily avoid duplicates.
		HashSet<(int, int)> output = new HashSet<(int, int)>();
		int[] edgeCount = new int[vertices.Count];
		List<int> connected = new List<int> () { 0 };

		// Make sure the limits make sense.
		minEdges = Mathf.Max(1, minEdges);
		maxEdges = Mathf.Max(2, maxEdges);

		// Initial pass connects all vertices together with at least one edge, with respect to restrictions.
		// This shouldn't fail, but just in case, the do-while loop exits after 5 seconds if no valid edge has been created.
		for (int v = 1; v < vertices.Count; v++)
		{
			bool valid = false;
			float startTime = Time.time;
			do
			{
				int destination = Random.Range(0, connected.Count);
				if (edgeCount[destination] < maxEdges)
				{
					valid = true;
					// Edges should be ordered with the lesser value first.
					output.Add(v < destination ? (v, destination) : (destination, v));
					connected.Add(v);
					edgeCount[v]++;
					edgeCount[destination]++;
				}
				else if (Time.time - startTime > 5)
				{
					Debug.LogErrorFormat("Edge generation for Vertex {0} failed initial pass.", v);
					break;
				}
			} while (!valid);
		}

		// After the initial pass, add some more edges where applicable.
		for (int v = 0; v < vertices.Count; v++)
		{
			int numEdges = Random.Range(minEdges, maxEdges);
			for (int i = edgeCount[v]; i < numEdges; i++)
			{
				bool valid = false;
				float startTime = Time.time;
				do
				{
					int destination = Random.Range(0, vertices.Count);
					(int, int) candidate = v < destination ? (v, destination) : (destination, v);
					if (edgeCount[destination] < maxEdges && !output.Contains(candidate))
					{
						valid = true;
						output.Add(candidate);
						edgeCount[v]++;
						edgeCount[destination]++;
					}
					else if (Time.time - startTime > 5)
					{
						Debug.LogErrorFormat("Failed to generate Edge {0} for Vertex {1}.", i, v);
						break;
					}
				} while (!valid);
			}
		}

		return output.ToList();
	}
}
