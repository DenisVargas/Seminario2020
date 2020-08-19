using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.FSM;
using System;

public class IdleState : State
{
    public Func<bool> checkForPlayerAndClone = delegate { return false; };

    public override void Begin()
    {
        print($"{gameObject.name} entró en Idle");
        _anims.SetBool("Walking", false);
    }

    public override void Execute()
    {
        //Si el player es encontrado, automáticamente paso al estado correspondiente.
        if (checkForPlayerAndClone())
        {
            print($"Switch to Pursue!");
            SwitchToState(CommonState.pursue);
        }
    }

    public override void End()
    {
        print($"{gameObject.name} salió de Idle");
    }
}
