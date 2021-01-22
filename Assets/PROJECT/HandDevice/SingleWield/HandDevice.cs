using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;


public abstract class HandDevice : MonoBehaviour
{

    [SerializeField]
    protected HandDeviceUIData UIData;

    //Variables related to structure that is the target for this device
    protected RaycastHit structureHit;
    protected GameObject targetStructure;
    protected StructureSync structureSync;

    public HandDeviceUIData GetUIData() { return UIData; }

    protected Rigidbody RB;

    public Rigidbody GetRB() { return RB; }



    public abstract bool Using();

    public abstract void Equip(EHandSide hand);

    protected abstract bool ValidateRelevantState(GameObject target);

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
