using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


namespace Types
{
    /// <summary>
    /// The operational stages of using a HandDevice (IDLE, SCANNING, CONTROLLING)
    /// </summary>
    public enum EHandDeviceState
    {
        IDLE,
        SCANNING,
        CONTROLLING
    }

    /// <summary>
    /// The differents modes of the OmniDevice (GRAVITYFORCE, REPLICATOR etc)
    /// </summary>
    public enum EOmniDeviceMode
    {
        NONE,
        GRAVITYFORCE,
        REPLICATOR
    }
    //The different modes of the OmniDevice

    enum EPlatformMoveGlobal
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

    enum EZipLine
    {
        START,
        END
    }

    [Serializable]
    public struct HandDeviceUIData
    {
        public Material material;
        public Vector3 fullScale;
    }
}
