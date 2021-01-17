using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabHandle : MonoBehaviour
{
    StructureSync parentStructureSync;

    // Start is called before the first frame update
    void Start()
    {
        parentStructureSync = transform.root.GetComponent<StructureSync>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (parentStructureSync) parentStructureSync.PlayersOccupying++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (parentStructureSync) parentStructureSync.PlayersOccupying--;
    }
}
