using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class AvatarSpawner : MonoBehaviour
{
    // Start is called before the first frame update

    private OVRPlayerController playerController;
    Realtime realtime;

    [SerializeField]
    Material grabbingMaterial;

    [SerializeField]
    Material defaultMaterial;


    [SerializeField]
    GameObject avatarRoot;

    [SerializeField]
    GameObject leftControllerAnchor;

    [SerializeField]
    GameObject rightControllerAnchor;



    void Start()
    {
        playerController = GetComponent<OVRPlayerController>();
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();

        realtime.didConnectToRoom += DidConnectToRoom;
    }

    void DidConnectToRoom(Realtime realtime)
    {
        List<GameObject> parts = playerController.SpawnAvatar();

        parts[0].GetComponent<Hand>().Initialize(avatarRoot, leftControllerAnchor, defaultMaterial, grabbingMaterial, playerController);
        parts[0].GetComponent<RealtimeTransform>().RequestOwnership();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
