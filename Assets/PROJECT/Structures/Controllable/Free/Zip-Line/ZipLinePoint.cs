using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using UnityEditor;
using Normal.Realtime;

public class ZipLinePoint : MonoBehaviour
{
    //TODO?: Split up into two scripts inheriting from base?


    /// <summary>
    /// Start or end-point?
    /// </summary>
    [SerializeField]
    EZipLinePoint startOrEnd;

    public EZipLinePoint StartOrEnd { get => startOrEnd; }


    public ZipLinePoint otherPoint;

    ZipLineTransport transportBeam;
    public ZipLineTransport TransportBeam { get => transportBeam; }

    protected Vector3 selfToOther;

    //Is turned to false if base class of dynamic point
    protected bool staticZipLinePoint = true;


    public ZipLinePoint OtherPoint 
    {
        set
        {
            otherPoint = value;

            if (startOrEnd == EZipLinePoint.START)
            {
                transportLineActive = (value == null) ? false : true;

                if (value == null) transportBeam.transform.parent.localScale = Vector3.one; 
            }
        }

        get => otherPoint;
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        if (startOrEnd == EZipLinePoint.START)
        {

            transportBeam = transform.root.Find("PF_TransportBeam").GetChild(0).GetComponent<ZipLineTransport>();

            transportBeam.OnBeamTouchesObstacle += BreakConnection;

            transportBeam.StartPointTransform = transform;

            if (otherPoint)
            {
                transportBeam.EndPointTransform = otherPoint.transform;
                transportLineActive = true;
            }
        }
    }

    public virtual void FixedUpdate()
    {
        if (otherPoint)
        {
            selfToOther = otherPoint.transform.position - transform.position;

            if (staticZipLinePoint)
                transform.rotation = Quaternion.LookRotation(selfToOther);
        }

        //Start-points handles transport line
        if (startOrEnd == EZipLinePoint.START)
        {
            if (transportLineActive)
            {
                transportBeam.transform.parent.SetPositionAndRotation(transform.position + (selfToOther / 2), Quaternion.LookRotation(selfToOther));
                transportBeam.transform.parent.localScale = new Vector3(1f, 1f, selfToOther.magnitude * 4);
            }

            else transportBeam.transform.parent.position = transform.position;
        }
    }


    //Only for Start-points
    bool transportLineActive = false;

    public void BreakConnection()
    {
        if (otherPoint)
        {
            otherPoint.OtherPoint = null;
            OtherPoint = null;

            if (startOrEnd == EZipLinePoint.START) transportLineActive = false;
        }
    }
}
