using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using System;
using IA.PathFinding;
using Core.Interaction;

[RequireComponent(typeof(PathFindSolver), typeof(MouseContextTracker))]
public class Controller : MonoBehaviour, IDamageable<Damage, HitResult>
{
    //Stats.
    public int Health = 100;

    public event Action ImDeadBro;
    public event Action OnMovementChange = delegate { };

    public Transform manitodumacaco;
    public ParticleSystem BloodStain;

    public float moveSpeed = 6;
    [SerializeField] float _movementTreshold = 0.18f;
    public Transform MouseDebug;

    Queue<IQueryComand> comandos = new Queue<IQueryComand>();
    IInteractionComponent Queued_TargetInteractionComponent = null;
    Node QueuedMovementEndPoint = null;

    bool PlayerInputEnabled = true;
    bool ClonInputEnabled = true;
    //bool playerMovementEnabled = true;
    Vector3 velocity;

    #region Componentes
    Rigidbody _rb;
    Camera _viewCamera;
    Collider _mainCollider = null;
    CanvasController _canvasController = null;
    MouseView _mv;
    MouseContextTracker _mtracker;
    PathFindSolver _solver;
    #endregion
    #region Clon
    [Header("Clon")]
    public ClonBehaviour Clon = null;
    [SerializeField] float _clonLife = 20f;
    [SerializeField] float _clonCooldown = 4f;
    [SerializeField] float _ClonMovementTreshold = 0.1f;
    bool _canCastAClon = true;
    #endregion
    #region Animaciones
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
    bool _a_GetStunned
    {
        get => _anims.GetBool(animHash[7]);
        set => _anims.SetBool(animHash[7], value);
    }
    int _a_KillingMethodID
    {
        get => _anims.GetInteger(animHash[8]);
        set => _anims.SetInteger(animHash[8], value);
    }
    bool _a_GetSmashed
    {
        get => _anims.GetBool(animHash[9]);
        set => _anims.SetBool(animHash[9], value);
    }
    bool _a_Grabing
    {
        get => _anims.GetBool(animHash[10]);
        set => _anims.SetBool(animHash[10], value);
    }
    #endregion

    //================================= UnityEngine ========================================

    private void Awake()
    {
        //Componentes.
        _rb = GetComponent<Rigidbody>();
        _mainCollider = GetComponent<Collider>();
        _viewCamera = Camera.main;
        _canvasController = FindObjectOfType<CanvasController>();
        _mv = GetComponent<MouseView>();
        _mtracker = GetComponent<MouseContextTracker>();
        _solver = GetComponent<PathFindSolver>();

        if (_solver.Origin == null)
        {
            var closerNode = _solver.getCloserNode(transform.position);
            transform.position = closerNode.transform.position;
            _solver.SetOrigin(closerNode);
        }

        //Clon.
        if (Clon != null)
        {
            Clon.Awake();
            Clon.SetState(_clonLife, _ClonMovementTreshold);
            Clon.OnRecast += ClonDeactivate;
        }

        //Animaciones.
        _anims = GetComponent<Animator>();
        animHash = new int[11];
        var animparams = _anims.parameters;
        for (int i = 0; i < animHash.Length; i++)
            animHash[i] = animparams[i].nameHash;
    }
    void Update()
    {
        #region Input
        if (PlayerInputEnabled)
        {
            // MouseClic Derecho.
            if (Input.GetMouseButtonDown(1))
            {
                MouseContext _mouseContext = _mtracker.GetCurrentMouseContext();//Obtengo el contexto del Mouse.

                if (!_mouseContext.validHit) return; //Si no hay hit Válido.

                if (_mouseContext.interactuableHitted)
                {
                    //Muestro el menú en la posición del mouse, con las opciones soportadas por dicho objeto.
                    _canvasController.DisplayCommandMenu
                    (
                        Input.mousePosition,
                        _mouseContext.InteractionHandler,
                        QuerySelectedOperation
                     );
                }
                else
                {
                    bool mod1 = Input.GetKey(KeyCode.LeftShift);
                    bool mod2 = Input.GetKey(KeyCode.LeftControl);

                    if (mod1) //Si presiono shift, muestro donde estoy presionando de forma aditiva.
                    {
                        _mv.SetMousePositionAditive(_mouseContext.closerNode.transform.position);
                        AddMovementCommand(_mouseContext);
                    }
                    else
                    {
                        CancelAllCommands();
                        _mv.SetMousePosition(_mouseContext.closerNode.transform.position);
                        AddMovementCommand(_mouseContext);
                    }

                    if (mod2 && Clon.IsActive)
                        Clon.SetMovementDestinyPosition(_mouseContext.hitPosition);
                }
            }
        }
        #endregion
        #region Clon Input
        if (ClonInputEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && !Clon.IsActive)
            {
                comandos.Clear();
                _a_Clon = true;
                PlayerInputEnabled = false;
                ClonSpawn();
            }
        }
        #endregion

        if (comandos.Count > 0)
        {
            IQueryComand current = comandos.Peek();
            if (!current.isReady)
                current.SetUp();
            if (!current.cashed)
                current.Execute();
        }
    }

    private void AddMovementCommand(MouseContext _mouseContext)
    {
        _solver.SetOrigin(QueuedMovementEndPoint == null ? transform.position : QueuedMovementEndPoint.transform.position)
               .SetTarget(_mouseContext.closerNode)
               .CalculatePathUsingSettings();

        if (_solver.currentPath.Count == 0) //Si el solver no halló un camino, no hay camino posible.
            return;

        QueuedMovementEndPoint = _mouseContext.closerNode;

        IQueryComand moveCommand = new cmd_Move
                            (
                                _mouseContext.closerNode,
                                _solver.currentPath,
                                () => { OnMovementChange(); },
                                MoveToTarget,
                                (targetNode) =>
                                {
                                    float dst = Vector3.Distance(transform.position, targetNode.transform.position);
                                    bool completed = dst <= _movementTreshold;

                                    if (completed)
                                        _a_Walking = false;

                                    return completed;
                                },
                                () => { comandos.Dequeue(); }
                            );
        comandos.Enqueue(moveCommand);
    }

    //================================= Damage System ======================================

    public bool IsAlive => Health > 0;

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
    public HitResult GetHit(Damage damage)
    {
        HitResult result = new HitResult() { conected = true, fatalDamage = true };
        if (damage.instaKill)
        {
            Die(damage.KillAnimationType);
        }
        return result;
    }
    public void FeedDamageResult(HitResult result) { }
    public void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod)
    {
        Vector3 DirToAgressor = (AgressorPosition - transform.position).normalized;
        transform.forward = DirToAgressor;

        _a_Walking = false;
        _a_KillingMethodID = PosibleKillingMethod;
        _a_GetStunned = true;

        PlayerInputEnabled = false;
        comandos.Clear();
        //_agent.isStopped = true;
        //_agent.ResetPath();
    }

    //================================== Player Controller =================================

    #region Clon
    public void ClonSpawn()
    {
        if (!Clon.IsActive && _canCastAClon)
        {
            Clon.InvokeClon(transform.position + transform.forward * 1.5f, -transform.forward);
            _canCastAClon = false;
        }
    }
    public void finishClon()
    {
        _a_Clon = false;
        PlayerInputEnabled = true;
        _canCastAClon = true;
    }
    public void ClonDeactivate()
    {
        StartCoroutine(clonCoolDown());
    }
    IEnumerator clonCoolDown()
    {
        _canCastAClon = false;
        yield return new WaitForSeconds(_clonCooldown);
        _canCastAClon = true;
    } 
    #endregion

    public void FallInTrap()
    {
        PlayerInputEnabled = false;
        //playerMovementEnabled = false;
        comandos.Clear();

        _rb.useGravity = true;
        _rb.isKinematic = false;
        _mainCollider.isTrigger = true;
    }
    public void PlayBlood()
    {
        BloodStain.Play();
    }

    public bool MoveToTarget(Node targetNode)
    {
        Vector3 dirToTarget = (targetNode.transform.position - transform.position).normalized;
        transform.forward = dirToTarget;

        if (!_a_Walking)
            _a_Walking = true;

        transform.position += dirToTarget * moveSpeed * Time.deltaTime;
        return Vector3.Distance(transform.position, targetNode.transform.position) <= _movementTreshold;
    }

    /// <summary>
    /// Callback que se llama cuando seleccionamos una acción a realizar sobre un objeto interactuable desde el panel de comandos.
    /// </summary>
    /// <param name="target">El objetivo de dicha operación. Es un interaction Component que contiene dentro de si el tipo de la operación.</param>
    public void QuerySelectedOperation(IInteractionComponent target)
    {
        var safeInteractionPosition = target.requestSafeInteractionPosition(transform.position);
        Node targetNode = _solver.getCloserNode(safeInteractionPosition);

        if (Vector3.Distance(transform.position, safeInteractionPosition) > _movementTreshold)
        {
            _solver.SetOrigin(QueuedMovementEndPoint == null ? transform.position : QueuedMovementEndPoint.transform.position)
               .SetTarget(targetNode)
               .CalculatePathUsingSettings();

            if (_solver.currentPath.Count == 0) //Si el solver no halló un camino, no hay camino posible.
                return;

            QueuedMovementEndPoint = targetNode;

            IQueryComand closeDistance = new cmd_Move
            (
                _solver.getCloserNode(safeInteractionPosition),
                _solver.currentPath,
                () => { OnMovementChange(); },
                MoveToTarget,
                (targetPos) =>
                {
                    float dst = Vector3.Distance(transform.position, targetPos.transform.position);
                    bool completed = dst <= _movementTreshold;

                    if (completed && _a_Walking)
                        _a_Walking = false;

                    return completed;
                },
                () => { comandos.Dequeue(); }
            );
            comandos.Enqueue(closeDistance);
        }

        //añado el comando correspondiente a la query.
        IQueryComand _toActivateCommand;
        switch (target.OperationType)
        {
            case OperationType.Take:
                _toActivateCommand = new cmd_Take(target, manitodumacaco, () => { _a_Grabing = true; });
                comandos.Enqueue(_toActivateCommand);
                break;

            case OperationType.Ignite:

                _toActivateCommand = new cmd_Ignite( target,() => { _a_Ignite = true; });
                comandos.Enqueue(_toActivateCommand);

                break;

            case OperationType.Activate:

                _toActivateCommand = new cmd_Activate ( target, () => { _a_LeverPull = true; });
                comandos.Enqueue(_toActivateCommand);
                break;

            case OperationType.Equip:
                break;
            case OperationType.Throw:
                _toActivateCommand = new cmd_TrowRock
                    (
                       target,
                       () =>
                       {
                           _a_ThrowRock = true;
                           transform.forward = (target.transform.position - transform.position).normalized;
                       }
                    );
                comandos.Enqueue(_toActivateCommand);
                break;
            default:
                break;
        }
    }

    //========================================================================================

    void CancelAllCommands()
    {
        foreach (var command in comandos)
        {
            command.Cancel();
        }
        QueuedMovementEndPoint = null;
        comandos.Clear();
    }
    void Die(int KillingAnimType)
    {
        PlayerInputEnabled = false;
        //playerMovementEnabled = false;
        Health = 0;

        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
        _a_KillingMethodID = KillingAnimType;

        if (KillingAnimType == 1)
            _a_GetSmashed = true;

        _a_Dead = true;

        ImDeadBro();
    }

    //====================================== AnimEvents =======================================

    void AE_PullLeverStarted()
    {
        PlayerInputEnabled = false;

        if (Queued_TargetInteractionComponent != null)
        {
            transform.forward = Queued_TargetInteractionComponent.LookToDirection;
        }
    }
    void AE_PullLeverEnded()
    {
        PlayerInputEnabled = true;
        _a_LeverPull = false;
        comandos.Dequeue().Execute();
    }
    void AE_Ignite_Start()
    {
        PlayerInputEnabled = false;
    }
    void AE_Ignite_End()
    {
        PlayerInputEnabled = true;
        _a_Ignite = false;

        comandos.Dequeue().Execute();
    }
    void AE_TrowRock_Ended()
    {
        _a_ThrowRock = false;

        if (Queued_TargetInteractionComponent != null)
        {
            Queued_TargetInteractionComponent.ExecuteOperation();
            comandos.Dequeue().Execute();
        }
    }
    void AE_Grab_Star()
    {
        PlayerInputEnabled = false;
    }
    void AE_Grab_End()
    {
        _a_Grabing = false;

        //if (Queued_TargetInteractionComponent != null)
        //    Queued_TargetInteractionComponent.ExecuteOperation();
        comandos.Dequeue().Execute();
        PlayerInputEnabled = true;
    }
 }
