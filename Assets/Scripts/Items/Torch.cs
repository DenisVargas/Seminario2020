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

    protected override void Use(params object[] optionalParams)
    {
        base.Use(optionalParams); //Printea que se ha utilizado el objeto.
        //Deberíamos castear optional params para determinar que acción queremos hacer con este objeto.
        //testemaos un bool para saber si queremos prender la antorcha.
        if (optionalParams.Length > 1)
            isBurning = (bool) optionalParams[0];
    }

    //Colisiones. Si este objeto colisiona con un igniteable. Ejecuta su comando ignite, y luego se destruye.
}
