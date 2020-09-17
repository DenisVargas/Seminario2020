using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.InventorySystem
{
    [System.Serializable]
    public struct Combination
    {
        public int A;
        public int B;
        public int Result;

        /// <summary>
        /// Chequea si ambos ID están contenidos.
        /// </summary>
        /// <param name="A">ID item A</param>
        /// <param name="B">ID item B</param>
        /// <returns>Retorna true si ambos ID coínciden</returns>
        public bool checkIn(int A, int B)
        {
            return this.A == A && this.B == B || this.A == B && this.B == A;
        }
    }

    [CreateAssetMenu(fileName = "RecipesDatabase", menuName = "Item DataBase/Recipe DataBase", order = 3)]
    public class Recipes : ScriptableObject
    {
        public List<Combination> combinations = new List<Combination>();
    }
}
