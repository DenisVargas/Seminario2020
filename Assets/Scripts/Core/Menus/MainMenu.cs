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
        Director.Stop();
        Director.playableAsset = CreditsToMain;
        Director.Play();
    }

    public void GoToCredits()
    {
        Director.Stop();
        Director.playableAsset = MainToCredits;
        Director.Play();
    }

    public void StartNewGame()
    {
        Director.Stop();
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
        Director.playableAsset = MainToExit;
        Director.Play();
    }
    public void OnExitGame_SecuenceCompleted()
    {
        _sceneLoadingManager.ExitGame();
    }
}
