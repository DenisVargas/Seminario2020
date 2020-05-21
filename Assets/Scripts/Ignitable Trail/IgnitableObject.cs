using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using System;

[RequireComponent(typeof(Collider))]
public class IgnitableObject : MonoBehaviour, IInteractable, IIgnitableObject
{
    public Action OnDisable = delegate { };

    [SerializeField] GameObject fireParticle = null;
    [SerializeField] List<OperationOptions> suportedInteractions = new List<OperationOptions>();

    [SerializeField, Tooltip("Cuanto tiempo estará activo mientras ")]
    public float MaxLifeTime = 5f;
    [SerializeField, Tooltip("Cuanto tiempo estará prendido el fuego.")]
    public float BurningTime = 5f;
    [SerializeField] float ExplansionDelayTime = 0.8f;

    [SerializeField] float _ignitableSearchRadius;
    [SerializeField] float _interactionRadius = 3; //Esto tiene que ser dada al player para evitar que reciba daño del fuego.
    [SerializeField]LayerMask efectTargets = ~0;

    public List<IIgnitableObject> toIgnite = new List<IIgnitableObject>();

    [SerializeField] Collider _col = null;
    [SerializeField] TrapHitBox _trapHitBox;

    public bool Burning { get; private set; } = (false);

    public Vector3 position => transform.position;
    public Vector3 LookToDirection => transform.forward;

    public bool IsActive => gameObject.activeSelf;

    public void OnSpawn()
    {
        StopAllCoroutines();
        StartCoroutine(extinguish(MaxLifeTime));
    }

    public void Ignite(float delayTime = 0)
    {
        if (!Burning)
        {
            //print(gameObject.name + " IS IGNITED");
            //Desactivo mi collider. Ya no soy interactuable.
            _col.enabled = false;
            //Activo el área de daño.
            _trapHitBox.IsActive = true;
            //Activo la particula del fueguín.
            fireParticle.SetActive(true);

            StartCoroutine(DelayedOtherIgnition(delayTime));
            StartCoroutine(extinguish(BurningTime));

            Burning = true;
        }
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
        OnDisable();
    }

    void checkSurroundingIgnitionObjects()
    {
        //Busco objetos circundantes que puedan propagar el daño.
        var cols = Physics.OverlapSphere(transform.position, _ignitableSearchRadius, efectTargets, QueryTriggerInteraction.Collide);

        for (int i = 0; i < cols.Length; i++)
        {
            var igniteable = cols[i].GetComponent<IIgnitableObject>();
            if (igniteable != null && !toIgnite.Contains(igniteable) && igniteable.IsActive)
            {
                //lo añado a una lista a ignitar.
                toIgnite.Add(igniteable);
            }
        }
    }

    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return (transform.position + ((requester.position - transform.position).normalized) * _interactionRadius);
    }
    public List<OperationOptions> GetSuportedOperations()
    {
        return suportedInteractions;
    }
    public void Operate(OperationOptions operation, params object[] optionalParams)
    {
        if (operation == OperationOptions.Ignite)
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _ignitableSearchRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);
    }
#endif
}
