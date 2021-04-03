using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using Unity.Profiling;

/*
    The player sensor is the large field detecting if players are in are of influence,
    and pushing them to the core if they are
 */


public class AttractorRift_PlayerSensor : MonoBehaviour
{

    [SerializeField]
    float autoForce;

    [SerializeField]
    AttractorRift_Core core;

    [SerializeField]
    GameObject anchor;

    RealtimeTransform rtt;
    Rigidbody rb;

    
    //Only one instance of OVRPlayerController exists per client, only avatar parts parented to the anchors are network replicated.
     
    OVRPlayerController playerInReach = null;

    static List<AttractorRift_PlayerSensor> allSensors = new List<AttractorRift_PlayerSensor>();

    //---- beams
    float changeInterval = 0.466f;
    float nextTimeToChange = 0;
    GameObject dummyObject;

    //--------- Default beams
    List<LineRenderer> defaultBeams = new List<LineRenderer>();

    Vector3 pathToEdge;
    Vector3 randomDirection;

    Vector3[] offsetRight = new Vector3[4];
    Vector3[] offsetUp = new Vector3[4];

    Vector3[] endPointsInitial = new Vector3[4];
    Vector3[] endPointsMoveTo = new Vector3[4];

    //--------- PlayerLines
    LineRenderer playerBeam;
    



    // Start is called before the first frame update
    void Start()
    {
        dummyObject = new GameObject();
        dummyObject.layer = 9; //Ignore

        rtt = transform.GetComponentInParent<RealtimeTransform>();
        rb = GetComponentInParent<Rigidbody>();

        if (core)
        {
            core.OnPlayerReachedCore += RemovePlayerFromInfluence;
        }

        defaultBeams.Add(transform.GetChild(0).transform.GetChild(0).GetComponent<LineRenderer>());
        defaultBeams.Add(transform.GetChild(0).transform.GetChild(1).GetComponent<LineRenderer>());
        defaultBeams.Add(transform.GetChild(0).transform.GetChild(2).GetComponent<LineRenderer>());
        defaultBeams.Add(transform.GetChild(0).transform.GetChild(3).GetComponent<LineRenderer>());

        playerBeam = transform.GetChild(1).transform.GetChild(0).GetComponent<LineRenderer>();

        allSensors.Add(this);
    }

    Vector3 riftPosToAnchorPos = Vector3.zero;

    private void FixedUpdate()
    {
        //Handle beams
        
        if (Time.time > nextTimeToChange)
        {
            nextTimeToChange += changeInterval;

            //Default beams 
            for (int i = 0; i < defaultBeams.Count; i++)
            {
                randomDirection = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
                pathToEdge = randomDirection.normalized * (transform.localScale.x / 15.0f);


                dummyObject.transform.rotation = Quaternion.LookRotation(pathToEdge * 10);

                offsetUp[i] = dummyObject.transform.up * (randomDirection.x / 10);
                offsetRight[i] = dummyObject.transform.right * (randomDirection.y / 10);

                defaultBeams[i].SetPosition(1, (pathToEdge / 2) +
                    offsetUp[i] + offsetRight[i]);

                endPointsInitial[i] = pathToEdge + offsetUp[i] + offsetRight[i];
                endPointsMoveTo[i] = pathToEdge - offsetUp[i] * 1.5f - offsetRight[i] * 1.5f;

                defaultBeams[i].SetPosition(2, endPointsInitial[i]);
            }

            //Beams to players
            if (playerInReach)
            {

                playerBeam.SetPosition(0, transform.position);

                randomDirection = playerInReach.transform.position - transform.position;

                playerBeam.SetPosition(1, transform.position + (randomDirection / 2) +
                dummyObject.transform.up * (randomDirection.x /5) +
                dummyObject.transform.right * (randomDirection.y /5));

                playerBeam.SetPosition(2, transform.position + randomDirection);
                
            }

            else 
            {
                playerBeam.SetPosition(0, transform.position);
                playerBeam.SetPosition(1, transform.position);
                playerBeam.SetPosition(2, transform.position);
            }
        }

        else
        {
            //Move tip of beams along edges
            for (int i = 0; i < defaultBeams.Count; i++)
            {
                defaultBeams[i].SetPosition(2, Vector3.Lerp(endPointsInitial[i], endPointsMoveTo[i], (nextTimeToChange - Time.time) / changeInterval));
            }

            //Place playerBeams inside core when not used
            if (!playerInReach)
            {
                playerBeam.SetPosition(0, transform.position);
                playerBeam.SetPosition(1, transform.position);
                playerBeam.SetPosition(2, transform.position);
            }
            
            else playerBeam.SetPosition(0, transform.position);
        }

        if (!rtt.realtime.connected) return;

        //Place self
        if (rtt.ownerIDSelf == -1) rtt.RequestOwnership();

        else if (rtt.isOwnedLocallySelf)
        {
            riftPosToAnchorPos = anchor.transform.position - transform.position;

            //if ((transform.position - anchor.transform.position).sqrMagnitude > 0.02)
                rb.AddForce(riftPosToAnchorPos * autoForce);
        }

        //Attract player
        if (playerInReach && !playerInReach.GrabbingAnything) 
            playerInReach.Controller.Move((transform.position - playerInReach.transform.position).normalized * 0.04f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            OVRPlayerController player = other.GetComponent<OVRPlayerController>();

            if (player)
            {
                player.GravityModifier = 0.0f;
                playerInReach = player;
            }
        }
    }

    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            OVRPlayerController player = other.GetComponent<OVRPlayerController>();

            if (player)
            {
                RemovePlayerFromInfluence(player, false);
            }
        }
    }
    

    void RemovePlayerFromInfluence(OVRPlayerController player, bool removeFromAll)
    {
        if (removeFromAll)
            foreach (AttractorRift_PlayerSensor playerSensor in allSensors)
            {
                if (playerSensor.playerInReach == player)
                {
                    playerSensor.playerInReach.GravityModifier = 0.04f;
                    playerSensor.playerInReach = null;
                }
            }

        else if (playerInReach == player)
        {
            bool playerInfluencedByOtherRift = false;

            foreach (AttractorRift_PlayerSensor playerSensor in allSensors)
                if (playerSensor != this && playerSensor.playerInReach == player) playerInfluencedByOtherRift = true;

            if (!playerInfluencedByOtherRift) playerInReach.GravityModifier = 0.04f;

            playerInReach = null;
        }
    }
}
