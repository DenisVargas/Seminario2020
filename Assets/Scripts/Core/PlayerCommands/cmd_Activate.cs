using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cmd_Activate : IQueryComand
{
    Action OnStartOperate = delegate { };
    Action _dispose = delegate { };
    ActivationCommandData data;

    public bool completed { get; private set; } = false;

    public cmd_Activate(ActivationCommandData data, Action OnOperate, Action dispose)
    {
        this.data = data;
        OnStartOperate = OnOperate;
        _dispose = dispose;
    }

    public void Update()
    {
        //Ejecuto el comando.
        OnStartOperate();
        _dispose();
        //MonoBehaviour.print("COMANDO ACTIVAR");
    }

    public void setUp()
    {
        MonoBehaviour.print("Setup Query Comand: Activate");
    }
}
