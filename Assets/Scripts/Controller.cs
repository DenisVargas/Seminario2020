using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float moveSpeed = 6;
    public Transform MouseDebug;

    Rigidbody rb;
    Camera viewCamera;
    Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        viewCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 wMousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
        MouseDebug.position = wMousePos;

        transform.LookAt(wMousePos + Vector3.up * transform.position.y);
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * moveSpeed;
    }

    private void FixedUpdate()
    {
        //transform.forward = transform.position.DirTo(wMousePos).YComponent(0);
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
}

public static class vectorExtentions
{
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
