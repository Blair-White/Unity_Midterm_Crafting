using System.Collections;
using System.Collections.Generic;
using UnityEngine;



 [CreateAssetMenu(fileName = "CraftingRecipe", menuName = "ScriptableObjects/CraftingRecipe", order = 3)]
public class CraftingRecipe : ScriptableObject
{
    [SerializeField]
    public Item[] Components;
    [SerializeField]
    public Item Output;
   
}
