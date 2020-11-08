using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.InventorySystem
{
    public enum CombinationMethod
    {
        replace,
        changeSourceState
    }

    [System.Serializable]
    public struct Recipe
    {
        public string Name;
        public ItemID Result;
        [Header("Componentes")]
        public ItemID A;
        public ItemID B;
        [Header("Settings")]
        public CombinationMethod combinationMethod;

        public static Recipe defaultRecipe()
        {
            var recipe = new Recipe();

            recipe.Name = ItemID.nulo.ToString();
            recipe.A = ItemID.nulo;
            recipe.B = ItemID.nulo;
            recipe.Result = ItemID.nulo;
            recipe.combinationMethod = CombinationMethod.changeSourceState;
            return recipe;
        }
        /// <summary>
        /// Chequea si ambos ID están contenidos.
        /// </summary>
        /// <param name="A">ID item A</param>
        /// <param name="B">ID item B</param>
        /// <returns>Retorna true si ambos ID coínciden</returns>
        public bool checkIn(ItemID A, ItemID B)
        {
            return this.A == A && this.B == B || this.A == B && this.B == A;
        }
    }

    [CreateAssetMenu(fileName = "RecipesDatabase", menuName = "Item DataBase/Recipe DataBase", order = 3)]
    public class Recipes : ScriptableObject
    {
        public List<Recipe> combinations = new List<Recipe>();
    }
}
