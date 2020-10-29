using UnityEngine;

public class TrowManagement : MonoBehaviour
{
    public AnimationCurve curva;
    /// <summary>
    /// Aplica una velocidad a un objeto dado una posición objetivo.
    /// </summary>
    /// <param name="obj">El objeto que se lanzará. Debe tener un componente RigidBody</param>
    /// <param name="from">La posición inicial en donde se realizará el tiro.</param>
    /// <param name="to">La posición objetivo a donde el objeto debería caer.</param>
    /// <param name="Time">El tiempo que debe tardar el objeto en llegar al objetivo.</param>
    public void ThrowObject( Rigidbody obj, Vector3 from, Vector3 to, float Time = 1f)
    {
        Vector3 velocity = Vector3.zero;
        float gravity = Physics.gravity.y;

        velocity = GetInitialVelocity(from, to, Time);
        try
        {
            obj.velocity = velocity;
        }
        catch (MissingComponentException)
        {
            Debug.LogWarning("Este item no tiene seteado un rigidbody!");
            throw;
        }
    }

    private Vector3 GetInitialVelocity(Vector3 origin, Vector3 end, float t)
    {
        //Definimos la distancia x e y primero.
        Vector3 Delta = end - origin;
        Vector3 HorizontalDelta = Delta; //Le removemos el componente y al delta horizontal.
        HorizontalDelta.y = 0f;

        //Calculamos x.
        float Vxz = HorizontalDelta.magnitude / t;
        float Vy = HorizontalDelta.y / t + 0.5f * Mathf.Abs(Physics.gravity.y) * t;

        Vector3 result = HorizontalDelta.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }
}
