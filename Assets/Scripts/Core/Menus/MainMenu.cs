using UnityEngine;
using UnityEngine.Playables;

public class MainMenu : MonoBehaviour
{
    [SerializeField] PlayableDirector Director   = null;
    [SerializeField] PlayableAsset MainToCredits = null;
    [SerializeField] PlayableAsset CreditsToMain = null;

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
}
