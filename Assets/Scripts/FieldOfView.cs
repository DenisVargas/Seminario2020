using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;
    [Range(0,360)] public float ViewAngle;

    public LayerMask targetMask;
    public LayerMask ObstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    public float MeshResolution;

    private void Start()
    {
        StartCoroutine("FindVisibleTargetsWithDelay", 0.2f);
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
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    public Vector3 DirFromAngleInDegree(float Angle, bool isGlobal)
    {
        if (!isGlobal) Angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(Angle * Mathf.Deg2Rad), 0, Mathf.Cos(Angle * Mathf.Deg2Rad));
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(ViewAngle * MeshResolution);
        float stepAngleSize = ViewAngle / stepCount;
        List<Vector3> viewpoints = new List<Vector3>();

        for (int i = 0; i < stepCount; i++)
        {
            float angle = transform.eulerAngles.y - ViewAngle / 2 + stepAngleSize * i;
            Debug.DrawLine(transform.position, transform.position + DirFromAngleInDegree(angle, true) * viewRadius, Color.yellow);
            //viewpoints.Add(viewCast(angle).point);
            //https://youtu.be/73Dc5JTCmKI?t=594
        }
    }

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    // Update is called once per frame
    void Update()
    {
        DrawFieldOfView();
    }

    IEnumerator FindVisibleTargetsWithDelay(float Delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(Delay);
            FindVisibleTargets();
        }
    }

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

    ViewCastInfo viewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngleInDegree(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, ObstacleMask))
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        else
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
    }
}
