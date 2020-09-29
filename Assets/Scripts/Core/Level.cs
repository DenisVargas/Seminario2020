﻿using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Utility.ObjectPools.Generic; //Quizá este objeto podría utilizarse para generar pools de enemigos.
using Core.Serialization;

namespace Core.SaveSystem
{
    //Describe los componentes del gameplay dentro de una escena.
    public class Level : MonoBehaviour
    {
        public int LevelID = 0;
        public GameObject prefabBaboso;
        public GameObject prefabGrunt;
        public GameObject prefabPlayer;
        //public NodeGraphBuilder levelGraph = null;

        CheckPoint saveFile
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
                string completeFilePath = root + "slot1.sv";

                //Debo chequear que el archivo exista en Data
                if (Directory.Exists(root))
                {
                    //Debug.Log($"El directorio {root} existe.");
                    if (File.Exists(completeFilePath))
                    {
                        //Debug.Log($"El archivo {root + completeFilePath} Existe");
                        Serializer.Serialize<CheckPoint>(value, completeFilePath, false);
                    }
                }
                else
                {
                    Debug.Log("Creo el Datapath no existía así que se creó uno nuevo.");
                    Directory.CreateDirectory(root);
                    //File.Create(completeFilePath);

                    Serializer.Serialize<CheckPoint>(value, completeFilePath, false);
                }
            }
        }

        private void Awake()
        {
            //Chequear si tengo datos guardados del estado del nivel antes de iniciar.
            var lastSave = saveFile;
            if (lastSave != null && lastSave.levelID == LevelID)
            {
                Debug.LogWarning("Hay una partida guardada que corresponde a este nivel!");
                //LoadGameData(lastSave);
            }
        }

        public static bool currentLevelHasChekpoint()
        {
            Level current = FindObjectOfType<Level>();
            var save = current.saveFile;
            if (save != null && save.levelID == current.LevelID)
                return true;

            return false;
        }

        public static void LoadGameData()
        {
            Level currentLevel = GetCurrentLevel();
            CheckPoint lastSave = currentLevel.saveFile;

            //Busco al player, si no lo encuentro creo uno nuevo.
            Controller player = FindObjectOfType<Controller>();
            if (player == null)
            {
                var instantiatedPlayer = Instantiate(currentLevel.prefabPlayer);
                player = instantiatedPlayer.GetComponent<Controller>();
            }
            player.LoadPlayerCheckpoint(lastSave.playerData); //Con esto esta cargador la data del player.

            var loadedGrunts = FindObjectsOfType<Grunt>(); //Encuentro todos los grunts que ya están cargados.
            int findedGrunts = loadedGrunts.Length;
            for (int i = 0; i < findedGrunts; i++)
                Destroy(loadedGrunts[i].gameObject);
            var loadedBabosos = FindObjectsOfType<Baboso>();//Encuentro todos los babosos que ya están en escena.
            int findedBabosos = loadedBabosos.Length;
            for (int i = 0; i < findedBabosos; i++)
                Destroy(loadedBabosos[i].gameObject);

            int enemiesNeeded = lastSave.Enemies.Count;
            for (int i = 0; i < enemiesNeeded; i++)
            {
                BaseNPC npc = null;

                switch (lastSave.Enemies[i].enemyType)
                {
                    case EnemyType.baboso:
                        var babosoGo = Instantiate(currentLevel.prefabBaboso);
                        npc = babosoGo.GetComponent<Baboso>();
                        break;

                    case EnemyType.Grunt:
                        var gruntGo = Instantiate(currentLevel.prefabGrunt);
                        npc = gruntGo.GetComponent<Grunt>();

                        break;
                    default:
                        break;
                }

                if (npc != null)
                    npc.LoadEnemyData(lastSave.Enemies[i]);
            }

            //Loads the camera settings.
            var camera = FindObjectOfType<CameraBehaviour>();
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
            //Debug.Log("Seteo un CheckPoint");
            var currentLevel = FindObjectOfType<Level>();

            var newSave = new CheckPoint();
            newSave.levelID = currentLevel.LevelID;
            newSave.playerData = FindObjectOfType<Controller>().getCurrentPlayerData();
            List<EnemyData> enemiesInScene = new List<EnemyData>();
            var enemies = FindObjectsOfType<BaseNPC>();
            foreach (var enemy in enemies)
                enemiesInScene.Add(enemy.getEnemyData());
            newSave.Enemies = enemiesInScene;

            var camara = FindObjectOfType<CameraBehaviour>();
            newSave.CameraPosition = camara.transform.position;
            newSave.CameraRotation = camara.transform.rotation;

            currentLevel.saveFile = newSave;
            return false;
        }
    }
}
