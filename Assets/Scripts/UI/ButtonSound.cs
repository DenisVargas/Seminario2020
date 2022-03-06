using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnMouseEnter()
    {
        Debug.Log("Hola");
        AudioManger.instance.Play("Onbutton");
    }
}
