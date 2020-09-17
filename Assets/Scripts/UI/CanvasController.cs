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

public class CanvasController : MonoBehaviour
{
    [Header("Multi-Comand Menu")]
    [SerializeField] CommandMenu _MultiCommandMenu = null;
    public Image Fade;
    public Image ThrwImg;

    private void Awake()
    {
        _MultiCommandMenu.LoadData();
        Controller Con = FindObjectOfType<Controller>();
        Con.ImDeadBro += DisplayLoose;
        Con.Grabing += DisplayThrow;
        Fade.canvasRenderer.SetAlpha(1);
        ThrwImg.canvasRenderer.SetAlpha(0);
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
    void DisplayLoose()
    {
        StartCoroutine(Rutina());
    }
    void DisplayThrow()
    {
       ThrwImg.canvasRenderer.SetAlpha(1);
    }
    IEnumerator Rutina()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(FadeOut());

    }
    IEnumerator FadeIn()
    {
        for (int i = 9; i >= 0; i--)
        {
            yield return new WaitForSeconds(0.1f);
         
            Fade.canvasRenderer.SetAlpha(i*0.1f);
            if (i == 0)
                Fade.enabled = false;
        }
    }
    IEnumerator FadeOut()
    {
        Fade.enabled = true;
        for (int i = 1; i <= 10; i++)
        {
            yield return new WaitForSeconds(0.1f);
            Fade.canvasRenderer.SetAlpha(i * 0.1f);
            if(i==10)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }
    }

}
