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

    public bool CompetitionStarted { get => competitionStarted; }

    int numberOfActiveTeams = 0;

    List<int> clientsInRoom = new List<int>();

    int nonTeamFilteredRootCountInSceneOnStart = 0;

    GameManagerSync gameManagerSync;

    List<RealtimeView> avatarRtvs = new List<RealtimeView>();


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GGM: Start() called");

        realTime = GameObject.Find("Realtime").GetComponent<Realtime>();

        gameManagerSync = GameObject.Find("GAME MANAGER").GetComponent<GameManagerSync>();

        if (!gameManagerSync) Debug.Log("GGM: gameManagerSync reference is still null in " + name);
        else Debug.Log("GGM: gameManagerSync reference is valid in " + name);

        GameObject[] rootObjectsAtStart = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObject in rootObjectsAtStart)
        {
            if (!rootObject.CompareTag("TeamFiltered"))
            {
                nonTeamFilteredRootCountInSceneOnStart++;
            }
        }
            
    }


    public void RegisterClientID(int clientID)
    {
        //Debug.Log("GGM: Registering client ID " + clientID + " in GameManager");
        clientsInRoom.Add(clientID);
    }

    public bool AllPlayersAccountedFor()
    {
        foreach (int clientID in clientsInRoom)
        {
            bool playerAccountedFor = false;

            foreach (TeamCreationPod pod in teamCreationPods)
            {
                if (pod.TeamMembers.Contains(clientID))
                {
                    playerAccountedFor = true;
                    break;
                }
            }

            if (!playerAccountedFor) return false;
        }

        return true;
    }

    

    private void OnLevelWasLoaded(int level)
    {
        competitionStarted = false;
    }

    public void StartGame()
    {
        Debug.Log("GGM: StartGame() called in " + name);


        if (competitionStarted) return;

        competitionStarted = true;


        foreach (TeamCreationPod pod in teamCreationPods) if (pod.TeamFilledUp) numberOfActiveTeams++;


        GameObject[] teamFilteredObjects = GameObject.FindGameObjectsWithTag("TeamFiltered");

        //Filter out clones spawned by other clients
        List<GameObject> teamFilteredWOClones = new List<GameObject>();


        foreach (GameObject gameObject in teamFilteredObjects)
            if (!gameObject.name.Contains("Clone")) teamFilteredWOClones.Add(gameObject);


        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();


        //Separate gameplay objects and avatar objects
        foreach (GameObject gameObject in teamFilteredWOClones)
        {
            RealtimeView rtv = gameObject.GetComponent<RealtimeView>();
            if (rtv && !rtv.name.Contains("Head") && !rtv.name.Contains("Hand_"))
            {
                //Attractor Rifts spawn two objects on start to help with functionality. Needed for calculating correct root count
                if (rtv.gameObject.name.Contains("AttractorRift"))
                {
                    Debug.Log("GGM: Collected attractorRift object " + rtv.gameObject.name + " with position: " + rtv.gameObject.transform.position);
                    numberOfAttractorRifts++;
                }

                namesOfGameplayPrefabs.Add(rtv.gameObject.name);
                positions.Add(rtv.gameObject.transform.position);
                rotations.Add(rtv.gameObject.transform.rotation);

                rtv.gameObject.SetActive(false);
            }
        }

        //Debug.Log("GGM: Done collecting names. Gameplay objects without 'Clone': " + namesOfGameplayPrefabs.Count);

       // Debug.Log("GGM: Root count before spawning team objects: " + SceneManager.GetActiveScene().rootCount);
        //Debug.Log("GGM: Number of attractor rifts counted: " + numberOfAttractorRifts);

        //Network spawn objects if first member of a team
        foreach (TeamCreationPod creationPod in teamCreationPods)
        {
            if (realTime.clientID == creationPod.TeamMembers[0])
            {
                Debug.Log("GGM: Spawning new gameplay objects");

                int newSpawns = 0;

                //Instantiate objects on network for this team
                for (int i = 0; i < namesOfGameplayPrefabs.Count; i++)
                {
                    GameObject newObject = Realtime.Instantiate(namesOfGameplayPrefabs[i],
                                                    positions[i],
                                                    rotations[i],
                                                    ownedByClient: true,
                                                    preventOwnershipTakeover: false,
                                                    destroyWhenOwnerOrLastClientLeaves: true,
                                                    useInstance: realTime);

                    Debug.Log("GGM: Spawned " + newObject.name + " with position " + positions[i]);

                    newSpawns++;

                    RealtimeTransform rtt = newObject.GetComponent<RealtimeTransform>();
                    if (rtt) rtt.SetOwnership(realTime.clientID);

                    //newObject.transform.localPosition = positions[i];
                    //newObject.transform.rotation = rotations[i];
                }

                if (!gameManagerSync) Debug.Log("GGM: After Spawning: gameManagerSync is not set to an instance of an object in" + name);
                else Debug.Log("GGM: After Spawning: gameManagerSync is valid in " + name);

                gameManagerSync.ClientsDoneSpawning++;

                //Debug.Log("GGM: Spawned " + newSpawns + " gameplay objects. New root count: " + SceneManager.GetActiveScene().rootCount);
            }
        }

        Debug.Log("GGM: Root count after spawning team objects: " + SceneManager.GetActiveScene().rootCount);

        StartCoroutine(DisableNonTeamObjects());
    }

    IEnumerator DisableNonTeamObjects()
    {
        Debug.Log("GGM: waiting to disable objects. Number of active teams: " + numberOfActiveTeams);
        Debug.Log("GGM: member zero of existing teams: " + teamCreationPods[0].TeamMembers[0] + " & " + teamCreationPods[1].TeamMembers[0]);
        Debug.Log("GGM: nonFilteredRootCount in scene: " + nonTeamFilteredRootCountInSceneOnStart);
        Debug.Log("GGM: clients done spawning: " + gameManagerSync.ClientsDoneSpawning);

        //Wait for all clients to be done spawning
        while (gameManagerSync.ClientsDoneSpawning < numberOfActiveTeams ||
               SceneManager.GetActiveScene().rootCount < ((namesOfGameplayPrefabs.Count * numberOfActiveTeams) + 
                                                           nonTeamFilteredRootCountInSceneOnStart +
                                                           (numberOfAttractorRifts * numberOfActiveTeams * 2)))
        {
            Debug.Log("GGM: ClientsDoneSpawning is less than number of active teams & rootcount is less than gameplay objects * numberOfActiveTeams");

            yield return null;
        }
            

        Debug.Log("GGM: All spawning clients done spawning");
        Debug.Log("GGM: RootCount at this point: " + SceneManager.GetActiveScene().rootCount);

        //Disable gameplay objects not relevant to this team

        //Find this client's team
        for (int i = 0; i < teamCreationPods.Count; i++)
        {
            for (int j = 0; j < teamCreationPods[i].TeamMembers.Count; j++)
            {
                if (realTime.clientID == teamCreationPods[i].TeamMembers[j])
                {
                    //Found team (i) and spawning client for this team is index 0 of teamMembers

                    int teamSize = teamCreationPods[i].TeamMembers.Count;
                    int spawningClient = teamCreationPods[i].TeamMembers[0];

                    //Get all active root objects in scene after all spawning is done
                    GameObject[] teamFiltered = GameObject.FindGameObjectsWithTag("TeamFiltered");

                    List<GameObject> gameplayObjects = new List<GameObject>();
                    List<GameObject> avatars = new List<GameObject>();

                    foreach (GameObject gameObject in teamFiltered)
                        if (!gameObject.name.Contains("Head") && !gameObject.name.Contains("Hand_")) //GrabHandle contains "Hand"
                            gameplayObjects.Add(gameObject);

                        else avatarRtvs.Add(gameObject.GetComponent<RealtimeView>());



                    //Disable all gameplay objects not spawned by the first member of this team
                    foreach (GameObject gameplayObject in gameplayObjects)
                    {
                        RealtimeView rtv = gameplayObject.GetComponent<RealtimeView>();
                        if (rtv && rtv.ownerIDSelf != spawningClient)
                             rtv.gameObject.SetActive(false);
                    }

                    //Disable all avatar objects not part of this team
                    foreach (RealtimeView avatarRtv in avatarRtvs)
                    {
                            bool partOfTeam = false;
                            for (int k = 0; k < teamSize; k++)
                                if (avatarRtv.ownerIDSelf == teamCreationPods[i].TeamMembers[k])
                                    partOfTeam = true;

                            if (!partOfTeam)
                            {
                                Debug.Log("Setting " + avatarRtv.gameObject.name + " inactive");
                                avatarRtv.gameObject.SetActive(false);
                            }
                        
                    }
                }
            }
        }
    }

    public void RegisterFinishedPlayer(int clientID)
    {
        gameManagerSync.PlayerCrossedFinishLine = clientID;
    }

    public void ReactivateFinishedPlayer(int clientID)
    {
        foreach (RealtimeView avatarRtv in avatarRtvs)
            if (avatarRtv.ownerIDSelf == clientID && !avatarRtv.gameObject.activeInHierarchy)
                avatarRtv.gameObject.SetActive(true);
    }
}
