// Modified from Nightscape's camera system.

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	public float speed, actualSpeed, zoomSpeed, minZoom, maxZoom, minSpeedMult = 0.5f, maxSpeedMult = 2;
	public float goToObjectTime, goToObjectCancelThreshold = 1f;

	private CinemachineVirtualCamera myCamera;
	private float speedMult = 1, origZoom = 0;
	private Bounds camBounds;

	private GameObject goToTargetObject;
	private Vector3 goToObjectStartPos = Vector3.zero;
	private float goToLerpPercentage = 0;
	private bool goingToObject = false;

	// Start is called before the first frame update
	private void Start()
	{
		myCamera = GameObject.FindGameObjectWithTag("CMVCam").GetComponent<CinemachineVirtualCamera>();
		origZoom = myCamera.m_Lens.OrthographicSize;
		actualSpeed = speed;
	}

	// This is done in Update for smoother zooming
	private void Update()
	{
		myCamera.m_Lens.OrthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
		myCamera.m_Lens.OrthographicSize = Mathf.Clamp(myCamera.m_Lens.OrthographicSize, minZoom, maxZoom);

		if (Mathf.Abs(myCamera.m_Lens.OrthographicSize - origZoom) < 1e-6)
		{
			speedMult = 1;
		}
		else if (myCamera.m_Lens.OrthographicSize > origZoom)
		{
			speedMult = 1 + (myCamera.m_Lens.OrthographicSize - origZoom) / (maxZoom - origZoom) * (maxSpeedMult - 1);
		}
		else
		{
			speedMult = 1 + (origZoom - myCamera.m_Lens.OrthographicSize) / (origZoom - minZoom) * (minSpeedMult - 1);
		}
	}

	// Camera movement
	private void FixedUpdate()
	{
		actualSpeed = speed * speedMult;
		Vector3 boundExtents = camBounds.extents;
		Vector3 deltaPos = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * actualSpeed;
		Debug.Log(deltaPos);
		if (Mathf.Abs(transform.position.x + deltaPos.x - camBounds.center.x) > boundExtents.x)
		{
			deltaPos.x = 0;
		}
		if (Mathf.Abs(transform.position.y + deltaPos.y - camBounds.center.y) > boundExtents.y)
		{
			deltaPos.y = 0;
		}
		transform.position += deltaPos;

		if (goingToObject && (goToLerpPercentage > 1.0f) || deltaPos.magnitude > goToObjectCancelThreshold)
		{
			goingToObject = false;
			goToLerpPercentage = 0;
		}
		else if (goingToObject)
		{
			transform.position = Vector3.Lerp(goToObjectStartPos, goToTargetObject.transform.position, goToLerpPercentage);
			goToLerpPercentage += Time.deltaTime / goToObjectTime;
		}
	}

	public void GoToObject(GameObject target)
	{
		goToObjectStartPos = transform.position;
		goToTargetObject = target;
		goingToObject = true;
	}

	public void SetBounds(Bounds newBounds)
	{
		camBounds = newBounds;
	}
}
