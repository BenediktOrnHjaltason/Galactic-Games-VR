using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Types;
using System;

public class HandDeviceSync : RealtimeComponent<HandDeviceSync_Model>
{
    [SerializeField]
    Material searchingMaterial;

    [SerializeField]
    Material controllingMaterial;


    MeshRenderer deviceMesh;

    protected bool baseClassExtended = false;
    

    ControllingBeam beam;

    EHandDeviceState operationState;
    public EHandDeviceState OperationState { set => model.operationState = value; get => operationState; }

    protected virtual void Awake()
    {
        deviceMesh = transform.GetChild(10).GetComponent<MeshRenderer>();
        beam = GetComponentInChildren<ControllingBeam>();
    }


    protected override void OnRealtimeModelReplaced(HandDeviceSync_Model previousModel, HandDeviceSync_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.deviceModeDidChange -= DeviceModeDidChange;
            previousModel.operationStateDidChange -= OperationStateDidChange;
            previousModel.controlledStructurePositionDidChange -= ControlledStructurePositionDidChange;
            previousModel.controlForceDidChange -= ControlForceDidChange;
            previousModel.visibleDidChange -= VisibleDidChange;

        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                currentModel.deviceMode = EOmniDeviceMode.GRAVITYFORCE;
                currentModel.operationState = EHandDeviceState.IDLE;
                currentModel.controlForce = Vector3.zero;
                currentModel.visible = false;
            }

            // Update data to match the new model
            UpdateDeviceMode();
            UpdateOperationState();
            UpdateVisible();

            //Register for events so we'll know if data changes later
            currentModel.deviceModeDidChange += DeviceModeDidChange;
            currentModel.operationStateDidChange += OperationStateDidChange;
            currentModel.controlledStructurePositionDidChange += ControlledStructurePositionDidChange;
            currentModel.controlForceDidChange += ControlForceDidChange;
            currentModel.visibleDidChange += VisibleDidChange;
        }
    }

    void DeviceModeDidChange(HandDeviceSync_Model model, EOmniDeviceMode mode)
    {
        UpdateDeviceMode();
    }

    void UpdateDeviceMode()
    {
        //Will be implemented when I bring Replicator into OmniDeviceSync
        //Not necessary to sync since device operation happens locally anyway?
    }
    //-----------------

    void OperationStateDidChange(HandDeviceSync_Model model, EHandDeviceState state)
    {
        UpdateOperationState();
    }

    void UpdateOperationState()
    {
        operationState = model.operationState;

        SetVisuals(operationState);
        beam.SetVisuals(operationState);
    }
    //-----------------


    public Vector3 StructurePosition { set => model.controlledStructurePosition = value; }

    void ControlledStructurePositionDidChange(HandDeviceSync_Model model, Vector3 position)
    {
        UpdateControlledStructurePosition();
    }

    void UpdateControlledStructurePosition()
    {
        beam.StructurePosition = model.controlledStructurePosition;
    }
    //-----------------

    public Vector3 ControlForce { set => model.controlForce = value; }

    void ControlForceDidChange(HandDeviceSync_Model model, Vector3 force)
    {
        UpdateControlForce();
    }

    void UpdateControlForce()
    {
        beam.ControlForce = model.controlForce;
    }
    //-----------------

    public bool Visible { set => model.visible = value; }

    void VisibleDidChange(HandDeviceSync_Model model, bool visible)
    {
        UpdateVisible();
    }

    protected bool visible = false;

    protected virtual void UpdateVisible()
    {
        //Preventing object from getting value from data store when extended 
        //(OmniDeviceSync fetches it to set its animations)
        if (!baseClassExtended) visible = model.visible;

        if (!deviceMesh) deviceMesh = GetComponent<MeshRenderer>();

        if (visible) deviceMesh.enabled = true;
        else deviceMesh.enabled = false;
    }

    void SetVisuals(EHandDeviceState mode)
    {
        switch (mode)
        {
            case EHandDeviceState.IDLE:
                deviceMesh.material = searchingMaterial;
                break;

            case EHandDeviceState.SCANNING:
                deviceMesh.material = searchingMaterial;
                break;

            case EHandDeviceState.CONTROLLING:
                deviceMesh.material = controllingMaterial;
                break;
        }
    }

    public virtual void FixedUpdate()
    {

    }

    private void Update()
    {
        beam.UpdateLines(operationState);
    }
}
