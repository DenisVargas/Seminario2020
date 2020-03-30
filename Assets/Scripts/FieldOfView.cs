using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    //======================================= State =====================================================
    #region State

    [Header("Field of view Radius and Angle.")]
    public float viewRadius;
    [Range(0, 360)]
    public float ViewAngle;

    [Header("Visibility Masks")]
    public LayerMask targetMask;
    public LayerMask ObstacleMask;
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    [Header("Visualization Paramaters")]
    public MeshFilter viewMeshFilter;
    public float MeshResolution;
    public int EdgeResolveIterations;
    public float EdgeDstTreshold;

    Mesh viewMesh;

    #endregion
    //==================================== Unity Methods ================================================

    private void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        StartCoroutine("FindVisibleTargetsWithDelay", 0.2f);
    }
    void LateUpdate()
    {
        DrawFieldOfView();
    }

    //============================ User Defined Structures ==============================================

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool hit, Vector3 point, float dst, float angle)
        {
            this.hit = hit;
            this.point = point;
            this.dst = dst;
            this.angle = angle;
        }
    }
    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }

    //================================== Public Methods =================================================

    public Vector3 DirFromAngle(float angleInDegrees, bool isGlobal)
    {
        if (!isGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 
                           0,
                           Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    //================================== Member Methods =================================================

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(ViewAngle * MeshResolution);
        float stepAngleSize = ViewAngle / stepCount;
        List<Vector3> viewpoints = new List<Vector3>();
        ViewCastInfo OldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - ViewAngle / 2 + stepAngleSize * i;
            Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * viewRadius, Color.yellow);
            ViewCastInfo newViewCast = viewCast(angle);

            if (i > 0)
            {
                bool edgeDstTresholdExceeded = Mathf.Abs(OldViewCast.dst - newViewCast.dst) > EdgeDstTreshold;
                if (OldViewCast.hit != newViewCast.hit || (OldViewCast.hit && newViewCast.hit && edgeDstTresholdExceeded))
                {
                    //Encuentro el borde.
                    EdgeInfo edge = FindEdge(OldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                        viewpoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero)
                        viewpoints.Add(edge.pointB);
                }
            }

            viewpoints.Add(newViewCast.point);
            OldViewCast = newViewCast;
        }

        //Ensamblado del mesh.
        int vertexCount = viewpoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewpoints[i]);
            if (i < vertexCount - 2 )
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }
    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < ViewAngle / 2)
            {
                float disToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, disToTarget, ObstacleMask))
                    visibleTargets.Add(target);
            }
        }
    }

    ViewCastInfo viewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, ObstacleMask))
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        else
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
    }
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < EdgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle / 2);
            ViewCastInfo newViewCast = viewCast(angle);

            bool edgeDstTresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > EdgeDstTreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstTresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    //=================================== Corrutines ====================================================

    IEnumerator FindVisibleTargetsWithDelay(float Delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(Delay);
            FindVisibleTargets();
        }
    }

    //===================================================================================================
}
