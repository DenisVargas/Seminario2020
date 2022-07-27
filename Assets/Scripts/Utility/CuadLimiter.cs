using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuadLimiter : MonoBehaviour
{
    public Vector2 size = Vector2.one;
    float _rightLimit = float.MaxValue;
    float _frontLimit = float.MaxValue;

    private void Awake()
    {
        _rightLimit = size.x / 2;
        _frontLimit = size.y / 2;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float rightLimit = size.x / 2;
        float frontLimit = size.y / 2;

        float maxLimitX = transform.position.x + rightLimit;
        float minLimitX = transform.position.x - rightLimit;
        float maxLimitY = transform.position.z + frontLimit;
        float minLimitY = transform.position.z - frontLimit;

        Gizmos.DrawLine(new Vector3(maxLimitX, 0, maxLimitY), new Vector3(minLimitX, 0, maxLimitY));
        Gizmos.DrawLine(new Vector3(maxLimitX, 0, minLimitY), new Vector3(minLimitX, 0, minLimitY));

        Gizmos.DrawLine(new Vector3(maxLimitX, 0, minLimitY), new Vector3(maxLimitX, 0, maxLimitY));
        Gizmos.DrawLine(new Vector3(minLimitX, 0, minLimitY), new Vector3(minLimitX, 0, maxLimitY));
    }

    public Vector3 ClampToLimits(Vector3 desiredPosition)
    {
        //Calculo los limites y luego si el desiredposition se excede, clampear los valores.
        float maxLimitX = transform.position.x + _rightLimit;
        float minLimitX = transform.position.x - _rightLimit;
        float maxLimitZ = transform.position.z + _frontLimit;
        float minLimitZ = transform.position.z - _frontLimit;

        if (desiredPosition.x <= maxLimitX && desiredPosition.x >= minLimitX
            && desiredPosition.z <= maxLimitZ && desiredPosition.z >= minLimitZ)
            return desiredPosition;
        else
            return new Vector3(Mathf.Clamp(desiredPosition.x, minLimitX, maxLimitX), desiredPosition.y, Mathf.Clamp(desiredPosition.z, minLimitZ, maxLimitZ));
    }
}
