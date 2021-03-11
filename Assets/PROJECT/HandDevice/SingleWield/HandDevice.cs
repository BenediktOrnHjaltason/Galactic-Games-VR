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

    protected int layer_Structures = 10;
    protected int layer_UI = 5;

    //Restrict ray-casting through walls
    protected int layer_GeneralBlock = 16;

    //Buttons depending on HandSide

    protected OVRInput.Button indexTrigger;
    protected OVRInput.Axis1D handTrigger;
    protected OVRInput.Button structurePush;
    protected OVRInput.Button structurePull;
    protected OVRInput.Axis2D thumbStick;

    public void Initialize(EHandSide handSide)
    {
        if (handSide == EHandSide.RIGHT)
        {
            indexTrigger = OVRInput.Button.SecondaryIndexTrigger;
            handTrigger = OVRInput.Axis1D.SecondaryHandTrigger;
            structurePull = OVRInput.Button.One;
            structurePush = OVRInput.Button.Two;
            thumbStick = OVRInput.Axis2D.SecondaryThumbstick;
        }

        else if (handSide == EHandSide.LEFT)
        {
            indexTrigger = OVRInput.Button.PrimaryIndexTrigger;
            handTrigger = OVRInput.Axis1D.PrimaryHandTrigger;
            structurePull = OVRInput.Button.Three;
            structurePush = OVRInput.Button.Four;
            thumbStick = OVRInput.Axis2D.PrimaryThumbstick;
        }
    }


    public virtual void Using(ref HandDeviceData data)
    {

    }

    public virtual void Equip(EHandSide hand)
    {

    }

    /// <summary>
    /// Validate if allowed to manupulate structure
    /// </summary>

    protected virtual bool ValidateStructureState(GameObject target)
    {
        return false;
    }

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

                owner.OperationState = EHandDeviceState.IDLE;

                structureSync.OnBreakControl -= ReleaseStructureFromControl;
            }
        }

        else if (OperationState == EHandDeviceState.CONTROLLING)
        {
            structureSync.AvailableToManipulate = true;
            structureRtt.maintainOwnershipWhileSleeping = false;

            OperationState = EHandDeviceState.IDLE;
        }
    }



    GameObject buttonObjectPointedAtPreviously = null;
    InteractButton button;

    bool buttonHasBeenPushed = false;

    //Called only during Raytracing of buttons
    public void HandleUIButtons(GameObject buttonPointedAt)
    {
        if (buttonPointedAt != buttonObjectPointedAtPreviously)
        {

            buttonObjectPointedAtPreviously = buttonPointedAt;

            button = buttonObjectPointedAtPreviously.GetComponent<InteractButton>();
        }

        if (button) button.BeingHighlighted = true;

        buttonHasBeenPushed = false;
    }

    public virtual void Update()
    {
        if (button && !buttonHasBeenPushed && OVRInput.GetUp(indexTrigger))
        {
            button.Execute();
            buttonHasBeenPushed = true;
        }
    }
}
