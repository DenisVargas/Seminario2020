using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using System;

[RequireComponent(typeof(Collider))]
public class IgnitableObject : PooleableComponent, IInteractable, IIgnitableObject, IAgressor<Damage, HitResult>
{
    public Action registerInUpdateList_Callback = delegate { };
    public Action removeFromUpdateList_Callback = delegate { };
    [SerializeField] GameObject fireParticle = null;
    [SerializeField] List<OperationOptions> suportedInteractions = new List<OperationOptions>();

    [Tooltip("Cuanto tiempo estará activo mientras ")]
    public float nonActive_LifeTime = 5f;
    [Tooltip("Cuanto tiempo estará prendido el fuego.")]
    public float Active_FireTime = 5f;
    [HideInInspector]
    public float ExplansionDelayTime = 0.8f;

    [SerializeField] float lifeTime = 0;

    [SerializeField] float _damagePerSecond = 5f;
    [SerializeField] float _affectedRadius = 2;
    //[SerializeField] float _interactionRadius = 3; //Esto tiene que ser dada al player para evitar que reciba daño del fuego.
    [SerializeField]LayerMask efectTargets = ~0;

    public List<IIgnitableObject> toIgnite = new List<IIgnitableObject>();

    //Debug inspectorList.
    public List<GameObject> toIgniteObjects = new List<GameObject>();

    [SerializeField] Collider _col = null;
    [SerializeField] HitBox _hitBox = null;

    public bool Burning { get; private set; } = (false);
    public Vector3 position => transform.position;

    public bool IsActive => gameObject.activeSelf;

    public void UpdateLifeTime(float deltaTime)
    {
        lifeTime -= deltaTime;
        if (lifeTime <= 0)
        {
            Dispose();
        }
    }

    public void ResetCurrentLifeTime()
    {
        lifeTime = nonActive_LifeTime;
    }

    //Para retornar un objeto pooleable al pool del que se originó, utiliza Dispose();

    /// <summary>
    /// Callback que se llama cuando cuando el poolObject es activado.
    /// </summary>
    public override void Enable()
    {
        //Nos activamos.
        gameObject.SetActive(true);

        //Nos registramos en la lista de Updateo.
        registerInUpdateList_Callback();
    }
    /// <summary>
    /// Callback que se llama cuando cuando el poolObject es desactivado.
    /// </summary>
    public override void Disable()
    {
        gameObject.SetActive(false);

        //Nos removemos del sistema de updateo.
        removeFromUpdateList_Callback();
    }


    public void Ignite(float delayTime = 0)
    {
        if (!Burning)
        {
            //print(gameObject.name + " IS IGNITED");
            //Desactivo mi collider. Ya no soy interactuable.
            _col.enabled = false;
            //Activo el área de daño.
            _hitBox.active = true;
            //Activo la particula del fueguín.
            fireParticle.SetActive(true);

            StartCoroutine(DelayedOtherIgnition(delayTime));
            StartCoroutine(extinguish(Active_FireTime));

            Burning = true;
            lifeTime = Active_FireTime;
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
        Dispose();
    }

    void checkSurroundingIgnitionObjects()
    {
        //Busco objetos circundantes que puedan propagar el daño.
        var cols = Physics.OverlapSphere(transform.position, _affectedRadius, efectTargets, QueryTriggerInteraction.Collide);

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

    public Damage getDamageState()
    {
        return new Damage()
        {
            type = DamageType.e_fire,
            Ammount = _damagePerSecond,
            criticalMultiplier = 2,
            instaKill = true
        };
    }

    public void HitStatus(HitResult result)
    {
        //No hago nada en particular porque soy una trampa wii.
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _affectedRadius);
    }

#endif
}
