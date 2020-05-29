﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Core.DamageSystem;
using UnityEngine.UIElements;

public struct ActivationCommandData
{
    public IInteractable target;
    public OperationType operationOptions;
}
//Navigation Mesh Actor Controller.
public class NMA_Controller : MonoBehaviour, IDamageable<Damage>, IInteractor
{
    public event Action ImDeadBro = delegate { };
    [SerializeField] LayerMask mouseDetectionMask  = ~0;
    //[SerializeField] Transform MouseDebug          = null;
    //[SerializeField] Transform targetDebug         = null;
    [SerializeField] float _interactionMaxDistance = 0.1f;
    [SerializeField] float _movementTreshold       = 0.18f;

    Queue<IQueryComand> comandos = new Queue<IQueryComand>();


    Animator _anims;
    int[] animHash = new int[3];
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

    ActivationCommandData Queued_ActivationData = new ActivationCommandData();

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
        animHash = new int[5];
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
                    ExecuteOperation
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
                    disposeCommand
                );
                comandos.Enqueue(moveCommand);
            }
        }


        #endregion
        #region Clon
        if (PlayerInputEnabled && Input.GetKeyDown(KeyCode.Alpha1) && clonRecast)
        {
            if (!clon.activeInHierarchy)
            {
                clonlifeTime = clonLife;
                clon.SetActive(true);
            }

            else
            {
                clon.SetActive(false);
                clonRecast = false;
                clonCooldownRemain = clonCooldown;
            }
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
            if (clonCooldownRemain > 0)
                clonCooldownRemain -= Time.deltaTime;
            else
            {
                clonRecast = true;
                clonCooldownRemain = 0;
            }
        }
        #endregion

        //if (_currentTargetPos != Vector3.zero && transform.forward != _currentTargetPos)
        //    UpdateForward();

        //print("Hay " + comandos.Count + " comandos");

        if (comandos.Count > 0)
        {
            IQueryComand current = comandos.Peek();
            current.Update();
        }
    }

    /// <summary>
    /// Callback que se llama cuando seleccionamos una acción a realizar sobre un objeto interactuable desde el panel de comandos.
    /// </summary>
    /// <param name="operation">La operación que queremos realizar</param>
    /// <param name="target">El objetivo de dicha operación</param>
    public void ExecuteOperation(OperationType operation, IInteractable target)
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
                disposeCommand
            );
            comandos.Enqueue(closeDistance);
            //print("Comando CloseDistance añadido. Hay " + comandos.Count + " comandos");
        }

        //añado el comando correspondiente a la query.
        IQueryComand _toActivateCommand;
        switch (operation)
        {
            case OperationType.Take:
                break;
            case OperationType.Ignite:

                //Aquí tengo que decirle a mi target que se frezee.
                target.OnConfirmInput(OperationType.Ignite);

                Queued_ActivationData = new ActivationCommandData() { operationOptions = operation, target = target };
                _toActivateCommand = new cmd_Ignite(
                                                      new ActivationCommandData()
                                                      {
                                                         target = target,
                                                         operationOptions = operation
                                                      },
                                                      () => { _a_Ignite = true; },
                                                      disposeCommand
                                                   );
                comandos.Enqueue(_toActivateCommand);

                break;

            case OperationType.Activate:

                Queued_ActivationData = new ActivationCommandData() { target = target, operationOptions = operation };
                _toActivateCommand = new cmd_Activate
                    (
                        Queued_ActivationData,
                        () => { _a_LeverPull = true; },
                        disposeCommand
                    );
                comandos.Enqueue(_toActivateCommand);
                //print("Comando Activate añadido. Hay " + comandos.Count + " comandos");

                break;
            case OperationType.Equip:
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

    void disposeCommand()
    {
        var _currentC = comandos.Dequeue();
        if (comandos.Count > 0)
        {
            var next = comandos.Peek();
            print(string.Format("Comando {0} Finalizado\nSiguiente comando es {1}", _currentC, next));
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

    public void Hit(Damage damage)
    {
        if (damage.instaKill)
        {
            Die();
        }
    }

    //=============================================================== Animation Events =============================================================
    public void AE_PullLeverStarted()
    {
        PlayerInputEnabled = false;

        if (Queued_ActivationData.target != null)
        {
            transform.forward = Queued_ActivationData.target.LookToDirection;
        }
    }
    public void AE_PullLeverEnded()
    {
        PlayerInputEnabled = true;
        _a_LeverPull = false;

        if (Queued_ActivationData.target != null)
        {
            Queued_ActivationData.target.OnOperate(Queued_ActivationData.operationOptions);
            Queued_ActivationData = new ActivationCommandData();
        }
    }
    public void AE_Ignite_Start()
    {
        PlayerInputEnabled = false;
    }
    public void AE_Ignite_End()
    {
        PlayerInputEnabled = true;
        _a_Ignite = false;

        if (Queued_ActivationData.target != null)
        {
            Queued_ActivationData.target.OnOperate(Queued_ActivationData.operationOptions);
            Queued_ActivationData = new ActivationCommandData();
        }
    }

    //============================================================== DEBUG ==========================================================================

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _interactionMaxDistance);
    }

}

