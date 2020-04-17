using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class IgnitableObject : MonoBehaviour, IInteractable, IIgnitableObject
{
    [SerializeField] GameObject fireParticle = null;
    [SerializeField] ParticleSystem fire = null;
    [SerializeField] List<OperationOptions> suportedInteractions = new List<OperationOptions>();

    [SerializeField]
    [Tooltip("Cuanto tiempo pasará antes de que el fuego se expanda a nodos subyacentes.")]
    float _delay = 0.8f;

    [Tooltip("Cuanto tiempo estará prendido el fuego.")]
    [SerializeField] float _fireTime = 5f;
    [SerializeField] float _affectedRadius = 2;
    [SerializeField] float _interactionRadius = 3;
    [SerializeField]LayerMask efectTargets = ~0;

    public List<IIgnitableObject> toIgnite = new List<IIgnitableObject>();
    public List<GameObject> toIgniteObjects = new List<GameObject>();

    [SerializeField] Collider _col = null;
    [SerializeField] Collider _damageArea = null;
    
    public bool IsIgniteable { get; private set; } = (true);
    public Vector3 position => transform.position;

    //private void Start()
    //{
    //    checkSurroundingIgnitionObjects();
    //}

    // Update is called once per frame
    void Update()
    {

    }

    public void Ignite(float delayTime = 0)
    {
        if (IsIgniteable)
        {
            Debug.LogWarning(gameObject.name + " IS IGNITED");

            //Desactivo mi collider. Ya no soy interactuable.
            _col.enabled = false;
            //Activo el área de daño.
            _damageArea.gameObject.SetActive(true);
            //Activo la particula del fueguín.
            //fire.gameObject.SetActive(true);
            fireParticle.SetActive(true);

            checkSurroundingIgnitionObjects();
            StartCoroutine(DelayedOtherIgnition(delayTime));

            IsIgniteable = false;
        }
    }

    IEnumerator DelayedOtherIgnition(float Delay)
    {
        //Llamo la función Ignite a todos los de la lista de ignición con los que overlapeo.
        yield return new WaitForSeconds(Delay);
        checkSurroundingIgnitionObjects();
        foreach (var item in toIgnite)
        {
            if (item.IsIgniteable)
                item.Ignite(Delay);
        }
    }

    void checkSurroundingIgnitionObjects()
    {
        //Busco objetos circundantes que puedan propagar el daño.
        var cols = Physics.OverlapSphere(transform.position, _affectedRadius, efectTargets, QueryTriggerInteraction.Collide);

        for (int i = 0; i < cols.Length; i++)
        {
            var igniteable = cols[i].GetComponent<IIgnitableObject>();
            if (igniteable != null && !toIgnite.Contains(igniteable))
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
        if (operation == OperationOptions.Activate) Ignite(_delay);
    }

    private void OnMouseEnter()
    {
        //Feedback de interacción.
    }

    private void OnMouseExit()
    {
        //Feedback de interacción.
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _affectedRadius);
    }
}
