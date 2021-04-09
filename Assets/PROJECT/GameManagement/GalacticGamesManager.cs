using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using UnityEngine.SceneManagement;


public class GalacticGamesManager : Singleton<GalacticGamesManager>
{
    Realtime realTime;

    GameObject bridgeTest;

    bool objectsSpawned = false;

    List<TeamCreationPod> teamCreationPods = new List<TeamCreationPod>();

    public List<TeamCreationPod> TeamCreationPods { get => teamCreationPods; }

    IEnumerator DisableNonTeamObjects_coroutine;


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
        //Fetch all objects in scene
        GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();

        



        //Collect names and transforms of all PFR(PreFab Realtime)-objects except heads and hands
        List<string> PFRNames = new List<string>();
        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();

        foreach (GameObject sceneObject in sceneObjects)
            if (sceneObject.name.Contains("PFR") && (!sceneObject.name.Contains("Head") && !sceneObject.name.Contains("Hand")))
            {
                PFRNames.Add(sceneObject.name);
                positions.Add(sceneObject.transform.position);
                rotations.Add(sceneObject.transform.rotation);
            }

        int numberOfNonPFRSceneObjects = SceneManager.GetActiveScene().rootCount - PFRNames.Count;

        //Disable the collected sceneObjects
        foreach (GameObject sceneObject in sceneObjects)
            sceneObject.SetActive(false);

        //Network spawn objects if first member of the team
        foreach(TeamCreationPod creationPod in teamCreationPods)
        {
            if (creationPod.TeamMembers[0] == realTime.clientID)
            {
                for(int i = 0; i < PFRNames.Count; i++)
                {
                    GameObject newObject = Realtime.Instantiate(PFRNames[i],
                                                    ownedByClient: true,
                                                    preventOwnershipTakeover: false,
                                                    destroyWhenOwnerOrLastClientLeaves: true,
                                                    useInstance: realTime);

                    RealtimeTransform rtt = newObject.GetComponent<RealtimeTransform>();
                    if (rtt)
                    {
                        rtt.SetOwnership(realTime.clientID);

                        newObject.transform.position = positions[i];
                        newObject.transform.rotation = rotations[i];
                    }
                    else Debug.Log("GGM spawning sequence: object did not have a RealtimeTransform");
                }
            }
        }


    }

    /*
    IEnumerator DisableNonTeamObjects()
    {

    }
    */
}
