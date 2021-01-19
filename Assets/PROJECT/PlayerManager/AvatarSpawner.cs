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

    [SerializeField]
    GameObject eyeAnchor;



    void Start()
    {
        playerController = GetComponent<OVRPlayerController>();
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();

        realtime.didConnectToRoom += DidConnectToRoom;
    }

    void DidConnectToRoom(Realtime realtime)
    {
        List<GameObject> parts = playerController.SpawnAvatarParts();

        //Left hand
        parts[0].GetComponent<Hand>().Initialize(avatarRoot, leftControllerAnchor, eyeAnchor, defaultMaterial, grabbingMaterial, playerController);

        //Right hand
        parts[1].GetComponent<Hand>().Initialize(avatarRoot, rightControllerAnchor, eyeAnchor, defaultMaterial, grabbingMaterial, playerController);


        foreach (GameObject go in parts) go.GetComponent<RealtimeTransform>().RequestOwnership();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
