using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;


public abstract class HandDevice : MonoBehaviour
{

    [SerializeField]
    protected HandDeviceUIData UIData;

    public HandDeviceUIData GetUIData() { return UIData; }

    protected Rigidbody RB;

    public Rigidbody GetRB() { return RB; }



    public abstract bool Using();

    public abstract void Equip(EHandSide hand);

    


}
