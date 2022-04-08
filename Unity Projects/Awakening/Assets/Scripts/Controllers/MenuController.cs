using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
	public GameObject audioControllerPrefab;
	public GameObject creditsScreen, instructionsScreen;

	private void Start()
	{
		Application.targetFrameRate = 60;

		if (GameObject.FindGameObjectWithTag("AudioController") == null)
		{
			Instantiate(audioControllerPrefab);
		}
	}

	public void Play()
	{
		SceneManager.LoadScene("MainScene");
	}

	public void ToggleCredits()
	{
		creditsScreen.SetActive(!creditsScreen.activeSelf);
	}

	public void ToggleInstructions()
	{
		instructionsScreen.SetActive(!instructionsScreen.activeSelf);
	}

	public void ExitGame()
	{
		Application.Quit();
	}
}
