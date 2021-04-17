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

    int rootCountBeforeSpawningForTeams;

    int numberOfActiveTeams = 0;

    List<int> clientsInRoom = new List<int>();


    public void RegisterClientID(int clientID)
    {
        Debug.Log("GGM: Registering client ID " + clientID + " in GameManager");
        clientsInRoom.Add(clientID);

        //Placed here because new objects will sneak into the count after first client spawns gameplay objects and 
        //other clients experience delay before they start the process
        rootCountBeforeSpawningForTeams = SceneManager.GetActiveScene().rootCount;
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
                if (rtv.gameObject.name.Contains("AttractorRift")) numberOfAttractorRifts++;

                namesOfGameplayPrefabs.Add(rtv.gameObject.name);
                positions.Add(rtv.gameObject.transform.position);
                rotations.Add(rtv.gameObject.transform.rotation);

                rtv.gameObject.SetActive(false);
            }
        }

        Debug.Log("GGM: Done collecting names. Gameplay objects without 'Clone': " + namesOfGameplayPrefabs.Count);

        Debug.Log("GGM: Root count before spawning team objects: " + SceneManager.GetActiveScene().rootCount);
        Debug.Log("GGM: Number of attractor rifts counted: " + numberOfAttractorRifts);

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
                                                    ownedByClient: true,
                                                    preventOwnershipTakeover: false,
                                                    destroyWhenOwnerOrLastClientLeaves: true,
                                                    useInstance: realTime);

                    newSpawns++;

                    RealtimeTransform rtt = newObject.GetComponent<RealtimeTransform>();
                    if (rtt) rtt.SetOwnership(realTime.clientID);

                    newObject.transform.localPosition = positions[i];
                    newObject.transform.rotation = rotations[i];
                }

                Debug.Log("GGM: Spawned " + newSpawns + " gameplay objects. New root count: " + SceneManager.GetActiveScene().rootCount);
            }
        }

        Debug.Log("GGM: Root count after spawning team objects: " + SceneManager.GetActiveScene().rootCount);

        StartCoroutine(DisableNonTeamObjects());
    }

    IEnumerator DisableNonTeamObjects()
    {
        Debug.Log("GGM: waiting to disable objects. Number of active teams: " + numberOfActiveTeams);
        Debug.Log("GGM: member zero of existing teams: " + teamCreationPods[0].TeamMembers[0] + " & " + teamCreationPods[1].TeamMembers[0]);

        //Wait for all clients to be done spawning
        while (SceneManager.GetActiveScene().rootCount != 
            rootCountBeforeSpawningForTeams +
            (namesOfGameplayPrefabs.Count * numberOfActiveTeams) +
            (numberOfAttractorRifts * 2 * numberOfActiveTeams)) //Accounting for anchor and dummy object that AttractorRift creates on Start
        {

            Debug.Log("GGM: Expected result: " + 
                (rootCountBeforeSpawningForTeams +
                 (namesOfGameplayPrefabs.Count * numberOfActiveTeams) +
                 (numberOfAttractorRifts * 2 * numberOfActiveTeams)) + ". Current root count: " + SceneManager.GetActiveScene().rootCount);

            yield return null;
        }
            

        Debug.Log("GGM: All spawning clients done spawning");
        

        
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

                        else avatars.Add(gameObject);



                    //Disable all gameplay objects not spawned by the first member of this team
                    foreach (GameObject gameplayObject in gameplayObjects)
                    {
                        RealtimeView rtv = gameplayObject.GetComponent<RealtimeView>();
                        if (rtv && rtv.ownerIDSelf != spawningClient)
                             rtv.gameObject.SetActive(false);
                    }

                    //Disable all avatar objects not part of this team
                    foreach (GameObject avatarObject in avatars)
                    {
                        RealtimeView rtv = avatarObject.GetComponent<RealtimeView>();
                        if (rtv)
                        {
                            bool partOfTeam = false;
                            for (int k = 0; k < teamSize; k++)
                                if (rtv.ownerIDSelf == teamCreationPods[i].TeamMembers[k])
                                    partOfTeam = true;

                            if (!partOfTeam)
                            {
                                Debug.Log("Setting " + rtv.gameObject.name + " inactive");
                                rtv.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        competitionStarted = true;
    }
}
