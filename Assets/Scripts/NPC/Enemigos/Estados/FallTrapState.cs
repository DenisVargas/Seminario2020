using IA.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallTrapState : State
{
    public Action DeactivateMovement = delegate { };

    [SerializeField, Tooltip("Un Collider que pasa a ser Trigger al caer en una trampa.")]
    Collider _mainCollider = null;

    Rigidbody _rb = null;
    PathFindSolver _solver = null;

    public override void Begin()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
        _rb.isKinematic = false;

        DeactivateMovement();

        _mainCollider.isTrigger = true;
    }
}
