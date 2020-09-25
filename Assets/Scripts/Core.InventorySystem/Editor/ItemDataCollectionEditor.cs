using UnityEngine;
using UnityEditor;
using Core.InventorySystem;

[CustomEditor(typeof(ItemDataCollection))]
public class ItemDataCollectionEditor : Editor
{
    const string _dataPath =  "Data/ItemDataBase/Items";

    public override void OnInspectorGUI()
    {
        DrawAutoLoad();
        base.OnInspectorGUI();
    }

    private void DrawAutoLoad()
    {
        if (GUILayout.Button("Update Database"))
        {
            //Lo hago de esta forma para que la misma funcionalidad este disponible desde el menú principal.
            UpdateDatabase();
        }
    }

    [MenuItem("Utility/Update Database")]
    private static void UpdateDatabase()
    {
        var itemcoll = AssetDatabase.LoadAssetAtPath<ItemDataCollection>("Assets/Data/ItemDataBase/ItemCollection1.asset");
        var files = CustomEditorUtilities.GetAllAssetsAtRelativePath<ItemData>(_dataPath);

        if (itemcoll != null && files != null)
        {
            Undo.RecordObject(itemcoll, "Updated the Database");
            //EditorUtility.SetDirty(itemcoll);
            itemcoll.existingItemData.Clear();
            itemcoll.existingItemData.AddRange(files);
            AssetDatabase.SaveAssets();
        }
    }
}