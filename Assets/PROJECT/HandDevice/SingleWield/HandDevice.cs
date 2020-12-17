using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;


public abstract class HandDevice : MonoBehaviour
{

    [SerializeField]
    protected UIData UIData;

    

    public abstract bool Using();

    public UIData GetUIData()
    {
        return UIData;
    }


}
