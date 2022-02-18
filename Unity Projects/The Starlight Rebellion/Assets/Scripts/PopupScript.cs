using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupScript : MonoBehaviour
{
	public TMP_Text title, body;

	public void SetText(string titleText, string bodyText)
	{
		title.text = titleText;
		body.text = bodyText;
	}

	public void Close()
	{
		Destroy(gameObject);
	}
}
