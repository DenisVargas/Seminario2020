using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.ObjectPools;

public class MouseView : MonoBehaviour
{
    [SerializeField] GameObject Prefab_CommandTarget;
    [SerializeField] GameObject Prefab_MousePosition;

    Pool<PooleableComponent> _positionViews;
    Pool<PooleableComponent> _MousePosViews;

    public List<GameObject> positions = new List<GameObject>();


    private void Awake()
    {
        _positionViews = new Pool<PooleableComponent>(true);
        _positionViews.Populate(10,
                        () =>
                        {
                            var Target = Instantiate(Prefab_CommandTarget, transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
                            var comp = Target.GetComponent<PooleableComponent>();
                            comp.pool = _positionViews;

                            return comp;
                        });

        _MousePosViews = new Pool<PooleableComponent>(false);
        _MousePosViews.Populate(5, () =>
        {
            var mPos = Instantiate(Prefab_MousePosition, Vector3.zero, Quaternion.identity);
            var poolObj = mPos.GetComponent<PooleableComponent>();
            poolObj.pool = _MousePosViews;
            return poolObj;
        });
    }

    public void SetMousePosition(Vector3 position)
    {
        var Target = _positionViews.GetObject().gameObject;
        Target.transform.position = position;

        var mPos = _MousePosViews.GetObject().gameObject;
        mPos.transform.position = position;

        var anims = mPos.GetComponentsInChildren<Animator>();
        foreach (var anim in anims)
        {
            anim.SetTrigger("Position");
        }

        positions.Add(Target);
    }

    public void ClearView()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            //Destroy(positions[i]);
            positions[i].GetComponent<PooleableComponent>().Dispose();
        }
        positions.Clear();
    }
}
