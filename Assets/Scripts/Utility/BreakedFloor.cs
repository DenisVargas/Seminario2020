using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakedFloor : MonoBehaviour
{
    public List<Rigidbody> pieces = new List<Rigidbody>();
    [SerializeField] CanvasButtonManager _sceneLoadingManager = null;
    [SerializeField] int _levelToLoad = 0;
    [SerializeField] float _loadingDelay = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            foreach (var item in pieces)
            {
                item.useGravity = true;
                item.AddForce(0, Random.Range(0, 100),0,ForceMode.Force);
            }
            var player = other.GetComponent<Controller>();
            if (player)
                player.FallInTrap();

            AsyncSceneLoadOptions.LevelBuildIndex = 1;
            AsyncSceneLoadOptions.LoadActive = false;

            StartCoroutine(DelayedLoadLevel());
        }
    }

    IEnumerator DelayedLoadLevel()
    {
        yield return new WaitForSeconds(_loadingDelay);

        if (_sceneLoadingManager != null)
            _sceneLoadingManager.LoadLevel(_levelToLoad);
        else
            Debug.LogError($"{gameObject.name}::La referencia al Loading Manager no esta seteada Salame!");
    }
}
