using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTesting : MonoBehaviour
{

    /// <summary>
    /// Guarda el estado actual de la escena!
    /// </summary>
    public void CheckPoint()
    {

    }
    /// <summary>
    /// Carga el último CheckPoint Guardado!
    /// </summary>
    public void LoadLastCheckPoint()
    {

    }

    // Start is called before the first frame update
   public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            if(UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            else
#endif
            Application.Quit();
        }
    }
}
