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
    public Image Fade;
    public GameObject ThrwImg;

    private void Awake()
    {
        _MultiCommandMenu.LoadData();
        Fade.canvasRenderer.SetAlpha(1);
        StartCoroutine(FadeIn());
    }

    public void DisplayCommandMenu(Vector2 mouseScreenPosition, IInteractable interactionTarget, Inventory inventory, Action<OperationType, IInteractionComponent> callback)
    {
        //Le paso las nuevas opciones disponibles.
        _MultiCommandMenu.FillOptions( interactionTarget, inventory, callback);
        //Lo posiciono en donde debe estar.
        _MultiCommandMenu.Emplace(mouseScreenPosition);
        //Lo activo en el canvas.
        _MultiCommandMenu.gameObject.SetActive(true);
    }
    public void CloseCommandMenu()
    {
        _MultiCommandMenu.gameObject.SetActive(false);
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
        ThrwImg.SetActive(active);
    }
    IEnumerator FadeIn()
    {
        Fade.canvasRenderer.SetAlpha(1.0f);
        Fade.enabled = true;
        for (int i = 9; i >= 0; i--)
        {
            yield return new WaitForSeconds(0.1f);
            Fade.canvasRenderer.SetAlpha(i*0.1f);
            if (i == 0)
                Fade.enabled = false;
        }
    }
    IEnumerator FadeOut(float initialDelay = 0.1f)
    {
        yield return new WaitForSeconds(initialDelay);
        Fade.canvasRenderer.SetAlpha(0.0f);
        Fade.enabled = true;
        for (int i = 1; i <= 10; i++)
        {
            yield return new WaitForSeconds(0.1f);
            Fade.canvasRenderer.SetAlpha(i * 0.1f);
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
