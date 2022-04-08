using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGM.Gameplay;
using ItemLib;
using TMPro;

public class CraftingScreen : MonoBehaviour
{
	public Transform recipesContentTransform;
	public GameObject recipeInfocardPrefab, materialInfocardPrefab;

	public Transform materialsContentTransform;
	public TMP_Text nameLabel, statsLabel;
	private Recipe selectedRecipe;

	[SerializeField]
	private List<Recipe> recipes = new List<Recipe>();

	private CustomGameController gc;

	private void Start()
	{
		gc = CustomGameController.GetController();

		for (int i = 0; i < recipes.Count; i++)
		{
			RecipeInfocard newInfocard = Instantiate(recipeInfocardPrefab, recipesContentTransform).GetComponent<RecipeInfocard>();
			newInfocard.Init(recipes[i], this);
		}
	}

	private void OnDisable()
	{
		WeaponInfocard[] infocards = recipesContentTransform.GetComponentsInChildren<WeaponInfocard>();
		for (int i = 0; i < infocards.Length; i++)
		{
			Destroy(infocards[i].gameObject);
		}
		MaterialInfocard[] matInfocards = materialsContentTransform.GetComponentsInChildren<MaterialInfocard>();
		for (int i = 0; i < matInfocards.Length; i++)
		{
			Destroy(matInfocards[i].gameObject);
		}
	}

	public void CraftRecipe()
	{
		if (selectedRecipe == null)
		{
			return;
		}

		selectedRecipe.Craft();
	}

	public void SelectRecipe(Recipe recipe)
	{
		MaterialInfocard[] matInfocards = materialsContentTransform.GetComponentsInChildren<MaterialInfocard>();
		for (int i = 0; i < matInfocards.Length; i++)
		{
			Destroy(matInfocards[i].gameObject);
		}

		selectedRecipe = recipe;
		nameLabel.text = recipe.product.name;
		statsLabel.text = string.Format("Damage Type: {0}   Damage: {1}", recipe.product.type.ToString(), recipe.product.damage);
		for (int i = 0; i < recipe.materials.Count; i++)
		{
			MaterialInfocard newInfocard = Instantiate(materialInfocardPrefab, materialsContentTransform).GetComponent<MaterialInfocard>();
			newInfocard.Init(recipe.materials[i]);
		}
	}
}
