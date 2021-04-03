using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class PlayerSensor : MonoBehaviour
{
    StructureSync structureSync;
    Transform parentTransform;

    int layer_Player = 14;
    Vector3 up = new Vector3(0,1,0);

    float xMultiplier;
    float yMultiplier;
    float zMultiplier;

    float extentPadding = 1.2f;
    Vector3 offsettToPlatform = new Vector3(0, 0.3f, 0);

    float dot1;
    float dot2;

    // Start is called before the first frame update
    void Start()
    {
        
        parentTransform = transform.GetComponentInParent<Transform>();
        structureSync = parentTransform.GetComponentInParent<StructureSync>();
    }


    private void FixedUpdate()
    {
        transform.localScale = CalculateLocalScale();
        transform.position = transform.root.position + offsettToPlatform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(layer_Player))
        {
            structureSync.PlayersOccupying = structureSync.PlayersOccupying + 1;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer.Equals(layer_Player))
        {
            structureSync.PlayersOccupying = structureSync.PlayersOccupying - 1;
        }
    }

    Vector3 CalculateLocalScale()
    {
        xMultiplier = yMultiplier = zMultiplier = 0;

        //x
        dot1 = Vector3.Dot(transform.right, up);
        dot2 = Vector3.Dot(-transform.right, up);

        if (dot1 > 0) xMultiplier = dot1;
        else if (dot2 > 0) xMultiplier = dot2;

        //y
        dot1 = Vector3.Dot(transform.up, up);
        dot2 = Vector3.Dot(-transform.up, up);

        if (dot1 > 0) yMultiplier = dot1;
        else if (dot2 > 0) yMultiplier = dot2;

        //z
        dot1 = Vector3.Dot(transform.forward, up);
        dot2 = Vector3.Dot(-transform.forward, up);

        if (dot1 > 0) zMultiplier = dot1;
        else if (dot2 > 0) zMultiplier = dot2;

        return new Vector3(1 + (extentPadding * xMultiplier) / parentTransform.lossyScale.x, 
                           1 + (extentPadding * yMultiplier) / parentTransform.lossyScale.y, 
                           1 + (extentPadding * zMultiplier) / parentTransform.lossyScale.z);
    }
}
