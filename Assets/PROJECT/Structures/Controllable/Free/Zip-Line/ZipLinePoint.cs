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

                if (value == null) transportBeam.transform.localScale = Vector3.zero; 
            }
        }

        get => otherPoint;
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        if (startOrEnd == EZipLinePoint.START)
        {

            transportBeam = transform.root.Find("PF_ZipLine-BeamCube").GetComponent<ZipLineTransport>();

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
                transportBeam.transform.SetPositionAndRotation(transform.position + (selfToOther / 2), Quaternion.LookRotation(selfToOther));
                transportBeam.transform.localScale = new Vector3(0.25f, 0.25f, selfToOther.magnitude);
            }

            else transportBeam.transform.position = transform.position;
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
