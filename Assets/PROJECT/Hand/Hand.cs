using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;


public enum EHandSide{ LEFT = 0, RIGHT = 1 }


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

    //We need reference to handle because we need handle position every update to place hand correctly on moving handle
    GameObject handle;
    Vector3 offsettToHandleOnGrab;


    static Hand leftHand;
    static Hand rightHand;

    Hand otherHand;

    OVRInput.Button grabButton;

    bool shouldGrab = false;
    bool shouldRelease = false;

    //Gravity controll (Only used by right hand)
    GravityController gravityController;
    bool usingGravityController = false;

    //UI screen for use of handheld devices
    UIDeviceInfo deviceUI;
    bool holdingDevice = false;


    // Start is called before the first frame update
    void Awake()
    {
        playerController = rootParent.GetComponent<OVRPlayerController>();
        mesh = GetComponent<MeshRenderer>();
        deviceUI = GetComponentInChildren<UIDeviceInfo>();

        if (eHandSide == EHandSide.LEFT)
        {
            leftHand = this;
            grabButton = OVRInput.Button.PrimaryHandTrigger;
        }

        if (eHandSide == EHandSide.RIGHT)
        {
            rightHand = this;
            gravityController = GetComponentInChildren<GravityController>();
            grabButton = OVRInput.Button.SecondaryHandTrigger;

            holdingDevice = true;

            deviceUI.material = gravityController.UIMaterial;
            deviceUI.fullScale = gravityController.UIFullScale;
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
        if (holdingDevice) deviceUI.Operate(eHandSide);

        if (eHandSide == EHandSide.RIGHT)
        {
            usingGravityController = gravityController.Using();
            playerController.EnableRotation = !usingGravityController;
        }

        if (OVRInput.GetDown(grabButton))
        {
            shouldGrab = true;
            
        }
        else if (OVRInput.GetUp(grabButton)) shouldRelease = true;
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

                if (eHandSide == EHandSide.RIGHT && usingGravityController) return;

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

        playerController.RegisterGrabEvent(true, (int)eHandSide, handle.transform);
        playerController.RegisterGrabEvent(false, (eHandSide == EHandSide.LEFT) ? (int)EHandSide.RIGHT : (int)EHandSide.LEFT);
    }

    void release()
    {
        handle = null;
        mesh.material = defaultColor;
        playerController.RegisterGrabEvent(false, (int)eHandSide);
    }
}
