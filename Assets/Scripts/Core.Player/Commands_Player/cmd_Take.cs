using Core.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class cmd_Take : IQueryComand
{
    Action TriggerAnimation = delegate { };
    Transform mano;

    IStaticInteractionComponent target;

    public cmd_Take(IStaticInteractionComponent target, Transform mano, Action TriggerAnimation)
    {
        this.TriggerAnimation = TriggerAnimation;
        this.mano = mano;
        this.target = target;
    }
    public bool completed { get; private set; } = false;
    public bool isReady  { get; private set; } = false;
    public bool cashed  => true;

    public void SetUp()
    {
        TriggerAnimation();
        isReady = true;
    }
    public void Execute()
    {
        target.ExecuteOperation(mano);
        completed = true;
    }
    public void Cancel() { }
}

