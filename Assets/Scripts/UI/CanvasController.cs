using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    [Header("Multi-Comand Menu")]
    [SerializeField] CommandMenu _MultiCommandMenu = null;
    public Image Fade;
   


    private void Awake()
    {
        _MultiCommandMenu.LoadData();
        FindObjectOfType<NMA_Controller>().ImDeadBro += DisplayLoose;
        Fade.canvasRenderer.SetAlpha(1);
        StartCoroutine(FadeIn());
    }

    public void DisplayCommandMenu(Vector2 mouseScreenPosition, InteractionParameters displayOptions, IInteractable interactionTarget, Action<OperationType, IInteractable> callback)
    {
        //Le paso las nuevas opciones disponibles.
        _MultiCommandMenu.FillOptions(displayOptions, interactionTarget, callback);
        //Lo posiciono en donde debe estar.
        _MultiCommandMenu.Emplace(mouseScreenPosition);
        //Lo activo en el canvas.
        _MultiCommandMenu.gameObject.SetActive(true);
    }
    void DisplayLoose()
    {
        StartCoroutine(Rutina());
    }
    IEnumerator Rutina()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(FadeOut());

    }
    IEnumerator FadeIn()
    {
        Debug.Log("entre");
        for (int i = 9; i >= 0; i--)
        {
            yield return new WaitForSeconds(0.1f);
         
            Fade.canvasRenderer.SetAlpha(i*0.1f);
        }
    }
    IEnumerator FadeOut()
    {
        for (int i = 1; i <= 10; i++)
        {
            yield return new WaitForSeconds(0.1f);
            Fade.canvasRenderer.SetAlpha(i * 0.1f);
            if(i==10)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }
    }

}
