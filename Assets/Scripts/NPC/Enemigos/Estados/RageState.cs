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

    bool _playerFound = false;
    bool _enemyFound = false;
    bool _destructibleFound = false;
    bool _targetSetted = false;

    bool _turningArround_Animation = false;

    IDamageable<Damage, HitResult> self = null;

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
        _anims.SetBool("RageState", true);
    }

    public override void Execute()
    {
        base.Execute();
        if (!_targetSetted && _turningArround_Animation && (!_playerFound || !_enemyFound))
        {
            CheckNearVisible();
        }
    }

    public override void End()
    {
        _anims.SetBool("RageState", false);
        _anims.SetBool("SmashGround", false);
        _playerFound = false;
        _enemyFound = false;
        _destructibleFound = false;
        _targetSetted = false;
        _posibleTargets.Clear();
    }

    public RageState Set(IDamageable<Damage, HitResult> self, LineOfSightComponent los)
    {
        this.self = self;
        _sight = los;
        return this;
    }
    void CheckNearVisible()
    {
        var posibleTargets = Physics.OverlapSphere(transform.position, _sight.range, _targeteables);
        List<IDamageable<Damage, HitResult>> Killeables = new List<IDamageable<Damage, HitResult>>();
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
                if (clon != null && clon.IsAlive)
                {
                    customLOSTargetDir = (clon.getLineOfSightTargetPosition() - _sight.CustomRayOrigin.position).normalized;
                    if (_sight.IsInSight(transform.position, customLOSTargetDir, clon.transform))
                    {
#if UNITY_EDITOR
                        if (debugThisState)
                            Debug.Log($"Clon has been founded");
#endif
                        _playerFound = true;
                        SetTargetFound(Damageable);
                        return;
                    }
                }

                var player = Damageable.GetComponent<Controller>();
                if (player != null && player.IsAlive)
                {
                    customLOSTargetDir = (player.getLineOfSightTargetPosition() - _sight.CustomRayOrigin.position).normalized;
                    if (_sight.IsInSight(transform.position, customLOSTargetDir, player.transform))
                    {
#if UNITY_EDITOR
                        if (debugThisState)
                            Debug.Log($"Player has been founded");
#endif
                        _playerFound = true;
                        SetTargetFound(Damageable);
                        return;
                    }
                }

                customLOSTargetDir = (Damageable.transform.position - _sight.CustomRayOrigin.position).normalized;
                if (_sight.IsInSight(transform.position, customLOSTargetDir, Damageable.transform))
                {
                    //Chequeamos si el destructible es una unidad viva.
                    var alive = Damageable.GetComponent<ILivingEntity>();
                    if (alive != null && Damageable.IsAlive)
                    {
#if UNITY_EDITOR
                        if (debugThisState)
                            Debug.Log($"An enemy {Damageable} has been founded");
#endif
                        _enemyFound = true;
                        SetTargetFound(Damageable);
                        return;
                    }
                    else
                    {
                        if (!_posibleTargets.Contains(Damageable))
                        {
#if UNITY_EDITOR
                            if (debugThisState)
                                Debug.Log($"A new destructible: {Damageable} has been founded");
#endif
                            _posibleTargets.Add(Damageable);
                        }
                    }
                }
            }
        }
    }

    public void SetTargetFound(IDamageable<Damage, HitResult> Damageable)
    {
        _targetSetted = true;
        _anims.SetBool("RageState", false);
        _anims.SetBool("targetFinded", true);
        SetAttackTarget(Damageable);
        LookAtTarget();

        _posibleTargets.Clear();
    }

    public void StartTurningAround()
    {
        _turningArround_Animation = true;
    }

    public void StartAngryAnimation()
    {
        _turningArround_Animation = false;
        Debug.Log("Start angry animation");

        if (!_playerFound && !_enemyFound)
        {
            if (_posibleTargets.Count > 0)
            {
                var targetFound = _posibleTargets.Where((damageable) => {
                    if (damageable != null)
                        return damageable.IsAlive;
                    return false;
                })
                .OrderBy(damageable =>
                {
                    return Vector3.Distance(transform.position, damageable.transform.position);
                })
                .FirstOrDefault();

                if (targetFound != null)
                {
#if UNITY_EDITOR
                    if (debugThisState)
                        Debug.Log("a Target has been found");
#endif
                    _destructibleFound = true;
                    SetTargetFound(targetFound);
                    return;
                }
            }
            else _anims.SetBool("SmashGround", true);
        }
    }

    public void EndOfAngryAnimation()
    {
        if((_playerFound || _enemyFound || _destructibleFound) && _targetSetted)
            SwitchToState(CommonState.pursue);
    }
}
