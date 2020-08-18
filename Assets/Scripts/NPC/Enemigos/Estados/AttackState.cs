using IA.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AttackState : State
{
    public Action StopMovement = delegate { };
    public Action LookAtAttackTarget = delegate { };
    public Action StunAttackTarget = delegate { };

    Rigidbody _rb = null;

    public override void Begin()
    {
        _anims.SetBool("Attack", true);
        _anims.SetBool("Walking", false);

        StopMovement();
        _rb.velocity = Vector3.zero;

        LookAtAttackTarget();

        StunAttackTarget();
        //    _killeableTarget.GetStun(transform.position, 1);
    }

    public override void End()
    {
        _anims.SetBool("Attack", false);
    }
}
