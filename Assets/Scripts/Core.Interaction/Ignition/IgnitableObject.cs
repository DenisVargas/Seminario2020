using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Core.Interaction;
using Core.InventorySystem;
using Core.DamageSystem;

[RequireComponent(typeof(Collider), typeof(InteractionHandler))]
public class IgnitableObject : MonoBehaviour, IIgnitableObject
{
    public Action RemoveFromNode = delegate { };
    public event Action CancelInputs = delegate { };

    [SerializeField] Damage toAplyDamage = new Damage();
    [SerializeField] float _chainReactionDelay = 0.1f;
    [SerializeField] GameObject[] fireParticles = null;
    [SerializeField] GameObject _root = null;
    [SerializeField] LayerMask efectTargets = ~0;
    [SerializeField] float _ignitableSearchRadius = 5f;
    [SerializeField] float _safeInteractionDistance = 3;
    [SerializeField] float _remainingLifeTime = 0;
    [SerializeField] bool _burning = false;
    public List<GameObject> patches = new List<GameObject>();

    public GameObject RootGameObject
    {
        get => _root;
        set => _root = value;
    }

    public List<IIgnitableObject> toIgnite = new List<IIgnitableObject>();

    public MeshRenderer myMesh;
    public List<Material> myMaterials = new List<Material>();
    Dictionary<int, Material> Materials = new Dictionary<int, Material>();

    bool _freezeInPlace = false;
    float _burningTime = 5f;
    float _expansionDelayTime = 0.8f;
    float _inputWaitTime = 2f;

#if UNITY_EDITOR

    [SerializeField] bool debugThisIgnitableObject = false;

#endif


    private void OnTriggerStay(Collider other)
    {
#if UNITY_EDITOR
        if (debugThisIgnitableObject)
            Debug.Log($"{gameObject.name} entró en colisión.");
#endif
        if (Burning)
        {
            var damageable = other.GetComponent<IDamageable<Damage, HitResult>>();
            if (damageable != null)
                damageable.GetHit(toAplyDamage);
        }
    }

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
        if(Burning)
        {
            if (_remainingLifeTime <= 0)
            {
                RemoveFromNode();
                KillIngnitableObject();
                return;
            }

            _remainingLifeTime -= Time.deltaTime;
        }
    }

    //Esto se llamaba desde Trail.
    public void OnSpawn(float LifeTime, float IgnitionLifeTime, float InputWaitTime, float ExpansionDelayTime = 0.8f)
    {
        //Iniciamos los contadores.
        _remainingLifeTime = LifeTime;
        _burningTime = IgnitionLifeTime;
        _inputWaitTime = InputWaitTime;
        _expansionDelayTime = ExpansionDelayTime;

        StopAllCoroutines();
    }

    public void KillIngnitableObject()
    {
        //print($"{gameObject.name} la baba se apaga ignite");
        Burning = false;
        // Desactivamos la interacción x ignite.
        // Desactivamos las particulas de fuego
        //_trapHitBox.IsActive = false;
        _freezeInPlace = false;
        CancelInputs();
        CancelInputs = delegate { };
        foreach (var item in patches)
            Destroy(item);
        Destroy(_root);
    }
    void checkSurroundingIgnitionObjects()
    {
        //Busco objetos circundantes que puedan propagar el daño.
        var cols = Physics.OverlapSphere(transform.position, _ignitableSearchRadius, efectTargets, QueryTriggerInteraction.Collide);

        for (int i = 0; i < cols.Length; i++)
        {
            var igniteable = cols[i].GetComponent<IIgnitableObject>();
            if (igniteable != null && !toIgnite.Contains(igniteable)) //&& igniteable.IsActive)
            {
                //lo añado a una lista a ignitar.
                toIgnite.Add(igniteable);
            }
        }
    }
    /// <summary>
    /// Callback que se llama cuando las trampas de pinchos se activan!
    /// </summary>
    public void OnGrundFall()
    {
        foreach (var patch in patches)
        {
            Destroy(patch.gameObject);
        }
        patches.Clear();
        RemoveFromNode();
        Destroy(gameObject);//Esta parte hay que reemplazarlo por una bonita partícula.
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
            if (igniteable != (IIgnitableObject)this)// && !igniteable.lockInteraction)
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

    public bool Burning
    {
        get => _burning;
        set => _burning = value;
    }
    public bool isFreezed
    {
        get => _freezeInPlace;
        set
        {
            _freezeInPlace = value;
            //if (_freezeInPlace)
            //    FreezeAll();
        }
    }

    public bool isDynamic => false;

    public void StainObjectWithSlime()
    {
        //Cambio los materiales a los que corresponden con el stain.
    }

    public InteractionParameters getInteractionParameters(Vector3 requesterPosition)
    {
        NodeGraphBuilder graph = FindObjectOfType<NodeGraphBuilder>();

        Vector3 safeInteractionPosition = (transform.position + ((requesterPosition - transform.position).normalized) * _safeInteractionDistance);
        IA.PathFinding.Node SafePosition = PathFindSolver.getCloserNodeInGraph(safeInteractionPosition, graph);
        Vector3 LookToDirection = transform.forward;

        return new InteractionParameters(SafePosition, LookToDirection);
    }
    public List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory)
    {
        return new List<Tuple<OperationType, IInteractionComponent>>()
        {
            new Tuple<OperationType, IInteractionComponent>(OperationType.Ignite, this)
        };
    }
    public void InputConfirmed(OperationType operation, params object[] optionalParams)
    {
        isFreezed = true;
    }
    public void ExecuteOperation(OperationType operation, params object[] optionalParams)
    {
        if (!Burning && operation == OperationType.Ignite)
        {
            //print($"{gameObject.name} empezo la operación ignite");
            // Desactivo las interacciones.
            foreach (var item in fireParticles)
                item.SetActive(true);
            //_trapHitBox.IsActive = true;

            StartCoroutine(DelayedOtherIgnition(_chainReactionDelay));
            _remainingLifeTime = _burningTime;

            Burning = true;
        }
    }
    public void CancelOperation(OperationType operation, params object[] optionalParams)
    {
        _freezeInPlace = false;
    }

    //======================================= Corrutines ==========================================================

    IEnumerator DelayedOtherIgnition(float Delay)
    {
        //Llamo la función Ignite a todos los de la lista de ignición con los que overlapeo.
        yield return new WaitForSeconds(Delay);
        checkSurroundingIgnitionObjects();
        foreach (var item in toIgnite)
        {
            if (!item.Burning)
                item.ExecuteOperation(OperationType.Ignite);
        }
        toIgnite.Clear();
    }
    IEnumerator UnlockInNextFrame()
    {
        yield return new WaitForSeconds(0.1f);
        //Locked = false; //Desbloqueo la "muerte" del objeto.
    }
}
