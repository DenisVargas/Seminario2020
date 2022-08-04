using IA.PathFinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DobleRotatingWall : MonoBehaviour
{
    [SerializeField] Animator animsA = null;
    [SerializeField] Animator animsB = null;
    [SerializeField, Tooltip("Si la pared esta activa, estos nodos de desactivaran al inicio.")]
    Node[] AffectedNodesA = null;
    [SerializeField, Tooltip("Este grupo siempre esta bloqueado.")]
    Node[] AffectedNodesB = null;
    [SerializeField, Tooltip("Si la pared esta activa, estos nodos de activaran al inicio.")]
    Node[] AffectedNodesC = null;

    AudioSource _mySound;

    public bool Active = false;

    private void Awake()
    {
        //if (anims == null)
        //    anims = GetComponent<Animator>();

        _mySound = GetComponentInChildren<AudioSource>();
    }

    private void Start()
    {
        //Los affected nodes cambian de estado al activarse la pared.
        foreach (var node in AffectedNodesB)
            node.ChangeNodeState(NavigationArea.blocked);
    }

    public void Activate()
    {
        //Activo/Desactivo la wall. Animación.
        Active = !Active;
        if (_mySound.isPlaying)
            _mySound.Stop();

        _mySound.Play();

        animsA.SetBool("Activated", Active);
        animsB.SetBool("Activated", Active);
    }

#if UNITY_EDITOR
    public bool _debugThisWall = false;
#endif

    public void ActivationStarted()
    {
#if UNITY_EDITOR
        if (_debugThisWall)
            print("Puerta doble rotatoria, Activation Started"); 
#endif
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
                node.ChangeNodeState(NavigationArea.blocked);
            foreach (var node in AffectedNodesC)
                node.ChangeNodeState(NavigationArea.Navegable);
        }
        else
        {
            foreach (var node in AffectedNodesA)
                node.ChangeNodeState(NavigationArea.Navegable);
            foreach (var node in AffectedNodesC)
                node.ChangeNodeState(NavigationArea.blocked);
        }
    }
}
