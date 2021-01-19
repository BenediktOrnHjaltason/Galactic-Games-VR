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

    [SerializeField]
    GameObject trackingSpaceAnchor;

    List<GameObject> parts;
    GameObject torso;

    void Start()
    {
        playerController = GetComponent<OVRPlayerController>();
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();

        realtime.didConnectToRoom += DidConnectToRoom;
    }

    void DidConnectToRoom(Realtime realtime)
    {
        parts = playerController.SpawnAvatarParts();

        //0 - Left hand
        parts[0].GetComponent<Hand>().Initialize(avatarRoot, leftControllerAnchor, eyeAnchor, defaultMaterial, grabbingMaterial, playerController);

        //1 - Right hand
        parts[1].GetComponent<Hand>().Initialize(avatarRoot, rightControllerAnchor, eyeAnchor, defaultMaterial, grabbingMaterial, playerController);

        //2 - Head

        //3 - Torso
        torso = parts[3];
        foreach (GameObject go in parts) go.GetComponent<RealtimeTransform>().RequestOwnership();
    }


    // Update is called once per frame
    void Update()
    {
        if (torso)
        {
            torso.transform.rotation = trackingSpaceAnchor.transform.rotation;
            torso.transform.position = eyeAnchor.transform.position + (-eyeAnchor.transform.up * 0.2f);
        }
    }
}
