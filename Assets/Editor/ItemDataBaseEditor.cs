using System;
using UnityEngine;
using UnityEditor;
using Core.InventorySystem;
using UnityEditorInternal;

public class ItemDataBaseEditor : EditorWindow
{
    //============================== Static Data Paths =====================================

    const string ItemsPath = "Assets/Data/ItemDataBase/Items";
    const string ItemCollectionBasePath = "Assets/Data/ItemDataBase/ItemCollection1.asset";
    const string ItemRecipesPath = "Assets/Data/ItemDataBase/Recipes.asset";

    //===================================== Items ==========================================

    bool foldItemCollection = false;

    //New Item Info.
    ItemID id = ItemID.nulo;
    string itemName = "";
    string itemDescription = "";
    Texture2D Icon = null;
    bool isCombinable = false;
    bool isDropeable = false;
    bool isThroweable = false;
    bool isConsumable = false;

    string defaultItemName = "new Item";
    string defaultItemDescription = "This is a new Item";
    ReorderableList CollectionData;

    //===================================== Recipes ========================================

    ItemDataObject a = null;
    ItemDataObject b = null;
    ItemDataObject result = null;

    int selection = 0;
    int pickerID = 0;
    ItemDataObject selectedInPicker;
    Recipes RecipesContainer = null;

    //===================================== Window =========================================

    [MenuItem("GamePlay/Data Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow(typeof(ItemDataBaseEditor)) as ItemDataBaseEditor;
        var content = new GUIContent();
        content.text = "Data Editor";
        window.titleContent = content;
        window.InitializeComponents();
    }

    public void InitializeComponents()
    {
        itemName = defaultItemName;
        itemDescription = defaultItemDescription;

        var collectionObject = AssetDatabase.LoadAssetAtPath<ItemDataCollection>(ItemCollectionBasePath);
        SerializedObject serializedCollection = new SerializedObject(collectionObject);
        CollectionData = new ReorderableList(collectionObject.existingItemData, typeof(ItemDataCollection), true, true, true, true);

        CollectionData.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Current Item Collection");
        };

        CollectionData.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = serializedCollection.FindProperty("existingItemData").GetArrayElementAtIndex(index);

            SerializedObject referenced = new SerializedObject(element.objectReferenceValue);
            SerializedProperty data = referenced.FindProperty("data").FindPropertyRelative("Name");

            float maxWidth = EditorGUIUtility.currentViewWidth;
            float maxElementSize = maxWidth / 2;
            float padding = 20f;

            Rect ButtonRect = new Rect(rect.x + maxElementSize, rect.y, maxElementSize - padding, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(rect, data.stringValue);
            if (GUI.Button(ButtonRect, "edit"))
            {
                Debug.Log("ButtonPressed");
            }
        };

        RecipesContainer = AssetDatabase.LoadAssetAtPath<Recipes>(ItemRecipesPath);
    }

    private void OnGUI()
    {
        //Desde esta window deberíamos poder:
        //modificar la lista de IDs
        //añadir nuevas recetas.
        //modificar las listas de Items.

        //Mostrar contenido actual del enum.
        //Rehacer el enum.
        //Crear Scriptable Object Correspondiente.

        DrawTitle("Items", 30, Color.white);
        CreateNewItemSection();
        DrawTitle("Collection", 30, Color.white);
        CollectionSection();
        DrawTitle("Recipes", 30, Color.white);
        DrawCombinationEditor();
    }


    private void CreateNewItemSection()
    {
        //añadir nuevos items.
        EditorGUILayout.Space();
        DrawTitle("Add new Item", 15, Color.white);
        GUILayoutOption[] limitedWidth = new GUILayoutOption[] { GUILayout.MaxWidth(100) };

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Selected ID:", limitedWidth);
            id = (ItemID)EditorGUILayout.EnumPopup(id, limitedWidth);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Item Name");
            itemName = EditorGUILayout.TextArea(itemName);
            EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Want to add a new ID?");
        if (GUILayout.Button("Item ID editor"))
        {
            Close();
            var type = typeof(ItemID);
        }

        EditorGUILayout.LabelField("Item Description");

        //Multi line-Text Field
        GUILayoutOption[] multiLineTextOptions = new GUILayoutOption[]
        { GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 4) };

        itemDescription = EditorGUILayout.TextArea(itemDescription, multiLineTextOptions);
        Icon = (Texture2D)EditorGUILayout.ObjectField("Item Icon (optional):", Icon, typeof(Texture2D), false);
        EditorGUILayout.Space();
        isCombinable = EditorGUILayout.Toggle("Is Combinable", isCombinable);
        isDropeable = EditorGUILayout.Toggle("Is Dropeable", isDropeable);
        isThroweable = EditorGUILayout.Toggle("Is Throweable", isThroweable);
        isConsumable = EditorGUILayout.Toggle("Is Consumable", isConsumable);

        GUI.color = Color.green;
        if (GUILayout.Button("Add New Item"))
        {
            Debug.Log("Clickeado Add");
        }
        GUI.color = Color.white;
        EditorGUILayout.Space();
    }
    private void CollectionSection()
    {
        //Updater del ItemDataCollection.
        if (GUILayout.Button("Load and Update Item Data Collection"))
        {
            ItemDataCollectionEditor.UpdateDatabase();
        }

        Rect lastRect = EditorGUILayout.GetControlRect();
        foldItemCollection = EditorGUI.BeginFoldoutHeaderGroup(lastRect, foldItemCollection, "Data Collection");
        if (foldItemCollection)
            CollectionData.DoLayoutList();
        EditorGUI.EndFoldoutHeaderGroup();
    }
    private void DrawCombinationEditor()
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

            SerializedObject Serialized_recipesContainer = new SerializedObject(RecipesContainer);

            Undo.RecordObject(RecipesContainer, "Added new Combination");
            Serialized_recipesContainer.Update();
            EditorUtility.SetDirty(RecipesContainer);
            RecipesContainer.combinations.Add(_comb);

            a = null;
            b = null;
            result = null;
        }

        if (GUILayout.Button("Select Recipes Container"))
        {
            Recipes obj = AssetDatabase.LoadAssetAtPath<Recipes>(ItemRecipesPath);
            if (obj != null)
                Selection.activeObject = obj;
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
