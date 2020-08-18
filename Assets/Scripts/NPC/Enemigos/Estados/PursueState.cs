using Core.DamageSystem;
using IA.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PursueState : State
{
    public Action StopMovement = delegate { };
    public Action MoveToTarget = delegate { };
    public Func<bool> checkDistanceToTarget = delegate { return false; };

    [SerializeField]
    float minDistanceToAttack = 3f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, minDistanceToAttack);
    }

    public override void Begin()
    {
        _anims.SetBool("Walking", true);
    }

    public override void Execute()
    {
        //Me muevo en dirección al objetivo.
        MoveToTarget();
        //if (_target != null)
        //    MoveToTarget(_target.transform.position);

        //Si la distancia de ataque es menor a un treshold.
        //float dst = Vector3.Distance(transform.position, _target.transform.position);

        if (checkDistanceToTarget())
            SwitchToState(CommonState.attack);
    }

    public override void End()
    {
        StopMovement();
        _anims.SetBool("Walking", false);
    }
}
