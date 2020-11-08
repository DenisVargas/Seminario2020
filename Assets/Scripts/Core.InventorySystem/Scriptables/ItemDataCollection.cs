using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core.InventorySystem;


[System.Serializable, CreateAssetMenu(fileName = "ItemCollection", menuName = "Item DataBase/ItemCollection", order = 3)]
public class ItemDataCollection : ScriptableObject
{
    public List<ItemDataObject> existingItemData = new List<ItemDataObject>();
}
