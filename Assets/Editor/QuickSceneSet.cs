using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using Core.InventorySystem;

public class QuickSceneSet : EditorWindow
{
    const string playerPrefabPath = "Assets/Prefabs/CoreGameplay/Player.prefab";
    const string playerClonePrefabPath = "Assets/Prefabs/CoreGameplay/clon jutsu.prefab";
    const string CameraPrefabPath = "Assets/Prefabs/CoreGameplay/Top Down Camera.prefab";
    const string CanvasPrefabPath = "Assets/Prefabs/UI/Canvas.prefab";
    const string Managers = "Assets/Prefabs/CoreGameplay/Managers.prefab";
    const string LevelInfo = "Assets/Prefabs/CoreGameplay/LevelInfo.prefab";

    [MenuItem("GamePlay/QuickSceneInitializer")]
    public static void OpenWindow()
    {
        var window = GetWindow(typeof(QuickSceneSet)) as QuickSceneSet;
    }

    private void OnGUI()
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
        if(GUILayout.Button("Check or Add Default GameObjects"))
        {
            CheckAndRemoveDefaultSceneObjects();

            var CoreParent = CheckOrAddDefaultGameObject("======== Core ========");
            EditorUtility.SetDirty(CoreParent);
                AddManagersAndLevelInfo();
                SetCCC(CoreParent);
            CheckOrAddDefaultGameObject("====== Enemigos ======");
            CheckOrAddDefaultGameObject("======= Lights =======");
            CheckOrAddDefaultGameObject("======= Level ========");
        }
        if (GUILayout.Button("Reassign Scene EnemyIDs"))
        {
            var Grunts = FindObjectsOfType<Grunt>();
            var Babosus = FindObjectsOfType<Baboso>();

            int id = 0;
            foreach (var grunt in Grunts)
            {
                Undo.RecordObject(grunt, "Reasign Scene ID");
                grunt.sceneID = id;
                id++;
            }
            foreach (var baboso in Babosus)
            {
                Undo.RecordObject(baboso, "Reasign Scene ID");
                baboso.sceneID = id;
                id++;
            }

            Debug.Log("Done assigning IDs to all enemies");

            if(Grunts.Length > 0)
                Selection.activeObject = Grunts[0];
        }
        if(GUILayout.Button("Reassign Scene ItemIDs"))
        {
            var items = FindObjectsOfType<Item>();
            int sceneID = 0;
            Item lastAssigned = null;
            foreach (var item in items)
            {
                if (item.canRespawn)
                {
                    Undo.RecordObject(item, "Reasign Respawning ID");
                    item.respawnID = sceneID;
                    sceneID++;
                    lastAssigned = item;
                }
                Debug.Log("Respawn IDs reasigned");
            }
            if(lastAssigned != null)
            {
                Selection.activeObject = lastAssigned;
            }
        }
        if(GUILayout.Button("Clear Save Data"))
        {
            string root = Application.persistentDataPath + "/Data/Saves/";
            string file = root + "slot1.sv";

            if (Directory.Exists(root))
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                    Debug.Log("El archivo de guardado ha sido eliminado.");
                }
                else Debug.Log("El archivo de guardado no existe");
            }
            else Debug.Log("El directorio no exite");
        }
    }

    /// <summary>
    /// Inicializa el jugador, el Clon, la cámara y el canvas!
    /// </summary>
    private void SetCCC(GameObject parentObject = null)
    {
        GameObject mainCanvas = (GameObject)AssetDatabase.LoadAssetAtPath(CanvasPrefabPath, typeof(GameObject));
        var instancedMainCanvas = Instantiate(mainCanvas, Vector3.zero, Quaternion.identity);
        instancedMainCanvas.name = "========= UI =========";

        GameObject playerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(playerPrefabPath, typeof(GameObject));
        var instantiatedPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        Controller controller = instantiatedPlayer.GetComponent<Controller>();
        controller.CommandMenu = instancedMainCanvas.GetComponent<CommandMenu>();

        GameObject clonPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(playerClonePrefabPath, typeof(GameObject));
        var instantiatedClone = Instantiate(clonPrefab, Vector3.zero, Quaternion.identity);
        ClonBehaviour clon = instantiatedClone.GetComponent<ClonBehaviour>();

        controller.Clon = clon;

        GameObject CameraPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(CameraPrefabPath, typeof(GameObject));
        var instanciatedCamera = Instantiate(CameraPrefab, Vector3.zero, Quaternion.identity);


        if (parentObject)
        {
            instantiatedPlayer.transform.SetParent(parentObject.transform);
            instantiatedClone.transform.SetParent(parentObject.transform);
            instanciatedCamera.transform.SetParent(parentObject.transform);
        }
    }
    void AddManagersAndLevelInfo(GameObject parentObject = null)
    {
        GameObject LevelInfoPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(LevelInfo, typeof(GameObject));
        var instantiatedLevelInfo = Instantiate(LevelInfoPrefab, Vector3.zero, Quaternion.identity);

        GameObject ManagersPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(Managers, typeof(GameObject));
        var instantiatedManagers = Instantiate(ManagersPrefab, Vector3.zero, Quaternion.identity);

        if (parentObject)
        {
            instantiatedLevelInfo.transform.SetParent(parentObject.transform);
            instantiatedManagers.transform.SetParent(parentObject.transform);
        }
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
    public GameObject CheckOrAddDefaultGameObject(string name)
    {
        GameObject item = GameObject.Find(name);
        if (item == null)
        {
            item = new GameObject();
            item.name = name;
        }
        return item;
    }
}
