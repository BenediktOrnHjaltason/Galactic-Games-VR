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

public class KeycardPort : RealtimeComponent<KeycardPort_Model>
{
    [SerializeField]
    MeshRenderer statusIndicator;

    [SerializeField]
    Material m_statusIdle;

    [SerializeField]
    Material m_StatusOccupied;

    [SerializeField]
    Vector3 snapPosition;

    [SerializeField]
    Vector3 snapRotation;


    public event Action<KeycardPort, EKeycardAction> OnKeycardAction;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(10))
        {
            if (statusIndicator.material != m_StatusOccupied) statusIndicator.material = m_StatusOccupied;

            SnapKeyCard(other);


            RealtimeTransform rt = other.GetComponentInParent<RealtimeTransform>();

            if (rt && rt.ownerIDSelf == rt.realtime.clientID)
                OnKeycardAction?.Invoke(this, EKeycardAction.INSERT);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(10))
        {
            if (statusIndicator.material != m_statusIdle) statusIndicator.material = m_statusIdle;

            UnSnapKeyCard(other);

            RealtimeTransform rt = other.GetComponentInParent<RealtimeTransform>();

            if (rt && rt.ownerIDSelf == rt.realtime.clientID)
                OnKeycardAction?.Invoke(this, EKeycardAction.REMOVE);
        }
    }

    void SnapKeyCard(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponentInParent<Rigidbody>();
        StructureSync ss = other.gameObject.GetComponentInParent<StructureSync>();

        if (rb && ss)
        {
            rb.transform.localPosition = snapPosition;
            rb.transform.localRotation = Quaternion.Euler(snapRotation);

            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
                 RigidbodyConstraints.FreezeRotation;

            ss.AllowRotationForces = false;
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

    //-------------Networking

    protected override void OnRealtimeModelReplaced(KeycardPort_Model previousModel, KeycardPort_Model currentModel)
    {

        if (previousModel != null)
        {
            // Unregister from events
            previousModel.occupiedDidChange -= OccupiedDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current availability value.
            if (currentModel.isFreshModel)
            {
                currentModel.occupied = portOccupied;
            }


            // Update data to match the new model
            UpdateOccupied();

            // Register for events so we'll know if data changes later
            currentModel.occupiedDidChange += OccupiedDidChange;
        }
    }

    bool portOccupied = false;
    public bool Occupied { get => portOccupied; set => model.occupied = value; }

    void OccupiedDidChange(KeycardPort_Model model, bool occupied)
    {
        UpdateOccupied();
    }

    void UpdateOccupied()
    {
        portOccupied = model.occupied;
    }
}
