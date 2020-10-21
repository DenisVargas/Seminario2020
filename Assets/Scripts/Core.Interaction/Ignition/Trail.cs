using UnityEngine;
using IA.PathFinding;
using Core.Interaction;

public class Trail : MonoBehaviour
{
    [Header("Igniteable Object Main Settings")]
    [SerializeField] GameObject _ignitionPoint_Prefab = null;
    [SerializeField] GameObject _slimePatch_Prefab    = null;

    public bool Emit { get; set; } = false;

    //En vez de utilizar Update vamos a utilizar un evento que chequee si el nodo mas cercano actual
    //Tiene o no un componente igniteable, sino, le añado uno.
    /// <summary>
    /// Este callback se llama al actualizarse el valor del nodo más cercano. Chequea si dicho nodo contiene un componente igniteable.
    /// Si no existe uno, le añade un sub-Objeto igniteable.
    /// </summary>
    /// <param name="current">El nuevo nodo actual.</param>
    public void OnCloserNodeChanged(Node current)
    {
        if (current == null) return;

        if (Emit)
        {
            if (current.handler == null || !current.handler.HasCompomponentOfType(OperationType.Ignite, true))
            {
                var ignition = Instantiate(_ignitionPoint_Prefab);
                ignition.transform.SetParent(current.gameObject.transform); //Añadimos el prefab como un subobjeto.
                ignition.transform.localPosition = Vector3.zero;

                var ignitionInteractionHandler = ignition.GetComponent<IInteractable>();
                var igniteObject = ignition.GetComponentInChildren<IgnitableObject>();
                if (igniteObject.RootGameObject == null) igniteObject.RootGameObject = ignition;
                igniteObject.RemoveFromNode += current.clearHandler;
                current.handler = ignitionInteractionHandler;

                //Seteo los parches.
                foreach (var connection in current.Connections)
                {
                    if (connection.handler != null && connection.handler.HasCompomponentOfType(OperationType.Ignite, true))
                    {
                        Vector3 dir = (connection.transform.position - current.transform.position).normalized;
                        Vector3 center = Vector3.Lerp(current.transform.position, connection.transform.position, 0.5f);
                        var patch = Instantiate(_slimePatch_Prefab,center, Quaternion.identity);
                        patch.transform.forward = dir;

                        var ignite = connection.GetComponentInChildren<IgnitableObject>();
                        ignite.patches.Add(patch);
                    }
                }
            }
        }
    }
}
