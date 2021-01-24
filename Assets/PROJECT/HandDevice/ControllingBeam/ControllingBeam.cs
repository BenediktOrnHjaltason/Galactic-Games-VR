using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class ControllingBeam : MonoBehaviour
{

    LineRenderer line;

    Vector3 structurePosition;

    public Vector3 StructurePosition { set => structurePosition = value; }

    [SerializeField]
    Material searchingMaterial;

    [SerializeField]
    Material controllingMaterial;

    Vector3 controlForce;
    public Vector3 ControlForce { set => controlForce = value; }

    // Start is called before the first frame update
    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void UpdateLines(EHandDeviceState mode)
    {
        switch (mode)
        {
            case EHandDeviceState.IDLE:

                line.startWidth = line.endWidth = 0.0f;

                line.SetPosition(0, transform.position);
                line.SetPosition(1, transform.position);
                line.SetPosition(2, transform.position);
                break;

            case EHandDeviceState.SCANNING:

                line.startWidth = line.endWidth = 0.01f;

                line.SetPosition(0, transform.position);
                line.SetPosition(1, transform.position);
                line.SetPosition(2, transform.position + transform.forward * 1000);
                break;

            case EHandDeviceState.CONTROLLING:

                line.startWidth = line.endWidth = 0.024f;

                line.SetPosition(0, transform.position);
                line.SetPosition(1, structurePosition + controlForce);
                line.SetPosition(2, structurePosition);
                break;
        }
    }

    public void SetControlForce(Vector3 force)
    {
        controlForce = force;
    }

    public void SetVisuals(EHandDeviceState mode)
    {
        switch(mode)
        {
            case EHandDeviceState.SCANNING:
                line.material = searchingMaterial;
                break;

            case EHandDeviceState.CONTROLLING:
                line.material = controllingMaterial;
                break;
        }
    }
}
