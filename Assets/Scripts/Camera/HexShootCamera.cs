using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexShootCamera : MonoBehaviour
{
    static HexShootCamera instance = null;

    Quaternion oldRotation;
    Vector3 oldPosition;

    readonly Vector3 offsetPosition = new Vector3(0f, 10f, 0f);

    public HexUnit Owner { get; set; }
    public List<HexUnit> Units { get; set; }

    List<string> percentage = new List<string>();

    int unitIndex = 0;
    public int UnitIndex
    {
        get
        {
            return unitIndex;
        }
        set
        {
            if(Units.Count > unitIndex)
                Units[unitIndex].Label.text = null;
            if (value < 0)
                value += Units.Count;
            unitIndex = value % Units.Count;
            Units[unitIndex].Label.text = percentage[unitIndex];

        }
    }

    bool canChangeUnit = false;

    public float speed = 5f;

    public bool IsFind { get; private set; }

    public bool IsWork { get; set; } = false;

    public bool NeedChangeUnits { get; set; }

    bool isReset = false;


    public static HexShootCamera Instance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }
    private void OnEnable()
    {
        EscapeManager.Instance().IsLock = true;
        oldRotation = HexMapCamera.GetTransform.rotation;
        oldPosition = HexMapCamera.GetTransform.position;
        HexGameUI.Instance().HUD.SetActive(true);
        HexGrid.Instance().gameUI.DestoyAbilities(HexGrid.Instance().gameUI.GetSelectedUnit());
        HexGrid.Instance().ClearPath();
        HexMapCamera.Locked = true;
        IsWork = true;
        IsFind = false;
        isReset = false;
        canChangeUnit = false;
        percentage.Clear();
        for (int i = 0; i < Units.Count; i++)
        {
            percentage.Add(((int)(Owner.Gun.GetHitPercentage(Units[i]) * 100)).ToString() + "%");  
        }
        UnitIndex = 0;
    }

    private void OnDisable()
    {
        if (HexGameUI.Instance())
            HexGameUI.Instance().HUD.SetActive(false);
        if (EscapeManager.Instance())
            EscapeManager.Instance().IsLock = false;
    }

    void ChangeUnit()
    {
        IsFind = Input.GetKeyDown(KeyCode.Return);
        IsWork = !Input.GetKeyDown(KeyCode.Escape);
        if (IsFind || !IsWork)
        {
            isReset = true;
            canChangeUnit = false;
            return;
        }

        float xDelta = Input.GetAxis("Horizontal");
        if (xDelta != 0f && canChangeUnit)
        {
            if (xDelta < 0)
                UnitIndex--;
            else
                UnitIndex++;
            canChangeUnit = false;
        }
    }

    public void ChangeIndex(int index)
    {
        if (canChangeUnit && gameObject.activeSelf)
        {
            UnitIndex = index;
            canChangeUnit = false;
        }
    }

    private void Update()
    {
        if (NeedChangeUnits)
            ChangeUnit();
        else
        {
            if (canChangeUnit)
            {
                isReset = true;
                canChangeUnit = false;
            }
        }


        if (!canChangeUnit)
            if (!isReset)
            {
                int max = 0;
                HexCell res = Owner.Location.GetNeighbor(HexDirection.NE);
                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                {
                    HexCell neighbor = Owner.Location.GetNeighbor(d);
                    if (neighbor == null)
                        continue;
                    int distance 
                        = neighbor.coordinates.DistanceTo(Units[UnitIndex].Location.coordinates);
                    if(max < distance)
                    {
                        max = distance;
                        res = neighbor;
                    }
                }
                if (res == null)
                    res = Owner.Location;
                AdjustPosition(res.GlobalPosition, Units[UnitIndex].Position.position);
            }
            else
            {
                AdjustPosition(oldPosition, oldRotation);
                if (Quaternion.Angle(HexMapCamera.GetTransform.rotation, oldRotation) < Quaternion.kEpsilon)
                {
                    if (HexGrid.Instance().gameUI.GetSelectedUnit() 
                            && HexGrid.Instance().gameUI.GetSelectedUnit().Action > 0)
                        HexGrid.Instance().gameUI.ShowAbilities(HexGrid.Instance().gameUI.GetSelectedUnit());
                    for (int i = 0; i < Units.Count; i++)
                    {
                        if (Units[i] != null)
                            Units[i].Label.text = null;
                    }
                    HexMapCamera.Locked = false;
                    enabled = false;
                }
            }
    }

    void AdjustPosition(Vector3 movePosition, Vector3 position)
    {
        Vector3 direction = position - HexMapCamera.GetTransform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        HexMapCamera.GetTransform.rotation 
            = Quaternion.Lerp(HexMapCamera.GetTransform.rotation, rotation, Time.deltaTime * speed);

        HexMapCamera.GetTransform.position
            = Vector3.Lerp(HexMapCamera.GetTransform.position, movePosition + offsetPosition, Time.deltaTime * speed);


        if (Quaternion.Angle(HexMapCamera.GetTransform.rotation, rotation) < Quaternion.kEpsilon)
            canChangeUnit = true;
    }

    void AdjustPosition(Vector3 oldPosition, Quaternion rotation)
    {
        HexMapCamera.GetTransform.position 
            = Vector3.Lerp(HexMapCamera.GetTransform.position, oldPosition, Time.deltaTime * speed);
        HexMapCamera.GetTransform.rotation 
            = Quaternion.Lerp(HexMapCamera.GetTransform.rotation, rotation, Time.deltaTime * speed);
    }

    public HexUnit GetTarget()
    {
        if (IsFind)
            return Units[UnitIndex];
        else
            return null;
    }
}
