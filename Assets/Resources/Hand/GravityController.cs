using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    LineRenderer line;

    bool currentlyUsing = false;

    RaycastHit structureHit;
    GameObject structure;
    Rigidbody structureRB;


    enum EMode
    {
        NONE,
        SCANNING,
        CONTROLLING
    } EMode mode = EMode.NONE;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    public bool Operating()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            currentlyUsing = true;
            mode = EMode.SCANNING;
        }

        else if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            currentlyUsing = false;
            mode = EMode.NONE;
        }

        SetLineRenderer(mode);


        //************ Operation logic **************//

        if (mode == EMode.NONE) return false;

        if (mode == EMode.SCANNING )
        {
         
            if (Physics.Raycast(transform.position, transform.forward, out structureHit, Mathf.Infinity, 1 << 10))
            {
                Debug.Log("GravityController: LOCKED ON FREEFLOATING STRUCTURE");

                structure = structureHit.collider.gameObject;

                structureRB = structure.GetComponent<Rigidbody>();

                mode = EMode.CONTROLLING;
            }
        }

        else if (mode == EMode.CONTROLLING)
        {
            Debug.Log("GravityController: IN CONTROLLING MODE");

            structureRB.AddForce(new Vector3(0, 0.2f, 0));
        }

        void SetLineRenderer(EMode mode)
        {
            switch (mode)
            {
                case EMode.NONE:
                    line.SetPosition(0, transform.position);
                    line.SetPosition(1, transform.position);
                    line.SetPosition(2, transform.position);
                    break;

                case EMode.SCANNING:
                    line.SetPosition(0, transform.position);
                    line.SetPosition(1, transform.position);
                    line.SetPosition(2, transform.position + transform.forward * 1000);
                    break;

                case EMode.CONTROLLING:
                    line.SetPosition(0, transform.position);
                    line.SetPosition(1, transform.position);
                    line.SetPosition(2, transform.position + transform.forward * 1000);
                    break;
            }
        }




        return currentlyUsing;
    }
}
