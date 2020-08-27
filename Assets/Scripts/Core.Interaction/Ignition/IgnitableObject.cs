using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Core.Interaction;

[RequireComponent(typeof(Collider))]
public class IgnitableObject : MonoBehaviour, IIgnitableObject
{
    public Action OnDisable = delegate { };
    public event Action CancelInputs = delegate { };

    [SerializeField] float _chainReactionDelay = 0.1f;
    [SerializeField] GameObject[] fireParticles = null;
    [SerializeField] LayerMask efectTargets = ~0;
    [SerializeField] float _ignitableSearchRadius = 5f;
    [SerializeField] float _safeInteractionDistance = 3;
    [SerializeField] float _remainingLifeTime = 0;
    [SerializeField] TrapHitBox _trapHitBox = null; //Se encarga de administrar daño por fuego.

    public List<IIgnitableObject> toIgnite = new List<IIgnitableObject>();

    public MeshRenderer myMesh;
    public List<Material> myMaterials = new List<Material>();
    Dictionary<int, Material> Materials = new Dictionary<int, Material>();

    bool _freezeInPlace = false;
    float _burningTime = 5f;
    float _expansionDelayTime = 0.8f;
    float _inputWaitTime = 2f;

    void Awake()
    {
        for (int i = 0; i < myMaterials.Count; i++)
        {
            Materials.Add(i, myMaterials[i]);
        }
    }
    #region DEBUG
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _ignitableSearchRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _safeInteractionDistance);
    }
    #endif 
    #endregion
    void Update()
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
    void FreezeAll()
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
    void UnFreezeAll()
    {
        checkSurroundingIgnitionObjects();
        foreach (var ignit in toIgnite)
        {
            if (!ignit.isFreezed && ignit != (IIgnitableObject)this)
            {
                ignit.isFreezed = false;
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
        // Desactivamos la interacción x ignite.
        // Desactivamos las particulas de fuego
        _trapHitBox.IsActive = false;
        _freezeInPlace = false;
        CancelInputs();
        CancelInputs = delegate { };
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
    public void OnInteractionEvent()
    {
        //isLocked = true;
        checkSurroundingIgnitionObjects();
        foreach (var igniteable in toIgnite)
        {
            if (igniteable != (IIgnitableObject)this && !igniteable.lockInteraction)
            {
                //igniteable.OnInteractionEvent();
            }
        }

        //Aumento el tiempo de vida de esta "particula" x el treshold
        _remainingLifeTime += _inputWaitTime;
        StartCoroutine(UnlockInNextFrame());
    }
    public IgnitableObject SetDirection(Vector3 Dir)
    {
        transform.right = Dir;
        return this;
    }
    public IgnitableObject SetMaterial(int Key)
    {
        myMesh.material = Materials[Key];
        return this;
    }

    //======================================= Ingnition System ====================================================

    public OperationType OperationType => OperationType.Ignite;
    public Vector3 LookToDirection => transform.forward;
    public bool IsActive => gameObject.activeSelf;
    public bool Burning { get; private set; } = (false);
    public bool isFreezed
    {
        get => _freezeInPlace;
        set
        {
            _freezeInPlace = value;
            if (_freezeInPlace)
                FreezeAll();
        }
    }
    public bool IsCurrentlyInteractable { get; private set; } = (true);
    public bool lockInteraction { get; set; } = (false);

    public void StainObjectWithSlime()
    {
        //Cambio los materiales a los que corresponden con el stain.
    }

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return (transform.position + ((requesterPosition - transform.position).normalized) * _safeInteractionDistance);
    }
    public void InputConfirmed(params object[] optionalParams)
    {
        isFreezed = true;
    }
    public void ExecuteOperation(params object[] optionalParams)
    {
        if (!Burning)
        {
            //print(gameObject.name + " IS IGNITED");

            // Desactivo las interacciones.
            foreach (var item in fireParticles)
            {
                //Activo las particulas!.
                item.SetActive(true);
            }
            _trapHitBox.IsActive = true;

            StartCoroutine(DelayedOtherIgnition(_chainReactionDelay));
            _remainingLifeTime = _burningTime;

            Burning = true;
        }
    }
    public void CancelOperation(params object[] optionalParams)
    {
        _freezeInPlace = false;
    }
    public void StartChainReaction() { }

    //======================================= Corrutines ==========================================================

    IEnumerator DelayedOtherIgnition(float Delay)
    {
        //Llamo la función Ignite a todos los de la lista de ignición con los que overlapeo.
        yield return new WaitForSeconds(Delay);
        checkSurroundingIgnitionObjects();
        foreach (var item in toIgnite)
        {
            if (!item.Burning)
                item.ExecuteOperation();
        }
        toIgnite.Clear();
    }
    IEnumerator UnlockInNextFrame()
    {
        yield return new WaitForSeconds(0.1f);
        //Locked = false; //Desbloqueo la "muerte" del objeto.
    }
}
