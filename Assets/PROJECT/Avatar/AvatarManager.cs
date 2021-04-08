using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Normal.Realtime;

public class AvatarManager : MonoBehaviour
{
    // Start is called before the first frame update

    private OVRPlayerController playerController;
    Realtime realtime;

    [SerializeField]
    GameObject avatarRoot;

    [SerializeField]
    GameObject leftControllerAnchor;

    [SerializeField]
    GameObject rightControllerAnchor;

    [SerializeField]
    GameObject eyeAnchor;

    [SerializeField]
    GameObject trackingSpaceAnchor;

    GameObject head;
    GameObject torso;

    void Start()
    {
        playerController = GetComponent<OVRPlayerController>();
        

        if (!SceneManager.GetActiveScene().name.Contains("MainMenu"))
        {
            realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
            torso = new GameObject();
            head = new GameObject();
            realtime.didConnectToRoom += SpawnAvatar;
        }
        else
        {
            head = eyeAnchor.transform.GetChild(1).gameObject;
            torso = head.transform.GetChild(1).gameObject;
        }
    }

    void SpawnAvatar(Realtime realtime)
    {

        // 0 - Left hand
        GameObject leftHand = Realtime.Instantiate("PF_Hand_Left", ownedByClient: true,
                                                      preventOwnershipTakeover: true,
                                                      destroyWhenOwnerOrLastClientLeaves: true,
                                                      useInstance: this.realtime);

        leftHand.transform.SetPositionAndRotation(leftControllerAnchor.transform.position, leftControllerAnchor.transform.rotation);
        leftHand.transform.SetParent(leftControllerAnchor.transform);
        leftHand.GetComponent<RealtimeTransform>().RequestOwnership();

        leftControllerAnchor.GetComponent<Hand>().Initialize(leftHand);


        //1 - Right hand

        GameObject rightHand = Realtime.Instantiate("PF_Hand_Right", ownedByClient: true,
                                                      preventOwnershipTakeover: true,
                                                      destroyWhenOwnerOrLastClientLeaves: true,
                                                      useInstance: this.realtime);

        rightHand.transform.SetPositionAndRotation(rightControllerAnchor.transform.position, rightControllerAnchor.transform.rotation);
        rightHand.transform.SetParent(rightControllerAnchor.transform);
        rightHand.GetComponent<RealtimeTransform>().RequestOwnership();

        //---- Initialize OmniDevice for right hand

        rightControllerAnchor.GetComponent<Hand>().Initialize(rightHand);


        //2 - Head
        GameObject.Destroy(head);
        head = Realtime.Instantiate("PF_Head", ownedByClient: true,
                                                      preventOwnershipTakeover: true,
                                                      destroyWhenOwnerOrLastClientLeaves: true,
                                                      useInstance: this.realtime);

        playerController.HeadRealtimeView = head.GetComponent<RealtimeView>();

        head.transform.SetPositionAndRotation(eyeAnchor.transform.position, eyeAnchor.transform.rotation);
        head.transform.SetParent(eyeAnchor.transform);
        head.GetComponent<RealtimeTransform>().RequestOwnership();

        //head.transform.GetChild(0).transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        //head.transform.GetChild(0).transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        //head.transform.GetChild(0).transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;


        /*
        PlayerSync ps = head.transform.GetComponent<PlayerSync>();

        RealtimeTransform rtt = head.GetComponent<RealtimeTransform>();

        rtt.realtime.didConnectToRoom += ps.SetPlayerNameFromPlayerPrefs;
        rtt.realtime.didConnectToRoom += ps.SetHeadIndexFromPlayerPrefs;
        */

        //3 - Torso
        GameObject.Destroy(torso);
        torso = head.transform.GetChild(0).gameObject;
        torso.GetComponent<RealtimeTransform>().RequestOwnership();

        OVRManager.display.RecenterPose();

    }

    // Update is called once per frame
    void Update()
    {
        torso.transform.rotation = Quaternion.Euler(new Vector3(0, head.transform.rotation.eulerAngles.y, 1));
    }
}
