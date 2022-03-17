using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterLib;

public class GameControllerScript : MonoBehaviour
{
	public CameraScript cameraScript;
	public Collider2D cameraBounds;

	private void Start()
	{
		cameraScript.SetBounds(cameraBounds.bounds);
	}
}
