using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class ZipLine : MonoBehaviour
{
    [SerializeField]
    EZipLine point;

    [SerializeField]
    GameObject PF_TransportLine;

    [SerializeField]
    GameObject otherPoint;

    GameObject transportLine;

    Vector3 startToEnd;

    RaycastHit structureHit;



    // Start is called before the first frame update
    void Start()
    {
        if (point == EZipLine.START) transportLine = Instantiate<GameObject>(PF_TransportLine, transform.position, transform.rotation);   
    }

    void Update()
    {
        if (point == EZipLine.START)
        {
            startToEnd = otherPoint.transform.position - transform.position;

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
}
