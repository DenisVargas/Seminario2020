using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Navigation Mesh Actor Controller.
public class NMA_Controller : MonoBehaviour
{
    [SerializeField] LayerMask mouseDetectionMask = ~0;
    [SerializeField] int maxMouseRayDistance = 200;
    [SerializeField] Transform MouseDebug = null;
    [SerializeField] Transform targetDebug = null;
    [SerializeField] float _interactionMaxDistance = 0.1f;
    [SerializeField] float _movementTreshold = 0.18f;

    Queue<IQueryComand> comandos = new Queue<IQueryComand>();

    Camera _viewCamera = null;
    NavMeshAgent _agent = null;
    CanvasController _canvasController = null;

    Vector3 _currentTargetPos;
    float forwardLerpTime;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _viewCamera = Camera.main;
        _canvasController = FindObjectOfType<CanvasController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 wMousePos = _viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                      Input.mousePosition.y,
                                                                      _viewCamera.transform.position.y));
        if (MouseDebug != null)
            MouseDebug.position = wMousePos;

        bool mod1 = Input.GetKey(KeyCode.LeftShift);

        //Asigno la posición como target a nuestro navMeshAgent.
        if (Input.GetMouseButtonDown(1))
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
                if (!mod1) comandos.Clear();

                IQueryComand moveCommand = new cmd_Move
                (   wMousePos,
                    MoveToTarget, 
                    (targetPos) => 
                    {
                        //print(string.Format("Distancia restante es {0}", _agent.remainingDistance));
                        float dst = Vector3.Distance(transform.position, targetPos);
                        return dst <= _movementTreshold;
                    },
                    disposeCommand
                );
                comandos.Enqueue(moveCommand);
            }
        }

        if (_currentTargetPos != Vector3.zero && transform.forward != _currentTargetPos)
            UpdateForward();


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
        if (Vector3.Distance(transform.position, target.position) > _interactionMaxDistance)
        {
            Vector3 vectorToPlayer = target.position - transform.position;
            Vector3 stopingPosition = target.position -  (vectorToPlayer.normalized * _interactionMaxDistance);

            if (targetDebug != null)
                targetDebug.position = stopingPosition;

            IQueryComand closeDistance = new cmd_Move
            (
                stopingPosition, 
                MoveToTarget, 
                (targetPos) => 
                {
                    //print(string.Format("Distancia restante es {0}", _agent.remainingDistance));
                    float dst = Vector3.Distance(transform.position.YComponent(0), targetPos.YComponent(0));
                    return dst <= _movementTreshold;
                },
                disposeCommand
            );
            comandos.Enqueue(closeDistance);
            //print("Comando CloseDistance añadido. Hay " + comandos.Count + " comandos");
        }

        //añado el comando correspondiente a la query.
        switch (operation)
        {
            case OperationOptions.Take:
                break;
            case OperationOptions.Ignite:
                break;
            case OperationOptions.Activate:
                IQueryComand activateCommand = new cmd_Activate(operation, target, disposeCommand);
                comandos.Enqueue(activateCommand);

                print("Comando Activate añadido. Hay " + comandos.Count + " comandos");
                break;
            case OperationOptions.Equip:
                break;
            default:
                break;
        }
    }

    //Movimiento
    public void MoveToTarget(Vector3 dst)
    {
        if (_currentTargetPos != dst)
        {
            forwardLerpTime = 0;
            _currentTargetPos = dst;
        }

        _agent.destination = dst;
    }

    public void UpdateForward()
    {
        //transform.LookAt(dst.YComponent(0)); //Esto funciona, pero realiza un comportamiento extraño que es indeseable ya que tiene en cuenta las 3 dimensiones.
        Vector3 _TargetForward = (_currentTargetPos.YComponent(0) - transform.position.YComponent(0)).normalized;

        //chequeamos el tiempo del lerp. (Aquí podriamos hacer que el lerp dependa de un tiempo target)
        //Ejemplo: que lerpee siempre en 0.5 seg o 2 seg.
        float time = Mathf.Clamp(forwardLerpTime + Time.deltaTime, 0f, 1f);

        //Lerpeamos y Aplicamos. Slerp para que la rotacion sea suave.
        transform.forward = Vector3.Slerp(transform.forward, _TargetForward, time);
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
        Ray ray = _viewCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x,
                                                          Input.mousePosition.y,
                                                          _viewCamera.transform.position.y));

        float DistanceToCamera = Vector3.Distance(_viewCamera.transform.position, transform.position);
        hits = Physics.RaycastAll(ray, DistanceToCamera + maxMouseRayDistance, mouseDetectionMask);

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
                _context.hitPosition = collider.transform.position;
            }
        }

        return _context;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _interactionMaxDistance);
    }
}
