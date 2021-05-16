using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using TMPro;

enum ETeamCreationPod
{
    OnePlayer,
    TwoPlayers,
    ThreePlayers
}

public class TeamCreationPod : RealtimeComponent<TeamCreationPod_Model>
{
    [SerializeField]
    ETeamCreationPod teamSize;

    //public static int teamSizeInt = 0;

    [SerializeField]
    Color teamColor;

    [SerializeField]
    MeshRenderer capacityIndicator;

    [SerializeField]
    Material availableCapacityMaterial;

    [SerializeField]
    Material fullCapacityMaterial;

    [SerializeField]
    List<MeshRenderer> colorIndicators;

    [SerializeField]
    MeshRenderer readyIndicator;

    [SerializeField]
    InteractButton readyButton;

    TextMeshPro readyText;

    [SerializeField]
    TextMeshPro teamSizeText;

    [SerializeField]
    Color AvatarTorsoDefault;

    Material teamNotReadyMaterial;
    Material teamReadyMaterial;

    [SerializeField]
    Animatic disappearAnimatic;

    [SerializeField]
    Material disappearMaterial;

    [SerializeField]
    TextMeshPro teamList;


    bool teamFilledUp = false;

    public bool TeamFilledUp { get => teamFilledUp; set => teamFilledUp = value; }

    bool teamEmpty = true;
    public bool TeamEmpty { get => teamEmpty; }

    bool readyToPlay;

    public bool ReadyToPlay { get => readyToPlay; set => readyToPlay = value; }

    public static List<TeamCreationPod> instances = new List<TeamCreationPod>();

    public static Dictionary<Color, int> ColorToTeamSize = new Dictionary<Color, int>();

    //For when players enter a pod when it's already full of members, and a member leaves
    List<RealtimeView> excessPlayersInCollider = new List<RealtimeView>();

    List<int> teamMembers = new List<int>();
    public List<int> TeamMembers { get => teamMembers; }

    Dictionary<int, string> memberClientIDToName = new Dictionary<int, string>();

    public Dictionary<int, string> MemberClientIDToName { get => memberClientIDToName; }

    private void Awake()
    {
        if (instances.Count > 0) instances.Clear();
        if (ColorToTeamSize.Count > 0) ColorToTeamSize.Clear();

        //if (teamSizeInt != 0) teamSizeInt = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        instances.Add(this);

        Debug.Log("TCP: Added this to instances. Count is now: " + instances.Count);

        int teamSizeInt = (int)teamSize + 1;

        ColorToTeamSize.Add(teamColor, teamSizeInt);

        //teamSizeInt = (int)teamSize + 1;

        for (int i = 0; i < teamSizeInt; i++)
            teamMembers.Add(-1);

        teamSizeText.text = teamSizeInt.ToString()/* + "\nPlayer" + (teamSizeInt > 1 ? "s" : "")*/;

        teamList.text = "";

        foreach (MeshRenderer colorIndicator in colorIndicators)
        colorIndicator.material.SetColor("_BaseColor",teamColor);


        readyButton.OnExecute += SetReady;


        teamNotReadyMaterial = availableCapacityMaterial;
        teamReadyMaterial = fullCapacityMaterial;

        readyText = readyButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();

        disappearAnimatic.InitializeSequenceDelegates();

        disappearAnimatic.onSequenceStart[1] = Hide;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.name.Contains("Head") && !GalacticGamesManager.Instance.CompetitionStarted)
        {
            RealtimeView rv = other.GetComponent<RealtimeView>();

            if (rv) AttemptEnterPlayerInTeam(rv);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(14) && other.gameObject.name.Contains("Head") && !GalacticGamesManager.Instance.CompetitionStarted)
        {
            Debug.Log("TCP: OnTriggerExit triggered. Competition started is false");


            RealtimeView rv = other.GetComponent<RealtimeView>();

            if (rv)
            {
                //Remove excess player if he didn't become part of team
                if (excessPlayersInCollider.Contains(rv))
                {
                    excessPlayersInCollider.Remove(rv);
                    Debug.Log("TCP: Removed non team player from queue");
                    return;
                }

                for (int i = 0; i < teamMembers.Count; i++)
                {
                    if (teamMembers[i] == rv.ownerIDSelf)
                    {
                        teamMembers[i] = -1;

                        teamMembers.Sort();
                        teamMembers.Reverse();

                        Debug.Log("TCP: removed " + rv.ownerIDSelf + " from " + transform.root.name);



                        capacityIndicator.material.SetColor("_BaseColor", Color.white);// = availableCapacityMaterial;

                        teamFilledUp = readyToPlay = false;
                        readyIndicator.material.SetColor("_BaseColor", Color.white); // = teamNotReadyMaterial;
                        readyText.text = "Ready?";

                        PlayerSync ps = other.GetComponent<PlayerSync>();
                        RealtimeView rtv = other.GetComponent<RealtimeView>();
                        if (ps && rtv)
                        {
                            ps.PlayerTorso.material.SetColor("_BaseColor", AvatarTorsoDefault);
                            memberClientIDToName.Remove(rtv.ownerIDSelf);
                            ConstructScreenText();
                        }


                        //Insert potential place holders in team
                        if (excessPlayersInCollider.Count != 0)
                        {
                            RealtimeView temp = excessPlayersInCollider[0];
                            excessPlayersInCollider.RemoveAt(0);
                            Debug.Log("TCP: Tranfering excess player from queue to team");

                            AttemptEnterPlayerInTeam(temp);
                        }


                        teamEmpty = true;
                        for (int j = 0; j < teamMembers.Count; j++)
                            if (teamMembers[j] != -1) teamEmpty = false;

                        /*
                        RaycastHit[] playersHit = Physics.SphereCastAll(transform.position + new Vector3(0, 0.4571f, 0), 1.4f, Vector3.zero, 0, 14);

                        foreach(RaycastHit playerObjectHit in playersHit)
                        {
                            Debug.Log("TCP1: Player standing in collider while other player exited");

                            if (playerObjectHit.collider.gameObject.name.Contains("Head"))
                            {
                                RealtimeView rtV = playerObjectHit.collider.GetComponent<RealtimeView>();

                                if (rtV) AttemptEnterPlayerInTeam(rtV, playerObjectHit.collider);
                            }
                        }*/

                        break;
                    }
                }
            }
        }
    }

    void AttemptEnterPlayerInTeam(RealtimeView rtv)
    {
        if (teamFilledUp)
        {
            excessPlayersInCollider.Add(rtv);
            Debug.Log("TCP: Added excess player to queue");
            return;
        }

        for (int i = 0; i < teamMembers.Count; i++)
        {
            //Add player to team
            if (teamMembers[i] == -1 && !teamMembers.Contains(rtv.ownerIDSelf))
            {
                teamMembers[i] = rtv.ownerIDSelf;

                teamMembers.Sort();
                teamMembers.Reverse();


                Debug.Log("TCP: Entered player " + rtv.ownerIDSelf + " to team in " + transform.root.name);

                teamEmpty = false;

                

                teamFilledUp = true;
                for (int j = 0; j < teamMembers.Count; j++)
                    if (teamMembers[j] == -1) teamFilledUp = false;

                if (teamFilledUp) capacityIndicator.material.SetColor("_BaseColor", teamColor);// = fullCapacityMaterial;

                PlayerSync ps = rtv.GetComponent<PlayerSync>();
                if (ps)
                {
                    ps.PlayerTorso.material.SetColor("_BaseColor", teamColor);
                    memberClientIDToName.Add(rtv.ownerIDSelf, ps.PlayerName.text);
                    ConstructScreenText();
                }

                break;
            }
        }
    }

    void SetReady()
    {
        Debug.Log("TCP: SetReady() called");

        readyToPlay = teamFilledUp;

        if (readyToPlay)
        {
            readyIndicator.material.SetColor("_BaseColor", teamColor);// = teamReadyMaterial;
            readyText.text = "Ready!";
        }
        else
        {
            Debug.Log("TCP: Team in " + name + " is not ready to play. Aborting");
            return;
        }

        int readyTeams = 0;
        int emptyPods = 0;

        foreach (TeamCreationPod pod in instances)
        {
            if (pod.readyToPlay) readyTeams++;
            else if (pod.teamEmpty) emptyPods++;
        }

        bool allPodsReadyTeamsOrEmpty = (readyTeams + emptyPods) == instances.Count;

        bool allPlayersAccountedFor = GalacticGamesManager.Instance.AllPlayersAccountedFor();

        if (allPodsReadyTeamsOrEmpty && allPlayersAccountedFor)
        {
            foreach (TeamCreationPod pod in instances)
                if (pod.readyToPlay) pod.readyText.text = "GO!";

            ScreenDebugMessage += "\n All pods ready or empty \nand all players accounted for \nChanged button text to GO!";

            /*
            if (readyToPlay) ScreenDebugMessage += "\n This pod readyToPlay";
            else ScreenDebugMessage += "\n This pod NOT readyToPlay";
            ConstructScreenText();
            */


            

            Debug.Log("TCP: All pods have ready teams or is empty and all players accounted for. Calling GameManager::StartGame()");
            GalacticGamesManager.Instance.StartCompetition();
        }
        else
        {
            if (!allPodsReadyTeamsOrEmpty) Debug.Log("GGM: Cannot start game because all pods do not have ready teams or are empty");
            if (!allPlayersAccountedFor) Debug.Log("GGM: Cannot start game because not all players are accounted for");
        }
    }

    public void OnTeamMemberLeftRoom(int teamMemberIndex)
    {
        Debug.Log("TCP: OnTeamMemberLeftRoom called with team member index " + teamMemberIndex);

        if (memberClientIDToName.ContainsKey(teamMembers[teamMemberIndex]))
            Debug.Log("TCP: ClientID was found as key in dictionary and points to " + memberClientIDToName[teamMembers[teamMemberIndex]]);

        else
        {
            Debug.Log("TCP: ClientID was NOT found in dictionary. teamMembers[teamMemberIndex] contains clientID " + teamMembers[teamMemberIndex] + 
                " and memberClientIDToName contains clientID");

            foreach (KeyValuePair<int, string> pair in memberClientIDToName)
            {
                Debug.Log(pair.Key);
            }
        }

        memberClientIDToName.Remove(teamMembers[teamMemberIndex]);
        ConstructScreenText();

        teamMembers[teamMemberIndex] = -1;
        teamFilledUp = readyToPlay = false;

        readyIndicator.material.SetColor("_BaseColor", Color.white); // = teamNotReadyMaterial;
    }

    public void DisableReadyButton()
    {
        readyButton.gameObject.layer = 9; //Ignore
    }

    //Disappear animatic

    public void StartDisappearAnimatic()
    {
        capacityIndicator.material = disappearMaterial;
        disappearAnimatic.Run();
    }

    void Hide()
    {
        gameObject.SetActive(false);

        /*
        teamSizeText.gameObject.SetActive(false);
        floor.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
        teamList.gameObject.SetActive(false);
        */
    }

    public string ScreenDebugMessage;

    public void ConstructScreenText()
    {
        string temp = "";

        //temp += "\nTeam Members: ";
        //foreach (int teamMember in TeamMembers) temp += (teamMember.ToString() + ", "); 
        

        Debug.Log("TCP1: teamMembers count: " + teamMembers.Count);


        for (int i = 0; i < teamMembers.Count; i++)
            if (teamMembers[i] != -1 && MemberClientIDToName.ContainsKey(teamMembers[i]))
                temp += (/*'\n' + "index: " + i + ", clientID: " + teamMembers[i] + ", " + */MemberClientIDToName[teamMembers[i]]);


        if (teamFilledUp) temp += "\n- Team full -";

        //temp += ScreenDebugMessage;

        teamList.text = temp;
    }

    //Networking

    protected override void OnRealtimeModelReplaced(TeamCreationPod_Model previousModel, TeamCreationPod_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.clientDoneDisablingNonTeamGameplayObjectsDidChange -= ClientDoneDisablingNonTeamGameplayObjectsDidChange;
            previousModel.index0DoneFilteringDidChange -= Index0DoneFilteringDidChange;
            previousModel.index1DoneFilteringDidChange -= Index1DoneFilteringDidChange;
            previousModel.index2DoneFilteringDidChange -= Index2DoneFilteringDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                currentModel.clientDoneDisablingNonTeamGameplayObjects = -1;
                currentModel.index0DoneFiltering = false;
                currentModel.index1DoneFiltering = false;
                currentModel.index2DoneFiltering = false;
            }

            // Update data to match the new model
            UpdateClientDoneDisablingNonTeamGameplayObjects();
            UpdateIndex0DoneFiltering();
            UpdateIndex1DoneFiltering();
            UpdateIndex2DoneFiltering();


            //Register for events so we'll know if data changes later
            currentModel.clientDoneDisablingNonTeamGameplayObjectsDidChange += ClientDoneDisablingNonTeamGameplayObjectsDidChange;
            currentModel.index0DoneFilteringDidChange += Index0DoneFilteringDidChange;
            currentModel.index1DoneFilteringDidChange += Index1DoneFilteringDidChange;
            currentModel.index2DoneFilteringDidChange += Index2DoneFilteringDidChange;
        }
    }

    

    List<int> clientsDoneDisablingGameplayObjects = new List<int>();

    public List<int> ClientsDoneFiltering { get => clientsDoneDisablingGameplayObjects; }

    public int ClientDoneDisablingGameplayObject 
    { 
        set { model.clientDoneDisablingNonTeamGameplayObjects = value; } 
    }

    void ClientDoneDisablingNonTeamGameplayObjectsDidChange(TeamCreationPod_Model model, int client)
    {
        UpdateClientDoneDisablingNonTeamGameplayObjects();
    }

    void UpdateClientDoneDisablingNonTeamGameplayObjects()
    {
        int client = model.clientDoneDisablingNonTeamGameplayObjects;

        if (model.clientDoneDisablingNonTeamGameplayObjects != -1) ;

        if (client != -1 && !clientsDoneDisablingGameplayObjects.Contains(client))
            clientsDoneDisablingGameplayObjects.Add(client);
    }

    //---Filtering

    public int numClientsNotifyingDoneFiltering = 0;

    //Index 0
    bool index0DoneFiltering = false;
    public bool Index0DoneFiltering { get => index0DoneFiltering; set => model.index0DoneFiltering = value; }
    
    void Index0DoneFilteringDidChange(TeamCreationPod_Model model, bool done)
    {
        UpdateIndex0DoneFiltering();
    }

    void UpdateIndex0DoneFiltering()
    {
        if (model.index0DoneFiltering == true) numClientsNotifyingDoneFiltering++;
        index0DoneFiltering = model.index0DoneFiltering;
    }

    //Index 1
    bool index1DoneFiltering = false;
    public bool Index1DoneFiltering { get => index1DoneFiltering; set => model.index1DoneFiltering = value; }
    
    void Index1DoneFilteringDidChange(TeamCreationPod_Model model, bool done)
    {
        UpdateIndex1DoneFiltering();
    }

    void UpdateIndex1DoneFiltering()
    {
        if (model.index1DoneFiltering == true) numClientsNotifyingDoneFiltering++;
        index1DoneFiltering = model.index1DoneFiltering;
    }

    //Index 2
    bool index2DoneFiltering = false;
    public bool Index2DoneFiltering { get => index2DoneFiltering; set => model.index2DoneFiltering = value; }
    
    void Index2DoneFilteringDidChange(TeamCreationPod_Model model, bool done)
    {
        UpdateIndex2DoneFiltering();
    }

    void UpdateIndex2DoneFiltering()
    {
        if (model.index2DoneFiltering == true) numClientsNotifyingDoneFiltering++;
        index2DoneFiltering = model.index2DoneFiltering;
    }
}
