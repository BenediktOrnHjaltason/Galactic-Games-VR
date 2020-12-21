using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


namespace Types
{
    public enum EControlBeamMode
    {
        IDLE,
        SCANNING,
        CONTROLLING
    }

    enum PlatformMoveGlobal
    {
        X_Positive,
        X_Negative,
        Y_Positive,
        Y_Negative,
        Z_Positive,
        Z_Negative
    }

    enum SpinDirection
    {
        CLOCKWISE,
        COUNTERCLOCKWISE
    }

    [Serializable]
    public struct HandDeviceUIData
    {
        public Material material;
        public Vector3 fullScale;
    }

}
