using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

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

    Transform attractionPoint;
    RealtimeTransform rtt;
    Rigidbody rb;

    Vector3 attractionPointToRoot = Vector3.zero;

    List<OVRPlayerController> playersInReach = new List<OVRPlayerController>();

    //---- beams
    float changeInterval = 0.166f;
    float nextTimeToChange = 0;
    float timeAtChange = 0;
    GameObject dummyObject;

    //--------- Default beams
    List<LineRenderer> defaultBeams = new List<LineRenderer>();

    Vector3 pathToEdge;
    Vector3 direction;

    //--------- PlayerLines
    List<LineRenderer> playerBeams = new List<LineRenderer>();
    



    // Start is called before the first frame update
    void Start()
    {
        dummyObject = new GameObject();
        dummyObject.layer = 9; //Ignore

        attractionPoint = transform.GetComponentInParent<Transform>();
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

        playerBeams.Add(transform.GetChild(1).transform.GetChild(0).GetComponent<LineRenderer>());
        playerBeams.Add(transform.GetChild(1).transform.GetChild(1).GetComponent<LineRenderer>());
        playerBeams.Add(transform.GetChild(1).transform.GetChild(2).GetComponent<LineRenderer>());

    }

    private void FixedUpdate()
    {
        //Handle beams

        if (Time.time > nextTimeToChange)
        {
            nextTimeToChange += changeInterval;

            //Default beams 
            foreach (LineRenderer beam in defaultBeams)
            {
                beam.SetPosition(0, attractionPoint.transform.position);

                direction = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
                pathToEdge = direction.normalized * transform.localScale.x / 2;


                dummyObject.transform.rotation = Quaternion.LookRotation(pathToEdge * 10);

                beam.SetPosition(1, attractionPoint.transform.position + (pathToEdge / 2) +
                    dummyObject.transform.up * direction.x +
                    dummyObject.transform.right * direction.y);

                beam.SetPosition(2, attractionPoint.transform.position + pathToEdge);
            }

            //Beams to players
            if (playersInReach.Count > 0)
            {
                for (int i = 0; i < playersInReach.Count; i++)
                {
                    playerBeams[i].SetPosition(0, transform.position);

                    direction = playersInReach[i].transform.position - transform.position;

                    playerBeams[i].SetPosition(1, transform.position + (direction / 2) +
                    dummyObject.transform.up * (direction.x /5) +
                    dummyObject.transform.right * (direction.y /5));

                    playerBeams[i].SetPosition(2, transform.position + direction);
                }
            }

            else 
                foreach (LineRenderer beam in playerBeams)
                {
                    beam.SetPosition(0, transform.position);
                    beam.SetPosition(1, transform.position);
                    beam.SetPosition(2, transform.position);
                }
        }

        

        if (!rtt.realtime.connected) return;

        //Place self
        if (rtt.isUnownedSelf) rtt.RequestOwnership();
        else if (rtt.isOwnedLocallySelf)
        {
            attractionPointToRoot = transform.root.position - attractionPoint.position;

            if ((transform.position - transform.root.transform.position).sqrMagnitude > 0.02)
                rb.AddForce(attractionPointToRoot * autoForce);
        }

        //Attract players
        foreach (OVRPlayerController player in playersInReach)
        {
            if (player.HeadRealtimeView.isOwnedLocallySelf) 
                player.Controller.Move((attractionPoint.position - player.transform.position).normalized * 0.04f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(14))
        {
            OVRPlayerController player = other.GetComponent<OVRPlayerController>();

            if (player)
            {
                player.GravityModifier = 0.0f;
                playersInReach.Add(player);
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
                RemovePlayerFromInfluence(player);
            }
        }
    }
    

    void RemovePlayerFromInfluence(OVRPlayerController player)
    {
        player.GravityModifier = 0.04f;
        playersInReach.Remove(player);
    }
}
