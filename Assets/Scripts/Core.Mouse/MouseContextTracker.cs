using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using IA.PathFinding;
using Core.Interaction;

[Serializable]
public struct MouseContext
{
    public bool interactuableHitted;
    public IInteractable InteractionHandler;
    public bool validHit;
    public Vector3 hitPosition;
    public Node closerNode;
}

[RequireComponent(typeof(PathFindSolver))]
public class MouseContextTracker : MonoBehaviour
{
    Camera _viewCamera;
    PathFindSolver _solver;
    [SerializeField] LayerMask mouseDetectionMask = ~0;
    float checkRate = 0.1f;
    MouseContext _mouseContext;

  [Header("Cursor Rendering")]
    public Texture2D defaultCursor;
    public Texture2D InteractiveCursor;

#if UNITY_EDITOR
    [SerializeField] List<Collider> hited = new List<Collider>();
#endif

    private void Awake()
    {
        _solver = GetComponent<PathFindSolver>();
        _viewCamera = Camera.main;

        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true;
        }
        
    }
    public void Update()
    {
        checkRate -= Time.deltaTime;
        if(checkRate<=0)
        {
            _mouseContext = GetCurrentMouseContext();
            if(_mouseContext.interactuableHitted)
            {
                Cursor.SetCursor(InteractiveCursor, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            }
            checkRate = 0.1f;
        }
    }
    public MouseContext GetCurrentMouseContext()
    {
        return m_MouseContextDetection();
    }

    MouseContext m_MouseContextDetection()
    {
        MouseContext _context = new MouseContext();
        int validHits = 0;

        //Calculo la posición del mouse en el espacio.
        RaycastHit[] hits;
        Ray mousePositionInWorld = _viewCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x,
                                                          Input.mousePosition.y,
                                                          _viewCamera.transform.position.y));
        hits = Physics.RaycastAll(mousePositionInWorld, float.MaxValue, mouseDetectionMask, QueryTriggerInteraction.Collide);

        #region DEBUG
#if UNITY_EDITOR
        hited = new List<Collider>();
        for (int i = 0; i < hits.Length; i++) // Lista debug para el inspector.
        {
            hited.Add(hits[i].collider);
        }
#endif 
        #endregion

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];

            IInteractable interactableObject = hit.transform.GetComponentInParent<IInteractable>();
            if (interactableObject != null && interactableObject.InteractionsAmmount > 0)
            {
                _context.interactuableHitted = true;
                _context.InteractionHandler = interactableObject;
                validHits++;
            }

            Collider collider = hit.collider;
            if (collider.transform.CompareTag("NavigationFloor"))
            {
                _context.hitPosition = hit.point;
                _context.closerNode = _solver.getCloserNode(hit.point);
                validHits++;
            }
            else continue;
        }

        _context.validHit = validHits > 0; //Validación del hit.

        return _context;
    }
}
