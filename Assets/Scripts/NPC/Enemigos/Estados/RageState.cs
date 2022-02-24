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
    public Action<IDamageable<Damage, HitResult>> SetAttackTarget = delegate { };
    public Action LookAtTarget = delegate { };
    LineOfSightComponent _sight = null;

    [SerializeField] LayerMask _targeteables = ~0;
    [SerializeField] float _MinDetectionRange = 5;

    int _Phase = 0;
    bool _playerFound = false;
    bool _otherUnitFound = false;
    bool _destructibleFound = false;

    IDamageable<Damage, HitResult> self = null;
    IDamageable<Damage, HitResult> _closerTargetFounded = null;

    HashSet<IDamageable<Damage, HitResult>> _posibleTargets = new HashSet<IDamageable<Damage, HitResult>>();

    #region DEBUG
    [Space(), Header("DEBUG")]
    [SerializeField] bool debugThisState = false;
    [SerializeField] bool DEBUG_RageMode_Ranges = true;
    [SerializeField] Color DEBUG_RM_TrgDetectRange_GIZMOCOLOR = Color.blue;
    private void OnDrawGizmos()
    {
        if (DEBUG_RageMode_Ranges)
        {
            Gizmos.color = DEBUG_RM_TrgDetectRange_GIZMOCOLOR;
            Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
            Gizmos.DrawWireSphere(transform.position, _MinDetectionRange);
        }
    } 
    #endregion

    public override void Begin()
    {
#if UNITY_EDITOR
        if(debugThisState)
            print($"{gameObject.name} entró al estado Rage"); 
#endif
        _anims.SetBool("GetHited", true);
    }

    public override void Execute()
    {
        base.Execute();
#if UNITY_EDITOR
        if (debugThisState)
            print($"RageState::Execute: Current Phase is: {_Phase}\nPlayer Founded: {_playerFound} | Other Target Founded:{_otherUnitFound} | Destructible Target Founded:{_destructibleFound}\n"); 
#endif
        if (_Phase == 1 && !_playerFound && !_otherUnitFound)
            CheckNearVisible();
        //if(_Phase == 2 && _otherTargetFounded)
        //    LookAtTarget();
        if(_Phase == 3 && !_playerFound && !_otherUnitFound && _destructibleFound)
        {
            LookAtTarget();
            SetAttackTarget(_closerTargetFounded);
        }
        if (_Phase == 4 && (_playerFound || _otherUnitFound || _destructibleFound))
            SwitchToState(CommonState.pursue);
    }

    public override void End()
    {
#if UNITY_EDITOR
        if (debugThisState)
            print($"RageState::End: Current Phase is: {_Phase}\nPlayer Founded: {_playerFound} | Other Target Founded:{_otherUnitFound} | Destructible Target Founded:{_destructibleFound}\n"); 
#endif
        _anims.SetBool("GetHited", false);
        _Phase = 0;
        _playerFound = false;
        _otherUnitFound = false;
        _destructibleFound = false;
        _posibleTargets.Clear();
        _closerTargetFounded = null;
    }

    public RageState Set(IDamageable<Damage, HitResult> self, LineOfSightComponent los)
    {
        this.self = self;
        _sight = los;
        return this;
    }
    void CheckNearVisible()
    {
#if UNITY_EDITOR
        if(debugThisState)
            Debug.Log("Checking Visibles");
#endif

        var posibleTargets = Physics.OverlapSphere(transform.position, _sight.range, _targeteables);
        List<IDamageable<Damage, HitResult>> Killeables = new List<IDamageable<Damage, HitResult>>();
        //Si no hay unidades vidas seleccionamos el target destructible mas cercano.

        foreach (var detectedCollider in posibleTargets)
        {
            //Chequeo primero por IDamageables.
            var Damageable = detectedCollider.GetComponent<IDamageable<Damage, HitResult>>();
            if (Damageable != null)
            {
                if (Damageable == self)
                    continue;

                Vector3 customLOSTargetDir = Vector3.zero;

                var clon = Damageable.GetComponent<ClonBehaviour>();
                if (clon != null)
                {
                    customLOSTargetDir = (clon.getLineOfSightTargetPosition() - _sight.CustomRayOrigin.position).normalized;
                    if (_sight.IsInSight(transform.position, customLOSTargetDir, clon.transform))
                    {
                        _playerFound = true;
                        LookAtTarget();
                        SetAttackTarget(Damageable);
                        _anims.SetBool("targetFinded", true);
                        return;
                    }
                }

                var player = Damageable.GetComponent<Controller>();
                if (player != null)
                {
                    customLOSTargetDir = (player.getLineOfSightTargetPosition() - _sight.CustomRayOrigin.position).normalized;
                    if (_sight.IsInSight(transform.position, customLOSTargetDir, player.transform))
                    {
                        _playerFound = true;
                        LookAtTarget();
                        SetAttackTarget(Damageable);
                        _anims.SetBool("targetFinded", true);
                        return;
                    }
                }

                customLOSTargetDir = (Damageable.transform.position - _sight.CustomRayOrigin.position).normalized;
                if (_sight.IsInSight(transform.position, customLOSTargetDir, Damageable.transform))
                {
                    //Chequeamos si el destructible es una unidad viva.
                    var alive = Damageable.GetComponent<ILivingEntity>();
                    if (alive != null)
                    {
                        _otherUnitFound = true;
                        SetAttackTarget(Damageable);
                        LookAtTarget();
                        _anims.SetBool("targetFinded", true);
                        return;
                    }
                    else
                    {
                        //Si no encuentro objetivos vivos, pero si un objeto destructible para ventilar la frustracion.
                        if (!_posibleTargets.Contains(Damageable))
                            _posibleTargets.Add(Damageable);
                    }
                }
            }
        }

        //Si llegamos a este punto, significa que no encontramos unidades vivas y que puede que haya unidades destructibles, seleccionamos el mas cercano.
        if (posibleTargets.Length > 0)
        {
            var targetFound = _posibleTargets.OrderBy(damageable =>
            {
                return Vector3.Distance(transform.position, damageable.transform.position);
            })
            .FirstOrDefault();

            if(targetFound != null)
            {
                _closerTargetFounded = targetFound;
                _destructibleFound = true;
                Debug.Log("a Target has been found");
            }
        }
    }

    public void SetAnimationStage(int current)
    {
#if UNITY_EDITOR
        if (debugThisState)
            print($"Current animation secuence state is {current}"); 
#endif
        //0 = turnAround Start. 1. Turn Arround End. 2. Inicio animacion de reclamo. 3.Fin de animacion de reclamo.
        _Phase = current;
    }
}
