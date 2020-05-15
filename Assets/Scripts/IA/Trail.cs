using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.ObjectPools.Generic;

public class Trail : MonoBehaviour
{
    [Tooltip("¿Tiene permitido este trail generar nuevos items?")]
    public bool Emit = true;
    [SerializeField] GameObject _ignitionSystemPrefab = null;
    [SerializeField] Transform _fireTrailParent = null;
    [SerializeField] float _spawnDistance             = 1f;
    [SerializeField] float _max_nonActiveLifeTime     = 10f;
    [SerializeField] float _max_ActiveLifeTime        = 5f;
    [SerializeField, Tooltip("Cuanto tiempo pasará antes de que el fuego se expanda a nodos subyacentes.")]
    float _expandDelayTimePerNode = 0.8f;

    GenericPool<IgnitableObject> spawnPool;
    List<IgnitableObject> UpdateList = new List<IgnitableObject>();
    List<IgnitableObject> RemoveList = new List<IgnitableObject>();
    GameObject lastSpawned = null;


    private void Awake()
    {
        spawnPool = new GenericPool<IgnitableObject>
        (
            20,
            () =>
            {
                //Ignit Factory Method
                var instance = Instantiate(_ignitionSystemPrefab, transform.position, Quaternion.identity, _fireTrailParent);

                IgnitableObject ignit = instance.GetComponent<IgnitableObject>();
                ignit.MaxLifeTime = _max_nonActiveLifeTime;
                ignit.BurningTime = _max_ActiveLifeTime;
                ignit.ExplansionDelayTime = _expandDelayTimePerNode;

                ignit.gameObject.SetActive(false);
                return ignit;
            },
            (ignit) =>
            {
                ignit.gameObject.SetActive(true);
            },
            (ignit) =>
            {
                ignit.gameObject.SetActive(false);
                spawnPool.DisablePoolObject(ignit);
            }
        );
    }

    private void Update()
    {
        if (Emit)
        {
            if (lastSpawned == null)
            {
                spawnPool.IsDinamic = true;
                SpawnObjectFromPool();
                _fireTrailParent.GetChild(0).GetComponent<IgnitableObject>();
            }
            if (Vector3.Distance(transform.position, lastSpawned.transform.position) > _spawnDistance)
                SpawnObjectFromPool();
        }

        foreach (var item in UpdateList)
        {
            item.UpdateLifeTime(Time.deltaTime);
        }

        if (RemoveList.Count > 0)
        {
            foreach (var item in RemoveList)
                UpdateList.Remove(item);
            RemoveList.Clear();
        }
    }

    private void OnDisable()
    {
        //Al deshabilitar este objeto que pasa con los Objetos del Pool?
        foreach (var item in UpdateList)
        {
            spawnPool.DisablePoolObject(item);
        }
    }

    void SpawnObjectFromPool()
    {
        //Obtengo un objeto del pool.
        var ignit = spawnPool.GetObjectFromPool();
        ignit.transform.position = transform.position;
        lastSpawned = ignit.gameObject;
        ignit.ResetCurrentLifeTime();
    }
}
