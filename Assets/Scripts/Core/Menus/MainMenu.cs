using UnityEngine;
using UnityEngine.Playables;

public class MainMenu : MonoBehaviour
{
    [SerializeField] PlayableDirector Director;
    [SerializeField] PlayableAsset MainToCredits;
    [SerializeField] PlayableAsset CreditsToMain;

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
