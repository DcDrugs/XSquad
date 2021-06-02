using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapCamera : MonoBehaviour
{
    public HexGrid grid;
    static HexMapCamera instance;

    public Transform swivel { get; set; }
    public Transform stick { get; set; } 
    public float moveMinSpeedZoom = 400, moveMaxSpeedZoom = 100;

    float zoom = 1f;
    public float stickMinZoom = -250, stickMaxZoom = -45;

    public float rotationSpeed = 180;
    float rotationAngle = 0; 

    public float swivelMinZoom = 90, swivelMaxZoom = 45;

    public static HexMapCamera Instance()
    {
        return instance;
    }

    public static Transform GetTransform
    {
        get
        {
            return instance.transform.GetChild(0).GetChild(0).GetChild(0);
        }
    }

    public static bool Locked
    {
        get
        {
            return !instance.enabled;
        }
        set
        {
            instance.enabled = !value;
        }
    }

    public static void ValidatePosition()
    {
        if(instance)
            instance.AdjustPosition(0f, 0f);
    }

    private void Awake()
    {
        instance = this;
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
        ValidatePosition();
    }

    private void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if(zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if(rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f)
            AdjustPosition(xDelta, zDelta);
    }

    protected void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);


        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    protected void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation 
            * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveMinSpeedZoom, moveMaxSpeedZoom, zoom) * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * damping * distance;
        transform.localPosition = ClampPosition(position);
    }

    protected void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if( rotationAngle > 360f)
            rotationAngle -= 360f;

        if (rotationAngle < 0f)
            rotationAngle += 360f;

        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        Vector3 result = position;

        float xMax = (grid.cellCountX - 0.5f) * (2f * HexSettings.innerRadius);
        result.x = Mathf.Clamp(result.x, 0f, xMax);

        float zMax = (grid.cellCountZ - 1) * (1.5f * HexSettings.outerRadius);
        result.z = Mathf.Clamp(result.z, 0f, zMax);
        return result;
    }
}
