using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    [Header("Multi-Comand Menu")]
    [SerializeField] CommandMenu _MultiCommandMenu = null;


    private void Awake()
    {
        _MultiCommandMenu.LoadDisplay();
    }

    public void DisplayCommandMenu(Vector2 mouseScreenPosition, List<OperationOptions> displayOptions, IInteractable interactionTarget, Action<OperationOptions, IInteractable> callback)
    {
        //Le paso las nuevas opciones disponibles.
        _MultiCommandMenu.FillOptions(displayOptions, interactionTarget, callback);
        //Lo posiciono en donde debe estar.
        _MultiCommandMenu.Emplace(mouseScreenPosition);
        //Lo activo en el canvas.
        _MultiCommandMenu.gameObject.SetActive(true);
    }
}
