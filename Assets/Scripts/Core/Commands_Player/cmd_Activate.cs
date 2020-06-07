using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cmd_Activate : IQueryComand
{
    Action _animationTrigger = delegate { };
    Action _dispose = delegate { };
    CommandData _data;

    public bool completed { get; private set; } = false;

    public cmd_Activate(CommandData data, Action AnimationTrigger, Action dispose)
    {
        _data = data;
        _dispose = dispose;
        _animationTrigger = AnimationTrigger;
    }

    public void Execute()
    {
        //Ejecuto el comando.
        _animationTrigger();
        _dispose();
    }
    public void Cancel() { }
}
