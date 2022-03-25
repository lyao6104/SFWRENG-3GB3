using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControllerScript : MonoBehaviour
{
	public GameObject instructionsPanel, creditsPanel;

	private void Start()
	{
		Application.targetFrameRate = 60;
	}

	public void Play()
	{
		SceneManager.LoadScene("IntroScene");
	}

	public void Exit()
	{
		Application.Quit();
	}

	public void ToggleInstructions()
	{
		instructionsPanel.SetActive(!instructionsPanel.activeSelf);
	}

	public void ToggleCredits()
	{
		creditsPanel.SetActive(!creditsPanel.activeSelf);
	}
}
