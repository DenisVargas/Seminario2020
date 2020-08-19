using UnityEngine;

namespace IA.LineOfSight
{
    public class LineOfSightComponent : MonoBehaviour
    {
        public LayerMask visibles = ~0;
        public float range = 1f;
        public float angle = 45f;
        public bool UseCustomOrientation = false;
        public Transform SightSocket = null;

        Vector3 orientation = Vector3.zero;

        #region DEBUG
#if UNITY_EDITOR
        [Space, Header("Debug")]
        [SerializeField] bool Debug_LineOfSight = false; 
        [SerializeField] Color rangeColor = Color.white;
        [SerializeField] Color angleColor = Color.white;

        void OnDrawGizmosSelected()
        {
            if (Debug_LineOfSight)
            {
                Vector3 normalizedOrientation = SightSocket.forward.YComponent(0).normalized;
                orientation = UseCustomOrientation && SightSocket != null ? normalizedOrientation : transform.forward;
                Vector3 origin = transform.position;

                //Rango
                Gizmos.color = rangeColor;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(origin, range);

                //Ángulo
                Gizmos.color = angleColor;
                Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, angle + 1, 0) * orientation * range);
                Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, -angle - 1, 0) * orientation * range);
            }
        }
#endif
#endregion

        /// <summary>
        /// El vector resultante de la resta de ambas posiciones: B - A.
        /// </summary>
        [ HideInInspector]
        public Vector3 positionDiference = Vector3.zero;
        /// <summary>
        /// Dirección normalizada hacia el objetivo.
        /// </summary>
        [ HideInInspector]
        public Vector3 dirToTarget = Vector3.zero;
        /// <summary>
        /// Último ángulo calculado entre la posición de origen y el objetivo.
        /// </summary>
        [HideInInspector]
        public float angleToTarget = 0;
        /// <summary>
        /// Última distancia calculada entre la posición de origen y el objetivo.
        /// </summary>
        [HideInInspector]
        public float distanceToTarget = 0;

        /// <summary>
        /// Indica si el objetivo específicado está dentro de la línea de visión
        /// </summary>
        /// <param name="target">Objetivo a comprobar</param>
        /// <returns>Verdadero si el Objetivo específicado está dentro de la línea de visión</returns>
        public bool IsInSight(Transform target)
        {
            if (target == null)
            {
                Debug.Log("El target es inválido");
                return false;
            }

            if (UseCustomOrientation && SightSocket != null)
                orientation = SightSocket.forward.YComponent(0).normalized;
            else
                orientation = transform.forward;

            positionDiference = (target.position - transform.position);
            distanceToTarget = positionDiference.magnitude;
            Vector3 BidimensionalProjection = positionDiference.YComponent(0);
            angleToTarget = Vector3.Angle(orientation, BidimensionalProjection);
            dirToTarget = positionDiference.normalized;

            dirToTarget = positionDiference.normalized;

            if (distanceToTarget > range || angleToTarget > angle) return false;

            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, dirToTarget, out hitInfo, range + 1, visibles))
                return hitInfo.transform == target;

            return false;
        }
    }
}

