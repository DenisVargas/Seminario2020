using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using IA.PathFinding;

[RequireComponent(typeof(BoxCollider))]
public class PitTrap : MonoBehaviour
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();

    [SerializeField] Animator _anims = null;

    [SerializeField] bool isActive = false;
    BoxCollider _col;

    [SerializeField] List<Collider> OnTop = new List<Collider>();
    [SerializeField] List<Slime> OnTopBabas = new List<Slime>();
    [SerializeField] Node[] AffectedNodes = new Node[0];
    AudioSource MySound;

#if UNITY_EDITOR
    [Header("================ DEBUG ================")]
    [SerializeField] bool debugThisTrap = false;
#endif

    private void Awake()
    {
        _col = GetComponent<BoxCollider>();
        _col.isTrigger = true;
        MySound = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (isActive)
            killOnTopEntities();
    }

    public void killOnTopEntities()
    {
        var FilteredOnTop = new List<Collider>();
        foreach (var coll in OnTop)
        {
            bool falling = false;
            var slug = coll.GetComponent<Baboso>();
            if (slug != null)
            {
                slug.ChangeStateTo(CommonState.fallTrap);
                falling = true;
            }

            var player = coll.GetComponent<Controller>();
            if (player != null)
            {
                //Le digo al player que valió verga :D
                player.FallInTrap();
                falling = true;
            }

            var grunt = coll.GetComponent<Grunt>();
            if (grunt != null)
            {
                grunt.ChangeStateTo(CommonState.fallTrap);
                falling = true;
            }

            if (!falling)
                FilteredOnTop.Add(coll);
        }

        OnTop = FilteredOnTop;
    }
    public void OnEnableTrap()
    {
#if UNITY_EDITOR
        if (debugThisTrap)
            print($"{gameObject.name} se desactiva.");
#endif
        if (MySound.isPlaying)
            MySound.Stop();

        MySound.Play();

        _anims.SetBool("Activate", true);
        OnActivate.Invoke();

        if (AffectedNodes.Length > 0)
        {
            foreach (var node in AffectedNodes)
                node.ChangeNodeState(NavigationArea.blocked);
        }
        if (OnTopBabas.Count > 0)
        {
            foreach (var baba in OnTopBabas)
                baba.OnGrundFall();
            OnTopBabas = new List<Slime>();
        }

        isActive = true;
        killOnTopEntities();
    }
    public void OnDisableTrap()
    {
#if UNITY_EDITOR
        if (debugThisTrap)
            print($"{gameObject.name} se desactiva.");
#endif
        if (MySound.isPlaying)
            MySound.Stop();

        MySound.Play();
        _anims.SetBool("Activate", false);
        OnDeActivate.Invoke();

        if (AffectedNodes.Length > 0)
        {
            foreach (var node in AffectedNodes)
                node.ChangeNodeState(NavigationArea.Navegable);
        }

        isActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //print($"{other.gameObject.name} entro a la trampa");
        var npc = other.GetComponent<BaseNPC>();

        if (npc != null)
        {
            OnTop.Add(other);
            npc.SubscribeToLifeCicleDependency(OnEntityDieForExternSource);
        }

        var player = other.GetComponent<Controller>();
        if (player != null)
        {
            OnTop.Add(other);
        }

        var Baba = other.GetComponent<Slime>();
        if (Baba != null)
        {
            OnTopBabas.Add(Baba);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //print($"{other.gameObject.name} Salió de la trampa");
        if (OnTop.Contains(other))
        {
            var npc = other.GetComponent<BaseNPC>();
            if (npc != null)
            {
                npc.UnsuscribeToLifeCicleDependency(OnEntityDieForExternSource);
                OnTop.Remove(other);
            }

            var player = other.GetComponent<Controller>();
            if (player != null)
            {
                OnTop.Remove(other);
            }

            var Baba = other.GetComponent<Slime>();
            if (Baba != null)
            {
                OnTopBabas.Remove(Baba);
            }
        }
    }

    /// <summary>
    /// Callback que se llama cuando una unidad muere por algún motivo diferente a esta trampa.
    /// </summary>
    /// <param name="go">GameObject del objeto a considerar.</param>
    void OnEntityDieForExternSource(Collider _coll)
    {
        if (OnTop.Contains(_coll))
            OnTop.Remove(_coll);
    }
}
