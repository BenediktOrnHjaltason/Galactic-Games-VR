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

    enum ERotationForceAxis
    {
        PLAYER,
        SELF
    }

    [Serializable]
    public struct HandDeviceUIData
    {
        public Material material;
        public Vector3 fullScale;
    }

    public struct HandDeviceData
    {
        public bool controllingStructure;
        public bool targetStructureAllowsRotation;
    }
}
