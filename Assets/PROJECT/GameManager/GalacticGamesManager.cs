﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using UnityEngine.SceneManagement;


public class GalacticGamesManager : Singleton<GalacticGamesManager>
{
    Realtime realTime;

    List<string> namesOfGameplayPrefabs = new List<string>();

    int numberOfAttractorRifts = 0;

    bool competitionStarted = false;

    public bool CompetitionStarted { get => competitionStarted; }

    int numberOfActiveTeams = 0;

    List<int> clientsInRoom = new List<int>();

    int originalRootCountOnGameStart = 0;

    GameManagerSync gameManagerSync;

    List<RealtimeView> avatarRtvs = new List<RealtimeView>();

    TeamCreationPod thisPlayersPod = null;

    List<RealtimeView> teamGameplayRtvs = new List<RealtimeView>();



    public void Initialize(GameManagerSync GMSync)
    {
        Debug.Log("GGM: Initialized called");

        gameManagerSync = GMSync;
        realTime = GameObject.Find("Realtime").GetComponent<Realtime>();

        avatarRtvs = new List<RealtimeView>();
        teamGameplayRtvs = new List<RealtimeView>();
        thisPlayersPod = null;
        competitionStarted = false;
        numberOfActiveTeams = 0;
        originalRootCountOnGameStart = 0;
    }

    public void RegisterClientArrival(int clientID)
    {
        Debug.Log("GGM: RegisterClientArrival called with clientID " + clientID);

        clientsInRoom.Add(clientID);

        Debug.Log("GGM: clientsInRoom now contains");

        foreach (int client in clientsInRoom) Debug.Log(client);
    }

    public void NetworkNotifyClientLeftRoom(int clientID)
    {
        gameManagerSync.ClientLeftRoom = clientID;
    }

    public void RegisterClientLeftRoom(int clientID)
    {
        Debug.Log("GGM: RegisterClientLeftRoom called with clientID: " + clientID );

        clientsInRoom.Remove(clientID);

        foreach (TeamCreationPod pod in TeamCreationPod.instances)
        {
            for (int i = 0; i < pod.TeamMembers.Count; i++)
                if (pod.TeamMembers[i] == clientID)
                {
                    Debug.Log("GGM: RegisterClientLeftRoom: clientID found in pod");

                    if (pod.MemberClientIDToName.ContainsKey(pod.TeamMembers[i]))
                        Debug.Log("GGM: pod memberClientIDToName contains clientID as key");

                    else Debug.Log("GGM: pod memberClientIDToName DOES NOT contain clientID as key");

                    pod.OnTeamMemberLeftRoom(i);
                }
        }
    }

    public bool AllPlayersAccountedFor()
    {
        foreach (int clientID in clientsInRoom)
        {
            bool playerAccountedFor = false;

            foreach (TeamCreationPod pod in TeamCreationPod.instances)
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

    Dictionary<string, Vector3> attractorRiftToPosition = new Dictionary<string, Vector3>();

    public Dictionary<string, Vector3> AttractorRiftToPosition { get => attractorRiftToPosition; }
    bool gameStartSequenceStarted = false;
    public void StartCompetition()
    {
        Debug.Log("GGMStart: StartGame() called in " + name);

        if (gameStartSequenceStarted)
        {
            Debug.Log("GGM: StartGame() called for the second time. Aborting");
            return;
        }
        gameStartSequenceStarted = true;

        //Count non TeamFiltered root objects in scene
        GameObject[] rootObjectsOnGameStart = SceneManager.GetActiveScene().GetRootGameObjects();


        //Count the rootobjects that were present before any clients spawned anything
        originalRootCountOnGameStart = 0;
        foreach (GameObject rootObject in rootObjectsOnGameStart)
        {
            if (rootObject.name.Contains("Clone") && (!rootObject.name.Contains("Head") && !rootObject.name.Contains("Hand_"))) continue;

            //Debug.Log("GGM: root object gathered on game start: " + rootObject.name);
            originalRootCountOnGameStart++;
        }

        Debug.Log("GGM: number of root objects on game start: " + originalRootCountOnGameStart);

        Debug.Log("GGM: On start game: number of team creation pods: " + TeamCreationPod.instances.Count + ". Finding numberOfActiveTeams");

        foreach (TeamCreationPod pod in TeamCreationPod.instances)
            if (pod.ReadyToPlay)
            {
                numberOfActiveTeams++;

                Debug.Log("GGM: Incremented numberOfActiveTeams to " + numberOfActiveTeams);
            }

        Debug.Log("GGM: On start game: numberOfActiveTeams: " + numberOfActiveTeams);


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
                    attractorRiftToPosition.Add(rtv.gameObject.name + "(Clone)", rtv.gameObject.transform.position);
                    numberOfAttractorRifts++;
                }

                namesOfGameplayPrefabs.Add(rtv.gameObject.name);

                Structure_RestrictedMove srm = rtv.GetComponent<Structure_RestrictedMove>();

                if (srm) positions.Add(srm.StartPos);
                else positions.Add(rtv.gameObject.transform.position);

                rotations.Add(rtv.gameObject.transform.rotation);

                rtv.gameObject.SetActive(false);
            }
        }

        //Debug.Log("GGM: Done collecting names. Gameplay objects without 'Clone': " + namesOfGameplayPrefabs.Count);

        // Debug.Log("GGM: Root count before spawning team objects: " + SceneManager.GetActiveScene().rootCount);
        //Debug.Log("GGM: Number of attractor rifts counted: " + numberOfAttractorRifts);

        bool didSpawnObjects = false;

        //Network spawn objects if first member of a team
        foreach (TeamCreationPod creationPod in TeamCreationPod.instances)
        {
            if (realTime.clientID == creationPod.TeamMembers[0])
            {
                didSpawnObjects = true;
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

                    newSpawns++;

                    RealtimeTransform rtt = newObject.GetComponent<RealtimeTransform>();
                    if (rtt) rtt.SetOwnership(realTime.clientID);

                    //newObject.transform.localPosition = positions[i];
                    //newObject.transform.rotation = rotations[i];
                }

                if (!gameManagerSync) Debug.Log("GGM: After Spawning: gameManagerSync is not set to an instance of an object in" + name);
                else Debug.Log("GGM: After Spawning: gameManagerSync is valid in " + name);

                //gameManagerSync.ClientsDoneSpawning++;

                //Debug.Log("GGM: Spawned " + newSpawns + " gameplay objects. New root count: " + SceneManager.GetActiveScene().rootCount);
            }
        }

        if (!didSpawnObjects)
        {
            //Debug.Log("GGM: Did not spawn objects. ClientID is " + realTime.clientID + " and index 0 of creationpods were: " + TeamCreationPod.instances[0].TeamMembers[0] + " and " + TeamCreationPod.instances[1].TeamMembers[0] );
        }

        Debug.Log("GGM: Root count after spawning team objects: " + SceneManager.GetActiveScene().rootCount);

        StartCoroutine(DisableNonTeamObjects());
    }

    IEnumerator DisableNonTeamObjects()
    {
        Debug.Log("GGM: waiting to disable objects. Number of active teams: " + numberOfActiveTeams);
        //Debug.Log("GGM: member zero of existing teams: " + TeamCreationPod.instances[0].TeamMembers[0] + " & " + TeamCreationPod.instances[1].TeamMembers[0]);
        Debug.Log("GGM: originalRootCountOnGameStart: " + originalRootCountOnGameStart);
        //Debug.Log("GGM: clients done spawning: " + gameManagerSync.ClientsDoneSpawning);

        //Wait for all clients to be done spawning
        while (/*gameManagerSync.ClientsDoneSpawning < numberOfActiveTeams ||*/
               SceneManager.GetActiveScene().rootCount < ((originalRootCountOnGameStart +
                                                           namesOfGameplayPrefabs.Count * numberOfActiveTeams) + 
                                                          (numberOfAttractorRifts * numberOfActiveTeams * 2)))
        {
            Debug.Log("GGM: Condition FAILED");
            //Debug.Log("GGM: ClientsDoneSpawning: " + gameManagerSync.ClientsDoneSpawning);
            Debug.Log("GGM: originalRootCountOnGameStart: " + originalRootCountOnGameStart);
            Debug.Log("GGM: numberOfActiveTeams: " + numberOfActiveTeams);
            Debug.Log("GGM: namesOfGameplayPrefabs.Count: " + namesOfGameplayPrefabs.Count);
            
            Debug.Log("GGM: numberOfAttractorRifts: " + numberOfAttractorRifts);

            Debug.Log("GGM: Expected root count: " + ((originalRootCountOnGameStart +
                                                       namesOfGameplayPrefabs.Count * numberOfActiveTeams) +
                                                      (numberOfAttractorRifts * numberOfActiveTeams * 2)).ToString());

            Debug.Log("GGM: Actual root count: " + SceneManager.GetActiveScene().rootCount);

            yield return null;
        }

        Debug.Log("GGM: Condition MET");

        Debug.Log("GGM: numberOfActiveTeams: " + numberOfActiveTeams);

        Debug.Log("GGM: namesOfGameplayPrefabs.Count: " + namesOfGameplayPrefabs.Count);
        Debug.Log("GGM: originalRootCountOnGameStart: " + originalRootCountOnGameStart);
        Debug.Log("GGM: numberOfAttractorRifts: " + numberOfAttractorRifts);

        Debug.Log("GGM: Expected root count: " + ((namesOfGameplayPrefabs.Count * numberOfActiveTeams) +
                                                       originalRootCountOnGameStart +
                                                       (numberOfAttractorRifts * numberOfActiveTeams * 2)).ToString());
        
        Debug.Log("GGM: Actual root count: " + SceneManager.GetActiveScene().rootCount);

        /*
        GameObject[] rootObjectsAfterConditionMet = SceneManager.GetActiveScene().GetRootGameObjects();

        Debug.Log("GGM: root count after counting array: " + rootObjectsAfterConditionMet.Length);

        originalRootCountOnGameStart = 0;
        foreach (GameObject rootObject in rootObjectsAfterConditionMet)
            Debug.Log("GGM: root objects found after condition met: " + rootObject.name);
        */

        //Disable gameplay objects not relevant to this team

        int thisClientIndex = -1;

        //Find this client's team
        for (int i = 0; i < TeamCreationPod.instances.Count; i++)
        {
            for (int j = 0; j < TeamCreationPod.instances[i].TeamMembers.Count; j++)
            {
                if (realTime.clientID == TeamCreationPod.instances[i].TeamMembers[j])
                {
                    //Found team (i) and spawning client for this team is index 0 of teamMembers

                    int teamSize = TeamCreationPod.instances[i].TeamMembers.Count;
                    int spawningClient = TeamCreationPod.instances[i].TeamMembers[0];
                    thisClientIndex = j;

                    thisPlayersPod = TeamCreationPod.instances[i];

                    //thisPlayersPod.ScreenDebugMessage = "Condition MET and found team. Spawning client: " + spawningClient;
                    //thisPlayersPod.ConstructScreenText();

                    //Get all active root objects in scene after all spawning is done
                    GameObject[] teamFiltered = GameObject.FindGameObjectsWithTag("TeamFiltered");

                    List<GameObject> gameplayObjects = new List<GameObject>();

                    //Get all spawned gameplay objects
                    foreach (GameObject gameObject in teamFiltered)
                        if (!gameObject.name.Contains("Head") && !gameObject.name.Contains("Hand_")) //GrabHandle contains "Hand"
                            gameplayObjects.Add(gameObject);

                        else avatarRtvs.Add(gameObject.GetComponent<RealtimeView>());

                    

                    int disabledNonTeamGameplayObjects = 0;

                    //Disable all gameplay objects not spawned by the first member of this team
                    //and set team objects to controllable by all members
                    foreach (GameObject gameplayObject in gameplayObjects)
                    {
                        RealtimeView rtv = gameplayObject.GetComponent<RealtimeView>();

                        if (rtv)
                        {
                            if (rtv.ownerIDSelf != spawningClient)
                            {
                                Debug.Log("GGM: Disabling non team object " + rtv.name + " . ownerID: " + rtv.ownerIDSelf + ". Spawning client: " + spawningClient);
                                rtv.gameObject.SetActive(false);

                                disabledNonTeamGameplayObjects++;
                            }
                                

                            else
                            {
                                //if (realTime.clientID == rtv.ownerIDSelf) rtv.ClearOwnership();

                                teamGameplayRtvs.Add(rtv);
                            }
                        }
                    }

                    //After all non team objects disabled, turn on collisions (turned off in prefab so they don't bounce all over the place when they are  
                    //spawned in the same place)
                    foreach (RealtimeView teamGameplayRtv in teamGameplayRtvs)
                    {
                        StructureSync ss = teamGameplayRtv.GetComponent<StructureSync>();

                        if (ss && teamGameplayRtv.transform.childCount > 0)
                            teamGameplayRtv.transform.GetChild(0).gameObject.layer = 10;

                        else
                        {
                            if (teamGameplayRtv.transform.childCount == 0)
                                Debug.Log("GGM: " + teamGameplayRtv.name + " has no children. Not setting layer to ControllableStructureFree");

                            if (!ss)
                                Debug.Log("GMM: " + teamGameplayRtv.name + " does not have a StructureSync component. Not setting layer to ControllableStructureFree");
                        }
                    }

                    //Disable all avatar objects not part of this team
                    foreach (RealtimeView avatarRtv in avatarRtvs)
                    {
                            bool partOfTeam = false;
                            for (int k = 0; k < teamSize; k++)
                                if (avatarRtv.ownerIDSelf == TeamCreationPod.instances[i].TeamMembers[k])
                                    partOfTeam = true;

                            if (!partOfTeam)
                            {
                                Debug.Log("Setting " + avatarRtv.gameObject.name + " inactive");
                                avatarRtv.gameObject.SetActive(false);
                            }
                    }

                    break;
                }
            }
        }

        //Start disappear animatics for non team pods
        foreach (TeamCreationPod pod in TeamCreationPod.instances)
        {
            pod.DisableReadyButton();

            if (pod != thisPlayersPod && !pod.TeamEmpty)
                pod.StartDisappearAnimatic();

            else if (pod.TeamEmpty)
                pod.gameObject.SetActive(false);
        }

        switch(thisClientIndex)
        {
            case 0: thisPlayersPod.Index0DoneFiltering = true; break;
            case 1: thisPlayersPod.Index1DoneFiltering = true; break;
            case 2: thisPlayersPod.Index2DoneFiltering = true; break;
        }


        thisPlayersPod.ClientDoneDisablingGameplayObject = realTime.clientID;
        StartCoroutine(ReleaseTeamObjectsFromOwnership());
    }

    public void RegisterFinishedPlayer(int clientID)
    {
        Debug.Log("GMM: RegisterFinishedPlayer called");

        gameManagerSync.PlayerCrossedFinishLine = clientID;
    }

    public void ReactivateFinishedPlayer(int clientID)
    {
        Debug.Log("GMM: ReactivatePlayer called in game manager");

        foreach (RealtimeView avatarRtv in avatarRtvs)
            if (avatarRtv.ownerIDSelf == clientID && !avatarRtv.gameObject.activeInHierarchy)
            {
                Debug.Log("GMM: Reactivating " + avatarRtv.name);
                avatarRtv.gameObject.SetActive(true);
            }
    }

    //Everything depends on that the spawning clients sets the objects RealtimeViews ownership to itself, so the other teams can filter accordingly
    //but in the end the ownership must be given up so that other team members can take control of RealtimeTransforms
    IEnumerator ReleaseTeamObjectsFromOwnership()
    {
        thisPlayersPod.ScreenDebugMessage = "";

        while (!AllMembersDoneFiltering())
        {
            thisPlayersPod.ScreenDebugMessage = ("\nClients done filtering not less than team members count. \nClients done filtering: " + thisPlayersPod.ClientsDoneFiltering.Count + ". \nTeam count: " + thisPlayersPod.TeamMembers.Count);

            thisPlayersPod.ScreenDebugMessage += ("\nNum clients notified done filtering" + thisPlayersPod.numClientsNotifyingDoneFiltering);

            thisPlayersPod.ScreenDebugMessage += "\nClients done filtering:";

            foreach (int clientDoneFiltering in thisPlayersPod.ClientsDoneFiltering)
                thisPlayersPod.ScreenDebugMessage += ("\n " + clientDoneFiltering);

            thisPlayersPod.ConstructScreenText();

            yield return null;
        }

        thisPlayersPod.ScreenDebugMessage = ("pod: " + thisPlayersPod.name + ". clients done filtering: " + thisPlayersPod.ClientsDoneFiltering.Count + ". team members count: " + thisPlayersPod.TeamMembers.Count); 
        thisPlayersPod.ScreenDebugMessage += "\nAll team members done filtering gameplay objects";

        Debug.Log("GGM: All team members done filtering gameplay objects");

        
        foreach (RealtimeView rtv in teamGameplayRtvs)
            rtv.ClearOwnership();

        //if (teamGameplayRtvs[0].ownerIDSelf == -1)
        //thisPlayersPod.ScreenDebugMessage += "\nTeam gameplay objects cleared ownership.";

        thisPlayersPod.ScreenDebugMessage += "\nCompetition started";

        thisPlayersPod.ConstructScreenText();

        competitionStarted = true;
    }

    bool AllMembersDoneFiltering()
    {
        switch(thisPlayersPod.TeamMembers.Count)
        {
            case 1: return (thisPlayersPod.Index0DoneFiltering);
            case 2: return (thisPlayersPod.Index0DoneFiltering && thisPlayersPod.Index1DoneFiltering);
            case 3: return (thisPlayersPod.Index0DoneFiltering && thisPlayersPod.Index1DoneFiltering && thisPlayersPod.Index2DoneFiltering);
            default: { Debug.LogError("Team members count is not in 1 - 3 range"); return false; };
        }
    }
}
