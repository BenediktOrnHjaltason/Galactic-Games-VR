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
    protected StructureLocal structureLocal;

    public HandDeviceUIData GetUIData() { return UIData; }

    protected Rigidbody RB;

    public Rigidbody GetRB() { return RB; }



    public abstract bool Using();

    public abstract void Equip(EHandSide hand);

    protected abstract bool ValidateRelevantState(GameObject target);

    protected void GetStateReferencesFromTarget(GameObject target)
    {
        //If this object is not the last one we aimed at, get it's state references, if any
        //(Keeping down use of GetComponent<> for objects that will be disregarded anyway, when contiunally beaming them)
        if (targetStructure != target)
        {
            targetStructure = target;

            structureSync = target.GetComponent<StructureSync>();
            structureLocal = target.GetComponent<StructureLocal>();
        }
    }
}
