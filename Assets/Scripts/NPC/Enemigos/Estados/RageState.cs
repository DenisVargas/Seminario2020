using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.FSM;
using System;
using System.Linq;
using IA.LineOfSight;
using Core.DamageSystem;

public class RageState : State
{
    public Action LookAtTarget = delegate { };

    [SerializeField] LayerMask _targeteables = ~0;
    [SerializeField] float _rageMode_TargetDetectionRange = 5;
    [SerializeField] float _detectionDelay = 0.5f;

    bool _otherKilleableTargetFounded = false;
    bool _otherDestructibleFounded = false;
    bool _playerFound;

    IDamageable<Damage, HitResult> _killeableTarget;
    List<Transform> _targetsfounded = new List<Transform>();

    LineOfSightComponent _sight = null;

    int _hitSecuence = 0;

    #region DEBUG
    [Space(), Header("DEBUG")]
    [SerializeField] bool DEBUG_RageMode_Ranges = true;
    [SerializeField] Color DEBUG_RM_TrgDetectRange_GIZMOCOLOR = Color.blue;
    private void OnDrawGizmos()
    {
        if (DEBUG_RageMode_Ranges)
        {
            Gizmos.color = DEBUG_RM_TrgDetectRange_GIZMOCOLOR;
            Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
            Gizmos.DrawWireSphere(transform.position, _rageMode_TargetDetectionRange);
        }
    } 
    #endregion

    public override void Begin()
    {
        //print($"{gameObject.name} entró al estado Rage");
        _targetsfounded = new List<Transform>();

        _anims.SetBool("GetHited", true);
        _hitSecuence = 1;
    }

    public override void Execute()
    {
        base.Execute();
        if (_hitSecuence == 2 && !_playerFound)
            CheckNearVisible();
        if (_hitSecuence == 3)
            LookAtTarget(); //Mira en dirección del target encontrado.
    }

    public override void End()
    {
        _anims.SetBool("GetHited", false);
        _hitSecuence = 0;
        _otherKilleableTargetFounded = false;
        _targetsfounded = new List<Transform>();
        _playerFound = false;
    }

    void CheckNearVisible()
    {
        //Priorizamos las unidades vivas.
        var posibleTargets = Physics.OverlapSphere(transform.position, _rageMode_TargetDetectionRange, _targeteables);
        List<IDamageable<Damage, HitResult>> OnSight_Killeables = new List<IDamageable<Damage, HitResult>>();
        //Si no hay unidades vidas seleccionamos el target destructible mas cercano.
        List<IDestructible> OnSight_Destructibles = new List<IDestructible>();

        foreach (var item in posibleTargets)
        {
            //Chequeo primero por IDamageables.
            var Damageable = item.GetComponent<IDamageable<Damage, HitResult>>();
            if (Damageable == (IDamageable<Damage, HitResult>)this)
                continue;

            if (Damageable != null && _sight.IsInSight(item.transform))
            {
                OnSight_Killeables.Add(Damageable);

                if (Damageable.gameObject.CompareTag("Player"))
                {
                    Debug.LogWarning("TE VI PUTO");
                    _playerFound = true;
                    _killeableTarget = Damageable;
                    StartCoroutine(DelayedDetection());
                    break;
                }
                else
                    _otherKilleableTargetFounded = true;

                continue;
            }

            //Chequeo por IDestructible.
            var Destructible = item.GetComponent<IDestructible>();
            if (Destructible != null && _sight.IsInSight(item.transform))
            {
                _otherDestructibleFounded = true;
                OnSight_Destructibles.Add(Destructible);
            }
        }

        if (OnSight_Killeables.Count > 0)
        {
            //Si hay objetivos dentro del rango de vision...
            var CloserTarget = OnSight_Killeables
                              .OrderBy(killeable =>
                              {
                                  return Vector3.Distance(transform.position,
                                                          killeable.gameObject.transform.position);
                              })
                              .First()
                              .gameObject.transform;

            if (!_targetsfounded.Contains(CloserTarget))
                _targetsfounded.Add(CloserTarget);
        }
        else if (OnSight_Destructibles.Count > 0)
        {
            //Si no hay objetivos killeables, chequeo los destructibles...
            var CloserTarget = OnSight_Destructibles
                .OrderBy(destructible =>
                {
                    return Vector3.Distance(transform.position, destructible.position);
                })
                .First()
                .transform;

            if (!_targetsfounded.Contains(CloserTarget))
                _targetsfounded.Add(CloserTarget);
        }
        print($"{gameObject.name}::CheckNearVisible");
    }

    IEnumerator DelayedDetection()
    {
        yield return new WaitForSeconds(_detectionDelay);
        Debug.LogWarning("PLAYER SPOTTED");
        _anims.SetBool("TargetFounded", true);
        SwitchToState(CommonState.pursue);
    }
}
