using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    [SerializeField] GameObject ignitionSystemPrefab;
    [SerializeField] ParticleSystem _trail;
    [SerializeField] float spawnDistance = 1f;
    [SerializeField] float maxLifeTime = 10f;

    public bool Emit = true;
    List<ignitionSystem> spawnedSystems = new List<ignitionSystem>();
    List<int> DisposeSystem = new List<int>();

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

            if (system.remainingLifeTime <= 0)
            {
                system.spawnedObject.GetComponent<IgnitableObject>().Kill();
                DisposeSystem.Add(i);
            }
            spawnedSystems[i] = system;
        }

        if (DisposeSystem.Count > 0)
        {
            foreach (var markedIndex in DisposeSystem)
                spawnedSystems.RemoveAt(markedIndex);
            DisposeSystem.Clear();
        }
    }

    public void SpawnNewSystem()
    {
        var system = new ignitionSystem();
        var element = Instantiate(ignitionSystemPrefab, transform.position, Quaternion.identity);
        system.spawnedObject = element;
        system.remainingLifeTime = maxLifeTime;
        spawnedSystems.Insert(0, system);
    }
}
