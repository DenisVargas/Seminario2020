using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core.Interaction;

public class CommandDatabaseEditor : EditorWindow
{
    string defaultFolderPath = "Assets/Data/Commands";
    string CommandMenuPrefabAsset = "Assets/Prefabs/UI/MultiCommandMenu.prefab";
    bool searchInChildren = false;

    Vector2 ScrollingPosition = Vector2.zero;

    int toolbarIndex = 0;
    readonly string[] tabs = { "Commands", "Database", "Settings" };

    public OperationType Operation = OperationType.Activate;
    public Sprite Icon;
    public string CommandName = "new Command";


    [MenuItem("GamePlay/Command Data Editor")]
    public static void OpenWindow()
    {
        var currentWindow = GetWindow<CommandDatabaseEditor>();
        var content = new GUIContent();
        content.text = "Command's Data Editor";
        currentWindow.titleContent = content;
        currentWindow.Show();
    }

    private void OnGUI()
    {
        toolbarIndex = GUILayout.Toolbar(toolbarIndex, tabs);
        switch (toolbarIndex)
        {
            case 0:
                AddNewCommand_Section();
                break;
            case 1:
                DrawDatabase_Section();
                break;
            case 2:
                DrawOptions_Section();
                break;
            default:
                break;
        }
    }

    private void DrawDatabase_Section()
    {
        EditorGUILayout.Space();
        DrawTitle("Current Database", 20, Color.white);

        GUIStyle headerStyle = new GUIStyle();
        headerStyle.normal.textColor = Color.white;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 12;

        //Headers
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Operation Type", headerStyle);
        EditorGUILayout.LabelField("Command Name", headerStyle);
        EditorGUILayout.LabelField("Options", headerStyle);
        EditorGUILayout.EndHorizontal();

        //Reference Box
        //var rect = EditorGUILayout.GetControlRect();
        var rect = GUILayoutUtility.GetLastRect();
        rect.height = 200;
        rect.y += 18;
        //rect.height = 200 + EditorGUIUtility.singleLineHeight;
        GUI.Box(rect, "");

        //Acá quiero un segmento.
        GUILayoutOption[] ScrollOptions = new GUILayoutOption[] { GUILayout.MinHeight(200), GUILayout.MaxHeight(200) };
        ScrollingPosition = GUILayout.BeginScrollView(ScrollingPosition,false, true, ScrollOptions);

        //Cargo la database.
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CommandMenuPrefabAsset);
        var commandMenu = searchInChildren ? prefab.GetComponentInChildren<CommandMenu>() : prefab.GetComponent<CommandMenu>();

        int count = commandMenu.presetDataBase.Count;
        var toRemove = new List<CommandMenuItemData>();
        for(int i = 0; i < count; i++)
        {
            var current = commandMenu.presetDataBase[i];

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(current.Operation.ToString());
            EditorGUILayout.LabelField(current.CommandName);
            if (GUILayout.Button("Select"))
                Selection.activeObject = current;
            if (GUILayout.Button("Delete"))
                toRemove.Add(current);

            EditorGUILayout.EndHorizontal();
        }

        if (toRemove.Count > 0)
        {
            foreach (var item in toRemove)
            {
                commandMenu.presetDataBase.Remove(item);
                AssetDatabase.DeleteAsset($"{defaultFolderPath}/{item.CommandName}.asset");
            }

            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
        }

        GUILayout.EndScrollView();
    }
    private void DrawOptions_Section()
    {
        EditorGUILayout.Space();
        DrawTitle("Main Options", 20, Color.white);
        defaultFolderPath = EditorGUILayout.TextField("Folder Path:", defaultFolderPath);
        EditorGUILayout.LabelField("Main CommandMenu prefab Path:");
        CommandMenuPrefabAsset = EditorGUILayout.TextField(CommandMenuPrefabAsset);
        GUIContent toogleContent = new GUIContent();
        toogleContent.text = " Search MainMenu Component in Children";
        toogleContent.tooltip = "If marked as false, the editor will look up the [CommandMenu] component in the root GameObject";
        searchInChildren = GUILayout.Toggle(searchInChildren, toogleContent);
    }
    private void AddNewCommand_Section()
    {
        EditorGUILayout.Space();
        DrawTitle("New Commands", 25, Color.white);
        Operation = (OperationType)EditorGUILayout.EnumPopup("OperationType:", Operation);
        CommandName = EditorGUILayout.TextField("Command Display Name", CommandName);
        Icon = (Sprite)EditorGUILayout.ObjectField("Command Icon (Optional):", Icon, typeof(Sprite), false);

        EditorGUILayout.HelpBox("Please complete al fields.", MessageType.Info);

        if (GUILayout.Button("Add new Command"))
        {
            var command = CreateInstance<CommandMenuItemData>();
            command.Operation = Operation;
            command.CommandName = CommandName;
            command.Icon = Icon;

            string path = $"{defaultFolderPath}/{CommandName}.asset";

            AssetDatabase.CreateAsset(command, path);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<CommandMenuItemData>(path);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CommandMenuPrefabAsset);
            var CommandMenu = searchInChildren ? prefab.GetComponentInChildren<CommandMenu>() : prefab.GetComponent<CommandMenu>();
            if (!CommandMenu.presetDataBase.Contains(command))
                CommandMenu.presetDataBase.Add(command);
            EditorUtility.SetDirty(prefab);

            Operation = OperationType.Activate;
            Icon = null;
            CommandName = "new Command";

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private void DrawTitle(string Text, int FontSize, Color color)
    {
        GUIStyle centeredText = new GUIStyle();
        centeredText.alignment = TextAnchor.MiddleCenter;
        centeredText.fontSize = FontSize;
        centeredText.fontStyle = FontStyle.Bold;
        centeredText.normal.textColor = color;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(Text, centeredText);
        EditorGUILayout.Space();
    }
}
