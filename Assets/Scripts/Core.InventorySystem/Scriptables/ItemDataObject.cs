using System;
using UnityEngine;

namespace Core.InventorySystem
{
    [Serializable]
    public struct ItemData
    {
        public string Name;
        [TextArea]
        public string Description;
        public Texture2D Icon;

        public bool isCombinable;
        public bool isDropeable;
        public bool isThroweable;
        public bool isConsumable;

        public static ItemData defaultItemData()
        {
            var data = new ItemData();

            data.Name = "";
            data.Description = "";
            data.Icon = null;
            data.isCombinable = false;
            data.isDropeable = false;
            data.isThroweable = false;
            data.isConsumable = false;
            return data;
        }
    }

    [Serializable, CreateAssetMenu(fileName = "new Item", menuName = "Item DataBase/new Item", order = 2)]
    public class ItemDataObject : ScriptableObject
    {
        public ItemID ID;
        public ItemData data = new ItemData();
        public GameObject[] inGamePrefabs;
        public GameObject GetRandomInGamePrefab()
        {
            if (inGamePrefabs.Length == 1)
                return inGamePrefabs[0];

            if (inGamePrefabs.Length > 1)
            {
                int resultIndex = UnityEngine.Random.Range(0, inGamePrefabs.Length);
                return inGamePrefabs[resultIndex];
            }

            return null;
        }
    }
}
