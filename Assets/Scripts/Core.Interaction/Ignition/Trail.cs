using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Utility.ObjectPools.Generic;

public class Trail : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] Transform _fireTrailParent = null;
    [SerializeField] float _spawnDistance       = 1f;
    [SerializeField] float _spawnOverlapTreshold = 0.9f;
    [Space]
    [Header("Igniteable Object Main Settings")]
    [SerializeField] GameObject _ignitionSystemPrefab = null;
    [SerializeField, Tooltip("Cuanto tiempo estará activo mientras ")]
    public float _activeTime = 5f;
    [SerializeField, Tooltip("Cuanto tiempo estará prendido el fuego.")]
    public float _burningTime = 5f;
    [SerializeField] float _expansionDelayTime = 0.8f;
    [SerializeField] float _inputWaitTime = 2f;

    GenericPool<IgnitableObject> _spawnPool = null;
    //List<IgnitableObject> _spawned = new List<IgnitableObject>();
    IgnitableObject _lastSpawned = null;
    bool _emit = false;

    private void Awake()
    {
        _spawnPool = new GenericPool<IgnitableObject>
        (
            5,
            () =>
            {
                var instance = Instantiate(_ignitionSystemPrefab, transform.position, Quaternion.identity, _fireTrailParent);

                IgnitableObject ignit = instance.GetComponent<IgnitableObject>();
                ignit.OnDisable += () =>
                {
                    _spawnPool.DisablePoolObject(ignit);
                };

                ignit.gameObject.SetActive(false);
                return ignit;
            },
            (ignit) =>
            {
                ignit.gameObject.SetActive(true);
                ignit.OnSpawn(_activeTime, _burningTime, _inputWaitTime, _expansionDelayTime);
                //_spawned.Add(ignit);
            },
            (ignit) =>
            {
                ignit.gameObject.SetActive(false);
                ignit.OnDie();
                //_spawned.Remove(ignit);
            },
            true
        );
    }

    private void Update()
    {
        if (_emit)
        {
            SpawnObjectFromPool();
        }
    }

    public void EnableEmission()
    {
        _emit = true;
    }
    public void DisableTrailEmission()
    {
        _emit = false;
    }

    void SpawnObjectFromPool()
    {
        if (_lastSpawned != null)
        {
            //print("La distancia con el ultimo es: " + Vector3.Distance(transform.position, _lastSpawned.position));
            if (Vector3.Distance(transform.position, _lastSpawned.transform.position) > _spawnDistance)
            {
                //Chequeo si no estoy overlapeando algun otro ignitable Object.
                var overlapped = getOverlappedIgnitableObject();
                if (overlapped != null)
                    overlapped.OnSpawn(_activeTime, _burningTime, _inputWaitTime, _expansionDelayTime);
                else
                    SpawnIgnitableObject();
            }
        }
        else
        {
            SpawnIgnitableObject();
        }
    }
    IgnitableObject getOverlappedIgnitableObject()
    {
        var cols = Physics.OverlapSphere(transform.position, _spawnOverlapTreshold);
        float findedMinDistance = float.MaxValue;
        IgnitableObject closestIgnitableFinded = null;
        foreach (var item in cols)
        {
            var igniteable = item.GetComponent<IgnitableObject>();
            if (igniteable != null)
            {
                var dst = Vector3.Distance(transform.position, igniteable.transform.position);
                if (igniteable.IsActive && dst < findedMinDistance)
                {
                    findedMinDistance = dst;
                    closestIgnitableFinded = igniteable;
                }
            }
            else continue;
        }

        return closestIgnitableFinded;
    }
    void SpawnIgnitableObject()
    {
        var poolSystem = _spawnPool.GetObjectFromPool();
        if (poolSystem != null)
        {
            //Reposiciono el objeto en el mundo.
            poolSystem.transform.position = transform.position - (transform.forward/2);
            //poolSystem.transform.right = transform.forward;
            //int Rnd = UnityEngine.Random.Range(0,2);
            //if(Rnd == 0)
            //    poolSystem.SetDirection(-1);
            //else
            //    poolSystem.SetDirection(1);
            poolSystem.SetDirection(UnityEngine.Random.Range(0, 2) == 0 ? transform.forward : -transform.forward).SetMaterial(UnityEngine.Random.Range(0, 3));

            _lastSpawned = poolSystem;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _spawnDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _spawnOverlapTreshold);
    } 
#endif
}
