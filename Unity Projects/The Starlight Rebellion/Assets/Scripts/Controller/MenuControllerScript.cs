using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuControllerScript : MonoBehaviour
{
	public Canvas optionsCanvas, creditsCanvas;

	public TMP_InputField sysCountIF, minHLanesIF, maxHLanesIF, minDistanceIF;
	public OptionsScript gameOptions;

	public void ToggleOptions()
	{
		optionsCanvas.enabled = !optionsCanvas.enabled;
	}

	public void StartGame()
	{
		SceneManager.LoadScene("MainScene");
	}

	public void ToggleCredits()
	{
		creditsCanvas.enabled = !creditsCanvas.enabled;
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void SysCountChanged(string value)
	{
		int newVal;
		if (int.TryParse(value, out newVal))
		{
			newVal = Mathf.Clamp(newVal, 5, 25);
			sysCountIF.text = newVal.ToString();
			gameOptions.numSystems = newVal;
		}
		// Reset the text field if input is invalid.
		else
		{
			sysCountIF.text = gameOptions.numSystems.ToString();
		}
	}

	public void MinHLanesChanged(string value)
	{
		int newVal;
		if (int.TryParse(value, out newVal))
		{
			newVal = Mathf.Clamp(newVal, 1, 5);
			minHLanesIF.text = newVal.ToString();
			gameOptions.minHyperlanes = newVal;
			// Also update max hyperlanes if necessary
			if (gameOptions.maxHyperlanes < newVal)
			{
				maxHLanesIF.text = newVal.ToString();
				gameOptions.maxHyperlanes = newVal;
			}
		}
		// Reset the text field if input is invalid.
		else
		{
			minHLanesIF.text = gameOptions.numSystems.ToString();
		}
	}

	public void MaxHLanesChanged(string value)
	{
		int newVal;
		if (int.TryParse(value, out newVal))
		{
			newVal = Mathf.Clamp(newVal, 2, 5);
			maxHLanesIF.text = newVal.ToString();
			gameOptions.maxHyperlanes = newVal;
			// Also update min hyperlanes if necessary
			if (gameOptions.minHyperlanes > newVal)
			{
				minHLanesIF.text = newVal.ToString();
				gameOptions.minHyperlanes = newVal;
			}
		}
		// Reset the text field if input is invalid.
		else
		{
			maxHLanesIF.text = gameOptions.numSystems.ToString();
		}
	}

	public void MinDistanceChanged(string value)
	{
		float newVal;
		if (float.TryParse(value, out newVal))
		{
			newVal = Mathf.Clamp(newVal, 5, 10);
			minDistanceIF.text = newVal.ToString();
			gameOptions.minDistance = newVal;
		}
		// Reset the text field if input is invalid.
		else
		{
			minDistanceIF.text = gameOptions.numSystems.ToString();
		}
	}
}
