using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    public RectTransform PlayerIndicatorRectTransform;
    [SerializeField, HideInInspector]
    Transform _player;

    //La imagen se emparente al canvas.
    //Indicator.indicatorUI seria el transform de la imagen.

    private void Start()
    {
        _player = FindObjectOfType<Controller>().transform;
        if (PlayerIndicatorRectTransform == null)
            Debug.LogError("No añadiste la referencia a la ui del player");
    }

    private void Update()
    {
        if (PlayerIndicatorRectTransform != null)
            UpdatePlayerIndicatorPosition();
    }

    private void UpdatePlayerIndicatorPosition()
    {
        var indicatorPos = Camera.main.WorldToScreenPoint(_player.position);
        
        //Si el indicador esta detras de la camara
        if(indicatorPos.z < 0)
        {
            indicatorPos.x *= -1;
            indicatorPos.y *= -1;
        }
        var lastPosition = new Vector3(indicatorPos.x, indicatorPos.y, indicatorPos.z);

        indicatorPos.x = Mathf.Clamp(indicatorPos.x, PlayerIndicatorRectTransform.rect.width / 2, Screen.width - PlayerIndicatorRectTransform.rect.width / 2);
        indicatorPos.y = Mathf.Clamp(indicatorPos.y, PlayerIndicatorRectTransform.rect.height / 2, Screen.height - PlayerIndicatorRectTransform.rect.height / 2);
        indicatorPos.z = 0;

        //Actualizamos la posicion y la rotacion.
        PlayerIndicatorRectTransform.up = (lastPosition - indicatorPos).normalized;
        PlayerIndicatorRectTransform.position = indicatorPos;
    }
}
