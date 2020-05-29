using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cmd_Ignite : IQueryComand
{
    Action _AnimationTrigger = delegate { };
    Action _dispose = delegate { };

    CommandData _data;

    public bool completed { get; private set; } = (false);

    public cmd_Ignite(CommandData data, Action OnActivation, Action Dispose)
    {
        _dispose = Dispose;
        _AnimationTrigger = OnActivation;
        _data = data;
    }

    public void Execute()
    {
        //Ejecuto el commando
        completed = true;
        _AnimationTrigger();
        _dispose();
    }

    public void Cancel()
    {
        _data.target.OnCancelOperation(OperationType.Ignite);
    }
}
