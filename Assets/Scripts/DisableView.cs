using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableView : PooleableComponent
{
    [SerializeField] bool DisablesInTime = false;
    [SerializeField] float DisableTime = 5f;

    public override void Enable()
    {
        gameObject.SetActive(true);
        if (DisablesInTime)
        {
            StartCoroutine(DelayedDisable());
        }
    }

    public override void Disable()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<NMA_Controller>() != null)
        {
            Dispose();
        }
    }

    IEnumerator DelayedDisable()
    {
        yield return new WaitForSeconds(DisableTime);
        Dispose();
    }
}
