using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Utility.ObjectPools.Generic;
using Core.Serialization;

public enum EnemyType
{
    baboso,
    Grunt
}

//Describe los componentes del gameplay dentro de una escena.
[Serializable]
public class Level : MonoBehaviour
{
    public int LevelID = 0;
    //public GenericPool<Baboso> babosos_pool;

    public List<Baboso> babosos = new List<Baboso>();
    public List<Grunt> grunts = new List<Grunt>();
    public List<IgnitableObject> ignitableObjects = new List<IgnitableObject>();

    public static string saveDataPath = "";

    private void Awake()
    {
        saveDataPath = Application.dataPath + "/Data/Saves";
        //Chequear si tengo datos guardados del estado del nivel antes de iniciar.

        //Sino, cargo dicha data.

        //Relleno inicial de las locaciones originales.
        foreach (var baboso in FindObjectsOfType<Baboso>())
        {
            babosos.Add(baboso);
        }
        foreach (var grunt in FindObjectsOfType<Grunt>())
        {
            grunts.Add(grunt);
        }
        SetCheckPoint();
    }

    /// <summary>
    /// Devuelve una referencia al nivel actual.
    /// </summary>
    public static Level GetCurrentLevel()
    {
        return FindObjectOfType<Level>();
    }

#if UNITY_EDITOR
    [MenuItem("Tests/TrySetCheckPoint")]
#endif
    /// <summary>
    /// Crea un snapshot del nivel actual que luego puede ser cargado!
    /// </summary>
    /// <returns>True si el snapshot se creo Satisfactoriamente!</returns>
    public static bool SetCheckPoint()
    {
        Debug.Log("Seteo un CheckPoint");
        var currentLevel = FindObjectOfType<Level>();

        string root = Application.dataPath + "/Data/Saves";

        //Debo chequear que el archivo exista en Data
        if (Directory.Exists(root))
        {
            Debug.Log("Existo we!!");
        }
        else
        {
            Directory.CreateDirectory(root);
            Debug.Log("Creo el Datapath");
        }

        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
