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



    // Start is called before the first frame update
    void Start()
    {
        if (point == EZipLine.START) transportLine = Instantiate<GameObject>(PF_TransportLine, transform.position, transform.rotation);   
    }

    // Update is called once per frame
    void Update()
    {
        if (point == EZipLine.START)
        {
            startToEnd = otherPoint.transform.position - transform.position;

            transportLine.transform.SetPositionAndRotation(transform.position + (startToEnd / 2), Quaternion.LookRotation(startToEnd));
            transportLine.transform.localScale = new Vector3(0.25f, 0.25f, startToEnd.magnitude); 
        }
    }
}
