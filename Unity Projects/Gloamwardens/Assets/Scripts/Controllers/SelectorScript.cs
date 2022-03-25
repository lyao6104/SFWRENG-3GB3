using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using CharacterLib;

public class SelectorScript : MonoBehaviour
{
	public LayerMask raycastLayers, pathObstacleLayers;
	public GameObject currentlySelected;
	public GameObject selectionInfocard;
	public InputAction mouseClick, cancel;

	public GameObject deployIndicatorPrefab;

	private PartyScript toDeploy;
	private GameObject deployIndicator;

	private void OnEnable()
	{
		mouseClick.Enable();
		cancel.Enable();

		mouseClick.performed += OnClick;
		cancel.performed += OnCancel;
	}

	private void OnDisable()
	{
		mouseClick.performed -= OnClick;
		cancel.performed -= OnCancel;

		mouseClick.Disable();
		cancel.Disable();
	}

	private void OnClick(InputAction.CallbackContext context)
	{
		//Debug.Log("Click");
		Vector3 mousePos = Mouse.current.position.ReadValue();
		RaycastHit2D mouseHit;
		if (!Camera.main.orthographic)
		{
			mousePos.z = -Camera.main.transform.position.z;
		}
		mouseHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero, Mathf.Infinity, raycastLayers);
		//if (mouseHit.collider != null)
		//{
		//	Debug.Log(string.Format("Raycast: {0}", mouseHit.transform.name));
		//}
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			if (toDeploy != null)
			{
				mouseHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero, Mathf.Infinity, pathObstacleLayers);
				if (mouseHit.collider == null)
				{
					toDeploy.Deploy(Camera.main.ScreenToWorldPoint(mousePos));
					toDeploy = null;
					Destroy(deployIndicator);
				}
			}
			else if (mouseHit.collider != null && mouseHit.collider.gameObject != currentlySelected)
			{
				Deselect();
				Select(mouseHit.collider.gameObject);
			}
			else if (mouseHit.collider == null || mouseHit.collider.gameObject != currentlySelected)
			{
				Deselect();
			}
		}
		//Debug.DrawLine(Camera.main.transform.position, Camera.main.ScreenToWorldPoint(mousePos), Color.green, 10, false);	
	}

	private void OnCancel(InputAction.CallbackContext context)
	{
		//Debug.Log("Cancel");
		Deselect();
	}

	public void StartDeploying(PartyScript party)
	{
		toDeploy = party;
		deployIndicator = Instantiate(deployIndicatorPrefab);
	}

	public void Deselect()
	{
		if (currentlySelected != null)
		{
			currentlySelected = null;
		}
		selectionInfocard.SetActive(false);
		GameControllerScript.GetController().skillPanel.SetActive(false);
	}

	private void Select(GameObject toSelect)
	{
		currentlySelected = toSelect;
		Character selectedCharacter =  toSelect.CompareTag("Enemy") ? currentlySelected.GetComponent<EnemyScript>().characterData :
			currentlySelected.GetComponent<AdventurerScript>().characterData;
		if (selectedCharacter != null)
		{
			selectionInfocard.SetActive(true);
			selectionInfocard.GetComponent<InfocardScript>().Init(selectedCharacter);
		}
	}
}
