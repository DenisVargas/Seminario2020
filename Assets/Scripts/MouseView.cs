using System.Collections.Generic;
using UnityEngine;
using Utility.ObjectPools;

public class MouseView : MonoBehaviour
{
    [SerializeField] GameObject Prefab_CommandTarget = null;
    [SerializeField] GameObject Prefab_MousePosition = null;

    Pool<PooleableComponent> _positionViews;
    Pool<PooleableComponent> _MousePosViews;

    public List<PooleableComponent> positions = new List<PooleableComponent>();

    private void Awake()
    {
        _positionViews = new Pool<PooleableComponent>(false);
        _positionViews.Populate(2,
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

    public void SetMousePositionAditive(Vector3 position)
    {
        var Target = _positionViews.GetObject();
        if (Target != null)
        {
            Target.transform.position = position;
            positions.Add(Target);
        }

        var mPos = _MousePosViews.GetObject();
        if (mPos != null)
        {
            //Si es nulo que hago?
            mPos.transform.position = position;

            var anims = mPos.GetComponentsInChildren<Animator>();
            foreach (var anim in anims)
            {
                anim.SetTrigger("Position");
            }
        }
    }

    public void SetMousePosition(Vector3 position)
    {
        for (int i = 0; i < positions.Count; i++)
            positions[i].Dispose();
        positions.Clear();

        SetMousePositionAditive(position);
    }
}
