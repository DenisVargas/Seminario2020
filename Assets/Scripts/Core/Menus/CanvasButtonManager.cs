using UnityEngine;
using UnityEngine.SceneManagement;
using Core.SaveSystem;

public class CanvasButtonManager : MonoBehaviour
{
    public void LoadLevel(int level)
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        AsyncSceneLoadOptions.LevelBuildIndex = level + 1;
        if (!AsyncSceneLoadOptions.LoadActive)
            SceneManager.LoadScene("LoadMenu", LoadSceneMode.Additive);
    }

    public void Continue()
    {
        //Esto tengo que cambiarlo.

        var lastSavedGame = Level.AutoSave;
        if (lastSavedGame != null)
            SceneManager.LoadScene(lastSavedGame.LevelBuildID);

        //TODO: Bloquar el deleteo del autosave.
    }

    //Estas son utilizadas por el menú de pausa.
    public void UnPause()
    {
        Level.TooglePauseGame();
        var c = FindObjectOfType<CanvasController>();
        c.setPauseMenu(false);
    }
    public void OpenScriptures()
    {
        //TODO: Añadir una colección de Escrituras y/o notas que se vayan desbloqueando.
    }
    public void ExitToMainMenu()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        AsyncSceneLoadOptions.LevelBuildIndex = 0;
        if (!AsyncSceneLoadOptions.LoadActive)
            SceneManager.LoadScene("LoadMenu", LoadSceneMode.Additive);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
