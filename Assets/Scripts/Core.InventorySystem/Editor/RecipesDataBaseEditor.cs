using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core.InventorySystem;

[CustomEditor(typeof(Recipes))]
public class RecipesDataBaseEditor : Editor
{
    int a = 0;
    int b = 0;
    int result = 0;

    int selection = 0;
    int pickerID = 0;
    ItemData selectedInPicker;

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
        EditorGUILayout.LabelField("Item ID A", tx, renderingOptions);
        a = EditorGUILayout.IntField(a, renderingOptions);
        if (GUILayout.Button("Pick", renderingOptions))
        {
            selection = 1;
            int id = EditorGUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<ItemData>(null, false, "", id);
            pickerID = id;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Item ID B", tx, renderingOptions);
        b = EditorGUILayout.IntField(b, renderingOptions);
        if (GUILayout.Button("Pick", renderingOptions))
        {
            selection = 2;
            int id = EditorGUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<ItemData>(null, false, "", id);
            pickerID = id;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Result ID B", tx, renderingOptions);
        result = EditorGUILayout.IntField(result, renderingOptions);
        if (GUILayout.Button("Pick", renderingOptions))
        {
            selection = 3;
            int id = EditorGUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<ItemData>(null, false, "", id);
            pickerID = id;
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();

        string commandName = Event.current.commandName;
        if (commandName == "ObjectSelectorUpdated")
        {
            selectedInPicker = (ItemData)EditorGUIUtility.GetObjectPickerObject();

            switch (selection)
            {
                case 1:
                    a = selectedInPicker == null ? 0 : (int)selectedInPicker.ID;
                    break;
                case 2:
                    b = selectedInPicker == null ? 0 : (int)selectedInPicker.ID;
                    break;
                case 3:
                    result = selectedInPicker == null ? 0 : (int)selectedInPicker.ID;
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
            if (a == -1)
            { 
                Debug.LogWarning("El ID A es 0. No se admiten valores menores a 0.");
                return;
            }
            if (b == -1)
            {
                Debug.LogWarning("El ID B es 0. No se admiten valores menores a 0.");
                return;
            }
            if (result == -1)
            {
                Debug.LogWarning("El resultado es inválido. No se admiten valores menores a 0.");
                return;
            }

            Combination _comb = new Combination() {  A = a, B = b, Result = result };

            Undo.RecordObject(inspected, "Added new Combination");
            serializedObject.Update();
            EditorUtility.SetDirty(inspected);
            inspected.combinations.Add(_comb);

            a = 0;
            b = 0;
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
