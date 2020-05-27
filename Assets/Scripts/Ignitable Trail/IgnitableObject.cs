using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class IgnitableObject : MonoBehaviour, IInteractable, IIgnitableObject
{
    public Action OnDisable = delegate { };
    public event Action CancelInputs = delegate { };

    [SerializeField] GameObject fireParticle = null;
    [SerializeField] List<OperationType> _suportedInteractions = new List<OperationType>();

    [SerializeField] float _ignitableSearchRadius = 5f;
    [SerializeField] float _interactionRadius = 3; //Esto tiene que ser dada al player para evitar que reciba daño del fuego.
    [SerializeField] LayerMask efectTargets = ~0;

    public List<IIgnitableObject> toIgnite = new List<IIgnitableObject>();

    [SerializeField] Collider _interactionCollider = null;
    [SerializeField] TrapHitBox _trapHitBox = null;

    public bool Burning { get; private set; } = (false);

    [SerializeField] float _remainingLifeTime = 0;
    bool _freezeInPlace = false;
    float _burningTime = 5f;
    float _expansionDelayTime = 0.8f;
    float _inputWaitTime = 2f;

    public Vector3 position => transform.position;
    public Vector3 LookToDirection => transform.forward;

    public bool IsActive => gameObject.activeSelf;

    public bool isFreezed
    {
        get => _freezeInPlace;
        set
        {
            _freezeInPlace = value;
            if (_freezeInPlace)
                FreezeInPlace();
        }
    }

    private void Update()
    {
        if (!_freezeInPlace)
        {
            reduxHealth();
        }
        else if(Burning)
        {
            reduxHealth();
        }
    }

    void reduxHealth()
    {
        if (_remainingLifeTime <= 0)
            OnDisable();
        else
            _remainingLifeTime -= Time.deltaTime;
    }
    void FreezeInPlace()
    {
        checkSurroundingIgnitionObjects();
        foreach (var ignit in toIgnite)
        {
            if (!ignit.isFreezed)
            {
                ignit.isFreezed = true;
            }
            else continue;
        }
    }

    public void OnSpawn(float LifeTime, float IgnitionLifeTime, float InputWaitTime, float ExpansionDelayTime = 0.8f)
    {
        //Iniciamos los contadores.
        _remainingLifeTime = LifeTime;
        _burningTime = IgnitionLifeTime;
        _inputWaitTime = InputWaitTime;
        _expansionDelayTime = ExpansionDelayTime;

        StopAllCoroutines();
    }

    public void OnDie()
    {
        Burning = false;
        _interactionCollider.enabled = true;
        _trapHitBox.IsActive = false;
        fireParticle.SetActive(false);
        _freezeInPlace = false;
        CancelInputs();
        CancelInputs = delegate { };
    }

    public void Ignite(float delayTime = 0)
    {
        if (!Burning)
        {
            //print(gameObject.name + " IS IGNITED");
            _interactionCollider.enabled = false;
            _trapHitBox.IsActive = true;
            fireParticle.SetActive(true);

            StartCoroutine(DelayedOtherIgnition(delayTime));
            _remainingLifeTime = _burningTime;

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

    /// <summary>
    /// Se llama cuando hacemos clic encima.
    /// </summary>
    /// <param name="toIgnore">Me mando a mismo</param>
    public void OnInteractionEvent(IIgnitableObject toIgnore)
    {
        checkSurroundingIgnitionObjects();
        foreach (var igniteable in toIgnite)
        {
            if (igniteable != (IIgnitableObject)this && igniteable != toIgnore)
            {
                igniteable.OnInteractionEvent(this);
            }
        }

        //Aumento el tiempo de vida de esta "particula" x el treshold
        _remainingLifeTime += _inputWaitTime;
    }

    public Vector3 requestSafeInteractionPosition(IInteractor requester)
    {
        return (transform.position + ((requester.position - transform.position).normalized) * _interactionRadius);
    }
    public InteractionParameters GetSuportedInteractionParameters()
    {
        OnInteractionEvent(this);

        return new InteractionParameters()
        {
            LimitedDisplay = true,
            ActiveTime = _inputWaitTime,
            SuportedOperations = _suportedInteractions
        };
    }
    public void OnConfirmInput(OperationType selectedOperation, params object[] optionalParams)
    {
        if (selectedOperation == OperationType.Ignite)
            isFreezed = true;
    }
    public void OnOperate(OperationType operation, params object[] optionalParams)
    {
        if (operation == OperationType.Ignite)
            Ignite(_expansionDelayTime);
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
