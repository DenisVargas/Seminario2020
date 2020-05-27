using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cmd_Activate : IQueryComand
{
    Action _animationTrigger = delegate { };
    Action _dispose = delegate { };
    ActivationCommandData _data;

    public bool completed { get; private set; } = false;

    public cmd_Activate(ActivationCommandData data, Action AnimationTrigger, Action dispose)
    {
        _data = data;
        _dispose = dispose;
        _animationTrigger = AnimationTrigger;
    }

    public void Update()
    {
        //Ejecuto el comando.
        _animationTrigger();
        _dispose();
    }

    public void setUp()
    {
        MonoBehaviour.print("Setup Query Comand: Activate");
    }

    public void Cancel() { }
}
