﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Core.DamageSystem;
using UnityEngine.UIElements;

public struct CommandData
{
    public IInteractable target;
    public OperationType operationOptions;
}
//Navigation Mesh Actor Controller.
public class NMA_Controller : MonoBehaviour, IDamageable<Damage, HitResult>, IInteractor
{
    public event Action ImDeadBro = delegate { };
    //[SerializeField] Transform MouseDebug          = null;
    //[SerializeField] Transform targetDebug         = null;
    [SerializeField] float _movementTreshold       = 0.18f;

    Queue<IQueryComand> comandos = new Queue<IQueryComand>();

    Animator _anims;
    int[] animHash = new int[4];
    bool _a_Walking
    {
        get => _anims.GetBool(animHash[0]);
        set => _anims.SetBool(animHash[0], value);
    }
    bool _a_Crouching
    {
        get => _anims.GetBool(animHash[1]);
        set => _anims.SetBool(animHash[1], value);
    }
    bool _a_LeverPull
    {
        get => _anims.GetBool(animHash[2]);
        set => _anims.SetBool(animHash[2], value);
    }
    bool _a_Dead
    {
        get => _anims.GetBool(animHash[3]);
        set => _anims.SetBool(animHash[3], value);
    }
    bool _a_Ignite
    {
        get => _anims.GetBool(animHash[4]);
        set => _anims.SetBool(animHash[4], value);
    }
    bool _a_Clon
    {
        get => _anims.GetBool(animHash[5]);
        set => _anims.SetBool(animHash[5], value);
    }
    bool _a_ThrowRock
    {
        get => _anims.GetBool(animHash[6]);
        set => _anims.SetBool(animHash[6], value);
    }

    public Vector3 position => transform.position;

    Camera _viewCamera = null;
    Collider _mainCollider = null;
    Rigidbody _rb = null;
    NavMeshAgent _agent = null;
    CanvasController _canvasController = null;
    MouseView _mv;
    MouseContextTracker _mtracker;
    public GameObject clon;
    public float clonLife;
    float clonlifeTime;
    public float clonCooldown;
    float clonCooldownRemain;
    bool clonRecast;

    Vector3 _currentTargetPos;
    float forwardLerpTime;
    bool PlayerInputEnabled = true;

    CommandData Queued_ActivationData = new CommandData();

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCollider = GetComponent<Collider>();
        _agent = GetComponent<NavMeshAgent>();
        _viewCamera = Camera.main;
        _canvasController = FindObjectOfType<CanvasController>();
        _mv = GetComponent<MouseView>();
        _mtracker = GetComponent<MouseContextTracker>();

        _anims = GetComponent<Animator>();
        animHash = new int[7];
        var animparams = _anims.parameters;
        for (int i = 0; i < animHash.Length; i++)
            animHash[i] = animparams[i].nameHash;
    }

    // Update is called once per frame
    void Update()
    {
        bool mod1 = Input.GetKey(KeyCode.LeftShift);

        #region Input
        if (PlayerInputEnabled && Input.GetMouseButtonDown(1))
        {
            //Hacer un raycast y fijarme si hay un objeto que se interactuable.
            MouseContext _mouseContext = _mtracker.GetCurrentMouseContext();

            if (!_mouseContext.validHit) return;

            if (_mouseContext.interactuableHitted)
            {
                //Muestro el menú en la posición del mouse, con las opciones soportadas por dicho objeto.
                _canvasController.DisplayCommandMenu
                (
                    Input.mousePosition,
                    _mouseContext.firstInteractionObject.GetSuportedInteractionParameters(),
                    _mouseContext.firstInteractionObject,
                    QuerySelectedOperation
                 );
            }
            else
            {
                if (mod1)
                    _mv.SetMousePositionAditive(_mouseContext.hitPosition);
                else
                {
                    CancelAllCommands();
                    _mv.SetMousePosition(_mouseContext.hitPosition);
                }

                IQueryComand moveCommand = new cmd_Move
                (
                    _mouseContext.hitPosition,
                    MoveToTarget,
                    (targetPos) =>
                    {
                        float dst = Vector3.Distance(transform.position, targetPos);
                        bool completed = dst <= _movementTreshold;

                        if (completed)
                            _a_Walking = false;

                        return completed;
                    },
                    _disposeCommand
                );
                comandos.Enqueue(moveCommand);
            }
        }


        #endregion
        #region Clon
        if (PlayerInputEnabled && Input.GetKeyDown(KeyCode.Alpha1) && clonRecast)
        {
            comandos.Clear();
            _a_Clon = true;
            PlayerInputEnabled = false;

        }
        if (clon != null && clon.activeInHierarchy)
        {
            if (clonlifeTime > 0)
                clonlifeTime -= Time.deltaTime;
            else
            {
                clon.SetActive(false);
                clonRecast = false;
                clonCooldownRemain = clonCooldown;
            }
        }
        else if(clon != null)
        {
            if(!clonRecast)
            {
                if (clonCooldownRemain > 0)
                     clonCooldownRemain -= Time.deltaTime;
                else
                {
                clonRecast = true;
                clonCooldownRemain = 0;
                }

            }
        }
        #endregion

        if (comandos.Count > 0)
        {
            IQueryComand current = comandos.Peek();
            current.Execute();
        }
    }

    /// <summary>
    /// Callback que se llama cuando seleccionamos una acción a realizar sobre un objeto interactuable desde el panel de comandos.
    /// </summary>
    /// <param name="operation">La operación que queremos realizar</param>
    /// <param name="target">El objetivo de dicha operación</param>
    public void QuerySelectedOperation(OperationType operation, IInteractable target)
    {
        var safeInteractionPosition = target.requestSafeInteractionPosition(this);
        if (Vector3.Distance(transform.position, safeInteractionPosition) > _movementTreshold)
        {
            IQueryComand closeDistance = new cmd_Move
            (
                safeInteractionPosition,
                MoveToTarget, 
                (targetPos) => 
                {
                    float dst = Vector3.Distance(transform.position, targetPos);
                    bool completed = dst <= _movementTreshold;

                    if (completed && _a_Walking)
                        _a_Walking = false;

                    return completed;
                },
                _disposeCommand
            );
            comandos.Enqueue(closeDistance);
            //print("Comando CloseDistance añadido. Hay " + comandos.Count + " comandos");
        }

        //añado el comando correspondiente a la query.
        IQueryComand _toActivateCommand;
        CommandData _currentOperationData = new CommandData()
        {
            target = target,
            operationOptions = operation
        };
        switch (operation)
        {
            case OperationType.Take:
                break;

            case OperationType.Ignite:

                target.OnConfirmInput(OperationType.Ignite);
                _toActivateCommand = new cmd_Ignite(
                                                      _currentOperationData,
                                                      () => 
                                                      {
                                                          _a_Ignite = true;
                                                          Queued_ActivationData = _currentOperationData;
                                                      },
                                                      _disposeCommand
                                                   );
                comandos.Enqueue(_toActivateCommand);

                break;

            case OperationType.Activate:

                _toActivateCommand = new cmd_Activate
                    (
                        _currentOperationData,
                        () => 
                        {
                            _a_LeverPull = true;
                            Queued_ActivationData = _currentOperationData;
                        },
                        _disposeCommand
                    );
                comandos.Enqueue(_toActivateCommand);
                break;

            case OperationType.Equip:
                break;
            case OperationType.TrowRock:
                _toActivateCommand = new cmd_TrowRock
                    (
                       _currentOperationData,
                       () => 
                       {
                           _a_ThrowRock = true;
                           transform.forward = (target.position - transform.position).normalized;
                           Queued_ActivationData = _currentOperationData;
                       },
                       _disposeCommand
                    );
                comandos.Enqueue(_toActivateCommand);
                break;
            default:
                break;
        }
    }

    //Movimiento
    public void MoveToTarget(Vector3 destinyPosition)
    {
        Vector3 _targetForward = (destinyPosition - transform.position).normalized.YComponent(0);
        transform.forward = _targetForward;
        if (_currentTargetPos != destinyPosition)
            _currentTargetPos = destinyPosition;
        if (!_a_Walking)
            _a_Walking = true;

        _agent.destination = destinyPosition;
    }

    void _disposeCommand()
    {
        var _currentC = comandos.Dequeue();
        if (comandos.Count > 0)
        {
            var next = comandos.Peek();
            //print(string.Format("Comando {0} Finalizado\nSiguiente comando es {1}", _currentC, next));
        }
    }
    void CancelAllCommands()
    {
        foreach (var command in comandos)
        {
            command.Cancel();
        }
        comandos.Clear();
    }
    public void ClonSpawn()
    {
        if (!clon.activeInHierarchy)
        {
            clonlifeTime = clonLife;
            clon.SetActive(true);
            clon.transform.position = transform.position + transform.forward * 1.5f;
            clon.transform.forward = -transform.forward;
        }
        else
        {
            clon.SetActive(false);
            clonRecast = false;
            clonCooldownRemain = clonCooldown;
        }
    }
    public void FallInTrap()
    {
        PlayerInputEnabled = false;
        _agent.isStopped = true;
        _agent.ResetPath();

        _agent.enabled = false;
        _rb.useGravity = true;
        _mainCollider.isTrigger = true;
    }
    void Die()
    {
        PlayerInputEnabled = false;

        if (_agent.isActiveAndEnabled)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        _a_Dead = true;
        ImDeadBro();
    }

    //============================================================== Damage System =================================================================

    public HitResult GetHit(Damage damage)
    {
        HitResult result = new HitResult() { conected = true, fatalDamage = true };
        if (damage.instaKill)
        {
            Die();
        }
        return result;
    }
    public void FeedDamageResult(HitResult result) { }
    public Damage GetDamageStats()
    {
        return new Damage()
        {
            Ammount = 10f,
            instaKill = false,
            criticalMultiplier = 2,
            type = DamageType.piercing
        };
    }

    //=============================================================== Animation Events =============================================================
    void AE_PullLeverStarted()
    {
        PlayerInputEnabled = false;

        if (Queued_ActivationData.target != null)
        {
            transform.forward = Queued_ActivationData.target.LookToDirection;
        }
    }
    void AE_PullLeverEnded()
    {
        PlayerInputEnabled = true;
        _a_LeverPull = false;

        if (Queued_ActivationData.target != null)
        {
            Queued_ActivationData.target.OnOperate(Queued_ActivationData.operationOptions);
            Queued_ActivationData = new CommandData();
        }
    }
    void AE_Ignite_Start()
    {
        PlayerInputEnabled = false;
    }
    void AE_Ignite_End()
    {
        PlayerInputEnabled = true;
        _a_Ignite = false;

        if (Queued_ActivationData.target != null)
        {
            Queued_ActivationData.target.OnOperate(Queued_ActivationData.operationOptions);
            Queued_ActivationData = new CommandData();
        }
    }
    void AE_TrowRock_Ended()
    {
        _a_ThrowRock = false;

        if (Queued_ActivationData.target != null)
        {
            Queued_ActivationData.target.OnOperate(Queued_ActivationData.operationOptions);
            Queued_ActivationData = new CommandData();
        }
    }

    public void finishClon()
    {
        _a_Clon = false;
        PlayerInputEnabled = true;
    }

    //============================================================== DEBUG ==========================================================================
}

