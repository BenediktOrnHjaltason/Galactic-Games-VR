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


    MeshRenderer mesh;
    ControllingBeam beam;

    EHandDeviceState operationState;
    public EHandDeviceState OperationState { set => model.operationState = value; get => operationState; }

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
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

        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                currentModel.deviceMode = EOmniDeviceMode.GRAVITYFORCE;
                currentModel.operationState = EHandDeviceState.IDLE;
                currentModel.controlForce = Vector3.zero;
            }

            // Update data to match the new model
            UpdateDeviceMode();
            UpdateOperationState();

            //Register for events so we'll know if data changes later
            currentModel.deviceModeDidChange += DeviceModeDidChange;
            currentModel.operationStateDidChange += OperationStateDidChange;
            currentModel.controlledStructurePositionDidChange += ControlledStructurePositionDidChange;
            currentModel.controlForceDidChange += ControlForceDidChange;
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

        SetMeshState(operationState);
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

    void SetMeshState(EHandDeviceState mode)
    {
        switch (mode)
        {
            case EHandDeviceState.IDLE:
                mesh.material = searchingMaterial;
                break;

            case EHandDeviceState.SCANNING:
                mesh.material = searchingMaterial;
                break;

            case EHandDeviceState.CONTROLLING:
                mesh.material = controllingMaterial;
                break;
        }
    }

    private void Update()
    {
        beam.UpdateLines(operationState);
    }
}
