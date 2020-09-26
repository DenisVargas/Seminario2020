using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.InventorySystem
{
    public class ItemDataBase : MonoBehaviour
    {
        static ItemDataBase _instance;

        public static ItemDataBase Manager
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ItemDataBase>();
                    if (_instance == null)
                        _instance = new GameObject("ItemDataBase").AddComponent<ItemDataBase>();
                }

                _instance.Init();
                DontDestroyOnLoad(_instance.gameObject);
                return _instance;
            }
        }

        [SerializeField] ItemDataCollection collection = null;
        [SerializeField] Recipes _recipes = null;

        Dictionary<int, ItemData> _dataBase = new Dictionary<int, ItemData>();

        public void Init()
        {
            if (collection == null)
                collection = ScriptableObject.CreateInstance<ItemDataCollection>();
            if (_recipes == null)
                _recipes = ScriptableObject.CreateInstance<Recipes>();

            _dataBase = new Dictionary<int, ItemData>();

            foreach (var idata in collection.existingItemData)
            {
                int id = (int)idata.ID;
                if (!_dataBase.ContainsKey(id))
                    _dataBase.Add(id, idata);
            }
        }

        /// <summary>
        /// Retorna la data correspondiente a un Item dado su ID.
        /// </summary>
        /// <param name="itemID">Identificador único del item.</param>
        /// <returns>Null si el item no existe dentro de la base de datos.</returns>
        public static ItemData getItemData(int itemID)
        {
            if (Manager._dataBase.ContainsKey(itemID))
                return Manager._dataBase[itemID];

            return null;
        }

        /// <summary>
        /// Combina 2 items dados los ID´s y retorna un prefab del Item resultante.
        /// </summary>
        /// <param name="a">Identificador del primer Item</param>
        /// <param name="b">Identificador del segundo Item</param>
        /// <returns>Null si no hay una combinación válida</returns>
        public GameObject Combine(int a, int b, out ItemData resultData)
        {
            if (CanCombineItems(a,b))
            {
                //Busco entre las combinaciones aquel que coíncida.
                var foundCombination = _recipes.combinations
                                       .Where(x => x.checkIn(a, b))
                                       .First();

                if (foundCombination.Result != -1)
                {
                    //obtengo el ID resultante y busco el item que corresponda.
                    //Retorno el item.

                    resultData = getItemData(foundCombination.Result);
                    return _dataBase[foundCombination.Result].GetRandomInGamePrefab();
                }
            }

            resultData = null;
            return null;
        }
        /// <summary>
        /// Chequea si existe la combinación entre 2 objetos
        /// </summary>
        /// <param name="a">Identificador del item A</param>
        /// <param name="b">Identificador del item A</param>
        /// <returns>false si no es posible combinar los items con los ID´s dados</returns>
        public bool CanCombineItems(int a, int b)
        {
            int combinationsPosible = _recipes.combinations.Count(x => x.checkIn(a, b));
            return combinationsPosible > 0;
        }
    }
}
