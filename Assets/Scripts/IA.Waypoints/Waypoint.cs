using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Waypoint : MonoBehaviour
{
    public int CurrentPositionIndex = 0;
    public List<Transform> points = new List<Transform>();

    public Vector3 getNextPosition(int currentPosition)
    {
        if (currentPosition >= points.Count)
            currentPosition = 0;

        return points[currentPosition].position;
    }
    //public Vector3 getNextPosition(int from)
    //{
    //    from++;
    //    if (from >= points.Count)
    //        from = 0;
    //}

    //public void GetNextPoints()
    //{
    //var childs = transform.GetComponentsInChildren<Transform>();
    //if (childs != null && childs.Length > 0)
    //{
    //    for (int i = 1; i < childs.Length; i++)
    //    {
    //        Vector3 position = (childs[i]).position;
    //        if (!point.Contains(position))
    //            point.Add(position);
    //    }
    //}
    //else
    //{
    //    string message = childs != null ? "El resultado es nulo" : "El count es 0";
    //    Debug.LogError(message);
    //}

    #region Lerping
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return a + (b - a) * t;
    }

    public static Vector2 QuadraticCurve(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        //Esto termina como un polinomio cuadrático.
        // (1-t)2A + 2(1-t)tB + t2C
        Vector2 p0 = Lerp(a, b, t);
        Vector2 p1 = Lerp(b, c, t);
        return Lerp(p0, p1, t);
    }

    public static Vector2 CubicCurve(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        //(1-t)3A + 3(1-t)2tB + 3(1-t)t2C + t3D
        Vector2 p0 = QuadraticCurve(a, b, c, t);
        Vector2 p1 = QuadraticCurve(b, c, d, t);
        return Lerp(p0, p1, t);
    }
    #endregion
}
