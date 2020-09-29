using UnityEngine;
using Core.SaveSystem;

public class CheckPointTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("CheckPointAdded");
            //Me aseguro que el checkpoint solo se añada si el jugador esta vivo en el momento del checkPoint.
            var controller = other.GetComponent<Controller>();
            if (controller.IsAlive)
                Level.SetCheckPoint();
        }
    }
}
