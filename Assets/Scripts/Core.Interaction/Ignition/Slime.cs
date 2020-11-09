using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Core.Interaction;
using Core.InventorySystem;
using Core.DamageSystem;
using IA.PathFinding;

[RequireComponent(typeof(Collider), typeof(InteractionHandler))]
public class Slime : Item, IIgnitableObject
{
    public Action RemoveFromNode = delegate { };
    public event Action CancelInputs = delegate { };

    [Header("================== Slime Settings ======================")]
    [SerializeField] Damage toAplyDamage = new Damage();
    [SerializeField] float _chainReactionDelay = 0.1f;
    [SerializeField] GameObject[] fireParticles = null;
    [SerializeField] ParticleSystem _dropParticle = null;
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

    float _burningTime = 5f;

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

    protected override void Awake()
    {
        for (int i = 0; i < myMaterials.Count; i++)
        {
            Materials.Add(i, myMaterials[i]);
        }
    }
    void Update()
    {
        if(Burning)
        {
            if (_remainingLifeTime <= 0)
            {
                RemoveFromNode();
                TurnOffBurningAndDestroy();
                return;
            }

            _remainingLifeTime -= Time.deltaTime;
        }
    }

    private void OnTriggerStay(Collider other)
    {
#if UNITY_EDITOR
        if (debugThisUnit)
            Debug.Log($"{gameObject.name} entró en colisión.");
#endif
        if (Burning)
        {
            var damageable = other.GetComponent<IDamageable<Damage, HitResult>>();
            if (damageable != null)
                damageable.GetHit(toAplyDamage);
        }
    }

    public void TurnOffBurningAndDestroy()
    {
        Burning = false;
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
        var dropParticle = Instantiate(_dropParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);//Esta parte hay que reemplazarlo por una bonita partícula.
    }

    //======================================= Ingnition System ====================================================

    public bool Burning
    {
        get => _burning;
        set => _burning = value;
    }

    public void StainObjectWithSlime()
    {
        //Cambio los materiales a los que corresponden con el stain.
    }
    public override InteractionParameters getInteractionParameters(Vector3 requesterPosition)
    {
        NodeGraphBuilder graph = FindObjectOfType<NodeGraphBuilder>();
        Vector3 safeInteractionPosition = (transform.position + ((requesterPosition - transform.position).normalized) * _safeInteractionDistance);
        Node SafeNode = PathFindSolver.getCloserNodeInGraph(safeInteractionPosition, graph);
        Vector3 LookToDirection = transform.forward;

        return new InteractionParameters(SafeNode, LookToDirection);
    }
    public override List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory inventory, bool ignoreInventory)
    {
        if (ignoreInventory)
        {
            return new List<Tuple<OperationType, IInteractionComponent>>()
            {
                new Tuple<OperationType, IInteractionComponent>(OperationType.Ignite, this)
            };
        }

        if (inventory != null)
        {
            if (inventory.equiped != null && inventory.equiped.ID == ItemID.Antorcha)
            {
                Torch torch = (Torch)inventory.equiped;
                if (torch.isBurning)
                    return new List<Tuple<OperationType, IInteractionComponent>>()
                    {
                        new Tuple<OperationType, IInteractionComponent>(OperationType.Ignite, this)
                    };
            }

            if (inventory.equiped != null && inventory.equiped.ID == (int)ItemID.Jarron)
            {
                return new List<Tuple<OperationType, IInteractionComponent>>()
                {
                    new Tuple<OperationType, IInteractionComponent>(OperationType.Combine, this)
                };
            };
        }

        return new List<Tuple<OperationType, IInteractionComponent>>();
    }
    public override void ExecuteOperation(OperationType operation, params object[] optionalParams)
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
        if (!Burning && operation == OperationType.Combine)
        {
            //Ejecuto combine(?
        }
    }

    //======================================= Corrutines ==========================================================

    IEnumerator DelayedOtherIgnition(float Delay)
    {
        //Llamo la función Ignite a todos los de la lista de ignición con los que overlapeo.
        Queue<GameObject> _toDestroy = new Queue<GameObject>();
        foreach (var patch in patches)
        {
            var patchComp = patch.GetComponent<SlimePatch>();
            if (patchComp && patchComp.DestroyOnBurning)
                _toDestroy.Enqueue(patch);
        }
        while(_toDestroy.Count > 0)
        {
            var item = _toDestroy.Dequeue();
            patches.Remove(item);
            Destroy(item);
        }
        yield return new WaitForSeconds(Delay);
        checkSurroundingIgnitionObjects();
        foreach (var item in toIgnite)
        {
            if (!item.Burning)
                item.ExecuteOperation(OperationType.Ignite);
        }
        toIgnite.Clear();
    }
}
