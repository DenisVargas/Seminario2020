using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.ObjectPools.Generic;

public class Trail : MonoBehaviour
{
    [Tooltip("¿Tiene permitido este trail generar nuevos items?")]
    public bool Emit = false;
    [SerializeField] GameObject _ignitionSystemPrefab = null;
    [SerializeField] Transform _fireTrailParent = null;
    [SerializeField] float _spawnDistance             = 1f;

    GenericPool<IgnitableObject> _spawnPool = null;
    List<IgnitableObject> _spawned = new List<IgnitableObject>();

    private void Awake()
    {
        _spawnPool = new GenericPool<IgnitableObject>
        (
            20,
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
                _spawned.Add(ignit);
            },
            (ignit) =>
            {
                ignit.gameObject.SetActive(false);
                _spawned.Remove(ignit);
            }
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
        foreach (var item in _spawned)
            _spawnPool.DisablePoolObject(item);
        _spawned.Clear();
    }

    void SpawnObjectFromPool()
    {
        //Obtengo un objeto del pool.
        var poolSystem = _spawnPool.GetObjectFromPool();

        //Tengo que chequear cosas para que funcione correctamente.


        //Reposiciono el objeto en el mundo.
        poolSystem.transform.position = transform.position;
    }
}
