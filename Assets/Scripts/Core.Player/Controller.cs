using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using System;
using IA.PathFinding;
using Core.Interaction;
using Core.InventorySystem;
using Core.SaveSystem;
using System.Linq;

[RequireComponent(typeof(PathFindSolver), typeof(MouseContextTracker))]
public class Controller : MonoBehaviour, IDamageable<Damage, HitResult>, ILivingEntity
{
    //Stats.
    public int Health = 100;

    public event Action OnPlayerDied = delegate { };
    public event Action OnMovementChange = delegate { };
    public event Action OnInputLocked = delegate { };
    public event Action<bool> DisplayThrowUI = delegate { };
    public event Action<GameObject> OnEntityDead = delegate { };

    public Transform manitodumacaco;
    public ParticleSystem BloodStain;

    public float moveSpeed = 6;
    [SerializeField] float _movementTreshold = 0.18f;
    public Transform MouseDebug;

    Queue<IQueryComand> comandos = new Queue<IQueryComand>();
    IInteractionComponent Queued_TargetInteractionComponent = null;
    Node QueuedMovementEndPoint = null;

    Inventory _inventory = new Inventory();
    public Inventory Inventory
    {
        get
        {
            if (_inventory == null)
                _inventory = new Inventory();

            return _inventory;
        }
        set =>_inventory = value;
    }

    //Grab Objgrabed;
    [SerializeField] bool _input = true;
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
    Vector3 velocity;

    public void SubscribeToLifeCicleDependency(Action<GameObject> OnEntityDead)
    {
        this.OnEntityDead += OnEntityDead;
    }
    public void UnsuscribeToLifeCicleDependency(Action<GameObject> OnEntityDead)
    {
        this.OnEntityDead -= OnEntityDead;
    }

    //================================= Save System ========================================

    public PlayerData getCurrentPlayerData()
    {
        var equiped = Inventory.equiped;
        return new PlayerData()
        {
            position = transform.position,
            rotacion = transform.rotation,
            EquipedItem = equiped != null ? Inventory.equiped.ID : -1,
            itemScale = equiped != null ? equiped.transform.localScale : Vector3.zero,
            itemRotation = equiped != null ? equiped.transform.rotation : Quaternion.identity,
            maxItemsSlots = Inventory.maxItemsSlots,
            inventory = Inventory.slots.Select(x => x.ID)
                                         .ToList()
        };
    }
    public void LoadPlayerCheckpoint(PlayerData data)
    {
        transform.position = data.position;
        transform.rotation = data.rotacion;

        if (Inventory.equiped != null)
            Destroy(Inventory.equiped.gameObject);
        Inventory = new Inventory();
        if (data.EquipedItem == -1)
            Inventory.equiped = null;
        else
        {
            var instance = Instantiate(ItemDataBase.getRandomItemPrefab(data.EquipedItem));
            instance.transform.localScale = data.itemScale;
            instance.transform.rotation = data.rotacion;
            AttachItemToHand(instance.GetComponent<Item>());
        }
        Inventory.maxItemsSlots = data.maxItemsSlots;
        Inventory.slots = new List<Item>();

        foreach (var item in data.inventory) //Reconstruimos el inventario.
        {
            var itemdata = ItemDataBase.getItemData(item);
            var toAddItem = itemdata.inGamePrefabs[0].GetComponent<Item>();

            Inventory.slots.Add(toAddItem);
        }

        _a_GetSmashed = false;
        _a_GetStunned = false;
        _a_Dead = false;
        _a_Walking = false;
        Health = 100;
        PlayerInputEnabled = true;
        BloodStain.Clear();
        BloodStain.Stop();

        //_rb.useGravity = false;
        _rb.velocity = Vector3.zero;
        OnEntityDead = delegate { };
        _hitbox.enabled = true;
    }

    //======================================================================================

    #region Componentes
    [SerializeField] CommandMenu _MultiCommandMenu = null;
    Rigidbody _rb;
    Camera _viewCamera;
    Collider _hitbox = null;
    CanvasController _canvasController = null;
    MouseView _mv;
    MouseContextTracker _mtracker;
    PathFindSolver _solver;
    MouseContext _mouseContext;
    TrowManagement _tm;
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
        _hitbox = GetComponent<Collider>();
        _viewCamera = Camera.main;
        _canvasController = FindObjectOfType<CanvasController>();
        OnPlayerDied += _canvasController.DisplayLoose;
        DisplayThrowUI += _canvasController.DisplayThrow;
        _mv = GetComponent<MouseView>();
        _mtracker = GetComponent<MouseContextTracker>();
        _solver = GetComponent<PathFindSolver>();
        _tm = GetComponent<TrowManagement>();

        if (_MultiCommandMenu)
        {
            _MultiCommandMenu.OnCancelByRightClic += () => 
            {
                Debug.LogWarning("Controller::SetPlayerInput as True.");
                PlayerInputEnabled = true;
            };
        }

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
        _mouseContext = _mtracker.GetCurrentMouseContext();
        //print("=================================== Frame Update =========================================");

        #region Input
        if (PlayerInputEnabled)
        {
            if (_Aiming)
            {
                //print("Bloque 1:");
                //Cancelo el tiro.
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    //print("Cancelo Aiming");
                    _mtracker.ChangeCursorView(1);
                    _Aiming = false;
                    return;
                }

                //Confirmar el tiro.
                if (Input.GetMouseButtonDown(0))
                {
                    //print("Confirmo el tiro");
                    _Aiming = false;
                    _mtracker.ChangeCursorView(1);
                    _mouseContext = _mtracker.GetCurrentMouseContext();
                    Node targetNode = _mouseContext.closerNode;//El objetivo.
                    Vector3 origin = manitodumacaco.position;

                    //En vez de ejecutarlo directamente. Añadimos un TrowCommand.
                    var command = new cmd_ThrowEquipment(1f, manitodumacaco, targetNode, _tm, ReleaseEquipedItemFromHand,
                    () =>
                    {
                        _a_ThrowRock = true;
                        transform.forward = (targetNode.transform.position - transform.position).normalized;
                    });
                    comandos.Enqueue(command);
                    return;
                }
            }
            else
            {
                //print("Bloque 2:");
                if (Input.GetKeyDown(KeyCode.Alpha2) && Inventory.equiped != null)
                {
                    //Debug.LogWarning("INICIO AIMING");
                    _mtracker.ChangeCursorView(3);
                    _Aiming = true;
                    return;
                }

                if (_mouseContext.interactuableHitted)
                    _mtracker.ChangeCursorView(1);
                else
                    _mtracker.ChangeCursorView(0);

                // MouseClic Derecho.
                if (Input.GetMouseButtonDown(1))
                {
                    //MouseContext _mouseContext = _mtracker.GetCurrentMouseContext();//Obtengo el contexto del Mouse.
                    if (!_mouseContext.validHit) return; //Si no hay hit Válido.

                    if (_mouseContext.interactuableHitted)
                    {
                        //Le paso las nuevas opciones disponibles.
                        _MultiCommandMenu.FillOptions(_mouseContext.InteractionHandler, _inventory, QuerySelectedOperation);
                        //if (!_MultiCommandMenu.gameObject.activeSelf) //Lo activo en el canvas. Esto no cambia nada.
                            _MultiCommandMenu.gameObject.SetActive(true);
                        //Lo posiciono en donde debe estar.
                        _MultiCommandMenu.Emplace(Input.mousePosition);

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
                        }
                    }
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
        if (IsAlive && damage.instaKill)
            Die(damage.KillAnimationType);
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
        //print($"{ gameObject.name} ha pikeado un item. {item.name} se attachea a la mano.");
        //Presuponemos que el objeto es troweable.
        //Emparentamos el item al transform correspondiente.
        if (item != null)
        {
            item.SetOwner(_hitbox);
            item.SetPhysicsProperties(false, Vector3.zero);
            item.transform.SetParent(manitodumacaco);
            item.transform.localPosition = Vector3.zero;
            item.ExecuteOperation(OperationType.Take);
            _inventory.EquipItem(item);
            DisplayThrowUI(_inventory.equiped != null && _inventory.equiped.isThroweable);
        }
    }
    public Item ReleaseEquipedItemFromHand(bool setToDefaultPosition = false, params object[] options)
    {
        //print($"{ gameObject.name} soltará un item equipado. {_inventory.equiped.name} se attachea a la mano.");
        //Desemparentamos el item equipado de la mano.
        Item released = _inventory.UnEquipItem();

        if (setToDefaultPosition)
            released.ExecuteOperation(OperationType.Drop);
        else
            released.ExecuteOperation(OperationType.Drop, options[0]);
        released.SetPhysicsProperties(true, Vector3.zero);
        released.transform.SetParent(null);
        DisplayThrowUI(_inventory.equiped != null && _inventory.equiped.isThroweable);
        return released;
    }

    public void FallInTrap()
    {
        PlayerInputEnabled = false;
        //playerMovementEnabled = false;
        comandos.Clear();

        _rb.useGravity = true;
        _rb.isKinematic = false;
        _hitbox.isTrigger = true;
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
        Health = 0;

        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
        _a_KillingMethodID = KillingAnimType;
        _hitbox.enabled = false;

        if (KillingAnimType == 1)
            _a_GetSmashed = true;

        _a_Dead = true;

        comandos.Clear();
        Queued_TargetInteractionComponent = null;
        QueuedMovementEndPoint = null;

        OnEntityDead(gameObject);
        OnPlayerDied();
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
    }
}
