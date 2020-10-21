using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using IA.PathFinding;
using Core.Interaction;
using Core.InventorySystem;

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
    [SerializeField] IInteractable lastFinded = null;

    [Header("Cursor Rendering")]
    public Texture2D defaultCursor;
    public Texture2D InteractiveCursor;
    public Texture2D AimCursor;

    int current = 0;

#if UNITY_EDITOR
    [SerializeField] List<Collider> hited = new List<Collider>();
    [SerializeField] Transform HITPOSITION = null;
    [SerializeField] Transform HITNODE = null;
#endif

    private void Awake()
    {
        _solver = GetComponent<PathFindSolver>();
        _viewCamera = Camera.main;

        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true;
            current = 1;
        }
    }

    public void ChangeCursorView(int index)
    {
        if (index == current) return;

        //Debug.Log($"Cursor view changed to: {index}");
        switch (index)
        {
            case 1:
                Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
                current = 1;
                break;
            case 2:
                Cursor.SetCursor(InteractiveCursor, Vector2.zero, CursorMode.Auto);
                current = 2;
                break;
            case 3:
                Cursor.SetCursor(AimCursor, new Vector2(AimCursor.width/2, AimCursor.height/2), CursorMode.Auto);
                current = 3;
                break;
        }
    }

    public MouseContext GetCurrentMouseContext()
    {
        MouseContext _context = new MouseContext();
        int validHits = 0;

        //Calculo la posición del mouse en el espacio.
        RaycastHit[] hits;
        Ray mousePositionInWorld = _viewCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x,
                                                          Input.mousePosition.y,
                                                          _viewCamera.transform.forward.z));
        hits = Physics.RaycastAll(mousePositionInWorld, float.MaxValue, mouseDetectionMask, QueryTriggerInteraction.Collide);


#if UNITY_EDITOR
        hited = new List<Collider>();
        for (int i = 0; i < hits.Length; i++) // Lista debug para el inspector.
        {
            hited.Add(hits[i].collider);
        }
#endif

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

#if UNITY_EDITOR
                if (HITPOSITION)
                {
                    HITPOSITION.transform.position = _context.hitPosition;
                }
                if (HITNODE)
                {
                    HITNODE.transform.position = _context.closerNode.transform.position;
                }
#endif

                validHits++;
            }
            else continue;
        }

        _context.validHit = validHits > 0; //Validación del hit.
        ThrowMouseEvents(_context.InteractionHandler);

        return _context;
    }


    public void ThrowMouseEvents(IInteractable target)
    {
        if (target == null)
        {
            if (lastFinded != null)
            {
                lastFinded.OnInteractionMouseExit();
                lastFinded = null;
            }
        }
        else
        {
            if (lastFinded != null && lastFinded != target)
            {
                lastFinded.OnInteractionMouseExit();
                lastFinded = target;
                lastFinded.OnInteractionMouseOver();
            }
            else
            {
                lastFinded = target;
                lastFinded.OnInteractionMouseOver();
            }
        }
    }
}
