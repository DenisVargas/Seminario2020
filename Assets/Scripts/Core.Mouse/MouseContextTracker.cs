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
    [SerializeField] LayerMask _interactableMask = ~0;
    [SerializeField] LayerMask _groundMask = ~0;
    [SerializeField] IInteractable lastFinded = null;

    IInteractable _cashedInteractable = null;
    InteractionDisplaySettings _cashedSettings = new InteractionDisplaySettings();

    [Header("Cursor Rendering")]
    public Texture2D defaultCursor;
    public Texture2D InteractiveCursor;
    public Texture2D AimCursor;

    int current = 0;

#if UNITY_EDITOR
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

    /// <summary>
    /// Retorna el contexto actual del mouse.
    /// </summary>
    /// <returns></returns>
    public MouseContext GetCurrentMouseContext(Inventory inventory = null)
    {
        MouseContext _context = new MouseContext();
        int validHits = 0;

        //Calculo la posición del mouse en el espacio.
        RaycastHit groundHit;
        RaycastHit interactableHit;
        Ray mousePositionInWorld = _viewCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x,
                                                          Input.mousePosition.y,
                                                          _viewCamera.transform.forward.z));

        //Interactable Detection:
        if (Physics.Raycast(mousePositionInWorld, out interactableHit, float.MaxValue, _interactableMask))
        {
            validHits++;
            IInteractable interactableObject = interactableHit.transform.GetComponentInParent<IInteractable>();
            //Este cambio reduce el costo computacional de los siguientes chequeos sobre el mismo objeto.
            //Sin embargo, si alteramos el inventario, el cambio no se verá reflejado hasta que hayamos movido el mouse.
            if (interactableObject != null)
            {
                if (_cashedInteractable != null && _cashedInteractable == interactableObject)
                {
                    if (_cashedSettings.AviableInteractionsAmmount > 0)
                    {
                        _context.interactuableHitted = true;
                        _context.InteractionHandler = interactableObject;
                    }
                }
                else
                {
                    var displaySettings = interactableObject.GetInteractionDisplaySettings(inventory, false);
                    _cashedSettings = displaySettings;
                    _cashedInteractable = interactableObject;
                    if (displaySettings.AviableInteractionsAmmount > 0)
                    {
                        _context.interactuableHitted = true;
                        _context.InteractionHandler = interactableObject;
                    }
                }
            }
            else _cashedInteractable = null;
        }

        //Ground Detection:
        if (Physics.Raycast(mousePositionInWorld, out groundHit, float.MaxValue, _groundMask))
        {
            validHits++;
            Collider collider = groundHit.collider;
            if (collider.transform.CompareTag("NavigationFloor"))
            {
                _context.hitPosition = groundHit.point;
                _context.closerNode = _solver.getCloserNode(groundHit.point);
            }
        }

#if UNITY_EDITOR
        if (HITPOSITION)
        {
            HITPOSITION.transform.position = _context.hitPosition;
        }
        if (HITNODE && _context.closerNode != null)
        {
            HITNODE.transform.position = _context.closerNode.transform.position;
        }
#endif

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
