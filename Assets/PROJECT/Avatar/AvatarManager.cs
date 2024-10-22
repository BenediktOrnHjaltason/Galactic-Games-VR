﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Normal.Realtime;
using System;

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

    [SerializeField]
    GameObject loadingScreenBox;

    event Action OnAvatarSpawned;


    void Awake()
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

        if (loadingScreenBox && realtime)
        {
            GameObject spawnedLoadingScreenBox = 
                        Instantiate(loadingScreenBox,
                                    playerController.transform.position + new Vector3(0,0.8f,0) + playerController.transform.forward * 0.4f,
                                    Quaternion.LookRotation(playerController.transform.forward,
                                    Vector3.up));

            LoadingScreenBox lsb = spawnedLoadingScreenBox.GetComponent<LoadingScreenBox>();

            
            lsb.backScaleDown.OnAnimaticEnds += lsb.DestroySelf;
            lsb.LogoGraphicScaleDown.OnAnimaticEnds += OVRManager.display.RecenterPose;

            lsb.leftDoorOpen.OnAnimaticEnds += playerController.EnableMovement;

            OnAvatarSpawned += lsb.StartAnimatics;
            
        }
    }


    void SpawnAvatar(Realtime realtime)
    {

        // 0 - Left hand
        GameObject leftHand = Realtime.Instantiate("Hand_Left", ownedByClient: true,
                                                      preventOwnershipTakeover: true,
                                                      destroyWhenOwnerOrLastClientLeaves: true,
                                                      useInstance: this.realtime);

        leftHand.transform.SetPositionAndRotation(leftControllerAnchor.transform.position, leftControllerAnchor.transform.rotation);
        leftHand.transform.SetParent(leftControllerAnchor.transform);
        leftHand.GetComponent<RealtimeTransform>().RequestOwnership();

        leftControllerAnchor.GetComponent<Hand>().Initialize(leftHand);


        //1 - Right hand

        GameObject rightHand = Realtime.Instantiate("Hand_Right", ownedByClient: true,
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
        head = Realtime.Instantiate("Head_Torso", ownedByClient: true,
                                                      preventOwnershipTakeover: true,
                                                      destroyWhenOwnerOrLastClientLeaves: true,
                                                      useInstance: this.realtime);

        playerController.HeadRealtimeView = head.GetComponent<RealtimeView>();

        head.transform.SetPositionAndRotation(eyeAnchor.transform.position, eyeAnchor.transform.rotation);
        head.transform.SetParent(eyeAnchor.transform);
        head.GetComponent<RealtimeTransform>().RequestOwnership();


        //3 - Torso
        GameObject.Destroy(torso);
        torso = head.transform.GetChild(0).gameObject;
        torso.GetComponent<RealtimeTransform>().RequestOwnership();

        OnAvatarSpawned?.Invoke();

    }

    // Update is called once per frame
    void Update()
    {
        torso.transform.rotation = Quaternion.Euler(new Vector3(0, head.transform.rotation.eulerAngles.y, 1));
    }
}
