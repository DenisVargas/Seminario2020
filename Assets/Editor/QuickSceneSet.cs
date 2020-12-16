using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

//Motores 2.

public class QuickSceneSet : EditorWindow
{
    string playerPrefabPath = "Assets/Prefabs/CoreGameplay/Player.prefab";
    string playerClonePrefabPath = "Assets/Prefabs/CoreGameplay/clon jutsu.prefab";
    string CameraPrefabPath = "Assets/Prefabs/CoreGameplay/Top Down Camera.prefab";
    string CanvasPrefabPath = "Assets/Prefabs/UI/Canvas.prefab";
    string ManagersPrefabPath = "Assets/Prefabs/CoreGameplay/Managers.prefab";

    int toolbarIndex = 0;
    readonly string[] tabs = { "Operations", "Settings" };

    [MenuItem("GamePlay/QuickSceneInitializer")]
    public static void OpenWindow()
    {
        var window = GetWindow(typeof(QuickSceneSet)) as QuickSceneSet;
    }

    private void OnGUI()
    {
        toolbarIndex = GUILayout.Toolbar(toolbarIndex, tabs);
        switch (toolbarIndex)
        {
            case 0:
                DrawMainOperationsTab();
                break;
            case 1:
                DrawSettingsTab();
                break;

            default:
                break;
        }
    }

    private void DrawSettingsTab()
    {
        EditorGUILayout.LabelField("Settings!");
        EditorGUILayout.LabelField("Prefab Paths:");
        playerPrefabPath = EditorGUILayout.TextField("Player:", playerPrefabPath);
        playerClonePrefabPath = EditorGUILayout.TextField("Clone:", playerClonePrefabPath);
        CameraPrefabPath = EditorGUILayout.TextField("Camera:", CameraPrefabPath);
        CanvasPrefabPath = EditorGUILayout.TextField("Canvas:", CanvasPrefabPath);
        ManagersPrefabPath = EditorGUILayout.TextField("Managers:", ManagersPrefabPath);
    }

    private void DrawMainOperationsTab()
    {
        GUIContent buttonContent = new GUIContent("Clear Scene", "Deletes all gameobjects in the current Scene\nBe carefull!!");
        if (GUILayout.Button(buttonContent))
        {
            var objectsInScene = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < objectsInScene.Length; i++)
            {
                DestroyImmediate(objectsInScene[i]);
            }
            SceneView.RepaintAll(); //Por si las dudas.
        }
        if (GUILayout.Button("Check or Add Default GameObjects"))
        {
            CheckAndRemoveDefaultSceneObjects();

            CheckOrAddDefaultGameObject("======== Core ========");
            GameObject ManagersIntstance = (GameObject)AssetDatabase.LoadAssetAtPath(ManagersPrefabPath, typeof(GameObject));
            var managerObject = Instantiate(ManagersIntstance, Vector3.zero, Quaternion.identity);
            CheckOrAddDefaultGameObject("======== CCC =========");
            SetCCC();
            CheckOrAddDefaultGameObject("====== Enemigos ======");
            CheckOrAddDefaultGameObject("======= Lights =======");
            CheckOrAddDefaultGameObject("======= Level ========");
        }
    }

    /// <summary>
    /// Inicializa el jugador, el Clon, la cámara y el canvas!
    /// </summary>
    private void SetCCC()
    {
        GameObject playerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(playerPrefabPath, typeof(GameObject));
        var instantiatedPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        Controller controller = instantiatedPlayer.GetComponent<Controller>();

        GameObject clonPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(playerClonePrefabPath, typeof(GameObject));
        var instantiatedClone = Instantiate(clonPrefab, Vector3.zero, Quaternion.identity);
        ClonBehaviour clon = instantiatedClone.GetComponent<ClonBehaviour>();

        controller.Clon = clon;

        GameObject CameraPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(CameraPrefabPath, typeof(GameObject));
        Instantiate(CameraPrefab, Vector3.zero, Quaternion.identity);

        GameObject mainCanvas = (GameObject)AssetDatabase.LoadAssetAtPath(CanvasPrefabPath, typeof(GameObject));
        Instantiate(mainCanvas, Vector3.zero, Quaternion.identity);
    }
    private void CheckAndRemoveDefaultSceneObjects()
    {
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera)
            DestroyImmediate(mainCamera);

        GameObject DirectionalLight = GameObject.Find("Directional Light");
        if (DirectionalLight)
            DestroyImmediate(DirectionalLight);
    }
    public void CheckOrAddDefaultGameObject(string name)
    {
        GameObject item = GameObject.Find(name);
        if (item == null)
        {
            item = new GameObject();
            item.name = name;
        }
    }
}
