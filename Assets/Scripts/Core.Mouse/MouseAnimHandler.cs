using System;
using UnityEngine;

public class MouseAnimHandler : MonoBehaviour
{
    public event Action OnAnimationEnded = delegate { };
    public Action<GameObject> OnDisableCommand = delegate { };
    public float maxDistanceTillDesapear = 1f;

    Controller _player;

    bool _enabled = false;
    public bool Enable
    {
        get => _enabled;
        set => _enabled = value;
    }

    public MouseAnimHandler SetPlayerReference(Controller player)
    {
        _player = player;
        return this;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistanceTillDesapear);
    }

    void _AV_AnimationFinalized()
    {
        OnAnimationEnded();
    }

    private void Update()
    {
        if (_enabled && _player != null)
        {
            var distance = Vector3.Distance(transform.position, _player.transform.position);
            if(distance < maxDistanceTillDesapear)
            {
                _enabled = false;
                OnDisableCommand(gameObject);
            }
        }
    }
}
