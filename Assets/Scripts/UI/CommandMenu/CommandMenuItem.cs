using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Core.Interaction;
using UnityEngine.EventSystems;

public class CommandMenuItem : MonoBehaviour
{
    public Action<OperationType, IInteractionComponent> OnOperationSelected = delegate { };
    public Action CloseMenu = delegate { };

    [SerializeField]
    CommandMenuItemData data = null;

    public IInteractionComponent referenceComponent = null;

    //Componentes que queremos modificar.
    [SerializeField] TMP_Text _text = null;
    [SerializeField] Image _iconImageField = null;
    [SerializeField] Image _backGroundImage = null;
    public Sprite normalState;
    public Sprite onHover;
    public Sprite onPress;

    [SerializeField] Color _normalColor  = Color.white;
    [SerializeField] Color _hoverColor   = Color.white;
    [SerializeField] Color _pressedColor = Color.white;

    public CommandMenuItemData Data
    {
        get => data;
        set
        {
            //Cargamos y mostramos nuestra Data.
            if (value != null)
            {
                data = value;
                _text.SetText(data.CommandName);
                _iconImageField.sprite = data.Icon;
            }
        }
    }

    public void OnMouseHoverStart(BaseEventData currentEvent)
    {
        //Si ponemos el mouse encima.
        //_backGroundImage.color = _hoverColor;
        _backGroundImage.sprite = onHover;
        _text.color = Color.white;
    }
    public void OnMouseClickDown(BaseEventData currentEvent)
    {
        //print("Clic");
        var input = currentEvent.currentInputModule.input;
        if (input.GetMouseButtonDown(0))
        {
            //print($"{gameObject.name}:: Activo el comando{{ {data.Operation} }}");
            _backGroundImage.sprite = onPress;
            currentEvent.Use();
            OnOperationSelected(data.Operation, referenceComponent);
            CloseMenu();
        }
        //else
        //if (input.GetMouseButtonDown(1))
        //{
        //    print("Cancelo el comando.");
        //    currentEvent.Use();
        //    CloseMenu();
        //}
    }
    public void OnMouseClickUp(BaseEventData currentEvent)
    {
        _backGroundImage.color = _hoverColor;
    }
    public void OnMouseHoverEnd(BaseEventData currentEvent)
    {
        //Si sacamos el mouse de encima.
        //_backGroundImage.color = _normalColor;
        _backGroundImage.sprite = normalState;
        _text.color = Color.black;
    }
}
