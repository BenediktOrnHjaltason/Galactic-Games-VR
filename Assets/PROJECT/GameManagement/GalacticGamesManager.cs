using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;


public class GalacticGamesManager : Singleton<GalacticGamesManager>
{
    Realtime realTime;

    GameObject bridgeTest;

    bool objectsSpawned = false;

    List<TeamCreationPod> teamCreationPods = new List<TeamCreationPod>();

    public List<TeamCreationPod> TeamCreationPods { get => teamCreationPods; }


    // Start is called before the first frame update
    void Start()
    {
        realTime = GameObject.Find("Realtime").GetComponent<Realtime>();

    }

    void SpawnTestBridge(Realtime realtime)
    {
        if (realTime.clientID != 0 || objectsSpawned) return;

        string objectName = bridgeTest.name;

        string bridgePrefabName = "";

        //Extract original prefab name
        for (int i = 0; i < objectName.Length; i++)
        {
            if (objectName[i] > 47) bridgePrefabName += objectName[i];
            else break;
        }

        GameObject newObject = Realtime.Instantiate(bridgePrefabName,
                                                    ownedByClient: false,
                                                    preventOwnershipTakeover: false,
                                                    destroyWhenOwnerOrLastClientLeaves: true,
                                                    useInstance: realTime);

        newObject.GetComponent<RealtimeTransform>().RequestOwnership();

        newObject.transform.position = bridgeTest.transform.position + new Vector3(-50, 0, 0);
        newObject.transform.rotation = bridgeTest.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        Debug.Log("GameManager: StartGame called");
    }
}
