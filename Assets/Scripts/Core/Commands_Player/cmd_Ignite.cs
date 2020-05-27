using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cmd_Ignite : IQueryComand
{
    Action _OnActivation = delegate { };
    Action _dispose = delegate { };

    ActivationCommandData _data;
    bool _IsDone = false;

    public bool completed => _IsDone;

    public cmd_Ignite(ActivationCommandData data, Action OnActivation, Action Dispose)
    {
        _dispose = Dispose;
        _OnActivation = OnActivation;
        _data = data;
    }

    public void setUp()
    {
        //Por ahora no hay mucho para contar.
    }

    public void Update()
    {
        //Ejecuto el commando
        _IsDone = true;
        _OnActivation();
        _dispose();
    }

    public void Cancel()
    {
        _data.target.OnCancelOperation(OperationType.Ignite);
    }
}
