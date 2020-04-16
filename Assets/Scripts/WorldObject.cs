using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour, IInteractable
{
    [SerializeField] Material onEnterMat = null;
    [SerializeField] Material onExitMat  = null;

    Material _normalMat = null;
    Renderer _renderer = null;

    public List<OperationOptions> suportedOperations = new List<OperationOptions>();

    public void Operate(int opID, params object[] optionalParams)
    {
        throw new System.NotImplementedException();
    }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _normalMat = _renderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseEnter()
    {
        _renderer.material = onEnterMat;
    }

    private void OnMouseExit()
    {
        _renderer.material = onExitMat;
    }

    List<OperationOptions> IInteractable.GetSuportedOperations()
    {
        return suportedOperations;
    }
}
