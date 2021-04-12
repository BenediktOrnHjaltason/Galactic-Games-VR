using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using UnityEngine.SceneManagement;


public class GalacticGamesManager : Singleton<GalacticGamesManager>
{
    Realtime realTime;

    List<TeamCreationPod> teamCreationPods = new List<TeamCreationPod>();

    public List<TeamCreationPod> TeamCreationPods { get => teamCreationPods; }

    List<string> namesOfGameplayPrefabs = new List<string>();

    int numberOfAttractorRifts = 0;

    bool competitionStarted = false;

    int rootCountBeforeSpawningForTeams;


    // Start is called before the first frame update
    void Start()
    {
        realTime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    private void OnLevelWasLoaded(int level)
    {
        competitionStarted = false;
    }

    public void StartGame()
    {
        if (competitionStarted) return;

        competitionStarted = true;

        rootCountBeforeSpawningForTeams = SceneManager.GetActiveScene().rootCount;


        GameObject[] teamFilteredObjects = GameObject.FindGameObjectsWithTag("TeamFiltered");

        
        List<RealtimeView> gameplayRealtimeViews = new List<RealtimeView>();
        List<RealtimeView> avatarRealtimeViews = new List<RealtimeView>();

        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();


        //Separate gameplay objects and avatar objects
        foreach (GameObject gameObject in teamFilteredObjects)
        {
            RealtimeView rtv = gameObject.GetComponent<RealtimeView>();
            if (rtv)
            {
                if (rtv.name.Contains("Head") || rtv.name.Contains("Hand_")) avatarRealtimeViews.Add(rtv);
                else
                {
                    gameplayRealtimeViews.Add(rtv);

                    if (rtv.gameObject.name.Contains("AttractorRift")) numberOfAttractorRifts++;

                    namesOfGameplayPrefabs.Add(rtv.gameObject.name);
                    positions.Add(rtv.gameObject.transform.position);
                    rotations.Add(rtv.gameObject.transform.rotation);

                    rtv.gameObject.SetActive(false);
                }
            }
        }

        Debug.Log("GGM: Root count before spawning team objects: " + SceneManager.GetActiveScene().rootCount);
        Debug.Log("GGM: Number of attractor rifts counted: " + numberOfAttractorRifts);

        //Network spawn objects if first member of a team
        foreach (TeamCreationPod creationPod in teamCreationPods)
        {
            if (realTime.clientID == creationPod.TeamMembers[0])
            {
                //Instantiate objects on network for this team
                for (int i = 0; i < namesOfGameplayPrefabs.Count; i++)
                {
                    GameObject newObject = Realtime.Instantiate(namesOfGameplayPrefabs[i],
                                                    ownedByClient: true,
                                                    preventOwnershipTakeover: false,
                                                    destroyWhenOwnerOrLastClientLeaves: true,
                                                    useInstance: realTime);

                    RealtimeTransform rtt = newObject.GetComponent<RealtimeTransform>();
                    if (rtt) rtt.SetOwnership(realTime.clientID);

                    newObject.transform.position = positions[i];
                    newObject.transform.rotation = rotations[i];
                }
            }
        }

        Debug.Log("GGM: Root count after spawning team objects: " + SceneManager.GetActiveScene().rootCount);

        StartCoroutine(DisableNonTeamObjects());
    }

    IEnumerator DisableNonTeamObjects()
    {
        //Wait for all clients to be done spawning
        while (SceneManager.GetActiveScene().rootCount != 
            rootCountBeforeSpawningForTeams +
            namesOfGameplayPrefabs.Count * teamCreationPods.Count +
            numberOfAttractorRifts * 2 * teamCreationPods.Count) //Accounting for anchor and dummy object that AttractorRift creates at start
        {

            Debug.Log("GGM: Expected result: " + (rootCountBeforeSpawningForTeams +
            namesOfGameplayPrefabs.Count * teamCreationPods.Count +
            numberOfAttractorRifts * teamCreationPods.Count));

            yield return null;
        }
            

        Debug.Log("GGM: All spawning clients done spawning");
        

        /*
        //Disable objects not relevant to this team

        //Find this client's team
        for (int i = 0; i < teamCreationPods.Count; i++)
        {
            for (int j = 0; j < teamCreationPods[i].TeamMembers.Count; j++)
            {
                if (realTime.clientID == teamCreationPods[i].TeamMembers[j])
                {
                    //Found team (i) and spawning client for this team is index 0

                    int teamSize = teamCreationPods[i].TeamMembers.Count;
                    int spawningClient = teamCreationPods[i].TeamMembers[0];

                    //Get all active root objects in scene after all spawning is done
                    GameObject[] teamFilteredObjects = GameObject.FindGameObjectsWithTag("TeamFiltered");


                    //Disable all objects not relevant for this team

                    foreach(GameObject teamFilteredObject in teamFilteredObjects)
                    {
                        RealtimeView rtv = teamFilteredObject.GetComponent<RealtimeView>();
                        if (rtv)
                        {
                            if (teamFilteredObject.name.Contains("Head") || gameObject.name.Contains("AvatarPart"))
                            {
                                bool partOfTeam = false;
                                for (int k = 0; k < teamSize; k++)
                                    if (rtv.ownerIDSelf == teamCreationPods[i].TeamMembers[k])
                                        partOfTeam = true;

                                if (!partOfTeam) rtv.gameObject.SetActive(false);
                            }
                            else if (rtv.ownerIDSelf != spawningClient) rtv.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }*/
    }
}
