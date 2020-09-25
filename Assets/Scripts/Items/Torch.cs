using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.InventorySystem;

public class Torch : Item
{
    public GameObject burningComponent;
    [SerializeField] bool _isOn;
    public bool isBurning
    {
        get => _isOn;
        set
        {
            _isOn = value;
            burningComponent.SetActive(_isOn);
        }
    }

    private void Awake()
    {
        if (_isOn == false)
            isBurning = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Colisiones. Si este objeto colisiona con un igniteable. Ejecuta su comando ignite, y luego se destruye.
}
