using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public struct MouseContext
{
    public bool interactuableHitted;
    public IInteractable firstInteractionObject;
    public bool validHit;
    public Vector3 hitPosition;
}

public class MouseContextTracker : MonoBehaviour
{
    Camera _viewCamera;
    [SerializeField] LayerMask mouseDetectionMask = ~0;
    public Texture2D defaultCursor;

#if UNITY_EDITOR
    [SerializeField] List<Collider> hited = new List<Collider>(); 
#endif

    private void Awake()
    {
        _viewCamera = Camera.main;
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true;
        }
    }

    public MouseContext GetCurrentMouseContext()
    {
        return m_MouseContextDetection();
    }

    MouseContext m_MouseContextDetection()
    {
        MouseContext _context = new MouseContext();

#if UNITY_EDITOR
        hited = new List<Collider>(); 
#endif

        //Calculo la posición del mouse en el espacio.
        RaycastHit[] hits;
        Ray mousePositionInWorld = _viewCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x,
                                                          Input.mousePosition.y,
                                                          _viewCamera.transform.position.y));

        hits = Physics.RaycastAll(mousePositionInWorld, float.MaxValue, mouseDetectionMask, QueryTriggerInteraction.Collide);

#if UNITY_EDITOR
        for (int i = 0; i < hits.Length; i++)
        {
            hited.Add(hits[i].collider);
        } 
#endif

        if (hits.Length > 0)
            _context.validHit = true;

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];

            IInteractable interactableObject = hit.transform.GetComponentInParent<IInteractable>();
            if (interactableObject != null && interactableObject.InteractionsAmmount > 0)
            {
                _context.interactuableHitted = true;
                _context.firstInteractionObject = interactableObject;
            }

            Collider collider = hit.collider;
            if (collider.transform.CompareTag("NavigationFloor"))
            {
                _context.hitPosition = hit.point;

                //Nos fijamos el punto mas cercano en el navmesh.
                NavMeshHit nh;
                if (NavMesh.SamplePosition(hit.point, out nh, 5, NavMesh.AllAreas))
                {
                    _context.hitPosition = nh.position;
                }
            }
            else continue;
        }

        return _context;
    }
}
