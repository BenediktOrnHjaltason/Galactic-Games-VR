﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class ZipLinePoint_Dynamic : ZipLinePoint
{

    RealtimeTransform rtt;

    // Start is called before the first frame update
    public override void Start()
    {
        staticZipLinePoint = false;

        base.Start();
        rtt = GetComponent<RealtimeTransform>();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (otherPoint)
        transform.root.rotation = Quaternion.LookRotation(selfToOther);
    }
}
