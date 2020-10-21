using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Core.Interaction;
using Core.InventorySystem;
using Core.SaveSystem;

public class CanvasController : MonoBehaviour
{
    [Header("Multi-Comand Menu")]
    [SerializeField] CommandMenu _MultiCommandMenu = null;
    [SerializeField] GameObject ThrowHUD = null;
    [SerializeField] GameObject ClonHUD  = null;
    [SerializeField] Image FadeImage     = null;

    private void Awake()
    {
        _MultiCommandMenu.LoadData();
        FadeImage.canvasRenderer.SetAlpha(1);
        StartCoroutine(FadeIn());


        var inpsectionMenu = FindObjectOfType<InspectionMenu>();
        inpsectionMenu.OnSetInspection += (enableInspection) => 
        {
            if (enableInspection)
            {
                ThrowHUD.SetActive(false);
                ClonHUD.SetActive(false);
            }
            else
            {
                ThrowHUD.SetActive(true);
                ClonHUD.SetActive(true);
            }
        };
    }

    /// <summary>
    /// Inicia un fade-Out
    /// </summary>
    public void DisplayLoose()
    {
        StartCoroutine(FadeOut(3f));
    }
    /// <summary>
    /// Muestra un ícono de target.
    /// </summary>
    public void DisplayThrow(bool active)
    {
        ThrowHUD.SetActive(active);
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
