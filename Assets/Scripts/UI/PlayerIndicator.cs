using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    [SerializeField, HideInInspector]
    Transform _target;

    public RectTransform indicatorRectTransform;
    //La imagen se emparente al canvas.
    //Indicator.indicatorUI seria el transform de la imagen.

    private void Start()
    {
        _target = FindObjectOfType<Controller>().transform;
        if (indicatorRectTransform == null)
            Debug.LogError("No añadiste la referencia a la ui del player");
    }

    private void Update()
    {
        if (indicatorRectTransform != null)
            UpdateIndicatorPosition();
    }

    private void UpdateIndicatorPosition()
    {
        var indicatorPos = Camera.main.WorldToScreenPoint(_target.position);
        
        //Si el indicador esta detras de la camara
        if(indicatorPos.z < 0)
        {
            indicatorPos.x *= -1;
            indicatorPos.y *= -1;
        }
        var lastPosition = new Vector3(indicatorPos.x, indicatorPos.y, indicatorPos.z);

        indicatorPos.x = Mathf.Clamp(indicatorPos.x, indicatorRectTransform.rect.width / 2, Screen.width - indicatorRectTransform.rect.width / 2);
        indicatorPos.y = Mathf.Clamp(indicatorPos.y, indicatorRectTransform.rect.height / 2, Screen.height - indicatorRectTransform.rect.height / 2);
        indicatorPos.z = 0;

        //Actualizamos la posicion y la rotacion.
        indicatorRectTransform.up = (lastPosition - indicatorPos).normalized;
        indicatorRectTransform.position = indicatorPos;
    }
}
