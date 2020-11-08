using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core.InventorySystem;

[CustomEditor(typeof(Recipes))]
public class RecipesDataBaseEditor : Editor
{
    ItemDataObject a = null;
    ItemDataObject b = null;
    ItemDataObject result = null;

    int selection = 0;
    int pickerID = 0;
    ItemDataObject selectedInPicker;

    Recipes inspected = null;

    private void OnEnable()
    {
        inspected = target as Recipes;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawCombinationEditor();

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        //EditorGUILayout.PropertyField(serializedObject.FindProperty("combinations"));
        base.OnInspectorGUI();
    }

    public void DrawCombinationEditor()
    {
        //Manual Add && Item Search and Add.

        GUIStyle tx = new GUIStyle(); //Text Styling.
        tx.alignment = TextAnchor.MiddleCenter;
        tx.normal.textColor = Color.white;

        float maxWidth = Screen.width / 3;
        GUILayoutOption[] renderingOptions = new GUILayoutOption[]
        {
            GUILayout.ExpandWidth(true),
            GUILayout.MaxWidth(maxWidth - 10),
        };

        //  Label  ||  Label   ||  Label
        //    ID   ||    ID    ||    ID 
        //   Pick  ||   Pick   ||   Pick

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Componente A", tx, renderingOptions);
        string selectedAName = a == null ? "None" : a.ID.ToString();
        EditorGUILayout.LabelField(selectedAName, tx, renderingOptions);
        if (GUILayout.Button("Pick", renderingOptions))
        {
            selection = 1;
            int id = EditorGUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<ItemDataObject>(null, false, "", id);
            pickerID = id;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Componente B", tx, renderingOptions);
        string selectedBName = b == null ? "None" : b.ID.ToString();
        EditorGUILayout.LabelField(selectedBName, tx, renderingOptions);
        if (GUILayout.Button("Pick", renderingOptions))
        {
            selection = 2;
            int id = EditorGUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<ItemDataObject>(null, false, "", id);
            pickerID = id;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Resultado", tx, renderingOptions);
        string selectedResultName = result == null ? "None" : result.ID.ToString();
        EditorGUILayout.LabelField(selectedResultName, tx, renderingOptions);
        if (GUILayout.Button("Pick", renderingOptions))
        {
            selection = 3;
            int id = EditorGUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<ItemDataObject>(null, false, "", id);
            pickerID = id;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        string commandName = Event.current.commandName;
        if (commandName == "ObjectSelectorUpdated")
        {
            selectedInPicker = (ItemDataObject)EditorGUIUtility.GetObjectPickerObject();

            switch (selection)
            {
                case 1:
                    a = selectedInPicker;
                    break;
                case 2:
                    b = selectedInPicker;
                    break;
                case 3:
                    result = selectedInPicker;
                    break;
                default:
                    break;
            }

            Repaint();
        }
        else if (commandName == "ObjectSelectorClosed")
        {
            EditorGUIUtility.GetObjectPickerControlID();
            selectedInPicker = null;
        }

        if (GUILayout.Button("Add Combination"))
        {
            Recipe _comb = new Recipe()
            {
               A = a == null ? ItemID.nulo : a.ID,
               B = b == null ? ItemID.nulo : b.ID,
               Result = result == null ? ItemID.nulo : result.ID
            };

            Undo.RecordObject(inspected, "Added new Combination");
            serializedObject.Update();
            EditorUtility.SetDirty(inspected);
            inspected.combinations.Add(_comb);

            a = null;
            b = null;
            result = null;
        }

        //Remove Combination by ID. //TODO if Necessary!!
    }

    [MenuItem("Utility/Select Recipes Container")]
    public static void SelectRecipesContainer()
    {
        Recipes obj = (Recipes)AssetDatabase.LoadAssetAtPath("Assets/Data/ItemDataBase/Recipes.asset", typeof(Recipes));
        if (obj != null)
            Selection.activeObject = obj;
    }
}
