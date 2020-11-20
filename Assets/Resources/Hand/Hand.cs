using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EHandSide{ LEFT = 0, RIGHT = 1 }


public class Hand : MonoBehaviour
{

    [SerializeField]
    EHandSide eHandSide;

    [SerializeField]
    GameObject rootParent;

    [SerializeField]
    GameObject controllerAnchor;

    OVRPlayerController playerController; //Handles movement of Avatar when grabbing

    int layer_GrabHandle = 8;

    Vector3 DefaultLocalPosition = new Vector3( 0, 0, 0 );

    //We need reference to handle because we need handle position every update to place hand correctly on moving handle
    GameObject handle;
    Vector3 offsettToHandleOnGrab;


    static Hand leftHand;
    static Hand rightHand;

    //Gravity controll (Only used by right hand)
    GravityController gravityController;
    bool usingGravityController = false;


    // Start is called before the first frame update
    void Start()
    {
        playerController = rootParent.GetComponent<OVRPlayerController>();

        if (eHandSide == EHandSide.LEFT) leftHand = this;

        else if (eHandSide == EHandSide.RIGHT)
        {
            rightHand = this;
            gravityController = GetComponentInChildren<GravityController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Keeping hand mesh attached to handle while grabbing
        if (handle) transform.position = handle.transform.position + offsettToHandleOnGrab;

        else if (transform.position != DefaultLocalPosition) transform.localPosition = DefaultLocalPosition;

        if (eHandSide == EHandSide.RIGHT) usingGravityController = gravityController.Operating();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer.Equals(layer_GrabHandle))    
            detectGrab(other);
    }

    void detectGrab(Collider other)
    {
        switch (eHandSide)
        {
            case EHandSide.LEFT:

                if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
                {
                    Debug.Log("LEFT trigger pressed");

                    grab(other.gameObject);
                    rightHand.release();
                }

                else if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger))
                    release();

                break;

            case EHandSide.RIGHT:

                if (usingGravityController) return;

                if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
                {
                    Debug.Log("RIGHT trigger pressed");

                    grab(other.gameObject);
                    leftHand.release();
                }
                    
                else if (OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger))
                    release();
                    
                break;
        }
    }

    void grab(GameObject handleRef)
    {
        handle = handleRef;
        offsettToHandleOnGrab = transform.position - handle.transform.position;

        playerController.RegisterGrabEvent(true, (int)eHandSide, handle.transform);
        playerController.RegisterGrabEvent(false, (eHandSide == EHandSide.LEFT) ? (int)EHandSide.RIGHT : (int)EHandSide.LEFT);
    }

    void release()
    {
        handle = null;

        playerController.RegisterGrabEvent(false, (int)eHandSide);
    }
}
