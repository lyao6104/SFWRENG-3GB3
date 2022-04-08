using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGM.Gameplay;
using TMPro;

public class MaterialInfocard : MonoBehaviour
{
	public TMP_Text nameLabel;
	public Image iconImage;

	private InventoryItem material;

	public void Init(InventoryItem material)
	{
		this.material = material;

		nameLabel.text = string.Format("{0} x {1}", this.material.name, this.material.count);
		iconImage.sprite = this.material.sprite;
	}
}
