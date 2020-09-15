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
        ItemDataBase _instance;

        public ItemDataBase Manager
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ItemDataBase>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("ItemDataBase").AddComponent<ItemDataBase>();
                    }
                    _instance.Init();
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        [SerializeField] ItemDataCollection collection = null;
        [SerializeField] Recipes _combinations = null;

        Dictionary<int, ItemData> _dataBase = new Dictionary<int, ItemData>();

        public void Init()
        {
            if (collection == null)
                collection = ScriptableObject.CreateInstance<ItemDataCollection>();
            if (_combinations == null)
                _combinations = ScriptableObject.CreateInstance<Recipes>();

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
        public ItemData getItemData(int itemID)
        {
            if (_dataBase.ContainsKey(itemID))
                return _dataBase[itemID];

            return null;
        }

        //FactoryMethod. Retorna un prefab dado una combinación de Objetos, si eso es posible. 
        //Es la base de las combinaciones.


        //Checker. Chequea si existe la combinación entre 2 objetos devolviendo true o false.

    }
}
