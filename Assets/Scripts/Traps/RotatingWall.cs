using IA.PathFinding;
using UnityEngine;

public class RotatingWall : MonoBehaviour
{
    [SerializeField] Animator anims = null;
    [SerializeField, Tooltip("Si la pared esta activa, estos nodos de desactivaran al inicio.")]
    Node[] AffectedNodesA = null;
    [SerializeField, Tooltip("Si la pared esta activa, estos nodos de activaran al inicio.")]
    Node[] AffectedNodesB = null;

    AudioSource _mySound;

    public bool Active = false;

    private void Awake()
    {
        if (anims == null)
            anims = GetComponent<Animator>();

        _mySound = GetComponent<AudioSource>();
    }

    private void Start()
    {
        //Bloqueo uno de los caminos dependiendo del estado inicial.
        ActivationEnded();
    }

    public void Activate()
    {
        //Activo/Desactivo la wall. Animación.
        Active = !Active;
        if (_mySound.isPlaying)
            _mySound.Stop();

        _mySound.Play();

        anims.SetBool("Activated", Active);
    }

#if UNITY_EDITOR
    public bool _debugThisWall = false;
#endif

    /// <summary>
    /// Se llama al iniciar la animación, se bloquean ambos caminos!
    /// </summary>
    public void ActivationStarted()
    {
#if UNITY_EDITOR
        if (_debugThisWall)
            print("OnActivation Start");
#endif

        foreach (var node in AffectedNodesA)
            node.ChangeNodeState(NavigationArea.blocked);
        foreach (var node in AffectedNodesB)
            node.ChangeNodeState(NavigationArea.blocked);
    }
    /// <summary>
    /// Se llama al terminarse la animación, dependiendo del estado final, se desbloquea uno de los 2 caminos!.
    /// </summary>
    public void ActivationEnded()
    {
#if UNITY_EDITOR
        if (_debugThisWall)
            print("OnActivation End");
#endif

        //Los affected nodes cambian de estado al activarse la pared.
        if (Active)
        {
            foreach (var node in AffectedNodesA)
                if (node.area == NavigationArea.Navegable) node.ChangeNodeState(NavigationArea.blocked);
            foreach (var node in AffectedNodesB)
                if (node.area == NavigationArea.blocked) node.ChangeNodeState(NavigationArea.Navegable);
        }
        else
        {
            foreach (var node in AffectedNodesA)
                if (node.area == NavigationArea.blocked) node.ChangeNodeState(NavigationArea.Navegable);
            foreach (var node in AffectedNodesB)
                if (node.area == NavigationArea.Navegable) node.ChangeNodeState(NavigationArea.blocked);
        }
    }
}
