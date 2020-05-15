using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class IgnitableObject : MonoBehaviour, IInteractable, IIgnitableObject
{
    public Action registerInUpdateList_Callback = delegate { };
    public Action removeFromUpdateList_Callback = delegate { };
    public Action OnLifeEnded = delegate { };

    [SerializeField] GameObject fireParticle = null;
    [SerializeField] List<OperationOptions> suportedInteractions = new List<OperationOptions>();

    [Tooltip("Cuanto tiempo estará activo mientras ")]
    public float MaxLifeTime = 5f;
    [Tooltip("Cuanto tiempo estará prendido el fuego.")]
    public float BurningTime = 5f;
    [HideInInspector]
    public float ExplansionDelayTime = 0.8f;

    [SerializeField] float _lifeTime = 0;
    [SerializeField] float _interactionRadius = 3; //Esto tiene que ser dada al player para evitar que reciba daño del fuego.
    [SerializeField]LayerMask efectTargets = ~0;

    public List<IIgnitableObject> toIgnite = new List<IIgnitableObject>();
    //Debug inspectorList.
    public List<GameObject> toIgniteObjects = new List<GameObject>();

    [SerializeField] Collider _col = null;
    [SerializeField] TrapHitBox _trapHitBox;
    [SerializeField] float _ignitableDetectionRadius = 1f;

    public bool Burning { get; private set; } = (false);

    public Vector3 position => transform.position;
    public Vector3 LookToDirection => transform.forward;

    public bool IsActive => gameObject.activeSelf;

    public void UpdateLifeTime(float deltaTime)
    {
        _lifeTime -= deltaTime;
        if (_lifeTime <= 0)
        {
            OnLifeEnded();
        }
    }

    public void ResetCurrentLifeTime()
    {
        _lifeTime = MaxLifeTime;
    }
    public void Ignite(float delayTime = 0)
    {
        if (!Burning)
        {
            //print(gameObject.name + " IS IGNITED");
            //Desactivo mi collider. Ya no soy interactuable.
            _col.enabled = false;
            //Activo el área de daño.
            _trapHitBox.TrapEnabled = true;
            //Activo la particula del fueguín.
            fireParticle.SetActive(true);

            StartCoroutine(DelayedOtherIgnition(delayTime));
            StartCoroutine(extinguish(BurningTime));

            Burning = true;
            _lifeTime = BurningTime;
        }
    }

    void checkSurroundingIgnitionObjects()
    {
        //Busco objetos circundantes que puedan propagar el daño.
        var cols = Physics.OverlapSphere(transform.position, _ignitableDetectionRadius, efectTargets, QueryTriggerInteraction.Collide);

        for (int i = 0; i < cols.Length; i++)
        {
            var igniteable = cols[i].GetComponent<IIgnitableObject>();
            if (igniteable != null && !toIgnite.Contains(igniteable) && igniteable.IsActive)
            {
                //lo añado a una lista a ignitar.
                toIgnite.Add(igniteable);
            }
        }

        foreach (var item in toIgnite)
        {
            toIgniteObjects.Add(item.gameObject);
        }
    }

    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return ((requester.position - transform.position).normalized) * _interactionRadius;
    }
    public List<OperationOptions> GetSuportedOperations()
    {
        return suportedInteractions;
    }
    public void Operate(OperationOptions operation, params object[] optionalParams)
    {
        if (operation == OperationOptions.Activate)
            Ignite(ExplansionDelayTime);
    }

    private void OnMouseEnter()
    {
        //Feedback de interacción.
    }
    private void OnMouseExit()
    {
        //Feedback de interacción.
    }

    IEnumerator DelayedOtherIgnition(float Delay)
    {
        //Llamo la función Ignite a todos los de la lista de ignición con los que overlapeo.
        yield return new WaitForSeconds(Delay);
        checkSurroundingIgnitionObjects();
        foreach (var item in toIgnite)
        {
            if (!item.Burning)
                item.Ignite(Delay);
        }
        toIgnite.Clear();
    }
    IEnumerator extinguish(float time)
    {
        //Aquí en vez de apagar el gameObject de una, deberíamos ir por partes.

        yield return new WaitForSeconds(time);
        OnLifeEnded();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _ignitableDetectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);
    }
#endif
}
