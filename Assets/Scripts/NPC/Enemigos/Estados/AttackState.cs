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
    public Action<bool> SetIgnoreEntityDead = delegate { };

    //Rigidbody _rb = null;

    public override void Begin()
    {
        //_rb = GetComponent<Rigidbody>(); //Si movemos por rigidbody.
        //_rb.velocity = Vector3.zero;

        SetIgnoreEntityDead(true);
        _anims.SetBool("Attack", true);
        LookAtAttackTarget();
    }

    public override void End()
    {
        _anims.SetBool("Attack", false);
    }

    //============================== Animation ==============================================

    public void setAttackStage(int stage)
    {
        switch (stage)
        {
            case 1:
                StunAttackTarget();
                break;

            case 2:
                KillAttackTarget(); //Evento.
                _anims.SetBool("Attack", false); //Animación
                break;

            case 3:
                //Evalua si el target fue eliminado y que hacer a continuación.
                SetIgnoreEntityDead(false);
                Think();
                break;

            default:
                break;
        }
    }
}
