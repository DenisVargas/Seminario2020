using UnityEngine;

public class ChangeLvl : MonoBehaviour
{
    [SerializeField] CanvasButtonManager _sceneLoadingManager = null;
    [SerializeField] int _levelToLoad = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (_sceneLoadingManager != null)
                _sceneLoadingManager.LoadLevel(_levelToLoad);
            else
                Debug.LogError($"{gameObject.name}::La referencia al Loading Manager no esta seteada Salame!");
        }
    }
}
