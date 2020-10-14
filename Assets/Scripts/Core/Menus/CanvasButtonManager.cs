using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.SaveSystem;

public class CanvasButtonManager : MonoBehaviour
{
    public void LoadLevel(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void Continue()
    {
        var lastSavedGame = Level.AutoSave;
        if (lastSavedGame != null)
            SceneManager.LoadScene(lastSavedGame.LevelBuildID);

        //TODO: Bloquar el deleteo del autosave.
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
