using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperlaneScript : MonoBehaviour
{
	public Material defaultMat, selectedMat;

	private LineRenderer lr;

	public void Init(Vector3 from, Vector3 to)
	{
		Vector3[] positions = new Vector3[] { from, to };
		lr = GetComponent<LineRenderer>();
		lr.SetPositions(positions);
		lr.material = defaultMat;
	}

	public void Select()
	{
		lr.material = selectedMat;
	}

	public void Deselect()
	{
		lr.material = defaultMat;
	}
}
