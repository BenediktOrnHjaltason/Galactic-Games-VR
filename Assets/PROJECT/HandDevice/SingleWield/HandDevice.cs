using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;


public abstract class HandDevice : MonoBehaviour
{

    [SerializeField]
    protected HandDeviceUIData UIData;

    /// <summary>
    /// The component that holds operation state (SEARCHING, CONTROLLING, etc), and handles network syncing of beam for this device
    /// </summary>
    protected OmniDeviceSync deviceSync;
    public OmniDeviceSync DeviceSync { get => deviceSync; set => deviceSync = value; }


    //Variables related to structure that is the target for this device
    protected RaycastHit structureHit;
    protected GameObject targetStructure;
    protected StructureSync structureSync;
    public StructureSync StructureSync { get => structureSync; }

    public HandDeviceUIData GetUIData() { return UIData; }

    protected Rigidbody RB;

    public Rigidbody GetRB() { return RB; }



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
        }
    }
}
