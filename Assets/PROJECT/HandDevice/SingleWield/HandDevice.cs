using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;


public abstract class HandDevice : MonoBehaviour
{

    [SerializeField]
    protected HandDeviceUIData UIData;

    /// <summary>
    /// The component that  handles network syncing of visuals, and holds deviceoperation state (SEARCHING, CONTROLLING, etc) for this device
    /// </summary>
    protected HandDeviceSync deviceSync;
    public HandDeviceSync DeviceSync { get => deviceSync; set => deviceSync = value; }

    public EHandDeviceState OperationState { set => deviceSync.OperationState = value; get => deviceSync.OperationState; }


    //Variables related to structure that is the target for this device
    protected RaycastHit structureHit;
    protected GameObject targetStructure;

    /// <summary>
    /// RealtimeTransform of target structure
    /// </summary>
    protected RealtimeTransform structureRtt;
    protected RealtimeView structureRtw;

    protected StructureSync structureSync;
    public StructureSync StructureSync { get => structureSync; }

    public HandDeviceUIData GetUIData() { return UIData; }

    protected Rigidbody RB;

    public Rigidbody GetRB() { return RB; }


    /// <summary>
    /// Reference to owner if one exists (e.g. OmniDevice)
    /// </summary>
    protected HandDevice owner;
    public HandDevice Owner { set => owner = value; get => owner; }

    public abstract bool Using();

    public abstract void Equip(EHandSide hand);

    /// <summary>
    /// Validate if allowed to manupulate structure
    /// </summary>

    protected abstract bool ValidateStructureState(GameObject target);

    protected void GetStateReferencesFromTarget(GameObject target)
    {
        //If it's the same as last time, we still have the references

        if (targetStructure != target)
        {
            targetStructure = target;

            structureSync = target.GetComponent<StructureSync>();
            structureRtt = targetStructure.GetComponent<RealtimeTransform>();
        }
    }

    public void ReleaseStructureFromControl()
    {
        if (owner)
        {
            if (owner.OperationState == EHandDeviceState.CONTROLLING)
            {
                structureSync.AvailableToManipulate = true;
                structureRtt.maintainOwnershipWhileSleeping = false;
                structureRtw.preventOwnershipTakeover = false;
            }
        }

        else if (OperationState == EHandDeviceState.CONTROLLING)
        {
            structureSync.AvailableToManipulate = true;
            structureRtt.maintainOwnershipWhileSleeping = false;
            structureRtw.preventOwnershipTakeover = false;
        }
    }
}
