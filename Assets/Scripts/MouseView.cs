using System.Collections.Generic;
using UnityEngine;
using Utility.ObjectPools.Generic;

public class MouseView : MonoBehaviour
{
    [SerializeField] GameObject Prefab_CommandTarget = null;
    [SerializeField] GameObject Prefab_MousePosition = null;

    GenericPool<GameObject> _MousePositionViews = null;
    public List<GameObject> _spawnedPositions = new List<GameObject>();

    private void Awake()
    {
        _MousePositionViews = new GenericPool<GameObject>
        (
            10,
            () =>
            {
                var spawned = Instantiate(Prefab_CommandTarget, transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
                spawned.SetActive(false);
                return spawned;
            },
            (poolObject) => { poolObject.SetActive(true); _spawnedPositions.Add(poolObject); },
            (poolObject) => { poolObject.SetActive(false); _spawnedPositions.Remove(poolObject); },
            true
        );
    }

    public void SetMousePositionAditive(Vector3 position)
    {
        var posPreview = GameObject.Instantiate(Prefab_MousePosition, position, Quaternion.identity);
        var animators = posPreview.GetComponentsInChildren<Animator>();
        if (animators != null)
            foreach (var anim in animators)
                anim.SetTrigger("Position");

        var mPos = _MousePositionViews.GetObjectFromPool();
        if (mPos != null)
            mPos.transform.position = position;
    }

    public void SetMousePosition(Vector3 position)
    {
        int spawnedCount = _spawnedPositions.Count;
        for (int i = 0; i < spawnedCount; i++)
            _MousePositionViews.DisablePoolObject(_spawnedPositions[0]);

        SetMousePositionAditive(position);
    }
}
