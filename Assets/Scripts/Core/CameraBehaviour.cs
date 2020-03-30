using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    Transform OperativeCamera;

    [SerializeField] public Transform Target = null;
    [SerializeField] public bool freeCamera = true;

    //[SerializeField] float zoomVelocity = 10f;
    [SerializeField] float mousePanBorderThickness = 10f;
    [SerializeField] Vector2 panLimits = new Vector2(10f,10f);

    //Velocidades
    [SerializeField] float panSpeed = 20f;
    [SerializeField] float rotationSpeed = 20f;

    //Zoom.
    //Vector3 zoomMin, zoomMax;
    //float zoomSpeed;
    //float zoomDist;

    void Awake()
    {
        //ZoomMax = new Vector3(operativeCamera.transform.localPosition.x, operativeCamera.transform.localPosition.y, operativeCamera.transform.localPosition.z);
        //var BNormal = operativeCamera.transform.forward;
        //ZoomMin = ZoomMax + BNormal * ZoomDist;
        OperativeCamera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
            freeCamera = false;
        if (Input.GetKeyUp(KeyCode.Space))
            freeCamera = true;

        #region Movimiento

        if (freeCamera)
        {
            // Input en Y
            if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - mousePanBorderThickness)
                transform.position += transform.forward * panSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= mousePanBorderThickness)
                transform.position += -transform.forward * panSpeed * Time.deltaTime;

            // Input en X
            if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - mousePanBorderThickness)
                transform.position += transform.right * panSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= mousePanBorderThickness)
                transform.position -= transform.right * panSpeed * Time.deltaTime;
            //La z es mi valor forward, mientras que mi x es mi right.
            var xpos = transform.position.x;
            xpos = Mathf.Clamp(xpos, -panLimits.x, panLimits.x);
            var zPos = transform.position.z;
            zPos = Mathf.Clamp(zPos, -panLimits.y, panLimits.y);

            transform.position = new Vector3(xpos, transform.position.y, zPos);
        }
        else
        {
            //if (MainGameControl.SelectedObjects.Count > 0)
            //    targetLock = MainGameControl.returnSelectedObjectsRelativePosition(); //Obtengo la posicion relativa de los objetos seleccionados.
            //else if (MainGameControl.LastObjectSelected)
            //    targetLock = MainGameControl.LastObjectSelected.transform.position;

            var xpos = Target.position.x;
            xpos = Mathf.Clamp(xpos, -panLimits.x, panLimits.x);
            var zPos = Target.position.z;
            zPos = Mathf.Clamp(zPos, -panLimits.y, panLimits.y);
            transform.position = new Vector3(xpos, transform.position.y, zPos);
        }
        #endregion

        #region Rotacion
        if (Input.GetKey("e"))
        {
            //Usamos quaterniones, pero traducidos con eulerAngles, de otro modo es imposible trabajar las rotaciones con vectores tridimencionales.
            Quaternion A = transform.rotation; //Valor por defecto.
            Quaternion B = transform.rotation; //Valor que será modificado(necesario para tener los valores orig).
            B = Quaternion.Euler(B.eulerAngles.x, B.eulerAngles.y + rotationSpeed, B.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(A, B, rotationSpeed * Time.deltaTime);

        }

        if (Input.GetKey("q"))
        {
            Quaternion A = transform.rotation;
            Quaternion B = transform.rotation;
            B = Quaternion.Euler(B.eulerAngles.x, B.eulerAngles.y - rotationSpeed, B.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(A, B, rotationSpeed * Time.deltaTime);
        }
        #endregion

        /*
        #region Zoom

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0)
        {
            operativeCamera.transform.localPosition = Vector3.Lerp(operativeCamera.transform.localPosition, ZoomMin, (zoomSpeed * 100f * Time.deltaTime) / 100f);
        }
        else if (scroll < 0)
        {
            operativeCamera.transform.localPosition = Vector3.Lerp(operativeCamera.transform.localPosition, ZoomMax, (zoomSpeed * 100f * Time.deltaTime) / 100f);
        }

        #endregion
        */
    }
}
