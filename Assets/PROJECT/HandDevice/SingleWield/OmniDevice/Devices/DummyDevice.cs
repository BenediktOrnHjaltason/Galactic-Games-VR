using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class DummyDevice : HandDevice
{
    public override void Equip(EHandSide hand)
    {
        //Dummy
    }

    protected override bool ValidateStructureState(GameObject target)
    {
        //Dummy
        return true;
    }

    public override void Using(ref HandDeviceData data)
    {

    }
}
