using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    public bool Emit;
    GameObject ignitionSystemPrefab;
    ParticleSystem _trail;
    ParticleSystem _IgniteFire;
    float spawnDistance;
    float maxLifeTime;
    List<ignitionSystem> spawnedSystems;

    [System.Serializable]
    public struct ignitionSystem
    {
        public float remainingLifeTime;
        public GameObject spawnedObject;
    }

    private void Update()
    {
        if (Emit)
        {
            if (spawnedSystems.Count == 0)
            {
                SpawnNewSystem();
            }
            if (Vector3.Distance(transform.position, spawnedSystems[0].spawnedObject.transform.position) > spawnDistance)
            {
                SpawnNewSystem();
            } 
        }

        for (int i = 0; i < spawnedSystems.Count; i++)
        {
            var system = spawnedSystems[i];
            system.remainingLifeTime -= Time.deltaTime;
        }
    }

    public void SpawnNewSystem()
    {
        var system = new ignitionSystem();
        var element = Instantiate(ignitionSystemPrefab, transform);
        system.spawnedObject = element;
        system.remainingLifeTime = maxLifeTime;
        spawnedSystems.Insert(0, system);
    }
}
