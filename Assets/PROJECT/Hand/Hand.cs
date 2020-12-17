using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;


public enum EHandSide{LEFT, RIGHT}


public class Hand : MonoBehaviour
{

    [SerializeField]
    EHandSide eHandSide;

    [SerializeField]
    GameObject rootParent;

    [SerializeField]
    GameObject controllerAnchor;

    [SerializeField]
    Material defaultColor;

    [SerializeField]
    Material grabbingColor;

    MeshRenderer mesh;

    OVRPlayerController playerController; //Handles movement of Avatar when grabbing

    int layer_GrabHandle = 8;

    Vector3 DefaultLocalPosition = new Vector3( 0, 0, 0 );

    //We need reference to handle because we need handle position
    //every update to place hand correctly on moving handles
    GameObject handle;
    Vector3 offsettToHandleOnGrab;

    static Hand leftHand;
    static Hand rightHand;

    Hand otherHand;

    OVRInput.Button grabButton;

    bool shouldGrab = false;
    bool shouldRelease = false;

    //(Gravity controller default for right arm when not holding other device)
    HandDevice handDevice = null;
    bool usingHandDevice = false;

    /// <summary>
    /// UI screen that shows details about the held device
    /// </summary>
    UIHandDevice deviceUI;


    // Start is called before the first frame update
    void Awake()
    {
        playerController = rootParent.GetComponent<OVRPlayerController>();
        mesh = GetComponent<MeshRenderer>();
        deviceUI = GetComponentInChildren<UIHandDevice>();

        if (eHandSide == EHandSide.LEFT)
        {
            leftHand = this;
            grabButton = OVRInput.Button.PrimaryHandTrigger;
        }

        if (eHandSide == EHandSide.RIGHT)
        {
            rightHand = this;
            handDevice = GetComponentInChildren<GravityController>();
            grabButton = OVRInput.Button.SecondaryHandTrigger;

            deviceUI.Set(handDevice.GetUIData());
        }
    }

    private void Start()
    {
        otherHand = (eHandSide == EHandSide.LEFT) ? rightHand : leftHand;
    }

    // Update is called once per frame
    void Update()
    {
        //Keeping hand mesh attached to handle while grabbing
        if (handle) transform.position = handle.transform.position + offsettToHandleOnGrab;

        else if (transform.position != DefaultLocalPosition) transform.localPosition = DefaultLocalPosition;

        //Operate info screen for handheld device 
        if (handDevice)
        {
            deviceUI.Operate(eHandSide);

            usingHandDevice = handDevice.Using();
            playerController.EnableRotation = !usingHandDevice;
        }

        if (OVRInput.GetDown(grabButton))
        {
            shouldGrab = true;
            shouldRelease = false;
        }
        else if (OVRInput.GetUp(grabButton))
        {
            shouldGrab = false;
            shouldRelease = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer.Equals(layer_GrabHandle))
        {

            if (shouldRelease) 
            {
                shouldRelease = false;
                release(); 
            }

            if (shouldGrab)
            {
                shouldGrab = false;

                if (eHandSide == EHandSide.RIGHT && usingHandDevice) return;

                grab(other.gameObject);
                otherHand.release();
            }
        }
    }

    void grab(GameObject handleRef)
    {
        handle = handleRef;
        offsettToHandleOnGrab = transform.position - handle.transform.position;
        mesh.material = grabbingColor;

        playerController.RegisterGrabEvent(false, (int)otherHand.eHandSide);
        playerController.RegisterGrabEvent(true, (int)eHandSide, handle.transform);
    }

    void release()
    {
        handle = null;
        mesh.material = defaultColor;
        playerController.RegisterGrabEvent(false, (int)eHandSide);
    }
}
