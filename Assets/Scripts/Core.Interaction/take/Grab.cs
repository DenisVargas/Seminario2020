using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Interaction;

[RequireComponent(typeof(InteractionHandler))]
public class Grab : MonoBehaviour , IInteractionComponent
{
    public OperationType OperationType => OperationType.Take;
    public Vector3 LookToDirection => transform.position;
    Vector3 firstPosition;

    public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
    {
        return transform.position;
    }
    public void ExecuteOperation(params object[] optionalParams)
    {
        print("Paso algo relevante para la trama");
        Transform manito = (Transform)optionalParams[0];
        transform.SetParent(manito);
        transform.localPosition = Vector3.zero;
    }
    public void InputConfirmed(params object[] optionalParams)
    {
        
    }
    public void CancelOperation(params object[] optionalParams)
    {

    }
    public void Throwed(Transform target)
    {
        //uso para tirar
        transform.SetParent(null);
        firstPosition = transform.position;
        StartCoroutine(ParabolicMove(target));


    }
    IEnumerator ParabolicMove(Transform target)
    {
        for (float i = 0; i < 1; i+=0.1f)
        {
            yield return new WaitForSeconds(0.01f);
            //transform.position = Vector3.Slerp(firstPosition, target.transform.position, i);
            transform.position = new Vector3(Mathf.Lerp(firstPosition.x, target.transform.position.x, i), Mathf.Lerp(firstPosition.y, target.position.y, i) + Mathf.Sin(i * Mathf.PI) *5, Mathf.Lerp(firstPosition.z, target.transform.position.z, i));
           
        }
       
    }
}
