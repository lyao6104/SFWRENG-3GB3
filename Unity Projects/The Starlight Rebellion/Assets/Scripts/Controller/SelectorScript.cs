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
				DisableAllInfocards();
				currentlySelected = mouseHit.collider.gameObject;
				StarSystemScript selectedSystem = currentlySelected.GetComponent<StarSystemScript>();
				if (selectedSystem != null)
				{
					selectionInfocard.SetActive(true);
					selectionInfocard.GetComponent<InfocardScript>().Init(selectedSystem);
				}
			}
			else if (!EventSystem.current.IsPointerOverGameObject() && (mouseHit.collider == null || mouseHit.collider.gameObject != currentlySelected))
			{
				DisableAllInfocards();
				currentlySelected = null;
			}
			//Debug.DrawLine(Camera.main.transform.position, Camera.main.ScreenToWorldPoint(mousePos), Color.green, 10, false);
		}
		if (Input.GetButtonDown("Cancel"))
		{
			currentlySelected = null;
			DisableAllInfocards();
		}
	}

	private void DisableAllInfocards()
	{
		selectionInfocard.SetActive(false);
	}
}
