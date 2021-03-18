using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Normal.Realtime;


public enum EKeycardPortSide
{
    LEFT,
    RIGHT
}

public class Puzzle_Keycards_Port : MonoBehaviour
{

    [SerializeField]
    EKeycardPortSide side;

    Realtime realTime;

    [SerializeField]
    MeshRenderer statusIndicator;

    [SerializeField]
    Material m_statusIdle;

    [SerializeField]
    Material m_StatusOccupied;


    public event Action<EKeycardPortSide, EKeycardAction, int> OnKeycardAction;

    private void Awake()
    {
        realTime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(10))
        {
            if (statusIndicator.material != m_StatusOccupied) statusIndicator.material = m_StatusOccupied;

            SnapKeyCard(other);

            OnKeycardAction?.Invoke(side, EKeycardAction.INSERT, realTime.clientID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (statusIndicator.material != m_statusIdle) statusIndicator.material = m_statusIdle;

        UnSnapKeyCard(other);

        if (other.gameObject.layer.Equals(10)) OnKeycardAction?.Invoke(side, EKeycardAction.REMOVE, realTime.clientID);
    }

    void SnapKeyCard(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponentInParent<Rigidbody>();
        StructureSync ss = other.gameObject.GetComponentInParent<StructureSync>();

        if (rb && ss)
        {
            if (side == EKeycardPortSide.LEFT)
            {
                rb.transform.localPosition = new Vector3(6.614f, -1.51f, 1.678f);
                rb.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 90.0f, 0.0f));

                rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                     RigidbodyConstraints.FreezeRotation;

                ss.AllowRotationForces = false;
            }
            else
            {
                rb.transform.localPosition = new Vector3(-6.094f, -1.455f, 1.664f);
                rb.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);

                rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                     RigidbodyConstraints.FreezeRotation;

                ss.AllowRotationForces = false;
            }
        }
    }

    void UnSnapKeyCard(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponentInParent<Rigidbody>();
        StructureSync ss = other.gameObject.GetComponentInParent<StructureSync>();

        if (rb && ss)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            ss.AllowRotationForces = true;
        }
    }
}
