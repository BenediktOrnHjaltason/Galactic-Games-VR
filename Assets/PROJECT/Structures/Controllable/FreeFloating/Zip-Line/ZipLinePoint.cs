using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;

public class ZipLinePoint : MonoBehaviour
{
    [SerializeField]
    EZipLine point;

    [SerializeField]
    GameObject PF_TransportLine;

    [SerializeField]
    GameObject otherPoint;

    GameObject transportLine;
    ZipLineTransport zipLineTransport;

    Vector3 startToEnd;

    RaycastHit structureHit;

    Realtime realtime;

    // Start is called before the first frame update
    void Start()
    {
        if (point == EZipLine.START)
        {
            realtime  = GameObject.Find("Realtime").GetComponent<Realtime>();

            realtime.didConnectToRoom += DidConnectToRoom;
        }
    }

    void Update()
    {
        if (transportLine)
        {
            startToEnd = otherPoint.transform.position - transform.position;
            zipLineTransport.TransportDirection = startToEnd;

            if (Vector3.Dot(startToEnd.normalized, transform.forward) > 0.96f && Vector3.Dot(-startToEnd.normalized, otherPoint.transform.forward) > 0.96f)
            {
                transportLine.transform.SetPositionAndRotation(transform.position + (startToEnd / 2), Quaternion.LookRotation(startToEnd));
                transportLine.transform.localScale = new Vector3(0.25f, 0.25f, startToEnd.magnitude);
            }

            else
            {
                transportLine.transform.position = transform.position;
                transportLine.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
    }

    private void DidConnectToRoom(Realtime realtime)
    {
        transportLine = Realtime.Instantiate("PF_TransportLine",
                                                  ownedByClient: false,
                                                  preventOwnershipTakeover: false,
                                                  destroyWhenOwnerOrLastClientLeaves: true,
                                                  useInstance: GameObject.Find("Realtime").GetComponent<Realtime>());

        GetComponentInParent<LocalState>().AddSubObject(transportLine);



        transportLine.transform.position = transform.position;
        transportLine.transform.rotation = transform.rotation;

        zipLineTransport = transportLine.GetComponent<ZipLineTransport>();
    }
}
