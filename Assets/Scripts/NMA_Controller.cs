using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Core.DamageSystem;

public struct ActivationCommandData
{
    public IInteractable target;
    public OperationOptions operationOptions;
}
//Navigation Mesh Actor Controller.
public class NMA_Controller : MonoBehaviour, IDamageable<Damage>
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


    Camera _viewCamera = null;
    NavMeshAgent _agent = null;
    CanvasController _canvasController = null;
    MouseView _mv;

    Vector3 _currentTargetPos;
    float forwardLerpTime;
    bool PlayerInputEnabled = true;

    ActivationCommandData Queued_ActivationData = new ActivationCommandData();

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _viewCamera = Camera.main;
        _canvasController = FindObjectOfType<CanvasController>();
        _mv = GetComponent<MouseView>();

        _anims = GetComponent<Animator>();
        animHash = new int[4];
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
            MouseContext _mouseContext = m_GetMouseContextDetection();

            if (!_mouseContext.validHit) return;

            if (_mouseContext.interactuableHitted)
            {
                //Muestro el menú en la posición del mouse, con las opciones soportadas por dicho objeto.
                _canvasController.DisplayCommandMenu
                (
                    Input.mousePosition,
                    _mouseContext.firstInteractionObject.GetSuportedOperations(),
                    _mouseContext.firstInteractionObject,
                    ExecuteOperation
                 );
            }
            else
            {
                if (!mod1)
                {
                    _mv.ClearView();
                    comandos.Clear();
                }

                _mv.SetMousePosition(_mouseContext.hitPosition);

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

        //if (_currentTargetPos != Vector3.zero && transform.forward != _currentTargetPos)
        //    UpdateForward();

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
    public void ExecuteOperation(OperationOptions operation, IInteractable target)
    {
        //Aqui tenemos una referencia a un target y una operación que quiero ejectar sobre él.
        //(¿Necesito moverme hacia el objetivo primero?) - Opcionalmente me muevo hasta la ubicación del objeto.
        //print(string.Format("Ejecuto la operación {0} sobre {1}", operation.ToString(), target));

        //Chequeo si estoy lo suficientemente cerca para activar el comando.
        if (Vector3.Distance(transform.position, target.position) > _movementTreshold)
        {
            Vector3 vectorToPlayer = target.position - transform.position;
            Vector3 stopingPosition = target.position -  (vectorToPlayer.normalized * _interactionMaxDistance);

            //if (targetDebug != null)
            //    targetDebug.position = stopingPosition;

            IQueryComand closeDistance = new cmd_Move
            (
                stopingPosition, 
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
            print("Comando CloseDistance añadido. Hay " + comandos.Count + " comandos");
        }

        //añado el comando correspondiente a la query.
        switch (operation)
        {
            case OperationOptions.Take:
                break;
            case OperationOptions.Ignite:
                break;
            case OperationOptions.Activate:
                Action beforeCommandExecution = () => { _a_LeverPull = true; };
                Queued_ActivationData = new ActivationCommandData() { target = target, operationOptions = operation };
                IQueryComand activateCommand = new cmd_Activate(Queued_ActivationData, beforeCommandExecution, disposeCommand);
                comandos.Enqueue(activateCommand);
                //print("Comando Activate añadido. Hay " + comandos.Count + " comandos");
                break;
            case OperationOptions.Equip:
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
        {
            forwardLerpTime = 0;
            _currentTargetPos = destinyPosition;
        }
        if (!_a_Walking)
            _a_Walking = true;

        _agent.destination = destinyPosition;
    }

    //public void UpdateForward()
    //{
    //    //transform.LookAt(dst.YComponent(0)); //Esto funciona, pero realiza un comportamiento extraño que es indeseable ya que tiene en cuenta las 3 dimensiones.
    //    Vector3 _TargetForward = (_currentTargetPos.YComponent(0) - transform.position.YComponent(0)).normalized;

    //    //chequeamos el tiempo del lerp. (Aquí podriamos hacer que el lerp dependa de un tiempo target)
    //    //Ejemplo: que lerpee siempre en 0.5 seg o 2 seg.
    //    //float time = Mathf.Clamp(forwardLerpTime + Time.deltaTime, 0f, 1f);

    //    //Lerpeamos y Aplicamos. Slerp para que la rotacion sea suave.
    //    //transform.forward = Vector3.Slerp(transform.forward, _TargetForward, time);
    //}

    void disposeCommand()
    {
        var _currentC = comandos.Dequeue();
        if (comandos.Count > 0)
        {
            var next = comandos.Peek();
            print(string.Format("Comando {0} Finalizado\nSiguiente comando es {1}", _currentC, next));
        }
    }

    void Die()
    {
        PlayerInputEnabled = false;
        _agent.isStopped = true;
        _agent.ResetPath();

        _a_Dead = true;
        ImDeadBro();
    }

    struct MouseContext
    {
        public bool interactuableHitted;
        public IInteractable firstInteractionObject;
        public bool validHit;
        public Vector3 hitPosition;
    }

    MouseContext m_GetMouseContextDetection()
    {
        MouseContext _context = new MouseContext();

        //Calculo la posición del mouse en el espacio.
        RaycastHit[] hits;
        Ray mousePositionInWorld = _viewCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x,
                                                          Input.mousePosition.y,
                                                          _viewCamera.transform.position.y));

        hits = Physics.RaycastAll(mousePositionInWorld, Camera.main.farClipPlane, mouseDetectionMask);

        if (hits.Length > 0)
            _context.validHit = true;

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];

            IInteractable interactableObject = hit.transform.GetComponent<IInteractable>();
            if (interactableObject != null)
            {
                _context.interactuableHitted = true;
                _context.firstInteractionObject = interactableObject;
            }

            Collider collider = hit.collider;
            if (collider.transform.CompareTag("NavigationFloor"))
            {
                _context.hitPosition = hit.point;
                return _context;
            }
        }

        return _context;
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
            Queued_ActivationData.target.Operate(Queued_ActivationData.operationOptions);
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

