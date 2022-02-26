using System.Collections.Generic;
using System.Linq;
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
                    _instance.Init();
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        [SerializeField] ItemDataCollection collection = null;
        [SerializeField] Recipes _recipes = null;

        Dictionary<int, ItemDataObject> _dataBase = new Dictionary<int, ItemDataObject>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(gameObject);
            else
            {
                _instance = this;
                Init();
                DontDestroyOnLoad(_instance.gameObject);
            }
        }

        public void Init()
        {
            if (collection == null)
                collection = ScriptableObject.CreateInstance<ItemDataCollection>();
            if (_recipes == null)
                _recipes = ScriptableObject.CreateInstance<Recipes>();

            _dataBase = new Dictionary<int, ItemDataObject>();

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
        public static ItemData getItemData(ItemID itemID)
        {
            if (Manager._dataBase.ContainsKey((int)itemID))
                return Manager._dataBase[(int)itemID].data;

            return ItemData.defaultItemData();
        }
        /// <summary>
        /// Retorna una receta.
        /// </summary>
        /// <param name="a">Identificador del item A</param>
        /// <param name="b">Identificador del item A</param>
        /// <returns>Una receta vacía si no existe una combinación registrada para los objetos dados.</returns>
        public static Recipe getRecipe(ItemID a, ItemID b)
        {
            var recipe = Manager._recipes.combinations.Where(x => x.checkIn(a, b))
                                                      .DefaultIfEmpty(Recipe.defaultRecipe())
                                                      .First();

            return recipe;
        }
        /// <summary>
        /// Función de utilidad que permite obtener un Prefab.
        /// </summary>
        /// <param name="ID">Identificador único del item.</param>
        /// <returns>Una referencia a un prefab válido de dicho objeto.</returns>
        public static GameObject getRandomItemPrefab(ItemID ID)
        {
            var manager = Manager;
            if (manager._dataBase.ContainsKey((int)ID))
                return manager._dataBase[(int)ID].GetRandomInGamePrefab();

            return null;
        }
    }
}
