using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.ObjectPools.Generic;

public class Trail : MonoBehaviour
{
    public bool Emit = false;
    [SerializeField] GameObject _ignitionSystemPrefab = null;
    [SerializeField] Transform _fireTrailParent = null;
    [SerializeField] float _spawnDistance       = 1f;
    [SerializeField] float _spawnOverlapTreshold = 0.9f;

    GenericPool<IgnitableObject> _spawnPool = null;
    List<IgnitableObject> _spawned = new List<IgnitableObject>();
    IgnitableObject _lastSpawned = null;

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
                ignit.OnSpawn();
                _spawned.Add(ignit);
            },
            (ignit) =>
            {
                ignit.gameObject.SetActive(false);
                _spawned.Remove(ignit);
            },
            true
        );
    }

    private void Update()
    {
        if (Emit)
        {
            SpawnObjectFromPool();
        }
    }

    private void OnDisable()
    {
        List<IgnitableObject> toRemove = new List<IgnitableObject>(_spawned);
        _spawned.Clear();
        foreach (var item in toRemove)
            _spawnPool.DisablePoolObject(item);
    }

    void SpawnObjectFromPool()
    {
        if (_lastSpawned != null)
        {
            //print("La distancia con el ultimo es: " + Vector3.Distance(transform.position, _lastSpawned.position));
            if (Vector3.Distance(transform.position, _lastSpawned.position) > _spawnDistance)
            {
                //Chequeo si no estoy overlapeando algun otro ignitable Object.
                var overlapped = getOverlappedIgnitableObject();
                if (overlapped != null)
                    overlapped.OnSpawn();
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
                var dst = Vector3.Distance(transform.position, igniteable.position);
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
            poolSystem.transform.position = transform.position;
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
