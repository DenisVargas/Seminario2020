using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Utility.ObjectPools.Generic; //Quizá este objeto podría utilizarse para generar pools de enemigos.
using UnityEngine.SceneManagement;
using Core.Serialization;
using Core.InventorySystem;
using System.Collections;

namespace Core.SaveSystem
{
    //Describe los componentes del gameplay dentro de una escena.
    public class Level : MonoBehaviour
    {
        public int LevelID = 0;
        public int LevelBuildID = 1;
        public GameObject prefabBaboso;
        public GameObject prefabGrunt;
        public GameObject prefabPlayer;
        public static bool loadLastCheckpoint = false;
        public static bool checkpointActivated = false;

        [SerializeField] List<int> pursuers = new List<int>();
        public bool blockCheckpoint { get; set; } = false;

        public Dictionary<int, EnemyData> enemyStateRegister = new Dictionary<int, EnemyData>();
        public Dictionary<int, GameObject> enemyReferenceRegister = new Dictionary<int, GameObject>();
        public Dictionary<int, Item> respawneableItems = new Dictionary<int, Item>();
        Dictionary<int, bool> currentRespawning = new Dictionary<int, bool>();

#if UNITY_EDITOR
        [Header("================= Debugging ======================")]
        [SerializeField] bool debugThis = false; 
#endif

        public static bool isPaused { get; private set; } = false;

        public static CheckPoint AutoSave
        {
            get
            {
                string root = Application.persistentDataPath + "/Data/Saves/";
                string file = root + "slot1.sv";

                if (Directory.Exists(root))
                    if (File.Exists(root + "slot1.sv"))
                        return Serializer.Deserialize<CheckPoint>(file, false);

                return null;
            }
            set
            {
                string root = Application.persistentDataPath + "/Data/Saves/";
                string file = root + "slot1.sv";

                //Debo chequear que el archivo exista en Data
                if (Directory.Exists(root))
                    Serializer.Serialize<CheckPoint>(value, file, false);
                else
                {
                    Debug.Log("Creo el Datapath no existía así que se creó uno nuevo.");
                    Directory.CreateDirectory(root);

                    Serializer.Serialize<CheckPoint>(value, file, false);
                }
            }
        }

        public static void RestartCurrentLevel()
        {
            int levelID = FindObjectOfType<Level>().LevelBuildID;
            isPaused = false;
            Time.timeScale = 1;
            SceneManager.LoadScene(levelID);
        }

        private void Awake()
        {
            BaseNPC[] enemies = FindObjectsOfType<BaseNPC>();
            foreach (BaseNPC enemy in enemies)
            {
                int enemyID = enemy.sceneID;
                if(!enemyReferenceRegister.ContainsKey(enemyID))
                    enemyReferenceRegister.Add(enemyID, enemy.gameObject);
                if (!enemyStateRegister.ContainsKey(enemyID))
                    enemyStateRegister.Add(enemyID, enemy.getEnemyData());
            }

            isPaused = false;
            Time.timeScale = 1;
        }
        private void Start()
        {
            if (loadLastCheckpoint)
            {
                print("Debo cargar la data del nivel");
                LoadGameData();
            }
        }
        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public static void TooglePauseGame()
        {
            //Mecanismo de prender/apagar.
            isPaused = !isPaused;

            foreach (Grunt grunt in FindObjectsOfType<Grunt>())
                grunt.enabled = !isPaused;

            foreach (Baboso baboso in FindObjectsOfType<Baboso>())
                baboso.enabled = !isPaused;

            Time.timeScale = isPaused ? 0 : 1;
        }
        public static bool CurrentLevelHasChekpoint()
        {
            Level current = FindObjectOfType<Level>();
            CheckPoint save = Level.AutoSave;
            if (save != null && save.levelID == current.LevelID)
                return true;

            return false;
        }
        public static void LoadGameData()
        {
            Level currentLevel = GetCurrentLevel();
            CheckPoint lastSave = Level.AutoSave;

            if (lastSave.levelID != currentLevel.LevelID && lastSave.LevelBuildID != currentLevel.LevelBuildID) return;

            isPaused = false;
            Time.timeScale = 1;

            //Busco al player, si no lo encuentro creo uno nuevo.
            Controller player = FindObjectOfType<Controller>();
            if (player == null)
            {
                GameObject instantiatedPlayer = Instantiate(currentLevel.prefabPlayer);
                player = instantiatedPlayer.GetComponent<Controller>();
            }
            player.LoadPlayerCheckpoint(lastSave.playerData); //Con esto esta cargador la data del player.

            currentLevel.enemyStateRegister.Clear();
            List<EnemyData> lastRegister = lastSave.Enemies;
            Dictionary<int, EnemyData> loadedRegister = new Dictionary<int, EnemyData>();
            foreach (var item in lastRegister)
                loadedRegister.Add(item.sceneID, item);

            List<int> toDelete = new List<int>();

            foreach (var pair in loadedRegister)
            {
                int enemyID = pair.Value.sceneID;
                EnemyData registeredData = pair.Value;
                if (registeredData.hasBeenKilled)
                {
                    toDelete.Add(enemyID);
                    continue;
                }

                currentLevel.enemyStateRegister.Add(enemyID, lastRegister[enemyID]);
            }

            for (int i = 0; i < toDelete.Count; i++)
            {
                GameObject go = currentLevel.enemyReferenceRegister[toDelete[i]];
                currentLevel.enemyReferenceRegister.Remove(toDelete[i]);
                Destroy(go);
            }

            //Items. si es necesario.

            //Loads the camera settings.
            CameraBehaviour camera = FindObjectOfType<CameraBehaviour>();
            camera.transform.position = lastSave.CameraPosition;
            camera.transform.rotation = lastSave.CameraRotation;
        }

        /// <summary>
        /// Devuelve una referencia al nivel actual.
        /// </summary>
        public static Level GetCurrentLevel()
        {
            return FindObjectOfType<Level>();
        }
        /// <summary>
        /// Crea un snapshot del nivel actual que luego puede ser cargado!
        /// </summary>
        /// <returns>True si el snapshot se creo Satisfactoriamente!</returns>
        public static bool SetCheckPoint()
        {

            Debug.Log("Seteo un CheckPoint");
            Level currentLevel = FindObjectOfType<Level>();
            if (currentLevel.blockCheckpoint)
            {
                Debug.Log("Checkpoint setting is blocked");
                return false;
            }

            //Player
            CheckPoint newSave = new CheckPoint();
            newSave.levelID = currentLevel.LevelID;
            newSave.LevelBuildID = currentLevel.LevelBuildID;
            newSave.playerData = FindObjectOfType<Controller>().getCurrentPlayerData();

            //Enemies
            List<EnemyData> enemiesInScene = new List<EnemyData>();
            foreach (KeyValuePair<int, EnemyData> register in currentLevel.enemyStateRegister)
                enemiesInScene.Add(register.Value);
            newSave.Enemies = enemiesInScene;

            //Camara
            CameraBehaviour camara = FindObjectOfType<CameraBehaviour>();
            newSave.CameraPosition = camara.transform.position;
            newSave.CameraRotation = camara.transform.rotation;

            Level.AutoSave = newSave;
            checkpointActivated = true;
            return true;
        }
        public static void ClearCheckpoint()
        {
            string root = Application.persistentDataPath + "/Data/Saves/";
            string file = root + "slot1.sv";

            if (Directory.Exists(root))
                if (File.Exists(file))
                    File.Delete(file);

            checkpointActivated = false;
            loadLastCheckpoint = false;
        }

        public static void RegisterEnemy(BaseNPC enemy)
        {
            Level level = GetCurrentLevel();
            EnemyData enemyData = enemy.getEnemyData();
            if (level.enemyStateRegister.ContainsKey(enemyData.sceneID))
            {
                level.enemyStateRegister[enemy.sceneID] = enemyData;
#if UNITY_EDITOR
                if (level.debugThis)
                    Debug.Log($"{enemyData.enemyType} with id {enemy.sceneID} has updated his Data"); 
#endif
            }
            else
            {
                level.enemyStateRegister.Add(enemy.sceneID, enemyData);
#if UNITY_EDITOR
                if (level.debugThis)
                    Debug.Log($"{enemyData.enemyType} has been registered with id: {enemy.sceneID}");
#endif
            }

            if (!level.enemyReferenceRegister.ContainsKey(enemyData.sceneID))
            {
                level.enemyReferenceRegister.Add(enemyData.sceneID, enemy.gameObject);
#if UNITY_EDITOR
                if (level.debugThis)
                    Debug.Log($"{enemyData.enemyType} with id {enemy.sceneID} has registered a new reference"); 
#endif
            }
        }
        public static void RegisterEnemyDead(int levelID)
        {
            Level level = GetCurrentLevel();
            if (level.enemyStateRegister.ContainsKey(levelID))
            {
                EnemyData currentState = level.enemyStateRegister[levelID];
                currentState.hasBeenKilled = true;
                level.enemyStateRegister[levelID] = currentState;
            }
#if UNITY_EDITOR
            else
            {
                if(level.debugThis)
                    Debug.Log($"There is no register for an enemy with sceneID {levelID}");
            }
#endif
        }

        public static void LevelFailed()
        {
            loadLastCheckpoint = true;
            Level current = GetCurrentLevel();
            SceneManager.LoadScene(current.LevelBuildID);
        }
        public static void LevelCompleted()
        {
            ClearCheckpoint();
        }

        public static void registerPursuer(int id)
        {
            Level currentLevel = GetCurrentLevel();
            if (currentLevel.enemyStateRegister.ContainsKey(id) && !currentLevel.pursuers.Contains(id))
            {
                currentLevel.pursuers.Add(id);

                if (currentLevel.pursuers.Count > 0)
                {
                    currentLevel.blockCheckpoint = true;
                }
            }
        }
        public static void unregisterPursuer(int id)
        {
            Level currentLevel = GetCurrentLevel();
            if (currentLevel.enemyStateRegister.ContainsKey(id) && currentLevel.pursuers.Contains(id))
            {
                currentLevel.pursuers.Remove(id);

                if (currentLevel.pursuers.Count == 0)
                {
                    currentLevel.blockCheckpoint = false;
                }
            }
        }

        public static void registerRespawneableItem(Item respawneable) 
        {
            var level = GetCurrentLevel();
            int id = respawneable.respawnID;
            if (!level.respawneableItems.ContainsKey(id))
            {
                level.respawneableItems.Add(id, respawneable);
            }
        }
        public static void SetItemRespawn(int id, float timetoRespawn)
        {
            var level = GetCurrentLevel();
            if (level.respawneableItems.ContainsKey(id) && !level.currentRespawning.ContainsKey(id))
            {
                level.StartCoroutine(level.respawnItem(id, timetoRespawn));
            }
        }

        IEnumerator respawnItem(int id, float time)
        {
            //añado al diccionario de currently spawning
            if (respawneableItems.ContainsKey(id))
            {
                //Debug.Log($"Item with id {id} ha iniciado su respawn. Tiempo es {time} segundos");
                respawneableItems[id].gameObject.SetActive(false);
                currentRespawning.Add(id, true);
                yield return new WaitForSeconds(time);
                //UnityEditor.EditorApplication.isPaused = true;
                currentRespawning.Remove(id);
                respawneableItems[id].Respawn();
                respawneableItems[id].gameObject.SetActive(true);
            }
        }
    }
}
