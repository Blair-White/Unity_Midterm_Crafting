using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe Table", menuName = "ScriptableObjects/RecipeTable", order = 4)]
public class RecipeTable : ScriptableObject
{

    [SerializeField]
    public CraftingRecipe[] RecipeList;


}
