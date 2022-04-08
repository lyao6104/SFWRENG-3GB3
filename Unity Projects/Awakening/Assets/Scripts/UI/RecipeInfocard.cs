using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ItemLib;
using TMPro;

public class RecipeInfocard : MonoBehaviour
{
	public TMP_Text nameLabel, typeLabel, dmgLabel;
	public Image iconImage;

	private Recipe recipe;
	private CraftingScreen craftingUI;

	public void Init(Recipe recipe, CraftingScreen craftingScreen)
	{
		craftingUI = craftingScreen;
		this.recipe = recipe;

		nameLabel.text = this.recipe.product.name;
		typeLabel.text = this.recipe.product.type.ToString();
		dmgLabel.text = string.Format("{0} Damage", this.recipe.product.damage);
		iconImage.sprite = this.recipe.product.icon;
	}

	// Runs on the infocard that was clicked
	public void ButtonClicked()
	{
		if (recipe == null || recipe.product.name.Length < 1)
		{
			return;
		}

		craftingUI.SelectRecipe(recipe);
	}
}
