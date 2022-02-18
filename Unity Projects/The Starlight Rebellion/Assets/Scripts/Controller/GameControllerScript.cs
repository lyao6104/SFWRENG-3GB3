using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pathfinding;
using TMPro;

public enum Player { Rebellion, Empire };

public class GameControllerScript : MonoBehaviour
{
	public GameObject nodePrefab, fleetPrefab, popupPrefab;
	public List<StarSystemScript> starSystems;
	public List<FleetScript> fleets;

	public GameObject mainUI, gameOverUI, titlePanel;
	public TMP_Text gameOverTypeLabel, gameOverText;

	public StarSystemScript playerCapital, empireCapital;
	public int liberatedSystems = 1;

	private string empireName;
	private bool capitalLiberated = false, reserveFleetSpawned = false;
	private int playerSystems = 0, empireSystems = 0;
	private StarSystemScript lastConqueredSystem;
	private int turnCount = 0, turnsPerEmpireFleet = 2;

	private AstarData astarData;
	private PointGraph astarGraph;

	private OptionsScript gameOptions;
	private bool gameOver = false;
	private bool blockControls = false;

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
			Transform nodeParent = GameObject.Find("Star Systems").transform;

			List<PointNode> nodes = new List<PointNode>();
			List<Vector2> positions = StarGraph.GenerateVertices(gameOptions.numSystems, gameOptions.minDistance, new Vector2Int(40, 20), 100);
			List<(int, int)> edges = StarGraph.GenerateEdges(gameOptions.minHyperlanes, gameOptions.maxHyperlanes, positions);
			for (int i = 0; i < positions.Count; i++)
			{
				GameObject newStar = Instantiate(nodePrefab, positions[i], Quaternion.identity, nodeParent);
				starSystems.Add(newStar.GetComponent<StarSystemScript>());
				starSystems[i].SetPlayer(Player.Empire);

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
		empireName = NamesLib.NamesUtil.GetEmpireName();

		// Set up hyperlane graphics
		for (int i = 0; i < starSystems.Count; i++)
		{
			starSystems[i].GenerateHyperlanes();
		}

		// Spawn initial fleets
		// For player
		SpawnFleet(Player.Rebellion, playerCapital);
		playerCapital.UpdateUI();
		// For empire
		for (int i = 0; i < Mathf.FloorToInt(starSystems.Count / 2.6f) - 1; i++)
		{
			SpawnFleet(Player.Empire, starSystems[Random.Range(1, starSystems.Count)]);
		}
		SpawnFleet(Player.Empire, empireCapital); // Shadow's Reach also gets a fleet.

		// Set up counters
		playerSystems = 1;
		empireSystems = starSystems.Count - 1;

		// Game options are no longer needed
		Destroy(gameOptions.gameObject);
		gameOptions = null;

		// Move camera
		CameraScript camTarget = GameObject.FindGameObjectWithTag("Player").GetComponent<CameraScript>();
		Bounds camBounds = new Bounds(playerCapital.transform.position, Vector3.one);
		for (int i = 1; i < starSystems.Count; i++)
		{
			camBounds.Encapsulate(starSystems[i].transform.position);
		}
		camTarget.SetBounds(camBounds);
		camTarget.GoToObject(playerCapital.gameObject);

		// Intro popups
		string intro = string.Format("It's now or never!\n\nThe {0} has begun cracking down on rebel cells all across " +
			"the sector, and many of our contacts have gone dark. Yet, despite this setback we have managed to secure the system of " +
			"Starlight, and have organized our forces into our first fleet.\n\nWe only get one chance at this; if we fail here, " +
			"the Empire will be too entrenched for another uprising to succeed. Good luck, Commander.", GetPlayerName(Player.Empire));
		string instructions = string.Format("<color=#00ccb8>Left Click</color> to select and view star systems and fleets." +
			"\n<color=#00ccb8>Right Click</color> on another system while selecting a controlled system with a friendly fleet to move that fleet. " +
			"Moving onto a hostile system will capture that system, regardless of any fleets present." +
			"\nUse <color=#00ccb8>WASD</color> to move the map, and <color=#00ccb8>Mouse Scroll</color> to adjust zoom." +
			"\n\nThe Rebellion is able to organize new fleets for every <color=#00ccb8>3</color> liberated worlds, while the Empire will raise a new fleet " +
			"every <color=#00ccb8>{0}</color> turns.\n\nVictory or defeat will be achieved when one side completely controls the sector.", turnsPerEmpireFleet);
		CreatePopup("Instructions", instructions);
		CreatePopup("The Light Will Be Restored!", intro);
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
		if (!blockControls)
		{
			StartCoroutine(EndTurnIE());
		}
	}

	public IEnumerator EndTurnIE()
	{
		if (gameOver || blockControls)
		{
			yield break;
		}
		blockControls = true;

		// Update fleets using a stack to avoid issues when fleets attack each other
		Stack<FleetScript> toUpdate = new Stack<FleetScript>();
		for (int i = 0; i < fleets.Count; i++)
		{
			if (fleets[i] != null)
			{
				toUpdate.Push(fleets[i]);
			}
		}
		while (toUpdate.Count > 0)
		{
			if (toUpdate.Peek() != null)
			{
				toUpdate.Pop().EndTurn();
			}
		}
		// Allow fleets to finish before systems are updated
		yield return new WaitForEndOfFrame();

		// Update systems
		for (int i = 0; i < starSystems.Count; i++)
		{
			starSystems[i].UpdateUI();
		}

		// Don't run the rest if there's a game over
		if (gameOver)
		{
			yield break;
		}

		// Empire gets fleets every so often.
		if (++turnCount % turnsPerEmpireFleet == 0)
		{
			SpawnFleet(Player.Empire, empireCapital);
		}
		// The first time the Rebellion has no fleets left, they get a lifeline.
		if (!reserveFleetSpawned && fleets.FindAll(fleet => fleet.controller == Player.Rebellion).Count < 1)
		{
			reserveFleetSpawned = true;
			FleetScript reserveFleet = SpawnFleet(Player.Rebellion, playerCapital);
			reserveFleet.name = string.Format("{0}. Fleet \"Light's Last Champions\"", FleetScript.GetRebelCasualties() + 1);
			string reserveFleetText = string.Format("The recent battles have not gone well for the Starlight Rebellion. " +
				"{0} fleets were lost at the hands of the Empire, but good news has reached High Command today. It appears that " +
				"survivors of these battles have managed to rally together, and have organized themselves into a fleet near Starlight.\n\n" +
				"Do not squander this opportunity.", FleetScript.GetRebelCasualties());
			CreatePopup("Reserves", reserveFleetText);
		}

		// Wait for a bit so no double clicks are registered.
		yield return new WaitForSeconds(0.1f);
		blockControls = false;
	}

	private void GameOver(bool victory)
	{
		if (gameOver)
		{
			return;
		}
		gameOver = true;
		titlePanel.SetActive(false);
		GetComponent<SelectorScript>().Deselect();

		if (victory)
		{
			gameOverTypeLabel.text = "Victory";
			gameOverText.text = string.Format("The entire galaxy breathed a collective sigh of relief today, as the last remnants of the {0} " +
				"have finally surrendered.\n\nThough the mood is joyous for the moment, the peoples' thoughts soon turn to the gargantuan effort " +
				"that reconstruction will entail. Nowhere is the destruction from the civil war more apparent than the Imperial capital of Shadow's Reach. " +
				"Though the Empire is now gone, their stench still hangs over the ruins of Iridescence like a tenebrous miasma.\n\n" +
				"And yet, despite all that remains to be done, and those we have lost, the fact remains that today, the Rebellion has achieved the impossible. " +
				"And that, is worth <i>everything</i>.\n\nThe sector was liberated in {1} turns, and the Rebellion lost {2} fleets in doing so.",
				GetPlayerName(Player.Empire), turnCount, FleetScript.GetRebelCasualties());
		}
		else
		{
			gameOverTypeLabel.text = "Defeat";
			gameOverText.text = string.Format("As the {0} continues to pacify the last rebel holdouts around {1}, the mood is grim amongst the Rebellion's leadership. " +
				"Those gathered here today are few, as most of the High Command has either been captured by the Empire, or went into hiding. " +
				"Although there are those across the sector who still hold the hope that Light will shine again, this Rebellion was most likely " +
				"the final chance, as the Empire has all but secured their hold over the sector now.\n\nAs shots ring out across the complex you are in, " +
				"one thing is clear: this is the end.", lastConqueredSystem.fleet.name, lastConqueredSystem.name);
		}
		gameOverUI.SetActive(true);
	}

	public FleetScript SpawnFleet(Player controller, StarSystemScript location, bool moveOnFirstTurn = true)
	{
		// If the location is occupied, do a BFS to find the nearest valid location.
		// It should first try to locate a friendly unoccupied system with no nearby enemies,
		// or, failing that, just a friendly unoccupied system.
		if (location.fleet != null || location.controller != controller)
		{
			StarSystemScript backupSpot = null;
			Queue<StarSystemScript> toVisit = new Queue<StarSystemScript>();
			Dictionary<StarSystemScript, bool> visited = new Dictionary<StarSystemScript, bool>();
			for (int i = 0; i < starSystems.Count; i++)
			{
				visited[starSystems[i]] = false;
			}

			visited[location] = true;
			toVisit.Enqueue(location);

			while (toVisit.Count > 0)
			{
				StarSystemScript candidate = toVisit.Dequeue();
				if (candidate.fleet == null && candidate.controller == controller)
				{
					if (!candidate.connections.Exists(system => system.fleet != null && system.fleet.controller != controller))
					{
						location = candidate;
						backupSpot = null;
						break;
					}
					else if (backupSpot == null)
					{
						backupSpot = candidate;
					}
				}
				for (int i = 0; i < candidate.connections.Count; i++)
				{
					if (!visited[candidate.connections[i]])
					{
						visited[candidate.connections[i]] = true;
						toVisit.Enqueue(candidate.connections[i]);
					}
				}
			}

			// Use the backup spot if it exists
			if (backupSpot != null)
			{
				location = backupSpot;
			}
			// If location is still occupied then there's a problem.
			else if (location.fleet != null)
			{
				Debug.LogErrorFormat("Could not find spawn location for new fleet targeting Star {0}.", location.name);
				return null;
			}
		}

		FleetScript newFleet = Instantiate(fleetPrefab).GetComponent<FleetScript>();
		newFleet.controller = controller;
		newFleet.location = location;
		newFleet.hasMoved = !moveOnFirstTurn;
		newFleet.Init();
		fleets.Add(newFleet);
		location.fleet = newFleet;
		location.UpdateUI();
		return newFleet;
	}

	public void SystemConquered(StarSystemScript system)
	{
		lastConqueredSystem = system;
		if (system.controller == Player.Empire)
		{
			playerSystems--;
			empireSystems++;
		}
		else
		{
			playerSystems++;
			empireSystems--;
		}

		// Flavour event for the imperial capital
		if (system == empireCapital && system.controller == Player.Rebellion && !capitalLiberated)
		{
			capitalLiberated = true;
			string capitalText = string.Format("The tide is turning. Today, Rebel forces have liberated the Imperial capital, " +
				"located in what is now called Shadow's Reach. Yet, as the forces of Light celebrate on the capital planet, " +
				"it is clear that the price of their victory was steep. The old Federation capital of Iridescence has been in ruins " +
				"ever since the {0}'s original invasion several decades ago, and what was once a resplendent beacon of Light is now a monument " +
				"to Darkness.", GetPlayerName(Player.Empire));
			if (empireSystems > 1)
			{
				capitalText += "\n\nThere is much to rebuild, but the Rebellion cannot rest just yet. The remaining Imperial forces have been roused by " +
					"their defeat today, and the Empire's legions will not stop until the Darkness swallows the Light.";
			}
			CreatePopup("The Shadow's Fall", capitalText);
		}

		// Game over
		if (playerSystems < 1 || empireSystems < 1)
		{
			GameOver(empireSystems < 1);
		}
	}

	public void CreatePopup(string title, string body)
	{
		PopupScript newPopup = Instantiate(popupPrefab, mainUI.transform).GetComponent<PopupScript>();
		newPopup.SetText(title, body);
	}
}
