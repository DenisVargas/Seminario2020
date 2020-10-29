using IA.PathFinding;
using UnityEngine;

public class RotatingWall : MonoBehaviour
{
    [SerializeField] Animator anims = null;
    [SerializeField] Node[] AffectedNodesA = null;
    [SerializeField] Node[] AffectedNodesB = null;

    public bool Active = false;

    private void Awake()
    {
        if (anims == null)
            anims = GetComponent<Animator>();

        //Bloqueo uno de los caminos dependiendo del estado inicial.
        if (Active)
        {
            foreach (var node in AffectedNodesA)
                node.ChangeNodeState(NavigationArea.blocked);
            foreach (var node in AffectedNodesB)
                node.ChangeNodeState(NavigationArea.Navegable);
        }
        else
        {
            foreach (var node in AffectedNodesA)
                node.ChangeNodeState(NavigationArea.Navegable);
            foreach (var node in AffectedNodesB)
                node.ChangeNodeState(NavigationArea.blocked);
        }
    }

    public void Activate()
    {
        //Activo/Desactivo la wall. Animación.
        Active = !Active;
        anims.SetBool("Activated", Active);
    }
    /// <summary>
    /// Se llama al iniciar la animación, se bloquean ambos caminos!
    /// </summary>
    void OnActivationStart()
    {
        foreach (var node in AffectedNodesA)
            node.ChangeNodeState(NavigationArea.blocked);
        foreach (var node in AffectedNodesB)
            node.ChangeNodeState(NavigationArea.blocked);
    }
    /// <summary>
    /// Se llama al terminarse la animación, dependiendo del estado final, se desbloquea uno de los 2 caminos!.
    /// </summary>
    void OnActivationEnd()
    {
        //Los affected nodes cambian de estado al activarse la pared.
        if (Active)
        {
            foreach (var node in AffectedNodesA)
                node.ChangeNodeState(NavigationArea.blocked);
            foreach (var node in AffectedNodesB)
                node.ChangeNodeState(NavigationArea.Navegable);
        }
        else
        {
            foreach (var node in AffectedNodesA)
                node.ChangeNodeState(NavigationArea.Navegable);
            foreach (var node in AffectedNodesB)
                node.ChangeNodeState(NavigationArea.blocked);
        }
    }
}
