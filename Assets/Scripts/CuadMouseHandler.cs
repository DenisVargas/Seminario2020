using UnityEngine;

public class CuadMouseHandler : MonoBehaviour
{
    int finished = 0;

    private void Awake()
    {
        foreach (var item in GetComponentsInChildren<MouseAnimHandler>())
            item.OnAnimationEnded += MarkAsCompleted;
    }

    void MarkAsCompleted()
    {
        finished++;
        if (finished == 4)
            Destroy(gameObject);
    }
}
