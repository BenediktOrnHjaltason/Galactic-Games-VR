using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;


public enum EHandSide{LEFT, RIGHT}


public class Hand : MonoBehaviour
{

    [SerializeField]
    EHandSide handSide;

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
    int layer_HandDevice = 12;

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
    HandDevice gravityController;
    HandDevice handDevice = null;

    /// <summary>
    /// Holding non-gravity-controller-device (Only those need manually syncing with hand when holding)
    /// </summary>
    bool holdingNonGCHandDevice = false;

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

        if (handSide == EHandSide.LEFT)
        {
            leftHand = this;
            grabButton = OVRInput.Button.PrimaryHandTrigger;
        }

        if (handSide == EHandSide.RIGHT)
        {
            rightHand = this;
            handDevice = gravityController = GetComponentInChildren<GravityController>();
            grabButton = OVRInput.Button.SecondaryHandTrigger;

            deviceUI.Set(handDevice.GetUIData());
        }
    }

    private void Start()
    {
        otherHand = (handSide == EHandSide.LEFT) ? rightHand : leftHand;
    }

    // Update is called once per frame
    void Update()
    {
        //Keeping hand mesh attached to handle while grabbing
        if (handle) transform.position = handle.transform.position + offsettToHandleOnGrab;
        else if (transform.position != DefaultLocalPosition) transform.localPosition = DefaultLocalPosition;

        //Keeping device object attached to hand while holding
        if (holdingNonGCHandDevice) handDevice.transform.SetPositionAndRotation(transform.position, transform.rotation);

        //Operate handheld device 
        if (handDevice)
        {
            deviceUI.Operate(handSide);

            //(NOTE!) handDevice.Using() actually operates the device
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
            shouldRelease = true;
            shouldGrab = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (usingHandDevice) return;

        if (other.gameObject.layer.Equals(layer_GrabHandle))
        {

            if (shouldRelease) 
            {
                shouldRelease = false;

                ReleaseHandle(); 
            }

            else if (shouldGrab)
            {
                shouldGrab = false;

                GrabHandle(other.gameObject);
                otherHand.ReleaseHandle();
            }
        }

        else if (other.gameObject.layer.Equals(layer_HandDevice))
        {
            if (shouldGrab)
            {
                shouldGrab = false;
                GrabDevice(other.gameObject);
            }
            else if (shouldRelease)
            {
                shouldRelease = false;
                DropDevice();
            }
        }
    }

    void GrabHandle(GameObject handleRef)
    {
        handle = handleRef;
        offsettToHandleOnGrab = transform.position - handle.transform.position;
        mesh.material = grabbingColor;

        playerController.RegisterGrabEvent(false, (int)otherHand.handSide);
        playerController.RegisterGrabEvent(true, (int)handSide, handle.transform);
    }

    void ReleaseHandle()
    {
        handle = null;
        mesh.material = defaultColor;
        playerController.RegisterGrabEvent(false, (int)handSide);
    }

    void GrabDevice(GameObject device)
    {
        holdingNonGCHandDevice = true;

        handDevice = device.GetComponent<HandDevice>();

        deviceUI.Set(handDevice.GetUIData());

    }

    void DropDevice()
    {
        holdingNonGCHandDevice = false;

        handDevice = null;

        if (handSide == EHandSide.RIGHT)
        {
            handDevice = gravityController;
            deviceUI.Set(gravityController.GetUIData());
        }
    }
}
