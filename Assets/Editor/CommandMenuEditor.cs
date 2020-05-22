using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(CommandMenu))]
class CommandMenuEditor : Editor
{
    CommandMenu ins;

    //Adding.
    CommandMenuItemData itemData = null;

    //Removing 
    int toRemoveIndex = 0;

    //Para cargar la data
    const string _comandDataPath = "Data/Commands";
    bool showDataBase = true;

    private void OnEnable()
    {
        ins = target as CommandMenu;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_optionPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_verticalScroll"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_Content"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("posOffset"));

        EditorGUILayout.Space();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        //Dibujamos el editor de la database personalizado.
        DrawDatabaseDisplay();
        DrawDatabaseEditor();
    }

    private void DrawDatabaseDisplay()
    {
        EditorGUILayout.LabelField("Preset Database");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showDataBase = EditorGUILayout.BeginFoldoutHeaderGroup(showDataBase, "Database");


        if (ins.presetDataBase == null || ins.presetDataBase.Length == 0)
        {
            EditorGUILayout.HelpBox("The Databse is empty", MessageType.Info);
        }


        if (showDataBase)
        {
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;

            GUILayoutOption[] labelDisplayOp = new GUILayoutOption[] { GUILayout.MaxWidth(80f), GUILayout.Height(16f) };
            GUILayoutOption[] heightCorrection = new GUILayoutOption[] { GUILayout.Height(16f) };

            if (ins.presetDataBase.Length > 0)
            {
                foreach (var item in ins.presetDataBase)
                {
                    //string value = Enum.GetName(typeof(OperationOptions), item.ID);

                    if (item != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.SelectableLabel(item.Operation.ToString(), labelDisplayOp);
                        EditorGUILayout.ObjectField(item, typeof(CommandMenuItemData), false, heightCorrection);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        Debug.LogWarning("item es nulo");
                    }
                }
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.EndVertical();
    }

    public void DrawDatabaseEditor()
    {
        //Custom edition by modifiing the object instance.

        //------------------------------------------------- Add Element -----------------------------------------------------
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Edit Database");
        EditorGUILayout.Space();

        //EditorGUI.BeginChangeCheck();
        //Funcionalidad para añadir.
        //EditorGUILayout.EnumPopup(value);
        itemData = EditorGUILayout.ObjectField(itemData, typeof(CommandMenuItemData), false) as CommandMenuItemData;
        GUI.color = Color.green;

        EditorGUILayout.IntField("Valores actuales", ins.presetDataBase.Length);

        if (GUILayout.Button("Add individual"))
        {
            Undo.RecordObject(ins, "Added value");
            if (ins.presetDataBase.Length == 0)
            {
                ins.presetDataBase = new CommandMenuItemData[] { itemData };
            }
            else
            {
                //Chequear esto.
                var newValueS = new CommandMenuItemData[ins.presetDataBase.Length + 1];

                newValueS[newValueS.Length - 1] = itemData;
                for (int i = 0; i < newValueS.Length - 1; i++)
                {
                    newValueS[i] = ins.presetDataBase[i];
                }

                ins.presetDataBase = newValueS;
            }

            //value = OperationOptions.Activate;
        }
        GUI.color = Color.white;

        //if (EditorGUI.EndChangeCheck())
        //{
        //    serializedObject.ApplyModifiedProperties();
        //}
        EditorGUILayout.Space();

        //------------------------------------------------- Load Element -----------------------------------------------------

        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.EnumPopup(value);

        GUI.color = Color.green;
        if (GUILayout.Button("Load all Data from Data Folder"))
        {
            Undo.RecordObject(ins, "Added All Values");
            //Cargo los scriptable objects.
            var loadedAssets = GetAllAssetPathsAt<CommandMenuItemData>(_comandDataPath);

            //Relleno _menuFreeContent con 1 prefab de cada comando disponible.
            if (loadedAssets != null && loadedAssets.Length > 0)
            {
                ins.presetDataBase = new CommandMenuItemData[loadedAssets.Length];
                for (int i = 0; i < loadedAssets.Length; i++)
                {
                    ins.presetDataBase[i] = loadedAssets[i];
                }
                Repaint();
            }
            else
            {
                EditorGUILayout.HelpBox("The Database Folder is Empty", MessageType.Info);
            }
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

        //Removing Elements.
        //----------------------------------------- Remove one Element by Index -----------------------------------------------------

        EditorGUILayout.BeginHorizontal();
        GUILayoutOption[] remove_Index_LayoutOptions = new GUILayoutOption[] { GUILayout.MaxWidth(50f), GUILayout.Height(16f) };
        GUILayoutOption[] remove_Button_LayoutOptions = new GUILayoutOption[] { GUILayout.Height(16f) };
        toRemoveIndex = EditorGUILayout.IntField(toRemoveIndex, remove_Index_LayoutOptions);
        GUI.color = Color.red;
        if (GUILayout.Button("Remove at Index", remove_Button_LayoutOptions))
        {
            Undo.RecordObject(ins, "Removed Value at index");
            ins.presetDataBase = new CommandMenuItemData[0];
        }
        EditorGUILayout.EndHorizontal();

        //------------------------------------------------- Clean all Elements -----------------------------------------------------
        if (GUILayout.Button("Clear All"))
        {
            Undo.RecordObject(ins, "Cleared Database");
            ins.presetDataBase = new CommandMenuItemData[0];
        }
    }

    private T[] GetAllAssetPathsAt<T>(string relativePath)
    {
        string[] fileEntries = Directory.GetFiles(string.Format("{0}/{1}", Application.dataPath, relativePath));
        List<T> outputAssets = new List<T>();

        foreach (var FileName in fileEntries)
        {
            if (FileName.EndsWith(".meta")) continue;

            string temp = FileName.Replace("\\", "/");
            int index = temp.LastIndexOf("/");
            string localPath = "Assets/" + relativePath;

            if (index > 0)
                localPath += temp.Substring(index);

            object loadedAsset = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

            if (loadedAsset != null)
                outputAssets.Add((T)loadedAsset);
        }
        return outputAssets.ToArray();
    }
}
