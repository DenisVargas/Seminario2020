using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    Transform OperativeCamera;
    [SerializeField] LayerMask groundMask = ~0;

    [SerializeField] public Transform Target = null;
    [SerializeField] public bool freeCamera = true;

    //[SerializeField] float zoomVelocity = 10f;
    [SerializeField] float mousePanBorderThickness = 10f;
    [SerializeField] Vector3 navigationLimits = new Vector3(10f, 0f,10f);

    //Velocidades
    [SerializeField] float panSpeed = 20f;
    [SerializeField] float rotationSpeed = 20f;

    float LastYComponent = 0f;

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
        var Player = FindObjectOfType<NMA_Controller>();
        if (Player != null)
        {
            Target = Player.transform;
            transform.position = Target.transform.position;
        }
        else
            freeCamera = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
            freeCamera = false;
        if (Input.GetKeyUp(KeyCode.Space))
            freeCamera = true;

        //Vector3 position = transform.position;
        //position.y = Target.transform.position.y;
        //transform.position = position;

        #region Movimiento

        if (freeCamera)
        {
            Vector3 inputPos = Vector3.zero;

            // Input en Y
            if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - mousePanBorderThickness)
                inputPos += transform.forward;
            if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= mousePanBorderThickness)
                inputPos += -transform.forward;

            // Input en X
            if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - mousePanBorderThickness)
                inputPos += transform.right;
            if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= mousePanBorderThickness)
                inputPos += -transform.right;

            inputPos *= panSpeed * Time.deltaTime;
            transform.position = ClampToPanLimits(transform.position + inputPos, navigationLimits);
        }
        else
        {
            //if (MainGameControl.SelectedObjects.Count > 0)
            //    targetLock = MainGameControl.returnSelectedObjectsRelativePosition(); //Obtengo la posicion relativa de los objetos seleccionados.
            //else if (MainGameControl.LastObjectSelected)
            //    targetLock = MainGameControl.LastObjectSelected.transform.position;

            transform.position = ClampToPanLimits(Target.position, navigationLimits);
        }
        #endregion

        AdjustY();

        #region Rotacion
        if (Input.GetKey(KeyCode.E))
        {
            //Usamos quaterniones, pero traducidos con eulerAngles, de otro modo es imposible trabajar las rotaciones con vectores tridimencionales.
            Quaternion A = transform.rotation; //Valor por defecto.
            Quaternion B = transform.rotation; //Valor que será modificado(necesario para tener los valores orig).
            B = Quaternion.Euler(B.eulerAngles.x, B.eulerAngles.y + rotationSpeed, B.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(A, B, rotationSpeed * Time.deltaTime);

        }

        if (Input.GetKey(KeyCode.Q))
        {
            Quaternion A = transform.rotation;
            Quaternion B = transform.rotation;
            B = Quaternion.Euler(B.eulerAngles.x, B.eulerAngles.y - rotationSpeed, B.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(A, B, rotationSpeed * Time.deltaTime);
        }
        #endregion

        #region Zoom

        /*
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0)
        {
            operativeCamera.transform.localPosition = Vector3.Lerp(operativeCamera.transform.localPosition, ZoomMin, (zoomSpeed * 100f * Time.deltaTime) / 100f);
        }
        else if (scroll < 0)
        {
            operativeCamera.transform.localPosition = Vector3.Lerp(operativeCamera.transform.localPosition, ZoomMax, (zoomSpeed * 100f * Time.deltaTime) / 100f);
        }
        */

        #endregion
    }

    private Vector3 ClampToPanLimits(Vector3 position, Vector3 panLimits)
    {
        //La z es mi valor forward, mientras que mi x es mi right.
        return new Vector3(Mathf.Clamp(position.x, -panLimits.x, panLimits.x),
                           Mathf.Clamp(position.y, -panLimits.y, panLimits.y),
                           Mathf.Clamp(position.z, -panLimits.z, panLimits.z));
    }


    /// <summary>
    /// Centra la cámara en una posición específica.
    /// </summary>
    /// <param name="targetPosition"></param>
    public void CenterCameraToTarget(Vector3 targetPosition)
    {
        transform.position = ClampToPanLimits(targetPosition, navigationLimits);
    }

    public void AdjustY()
    {
        Ray ray = new Ray(OperativeCamera.position, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, Camera.main.farClipPlane, groundMask);
        bool navigationFloorFinded = false;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.CompareTag("NavigationFloor"))
            {
                navigationFloorFinded = true;
                transform.position = transform.position.YComponent(hits[i].point.y);
                LastYComponent = hits[i].point.y;
                break;
            }
        }

        if (!navigationFloorFinded)
        {
            transform.position = transform.position.YComponent(LastYComponent);
        }
    }
}
