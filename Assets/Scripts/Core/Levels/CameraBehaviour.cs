using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] Transform RayCaster;
    [SerializeField] LayerMask groundMask = ~0;

    [SerializeField] public Transform _target = null;
    [SerializeField] public bool freeCamera = true;
    [SerializeField] public bool locked = false;

    //[SerializeField] float zoomVelocity = 10f;
    [SerializeField] float mousePanBorderThickness = 10f;
    //[SerializeField] Vector3 navigationLimits = new Vector3(10f, 0f,10f);
    [SerializeField] CuadLimiter limits = null;

    //Velocidades
    [SerializeField] float panSpeed = 20f;
    [SerializeField] float rotationSpeed = 20f;

    bool isLerping = false;
    float targetYComponent = 0f;
    float lastValidYComponent = 0f;
    float smoothTreshold = 0.2f;
    float smoothTime = 1f;

    //Zoom.
    //Vector3 zoomMin, zoomMax;
    //float zoomSpeed;
    //float zoomDist;

    #region DEBUG
#if UNITY_EDITOR
    [Header("Debugging")]
    public GameObject pointDetection;
    public GameObject LastPosition;
    public bool debug_CameraGroundDetection = false;

    private void OnDrawGizmos()
    {
        if (debug_CameraGroundDetection)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(RayCaster.position + new Vector3(0, 12, 0), Vector3.down * 100);
            var result = CheckFloor(RayCaster.position + new Vector3(0, 12, 0), Vector3.down);
            if (result.isValid)
            {
                pointDetection.transform.position = result.Point;
                pointDetection.GetComponent<MeshRenderer>().sharedMaterial.color = Color.green;
            }
            else
            {
                pointDetection.GetComponent<MeshRenderer>().sharedMaterial.color = Color.red;
            }
        }
    }  
#endif
    #endregion

    void Awake()
    {
        //ZoomMax = new Vector3(operativeCamera.transform.localPosition.x, operativeCamera.transform.localPosition.y, operativeCamera.transform.localPosition.z);
        //var BNormal = operativeCamera.transform.forward;
        //ZoomMin = ZoomMax + BNormal * ZoomDist;

        if (RayCaster == null)
            RayCaster = Camera.main.transform;
        if (_target == null)
        {
            var Player = FindObjectOfType<Controller>();
            if (Player != null)
            {
                _target = Player.transform;
                transform.position = _target.transform.position;
            }
            else
                freeCamera = true;
        }
        else
            transform.position = _target.transform.position;

        //Inicializamos la variable lastValidYComponent
        var CameraInitialPositioning = CheckFloor(_target.transform.position + new Vector3(0, 15, 0), Vector3.down);
        if (CameraInitialPositioning.isValid)
        {
            transform.position = CameraInitialPositioning.Point;
            lastValidYComponent = CameraInitialPositioning.Point.y;
        }
        else lastValidYComponent = _target.transform.position.y; //Fallback.

        limits = FindObjectOfType<CuadLimiter>();

        var inspectionMenu = FindObjectOfType<InspectionMenu>();
        if (inspectionMenu)
            inspectionMenu.OnSetInspection += OnInspection;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
            locked = true;
        if (Input.GetKeyUp(KeyCode.Space))
            locked = false;
        if (Input.GetKeyDown(KeyCode.I))
            freeCamera = false;
        if (Input.GetKeyDown(KeyCode.I))
            freeCamera = true;
        #region Movimiento

        if (freeCamera && !locked)
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
            transform.position = limits.ClampToLimits(transform.position + inputPos);
        }
        else if(_target != null)
            transform.position = limits.ClampToLimits(_target.position);
        AdjustY();
        #endregion

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

    /// <summary>
    /// Centra la cámara en una posición específica.
    /// </summary>
    /// <param name="targetPosition"></param>
    public void CenterCameraToTarget(Vector3 targetPosition)
    {
        transform.position = limits.ClampToLimits(targetPosition);
    }

    struct SnapPosition
    {
        public bool isValid;
        public Vector3 Point;
    }
    SnapPosition CheckFloor(Vector3 from, Vector3 dir)
    {
        var snapping = new SnapPosition() { isValid = false };

        Ray ray = new Ray(from, dir);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100, groundMask);

        float maxY = float.MinValue;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.CompareTag("NavigationFloor") && hits[i].point.y > maxY)
            {
                snapping.isValid = true;
                snapping.Point = hits[i].point;
                maxY = hits[i].point.y;
            }
        }

        return snapping;
    }

    public void AdjustY()
    {
        //print("Ajusto la Y");
        var res = CheckFloor(RayCaster.transform.position + new Vector3(0, 12, 0), Vector3.down);
        //print($"Es valido: {{{res.isValid}}} y es diferente al ultimo registro: {{{transform.position.y == res.Point.y}}}");
        if (res.isValid == false)
        {
            transform.position = transform.position.YComponent(lastValidYComponent);
            return;
        }
        if (transform.position.y == res.Point.y) return; //cuando no hay diferencia.
        //print($"El delta es {Mathf.Abs(transform.position.y - res.Point.y)}");
        //print($"Esta lerpeando actualmente?: {{{isLerping}}}");
        transform.position = transform.position.YComponent(res.Point.y);
        lastValidYComponent = res.Point.y;
    }

    /// <summary>
    /// Callback que se llamará cuando el panel de Inspección se abre.
    /// Nos permite controlar que va a pasar con la cámara!
    /// </summary>
    /// <param name="inspectionEnabled">Estado actual del panel de inspección.</param>
    void OnInspection(bool inspectionEnabled)
    {
        if (_target != null)
            freeCamera = inspectionEnabled;
    }
}
