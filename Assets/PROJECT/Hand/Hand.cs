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
    Material defaultColor;

    [SerializeField]
    Material grabbingColor;
    
    OVRPlayerController playerController; //Handles movement of Avatar when grabbing

    int layer_GrabHandle = 8;
    int layer_HandDevice = 12;
    int layer_ZipLineHandle = 13;

    Vector3 DefaultLocalPosition = new Vector3(0, 0, 0);

    //We need reference to handle because we need handle position
    //every update to place hand correctly on moving handles
    GameObject handle;
    Vector3 offsettToHandleOnGrab;

    Vector3 handOffsetToPlayerControllerOnZipLineGrab;

    static Hand leftHand;
    static Hand rightHand;

    Hand otherHand;

    OVRInput.Button grabButton;

    bool shouldGrab = false;
    bool shouldRelease = false;

    //(OmniDevice controller default for right arm when not holding other device)

    HandDevice handDevice;

    HandDevice omniDevice;

    public HandDevice OmniDevice { set => omniDevice = value; get => omniDevice; }


    HandSync handSync;

    public HandSync HandSync { get => handSync; }


    /// <summary>
    /// Holding non-gravity-controller-device (Only those need manually syncing with hand when holding)
    /// </summary>
    bool grabbingHandDevice = false;

    bool usingHandDevice = false;

    bool grabbingZipLine = false;
    

    /// <summary>
    /// UI screen that shows details about the held device
    /// </summary>
    UIHandDevice deviceUI;

    // Start is called before the first frame update (1. Awake -> 2. Start)
    void Awake()
    {
        playerController = transform.root.GetComponent<OVRPlayerController>();

        deviceUI = GetComponentInChildren<UIHandDevice>();
        deviceUI.Initialize();

        if (handSide == EHandSide.LEFT)
        {
            leftHand = this;
            grabButton = OVRInput.Button.PrimaryHandTrigger;
        }

        if (handSide == EHandSide.RIGHT)
        {
            rightHand = this;
            grabButton = OVRInput.Button.SecondaryHandTrigger;

            handDevice = omniDevice = GetComponent<OmniDevice>();

            deviceUI.Set(handDevice.GetUIData());
        }
    }

    private void Start()
    {
        otherHand = (handSide == EHandSide.LEFT) ? rightHand : leftHand;

        //Temp
        if (handSide == EHandSide.LEFT) rightHand.otherHand = this;

    }

    //Called after hand prefabs are instantiated on network
    public void Initialize(GameObject spawnedHand)
    {
        if (handSide == EHandSide.RIGHT)
        {
            OmniDevice od = (OmniDevice)omniDevice;

            od.Initialize(spawnedHand.GetComponentInChildren<HandDeviceSync>());

            //Initialize OmniDeviceMenu
            ((UIOmniDeviceMenu)deviceUI).NumberOfDevices = od.NumberOfDevices;
            ((UIOmniDeviceMenu)deviceUI).OnMenuChange += od.SetDeviceMode;
        }

        handSync = spawnedHand.GetComponent<HandSync>();
    }

    // Update is called once per frame
    void Update()
    {
        //Keeping device object attached to hand while holding
        if (grabbingHandDevice) handDevice.transform.SetPositionAndRotation(transform.position, transform.rotation);

        //Operate handheld device 
        if (handDevice)
        {
            deviceUI.Operate(handSide);

            //(NOTE!) handDevice.Using() actually operates the device with input detection and all
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

        else if (shouldGrab)
        {
            shouldGrab = false;

            if (other.gameObject.layer.Equals(layer_GrabHandle))
            {
                GrabHandle(other.gameObject);
                otherHand.ReleaseHandle();
            }

            else if (other.gameObject.layer.Equals(layer_HandDevice))
                GrabDevice(other.gameObject);

            else if (other.gameObject.layer.Equals(layer_ZipLineHandle))
                GrabZipLine(other.gameObject.GetComponent<ZipLineTransport>().TransportDirection);
        }

        else if (shouldRelease) 
        {
            shouldRelease = false;

            if (other.gameObject.layer.Equals(layer_GrabHandle))
                ReleaseHandle();

            else if (other.gameObject.layer.Equals(layer_HandDevice))
                DropDevice();

            else if (other.gameObject.layer.Equals(layer_ZipLineHandle))
                ReleaseZipLine();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (grabbingZipLine && other.gameObject.layer.Equals(layer_ZipLineHandle))
            ReleaseZipLine();
    }

    void GrabHandle(GameObject handle)
    {
        this.handle = handle;
        offsettToHandleOnGrab = transform.position - handle.transform.position;

        handSync.GrabbingGrabHandle = true;
        if (otherHand.handSync.GrabbingGrabHandle) otherHand.handSync.GrabbingGrabHandle = false;

        playerController.RegisterGrabHandleEvent(false, (int)otherHand.handSide);
        playerController.RegisterGrabHandleEvent(true, (int)handSide, handle.transform);
    }

    void ReleaseHandle()
    {
        handle = null;
        handSync.GrabbingGrabHandle = false;
        playerController.RegisterGrabHandleEvent(false, (int)handSide);
    }

    void GrabZipLine(Vector3 moveDirection)
    {
        playerController.SetGrabbingZipLine(true, moveDirection);

        handOffsetToPlayerControllerOnZipLineGrab = transform.position - playerController.transform.position;

        grabbingZipLine = true;
    }

    void ReleaseZipLine()
    {
        playerController.SetGrabbingZipLine(false, new Vector3(0, 0, 0));

        grabbingZipLine = false;
    }

    void GrabDevice(GameObject device)
    {
        grabbingHandDevice = true;

        handDevice = device.GetComponent<HandDevice>();
        handDevice.Equip(handSide);

        if (handDevice && handDevice.GetRB()) handDevice.GetRB().useGravity = false;

        deviceUI.Set(handDevice.GetUIData());
    }

    void DropDevice()
    {
        grabbingHandDevice = false;

        if (handDevice && handDevice.GetRB()) handDevice.GetRB().useGravity = true;

        handDevice = null;

        if (handSide == EHandSide.RIGHT)
        {
            handDevice = omniDevice;
            deviceUI.Set(omniDevice.GetUIData());
        }
    }
}
