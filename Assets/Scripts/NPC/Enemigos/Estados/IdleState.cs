using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.FSM;
using System;

public class IdleState : State
{
    public Action checkForPlayerAndClone = delegate { };
    int walking = 0;

    public override void Begin()
    {
        if (walking == 0) Animator.StringToHash("Walking");

        print($"{gameObject.name} entró en Idle");
        _anims.SetBool(walking, false);
    }

    public override void Execute()
    {
        checkForPlayerAndClone(); //Si el player es encontrado, automáticamente paso al estado correspondiente.

        //Esta parte tiene que estar incluido en checkForPlayerAndClone();
        //    if ( _killeableTarget != null && _currentState != BoboState.pursue)
        //    {
        //        state.Feed(BoboState.pursue);
        //    }
    }

    public override void End()
    {
        print($"{gameObject.name} salió de Idle");
    }
}
