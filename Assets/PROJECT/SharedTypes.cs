using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


namespace Types
{
    public enum EMobility
    {
        DYNAMIC,
        STATIC
    }


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

    public enum EZipLinePoint
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

    public enum EDysonSphereBeam
    {
        SCANNING,
        ALLIGNEDWITHSTAR
    }
}
