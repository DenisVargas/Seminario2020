using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using System;
using IA.PathFinding;
using Core.Interaction;
using Core.InventorySystem;

[RequireComponent(typeof(PathFindSolver), typeof(MouseContextTracker))]
public class Controller : MonoBehaviour, IDamageable<Damage, HitResult>
{
    //Stats.
    public int Health = 100;

    public event Action ImDeadBro = delegate { };
    public event Action OnMovementChange = delegate { };
    public event Action OnInputLocked = delegate { };
    public event Action<Item> CheckItemDislayUI = delegate { };

    public Transform manitodumacaco;
    public ParticleSystem BloodStain;

    public float moveSpeed = 6;
    [SerializeField] float _movementTreshold = 0.18f;
    public Transform MouseDebug;

    Queue<IQueryComand> comandos = new Queue<IQueryComand>();
    IInteractionComponent Queued_TargetInteractionComponent = null;
    Node QueuedMovementEndPoint = null;

    [SerializeField] Inventory _inventory;

    //Grab Objgrabed;
    bool _input = true;
    bool PlayerInputEnabled
    {
        get => _input;
        set
        {
            _input = value;
            if (!value)
                OnInputLocked();
        }
    }
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
    MouseContext _mouseContext;
    TrowManagement _tm;
    float checkRate = 0.1f;
    bool _Aiming;
    Transform throwTarget;
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
    public float TRWRange;

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
        OnInputLocked += _canvasController.CloseCommandMenu;
        _mv = GetComponent<MouseView>();
       _mtracker = GetComponent<MouseContextTracker>();
        _solver = GetComponent<PathFindSolver>();
        _tm = GetComponent<TrowManagement>();

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
        #region mouse
        checkRate -= Time.deltaTime;
        if (checkRate <= 0)
        {
           _mouseContext = _mtracker.GetCurrentMouseContext();
            if(!_Aiming)
            {
                if (_mouseContext.interactuableHitted)
                {
                    _mtracker.ChangeCursorView(2);
                }
                else
                {
                    _mtracker.ChangeCursorView(1);
                }
                checkRate = 0.1f;
            }
            
        }

        #endregion
        #region Input
        if (PlayerInputEnabled)
        {
            // MouseClic Derecho.
            if (Input.GetMouseButtonDown(1))
            {
                //MouseContext _mouseContext = _mtracker.GetCurrentMouseContext();//Obtengo el contexto del Mouse.
                if (!_mouseContext.validHit) return; //Si no hay hit Válido.

                if (_mouseContext.interactuableHitted)
                {
                    if(!_Aiming)
                    {
                        //Muestro el menú en la posición del mouse, con las opciones soportadas por dicho objeto.
                        _canvasController.DisplayCommandMenu
                        (
                            Input.mousePosition,
                            _mouseContext.InteractionHandler,
                            _inventory,
                            QuerySelectedOperation
                         );
                    }
                    return;
                }

                if (Input.GetKey(KeyCode.LeftControl) && Clon.IsActive)
                    Clon.SetMovementDestinyPosition(_mouseContext.closerNode);
                else
                {
                    Node targetNode = _mouseContext.closerNode;
                    Node origin = _solver.getCloserNode(QueuedMovementEndPoint == null ? transform.position : QueuedMovementEndPoint.transform.position);

                    if (targetNode == null || origin == null)
                    {
                        Debug.LogError("targetNode o origin node es nulo");
                        return;
                    }

                    if (Input.GetKey(KeyCode.LeftShift)) //Si presiono shift, muestro donde estoy presionando de forma aditiva.
                    {
                        if (AddMovementCommand(origin, targetNode))
                            _mv.SetMousePositionAditive(_mouseContext.closerNode.transform.position);
                    }
                    else //Si no presiono nada, es una acción normal, sobreescribimos todos los comandos!.
                    {
                        CancelAllCommands();
                        if (AddMovementCommand(origin, targetNode))
                        {
                            _mv.SetMousePosition(_mouseContext.closerNode.transform.position);
                        }
                        _Aiming = false;
                    }
                }
            }

            if (_inventory.equiped != null)
            {
                if (Input.GetKeyDown(KeyCode.Alpha2) && !_Aiming)
                {
                    _mtracker.ChangeCursorView(3);
                    _Aiming = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2) && _Aiming)
                {
                    _mtracker.ChangeCursorView(1);
                    _Aiming = false;
                }

                if (Input.GetMouseButtonDown(0) && _Aiming)
                {
                    _Aiming = false;
                    Node targetNode = _mouseContext.closerNode;//El objetivo.
                    //Node origin = _solver.getCloserNode(transform.position); //El origen debería ser la manito.
                    Vector3 origin = manitodumacaco.position;

                    //En vez de ejecutarlo directamente. Añadimos un TrowCommand.
                    var command = new cmd_ThrowEquipment(1f, manitodumacaco, targetNode, _tm, ReleaseEquipedItemFromHand, () => 
                    {
                        _a_ThrowRock = true;
                        transform.forward = (targetNode.transform.position - transform.position).normalized;
                    });
                    comandos.Enqueue(command);

                    //Es posible que en el Trowmanager debamos construir la mecánica.
                    //Es decir la parte visual.

                    //La distancia debería limitarse al entrar en modo de selección.
                    //float dist = (targetNode.transform.position - origin.transform.position).magnitude;
                    //if (dist > TRWRange)
                    //{
                    //    _solver.SetOrigin(origin).SetTarget(targetNode).CalculatePathUsingSettings();
                    //    if (_solver.currentPath.Count > 0)
                    //    {
                    //        Node FinalNode = null;

                    //        while (FinalNode == null)
                    //        {
                    //            Node currentNode = _solver.currentPath.Dequeue();
                    //            if ((targetNode.transform.position - currentNode.transform.position).magnitude < TRWRange)
                    //                FinalNode = currentNode;
                    //        }
                    //        AddMovementCommand(origin, FinalNode);
                    //    }
                    //    else return;
                    //}

                    //if (_mouseContext.interactuableHitted)
                    //{
                    //    IInteractionComponent target = _mouseContext.InteractionHandler.GetInteractionComponent(OperationType.Throw, false);

                    //    IQueryComand _ActivateCommand = new cmd_TrowRock
                    //        (
                    //            target,
                    //            () =>
                    //            {
                    //                _a_ThrowRock = true;
                    //                transform.forward = (target.transform.position - transform.position).normalized;
                    //            },
                    //            false,
                    //            _mouseContext.closerNode
                    //          );
                    //    comandos.Enqueue(_ActivateCommand);
                    //    throwTarget = target.transform;
                    //}
                    //else
                    //{
                    //    Node finalNode = _mouseContext.closerNode;

                    //    IQueryComand _toActivateCommand = new cmd_TrowRock
                    //    (
                    //        null,
                    //        () =>
                    //        {
                    //            _a_ThrowRock = true;
                    //            transform.forward = (finalNode.transform.position - transform.position).normalized;
                    //        }, true, finalNode
                    //        );
                    //    comandos.Enqueue(_toActivateCommand);
                    //    throwTarget = finalNode.transform;
                    //}
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
            //print($"Comandos activados: {comandos.Count}");
            IQueryComand current = comandos.Peek();
            if (!current.isReady)
                current.SetUp();
            if (!current.cashed)
                current.Execute();
        }
    }

    /// <summary>
    /// Añade un comando Move si hay un camino posible entre los 2 nodos dados por parámetros. Si no hay uno, el comando es ignorado.
    /// </summary>
    /// <param name="OriginNode">El nodo mas cercano al agente.</param>
    /// <param name="TargetNode">El nodo objetivo al que se quiere desplazar.</param>
    /// <returns></returns>
    private bool AddMovementCommand(Node OriginNode, Node TargetNode)
    {
        //_solver.SetOrigin(QueuedMovementEndPoint == null ? transform.position : QueuedMovementEndPoint.transform.position)
        _solver.SetOrigin(OriginNode)
               .SetTarget(TargetNode)
               .CalculatePathUsingSettings();

        if (_solver.currentPath.Count == 0) //Si el solver no halló un camino, no hay camino posible.
        {
            //print("No hay camino Posible");
            return false;
        }

        QueuedMovementEndPoint = TargetNode;

        IQueryComand moveCommand = new cmd_Move
                            (
                                TargetNode,
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
        return true;
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
    }

    //================================== Player Controller =================================

    #region Clon
    public void ClonSpawn()
    {
        if (!Clon.IsActive && _canCastAClon)
        {
            Node position = _solver.getCloserNode(transform.position + transform.forward * 1.5f);
            Clon.InvokeClon(position, -transform.forward);
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

    public void AttachItemToHand(Item item)
    {
        print($"{ gameObject.name} ha pikeado un item. {item.name} se attachea a la mano.");
        //Presuponemos que el objeto es troweable.
        //Emparentamos el item al transform correspondiente.
        item.SetPhysicsProperties(false, Vector3.zero);
        item.transform.SetParent(manitodumacaco);
        item.transform.localPosition = Vector3.zero;
        item.ExecuteOperation(OperationType.Take);
        _inventory.EquipItem(item);
    }
    public Item ReleaseEquipedItemFromHand(bool setToDefaultPosition = false, params object[] options)
    {
        print($"{ gameObject.name} soltará un item equipado. {_inventory.equiped.name} se attachea a la mano.");
        //Desemparentamos el item equipado de la mano.
        Item released = _inventory.UnEquipItem();

        if (setToDefaultPosition)
            released.ExecuteOperation(OperationType.Drop);
        else
            released.ExecuteOperation(OperationType.Drop, options[0]);
        released.SetPhysicsProperties(true, Vector3.zero);
        released.transform.SetParent(null);
        return released;
    }

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
    public void QuerySelectedOperation(OperationType operation, IInteractionComponent target)
    {
        if (PlayerInputEnabled)
        {
            var parameters = target.getInteractionParameters(transform.position);

            Node origin = _solver.getCloserNode(QueuedMovementEndPoint == null ? transform.position : QueuedMovementEndPoint.transform.position);
            Node targetNode = parameters.safeInteractionNode;

            if (Vector3.Distance(origin.transform.position, targetNode.transform.position) > _movementTreshold)
                if (!AddMovementCommand(origin, targetNode))
                {
                    print("No se pudo añadir un camino posible, el camino está obstruído");
                    return;
                }

            //añado el comando correspondiente a la query.
            IQueryComand _toActivateCommand = null;
            switch (operation)
            {
                case OperationType.Ignite:
                    _toActivateCommand = new cmd_Ignite(target, operation, () => { _a_Ignite = true; });
                    break;

                case OperationType.Activate:
                    _toActivateCommand = new cmd_Activate(target, operation, () => { _a_LeverPull = true; });
                    break;

                case OperationType.Equip:
                    break;

                case OperationType.Take:
                    if (_inventory.equiped == null)
                        _toActivateCommand = new cmd_Take((Item)target, AttachItemToHand, () => { _a_Grabing = true; });
                    break;

                case OperationType.Exchange:
                    _toActivateCommand = new cmd_Exchange((Item)target, _inventory, ReleaseEquipedItemFromHand, AttachItemToHand, () => { _a_Grabing = true; });
                    break;

                case OperationType.Combine:
                    _toActivateCommand = new cmd_Combine((Item)target, _inventory, AttachItemToHand, () => { _a_Grabing = true; });
                    break;
                case OperationType.lightOnTorch:
                    if (_inventory.equiped.ID == 1)
                        _toActivateCommand = new cmd_LightOnTorch(() => { }, target, (Torch)_inventory.equiped);
                    break;

                default:
                    break;
            }

            if (_toActivateCommand != null)
                comandos.Enqueue(_toActivateCommand);
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
            var parameters = Queued_TargetInteractionComponent.getInteractionParameters(transform.position);
            transform.forward = parameters.orientation;
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
    void AE_Throw_Start()
    {
        PlayerInputEnabled = false;
    }
    void AE_Throw_Excecute()
    {
        //ReleaseEquipedItemFromHand()
        //.ExecuteOperation(OperationType.Throw, throwTarget);
        comandos.Peek().Execute();
        //throwTarget = null;
    }
    void AE_TrowRock_Ended()
    {
        PlayerInputEnabled = true;
        _a_ThrowRock = false;
        comandos.Dequeue();
    }
    void AE_Grab_Star()
    {
        PlayerInputEnabled = false;
    }
    void AE_Grab_End()
    {
        _a_Grabing = false;

        comandos.Dequeue().Execute();
        PlayerInputEnabled = true;
        CheckItemDislayUI(_inventory.equiped);
    }
 }
