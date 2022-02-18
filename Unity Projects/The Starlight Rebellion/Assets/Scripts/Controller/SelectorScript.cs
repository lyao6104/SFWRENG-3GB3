using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectorScript : MonoBehaviour
{
	public LayerMask raycastLayers;
	public GameObject currentlySelected;
	public GameObject selectionInfocard;

	private void Start()
	{

	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 mousePos = Input.mousePosition;
			RaycastHit2D mouseHit;
			if (!Camera.main.orthographic)
			{
				mousePos.z = -Camera.main.transform.position.z;
			}
			mouseHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero, Mathf.Infinity, raycastLayers);
			if (mouseHit.collider != null)
			{
				Debug.Log(string.Format("Raycast: {0}", mouseHit.transform.name));
			}
			if (!EventSystem.current.IsPointerOverGameObject() && mouseHit.collider != null && mouseHit.collider.gameObject != currentlySelected)
			{
				Deselect();
				Select(mouseHit.collider.gameObject);
			}
			else if (!EventSystem.current.IsPointerOverGameObject() && (mouseHit.collider == null || mouseHit.collider.gameObject != currentlySelected))
			{
				Deselect();
			}
			//Debug.DrawLine(Camera.main.transform.position, Camera.main.ScreenToWorldPoint(mousePos), Color.green, 10, false);
		}
		// Player right clicks to move fleets.
		else if (Input.GetMouseButtonDown(1) && currentlySelected != null)
		{
			StarSystemScript selectedFrom = currentlySelected.GetComponent<StarSystemScript>();
			if (selectedFrom != null && selectedFrom.fleet != null && selectedFrom.fleet.controller == Player.Rebellion)
			{
				// Raycast
				Vector3 mousePos = Input.mousePosition;
				RaycastHit2D mouseHit;
				if (!Camera.main.orthographic)
				{
					mousePos.z = -Camera.main.transform.position.z;
				}
				mouseHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero, Mathf.Infinity, raycastLayers);
				if (mouseHit.collider != null)
				{
					Debug.Log(string.Format("Raycast: {0}", mouseHit.transform.name));
				}
				if (!EventSystem.current.IsPointerOverGameObject() && mouseHit.collider != null && mouseHit.collider.gameObject != currentlySelected)
				{
					StarSystemScript selectedTo = mouseHit.collider.gameObject.GetComponent<StarSystemScript>();
					if (selectedTo != null)
					{
						if (selectedFrom.fleet.Move(selectedTo))
						{
							Deselect();
							Select(mouseHit.collider.gameObject);
						}
					}
				}
			}
		}
		if (Input.GetButtonDown("Cancel"))
		{
			Deselect();
		}
	}

	public void Deselect()
	{
		if (currentlySelected != null)
		{
			StarSystemScript selectedSystem = currentlySelected.GetComponent<StarSystemScript>();
			if (selectedSystem != null)
			{
				selectedSystem.Deselect();
			}
			currentlySelected = null;
		}
		selectionInfocard.SetActive(false);
	}

	private void Select(GameObject toSelect)
	{
		currentlySelected = toSelect;
		StarSystemScript selectedSystem = currentlySelected.GetComponent<StarSystemScript>();
		if (selectedSystem != null)
		{
			selectedSystem.Select();
			selectionInfocard.SetActive(true);
			selectionInfocard.GetComponent<InfocardScript>().Init(selectedSystem);
		}
	}
}
