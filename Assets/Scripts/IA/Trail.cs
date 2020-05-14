using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.ObjectPools;

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

    Pool<PooleableComponent> spawnPool = new Pool<PooleableComponent>(true);
    List<IgnitableObject> UpdateList = new List<IgnitableObject>();
    List<IgnitableObject> RemoveList = new List<IgnitableObject>();
    GameObject lastSpawned = null;


    private void Awake()
    {
        spawnPool = new Pool<PooleableComponent>(true);
        Func<PooleableComponent> IgniteObjectFactoryMethod = () =>
        {
            var instance = Instantiate(_ignitionSystemPrefab, transform.position, Quaternion.identity, _fireTrailParent);

            IgnitableObject ignit = instance.GetComponent<IgnitableObject>();
            ignit.pool = spawnPool;
            ignit.MaxLifeTime = _max_nonActiveLifeTime;
            ignit.BurningTime = _max_ActiveLifeTime;
            ignit.ExplansionDelayTime = _expandDelayTimePerNode;

            ignit.registerInUpdateList_Callback += () => { UpdateList.Add(ignit); };
            ignit.removeFromUpdateList_Callback += () => { RemoveList.Add(ignit); };

            return ignit;
        };
        spawnPool.Populate(30, IgniteObjectFactoryMethod);
    }

    private void Update()
    {
        if (Emit)
        {
            if (lastSpawned == null)
            {
                spawnPool.IsDinamic = true;
                SpawnObjectFromPool();
                _fireTrailParent.GetChild(0).GetComponent<IgnitableObject>().Enable();
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
        foreach (var item in UpdateList)
        {
            item.Dispose();
        }
    }

    void SpawnObjectFromPool()
    {
        //Obtengo un objeto del pool.
        var poolSystem = spawnPool.GetObject();
        //Reposiciono el objeto en el mundo.
        poolSystem.gameObject.transform.position = transform.position;
        //Actualizo la ultima "particula" spawneada.
        lastSpawned = poolSystem.gameObject;

        // Obtengo su componente Igniteable Object
        var igniteable = poolSystem.GetComponent<IgnitableObject>();
        // Reseteo sus tiempos de vida.
        igniteable.ResetCurrentLifeTime();
    }
}
