using IA.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AttackState : State
{
    public Action LookAtAttackTarget = delegate { };
    public Action StunAttackTarget = delegate { };
    public Action KillAttackTarget = delegate { };
    public Action Think = delegate { };

    //Rigidbody _rb = null;

    public override void Begin()
    {
        //_rb = GetComponent<Rigidbody>(); //Si movemos por rigidbody.
        //_rb.velocity = Vector3.zero;

        _anims.SetBool("Attack", true);

        LookAtAttackTarget();
        StunAttackTarget();
    }

    public override void End()
    {
        _anims.SetBool("Attack", false);
    }

    //============================== Animation Events =======================================

    void AV_AttackStart() { }
    void AV_Attack_Hit()
    {
        KillAttackTarget(); //Evento.
        _anims.SetBool("Attack", false); //Animación
    }
    void AV_Attack_Ended()
    {
        //Evalua si el target fue eliminado y que hacer a continuación.
        Think();
    }
}
