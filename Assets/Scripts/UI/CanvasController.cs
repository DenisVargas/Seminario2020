using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Core.SaveSystem;

public class CanvasController : MonoBehaviour
{
    [Header("Multi-Comand Menu")]
    [SerializeField] CommandMenu _MultiCommandMenu = null;
    [SerializeField] GameObject ThrowHUD = null;
    [SerializeField] GameObject ClonHUD  = null;
    [SerializeField] Image FadeImage     = null;

    bool playerHudLocked = false;

    private void Awake()
    {
        _MultiCommandMenu.LoadData();
        FadeImage.canvasRenderer.SetAlpha(1);
        StartCoroutine(FadeIn());

        var inpsectionMenu = FindObjectOfType<InspectionMenu>();
        inpsectionMenu.OnSetInspection += (enableInspection) => 
        {
            playerHudLocked = enableInspection;
            if (enableInspection)
            {
                playerHudLocked = true;
                ThrowHUD.SetActive(false);
                ClonHUD.SetActive(false);
            }
        };
    }

    public void DisplayPlayerUI(bool Clon, bool Throw)
    {
        if (!playerHudLocked)
        {
            ClonHUD.SetActive(Clon);
            ThrowHUD.SetActive(Throw);
        }
    }
    /// <summary>
    /// Inicia un fade-Out
    /// </summary>
    public void DisplayLoose()
    {
        StartCoroutine(FadeOut(3f));
    }

    IEnumerator FadeIn()
    {
        FadeImage.canvasRenderer.SetAlpha(1.0f);
        FadeImage.enabled = true;
        for (int i = 9; i >= 0; i--)
        {
            yield return new WaitForSeconds(0.1f);
            FadeImage.canvasRenderer.SetAlpha(i*0.1f);
            if (i == 0)
                FadeImage.enabled = false;
        }
    }
    IEnumerator FadeOut(float initialDelay = 0.1f)
    {
        yield return new WaitForSeconds(initialDelay);
        FadeImage.canvasRenderer.SetAlpha(0.0f);
        FadeImage.enabled = true;
        for (int i = 1; i <= 10; i++)
        {
            yield return new WaitForSeconds(0.1f);
            FadeImage.canvasRenderer.SetAlpha(i * 0.1f);
            if(i==10)
            {
                if (Level.currentLevelHasChekpoint())
                {
                    Level.LoadGameData();
                    StartCoroutine(FadeIn());
                }
                else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
