using System.Collections.Generic;
using UnityEngine;
using Utility.ObjectPools.Generic;

public class MouseView : MonoBehaviour
{
    [SerializeField] GameObject Prefab_CommandTarget = null;
    [SerializeField] GameObject Prefab_MousePosition = null;

    GenericPool<GameObject> _MousePositionViews = null;
    public List<GameObject> _spawnedPositions = new List<GameObject>();

    Controller _player;

    private void Awake()
    {
        _player = FindObjectOfType<Controller>();
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
        GameObject posPreview = GameObject.Instantiate(Prefab_MousePosition, position, Quaternion.identity);
        Animator[] animators = posPreview.GetComponentsInChildren<Animator>();
        if (animators != null)
            foreach (Animator anim in animators)
                anim.SetTrigger("Position");

        GameObject mPos = _MousePositionViews.GetObjectFromPool();
        if (mPos != null)
        {
            mPos.transform.position = position;
            MouseAnimHandler handler = mPos.GetComponent<MouseAnimHandler>().SetPlayerReference(_player);
            handler.OnDisableCommand = DisableMouseDisplay;
            handler.Enable = true;
        }
    }

    public void SetMousePosition(Vector3 position)
    {
        int spawnedCount = _spawnedPositions.Count;
        for (int i = 0; i < spawnedCount; i++)
            _MousePositionViews.DisablePoolObject(_spawnedPositions[0]);

        SetMousePositionAditive(position);
    }

    public void DisableMouseDisplay(GameObject display)
    {
        _MousePositionViews.DisablePoolObject(display);
    }
}
