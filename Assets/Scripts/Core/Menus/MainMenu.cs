using Core.SaveSystem;
using UnityEngine;
using UnityEngine.Playables;

public class MainMenu : MonoBehaviour
{
    [SerializeField] CanvasButtonManager _sceneLoadingManager = null;
    [SerializeField] PlayableDirector Director   = null;
    [SerializeField] PlayableAsset MainToCredits = null;
    [SerializeField] PlayableAsset CreditsToMain = null;
    [SerializeField] PlayableAsset MainToGame = null;
    [SerializeField] PlayableAsset MainToExit = null;

    public void ReturnToMain()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        Director.Stop();
        AudioManger.instance.Play("buttonPress");
        Director.playableAsset = CreditsToMain;
        Director.Play();
    }

    public void GoToCredits()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        Director.Stop();
        AudioManger.instance.Play("buttonPress");
        Director.playableAsset = MainToCredits;
        Director.Play();
    }

    public void StartNewGame()
    {
        if(Time.timeScale == 0)
            Time.timeScale = 1;

        Director.Stop();
        AudioManger.instance.Play("buttonPress");
        Director.playableAsset = MainToGame;
        Director.Play();
    }

    public void OnEnterGame_SecuenceComplete()
    {
        Debug.Log("Enter game, secuence complete");
        _sceneLoadingManager.LoadLevel(1);
    }

    public void StartExitingFromGame()
    {
        Director.Stop();
        AudioManger.instance.Play("buttonPress");
        Director.playableAsset = MainToExit;
        Director.Play();
    }
    public void OnExitGame_SecuenceCompleted()
    {
        _sceneLoadingManager.ExitGame();
    }
}
