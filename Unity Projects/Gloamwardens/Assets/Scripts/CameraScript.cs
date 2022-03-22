// Modified from Nightscape's camera system.

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
	public float speed, actualSpeed, zoomSpeed, minZoom, maxZoom, minSpeedMult = 0.5f, maxSpeedMult = 2;
	public float goToObjectTime, goToObjectCancelThreshold = 1f;

	private CinemachineVirtualCamera virtualCamera;
	private CinemachineInputProvider inputProvider;
	private Transform cameraTransform;
	private float speedMult = 1, origZoom = 0;
	private Vector2 keyboardDelta = Vector2.zero;
	private Vector2 panDirection = Vector2.zero;
	private Vector2 deltaPos = Vector2.zero;
	private Bounds camBounds;

	private GameObject goToTargetObject;
	private Vector3 goToObjectStartPos = Vector3.zero;
	private float goToLerpPercentage = 0;
	private bool goingToObject = false;

	// Start is called before the first frame update
	private void Start()
	{
		virtualCamera = GetComponent<CinemachineVirtualCamera>();
		inputProvider = GetComponent<CinemachineInputProvider>();
		cameraTransform = virtualCamera.VirtualCameraGameObject.transform;
		origZoom = virtualCamera.m_Lens.OrthographicSize;
		actualSpeed = speed;
	}

	// This is done in Update for smoother zooming
	private void Update()
	{
		if (!Application.isFocused)
		{
			return;
		}

		// Fine because it's normalized
		panDirection = GetMousePanDirection(inputProvider.GetAxisValue(0), inputProvider.GetAxisValue(1));
		panDirection += keyboardDelta.normalized;
		PanCamera(panDirection);

		int deltaZoom = (int)inputProvider.GetAxisValue(2);
		virtualCamera.m_Lens.OrthographicSize -= deltaZoom * zoomSpeed * Time.deltaTime;
		virtualCamera.m_Lens.OrthographicSize = Mathf.Clamp(virtualCamera.m_Lens.OrthographicSize, minZoom, maxZoom);
		if (deltaZoom != 0)
		{
			Vector3 boundExtents = camBounds.extents;
			float camHExtent = virtualCamera.m_Lens.Aspect * virtualCamera.m_Lens.OrthographicSize;
			float camVExtent = virtualCamera.m_Lens.OrthographicSize;
			bool badX = Mathf.Abs(cameraTransform.position.x - camBounds.center.x) > boundExtents.x - camHExtent;
			bool badY = Mathf.Abs(cameraTransform.position.y - camBounds.center.y) > boundExtents.y - camVExtent;
			Vector3 camPosition = cameraTransform.position;
			if (badX)
			{
				camPosition.x = Camera.main.transform.position.x;
			}
			if (badY)
			{
				camPosition.y = Camera.main.transform.position.x;
			}
			cameraTransform.position = camPosition;
		}

		if (Mathf.Abs(virtualCamera.m_Lens.OrthographicSize - origZoom) < 1e-6)
		{
			speedMult = 1;
		}
		else if (virtualCamera.m_Lens.OrthographicSize > origZoom)
		{
			speedMult = 1 + (virtualCamera.m_Lens.OrthographicSize - origZoom) / (maxZoom - origZoom) * (maxSpeedMult - 1);
		}
		else
		{
			speedMult = 1 + (origZoom - virtualCamera.m_Lens.OrthographicSize) / (origZoom - minZoom) * (minSpeedMult - 1);
		}
	}

	// Camera movement
	private void FixedUpdate()
	{
		actualSpeed = speed * speedMult;
		//Vector3 boundExtents = camBounds.extents;
		//Vector3 deltaPos = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * actualSpeed;
		//Debug.Log(deltaPos);
		//if (Mathf.Abs(transform.position.x + deltaPos.x - camBounds.center.x) > boundExtents.x)
		//{
		//	deltaPos.x = 0;
		//}
		//if (Mathf.Abs(transform.position.y + deltaPos.y - camBounds.center.y) > boundExtents.y)
		//{
		//	deltaPos.y = 0;
		//}
		//transform.position += deltaPos;

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

	private Vector2 GetMousePanDirection(float x, float y)
	{
		Vector2 dir = Vector2.zero;
		if (Mathf.Abs(keyboardDelta.y) < 1e-6f)
		{
			if (y > Screen.height * 0.99f)
			{
				dir.y = 1;
			}
			else if (y < Screen.height * 0.01f)
			{
				dir.y = -1;
			}
		}
		if (Mathf.Abs(keyboardDelta.x) < 1e-6f)
		{
			if (x > Screen.width * 0.99f)
			{
				dir.x = 1;
			}
			else if (x < Screen.width * 0.01f)
			{
				dir.x = -1;
			}
		}
		return dir.normalized;
	}

	public void KeyboardPan(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			keyboardDelta = context.ReadValue<Vector2>();
			//Debug.Log(keyboardDelta);
		}
	}

	private void PanCamera(Vector2 direction)
	{
		if (!Application.isFocused)
		{
			return;
		}

		direction = direction.normalized;
		deltaPos = direction * actualSpeed * Time.deltaTime;
		Vector3 boundExtents = camBounds.extents;
		float camHExtent = virtualCamera.m_Lens.Aspect * virtualCamera.m_Lens.OrthographicSize;
		float camVExtent = virtualCamera.m_Lens.OrthographicSize;
		if (Mathf.Abs(cameraTransform.position.x + deltaPos.x - camBounds.center.x) > boundExtents.x - camHExtent)
		{
			deltaPos.x = 0;
		}
		if (Mathf.Abs(cameraTransform.position.y + deltaPos.y - camBounds.center.y) > boundExtents.y - camVExtent)
		{
			deltaPos.y = 0;
		}
		cameraTransform.position += (Vector3)deltaPos;
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
