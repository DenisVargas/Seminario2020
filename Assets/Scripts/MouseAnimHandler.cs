using System;
using UnityEngine;

public class MouseAnimHandler : MonoBehaviour
{
    public event Action OnAnimationEnded = delegate { };

    void _AV_AnimationFinalized()
    {
        OnAnimationEnded();
    }
}
