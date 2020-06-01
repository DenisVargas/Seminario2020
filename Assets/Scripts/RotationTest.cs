using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTest : MonoBehaviour
{
    [SerializeField, Range(0, 1)] float interpolation = 0;
    [SerializeField, Range(0, 180)] float angle = 0;
    [SerializeField] Vector3 addRotation = Vector3.zero;

    [SerializeField, Range(0, 180)] float minRotationA = 0;
    [SerializeField, Range(0, 180)] float maxRotationA = 0;
    [SerializeField, Range(0, 180)] float minRotationB = 0;
    [SerializeField, Range(0, 180)] float maxRotationB = 0;

    Vector3 forward = Vector3.zero;
    Vector3 negativeForward = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        forward = transform.forward;
        negativeForward = -transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        //Quaternion toAddRot = Quaternion.Euler(transform.rotation.eulerAngles + addRotation);
        //transform.rotation = toAddRot;

        //var inter = Vector3.Slerp(forward, negativeForward, interpolation);
        //angle = Vector3.Angle(forward, inter);

        if (Input.GetKeyDown(KeyCode.H))
        {
            var range1 = Random.Range(minRotationA, maxRotationA);
            var range2 = Random.Range(minRotationB, maxRotationB);
            var finalRotation = Random.Range(range1, range2);

            print($"El resultado final es un rango que va de ({range1},{range2}, resultado final es {finalRotation})");

            float interPolationValue = ((float)finalRotation) / 180;
            int dir = Random.Range(-1f, 1f) <= 0 ? 1 : -1;
            print($"Direction is {dir}");

            var inter = Vector3.Slerp(transform.forward, -transform.forward, interPolationValue);

            interpolation = interPolationValue;


            transform.forward = inter * dir;

        }
    }
}
