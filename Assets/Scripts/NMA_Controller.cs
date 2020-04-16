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

    Queue<IQueryComand> comandos = new Queue<IQueryComand>();

    Camera _viewCamera = null;
    NavMeshAgent _agent = null;
    CanvasController _canvasController = null;

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
        MouseDebug.position = wMousePos;

        bool mod1 = Input.GetKey(KeyCode.LeftShift);

        //Asigno la posición como target a nuestro navMeshAgent.
        if (Input.GetMouseButton(1))
        {
            //Hacer un raycast y fijarme si hay un objeto que se interactuable.
            MouseContext _mouseContext = m_GetMouseContextDetection();

            if (!_mouseContext.validHit) return;

            if (_mouseContext.interactuableHitted)
            {
                //Muestro el menú en la posición del mouse, con las opciones soportadas por dicho objeto.
                _canvasController.DisplayCommandMenu(Input.mousePosition, _mouseContext.firstInteractionObject.GetSuportedOperations(), _mouseContext.firstInteractionObject, ExecuteOperation);
            }
            else
            {
                if (!mod1) comandos.Clear();

                IQueryComand moveCommand = new cmd_Move(wMousePos, MoveToTarget, () => { return _agent.remainingDistance == 0; } );
                comandos.Enqueue(moveCommand);
            }
        }


        if (comandos.Count > 0)
        {
            IQueryComand current = comandos.Peek();
            current.Update();

            if (current.completed)
                comandos.Dequeue();
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
        print(string.Format("Ejecuto la operación {0} sobre {1}", operation.ToString(), target));

        //Chequeo si estoy lo suficientemente cerca para activar el comando.
        if (Vector3.Distance(transform.position, target.position) > _interactionMaxDistance)
        {
            Vector3 vectorToPlayer = target.position - transform.position;
            Vector3 stopingPosition = target.position -  (vectorToPlayer.normalized * _interactionMaxDistance);
            targetDebug.position = stopingPosition;

            IQueryComand closeDistance = new cmd_Move(stopingPosition, MoveToTarget, () => { return _agent.remainingDistance == 0; });
            comandos.Enqueue(closeDistance);
        }

        //añado el comando correspondiente a la query.
        switch (operation)
        {
            case OperationOptions.Take:
                break;
            case OperationOptions.Ignite:
                break;
            case OperationOptions.Activate:
                IQueryComand activateCommand = new cmd_Activate(operation, target);
                comandos.Enqueue(activateCommand);
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
        transform.LookAt(dst.YComponent(0));
        _agent.destination = dst;
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
