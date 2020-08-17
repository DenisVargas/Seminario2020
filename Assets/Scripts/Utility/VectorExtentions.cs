using UnityEngine;

public static class VectorExtentions
{
    public static Vector3 RandomVector3(this Vector3 Vector, int min = 0, int max = 2)
    {
        Vector = new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
        return Vector;
    }
    public static Vector3 RandomVector3(this Vector3 Vector, float min = 0f, float max = 1f)
    {
        Vector = new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
        return Vector;
    }
    public static Vector3 getScaled(this Vector3 vector, Vector3 scale)
    {
        return new Vector3(vector.x * scale.x, vector.y * scale.y, vector.z * scale.z);
    }
    public static Vector3 DirTo(this Vector3 origin, Vector3 targetPosition)
    {
        return (targetPosition - origin).normalized;
    }
    public static Vector3 YComponent(this Vector3 vector, float value)
    {
        return new Vector3(vector.x, value, vector.z);
    }
    public static Vector3 XComponent(this Vector3 vector, float value)
    {
        return new Vector3(value, vector.y, vector.z);
    }
    public static Vector3 ZComponent(this Vector3 vector, float value)
    {
        return new Vector3(vector.x, vector.y, value);
    }
}
