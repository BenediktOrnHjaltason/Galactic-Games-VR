using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RealtimeModel]
public class AvatarSync_Model
{
    [RealtimeProperty(1, true, true)]
    Vector3 _omniBeamPosition0;

    [RealtimeProperty(2, true, true)]
    Vector3 _omniBeamPosition1;

    [RealtimeProperty(3, true, true)]
    Vector3 _omniBeamPosition2;

    // 0 for Searching, 1 for Controlling
    [RealtimeProperty(4, true, true)]
    int _beamMode;

}
