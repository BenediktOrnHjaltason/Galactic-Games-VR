using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabHandle : MonoBehaviour
{
    StructureSync parentStructureSync;

    GameObject handPositionReference;

    GameObject HandPositionReference { get => handPositionReference; }

    // Start is called before the first frame update
    void Start()
    {
        parentStructureSync = transform.root.GetComponent<StructureSync>();

        //Freely controllable structures with GrabHandles is not allowed to be moved while players are climbing it
        //to reduce the change of nausea because of unexpected movement. Structures with restricted movement are
        //allowed to be moved
        Structure_RestrictedMove srm = transform.root.GetComponent<Structure_RestrictedMove>();
        if (srm) parentStructureSync = null;

        handPositionReference = transform.GetChild(0).gameObject;
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
