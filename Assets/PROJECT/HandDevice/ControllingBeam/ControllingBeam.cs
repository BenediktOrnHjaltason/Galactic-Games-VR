using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class ControllingBeam : MonoBehaviour
{

    LineRenderer line;

    Transform structureTransform;

    Material inactiveMaterial;
    Material activeMaterial;

    Vector3 controlForce;

    // Start is called before the first frame update
    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void SetLines(EControlBeamMode mode)
    {
        switch (mode)
        {
            case EControlBeamMode.IDLE:

                line.startWidth = line.endWidth = 0.0f;

                line.SetPosition(0, transform.position);
                line.SetPosition(1, transform.position);
                line.SetPosition(2, transform.position);
                break;

            case EControlBeamMode.SCANNING:

                line.startWidth = line.endWidth = 0.01f;

                line.SetPosition(0, transform.position);
                line.SetPosition(1, transform.position);
                line.SetPosition(2, transform.position + transform.forward * 1000);
                break;

            case EControlBeamMode.CONTROLLING:

                line.startWidth = line.endWidth = 0.024f;

                line.SetPosition(0, transform.position);
                line.SetPosition(1, structureTransform.position + controlForce);
                line.SetPosition(2, structureTransform.position);
                break;
        }
    }

    public void SetStructureTransform(Transform transform)
    {
        this.structureTransform = transform;
    }

    public void SetControlForce(Vector3 force)
    {
        controlForce = force;
    }

    public void SetMaterialReferences(Material activeMaterial, Material inactiveMaterial)
    {
        this.activeMaterial = activeMaterial;
        this.inactiveMaterial = inactiveMaterial;
    }

    public void SetVisuals(EControlBeamMode mode)
    {
        switch(mode)
        {
            case EControlBeamMode.SCANNING:
                line.material = inactiveMaterial;
                break;

            case EControlBeamMode.CONTROLLING:
                line.material = activeMaterial;
                break;
        }
    }
}
