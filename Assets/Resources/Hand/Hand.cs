using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EHandSide
{
    LEFT = 0,
    RIGHT = 1
}

public class Hand : MonoBehaviour
{

    enum EHandsGrabbing
    {
        ONE,
        BOTH,
        NONE
    } static EHandsGrabbing eHandsGrabbing = EHandsGrabbing.NONE;

    [SerializeField]
    EHandSide eHandSide;

    [SerializeField]
    GameObject rootParent;

    [SerializeField]
    GameObject controllerAnchor;

    OVRPlayerController playerController;

    static int numHandsGrabbing = 0;

    int layer_GrabHandle = 8;

    Vector3 DefaultLocalPosition = new Vector3( 0, 0, 0 );

    //We need reference to handle because we need handle position every update to place hand correctly on moving handle
    GameObject handle;
    Vector3 offsettToHandleOnGrab;

    float handleRadius;
    float leeWay = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
        playerController = rootParent.GetComponent<OVRPlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        //Keeping hand meshes attached to handle
        if (handle /*&& (controllerAnchor.transform.position - handle.transform.position).magnitude < handleRadius + leeWay*/)
        {
            transform.position = handle.transform.position + offsettToHandleOnGrab;
        }
        else if (transform.position != DefaultLocalPosition) transform.localPosition = DefaultLocalPosition;

        //If hands 
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer.Equals(layer_GrabHandle))    
            detectGrab(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(layer_GrabHandle) && handle)
            release();
    }

    void detectGrab(Collider other)
    {
        switch (eHandSide)
        {
            case EHandSide.LEFT:

                if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
                {
                    grab(other.gameObject);
                }

                else if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger))
                    release();

                break;

            case EHandSide.RIGHT:

                if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
                    grab(other.gameObject);

                else if (OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger))
                    release();
                    
                break;
        }
    }

    void grab(GameObject handleRef)
    {
        handle = handleRef;
        offsettToHandleOnGrab = transform.position - handle.transform.position;

        //NOTE! Radius will only be correct if transform has uniform scaling in x,y,z
        handleRadius = handleRef.gameObject.GetComponent<SphereCollider>().radius * handle.transform.localScale.x;

        playerController.RegisterHandGrabEvent(true, (int)eHandSide, handle.transform);
    }

    void release()
    {
        handle = null;

        playerController.RegisterHandGrabEvent(false, (int)eHandSide);
    }
}
