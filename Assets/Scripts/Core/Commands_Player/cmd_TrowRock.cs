using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cmd_TrowRock : IQueryComand
{
    Action _animationTrigger = delegate { };
    Action _dispose = delegate { };

    CommandData _commandData;

    public bool completed { get; private set; } = (false);

    public cmd_TrowRock(CommandData commandData, Action AnimationTrigger, Action disposeCommandCallback)
    {
        _commandData = commandData;
        _animationTrigger = AnimationTrigger;
        _dispose = disposeCommandCallback;
    }

    public void Execute()
    {
        //Ejecuto el comando.
        _animationTrigger();
        completed = true;
        _dispose();
    }
    public void Cancel() { }
}
