using UnityEngine;
using UnityEngine.Events;

public class WallRotating_EListener : MonoBehaviour
{
    public bool EmitEvents = false;
    public UnityEvent ActivationStart;
    public UnityEvent ActivationEnd;

    void OnActivationStart()
    {
        if(EmitEvents)
            ActivationStart.Invoke();
    }
    void OnActivationEnd()
    {
        if (EmitEvents)
            ActivationEnd.Invoke();
    }
}
