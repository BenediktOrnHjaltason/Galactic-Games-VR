using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using System;

public class HandSync : RealtimeComponent<HandSync_Model>
{
    [SerializeField]
    Material defaultMaterial;

    [SerializeField]
    Material grabbingGrabHandleMaterial;

    MeshRenderer mesh;

    private void Awake()
    {
        mesh = transform.Find("HandMesh").GetComponent<MeshRenderer>();
    }

    protected override void OnRealtimeModelReplaced(HandSync_Model previousModel, HandSync_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.grabbingGrabHandleDidChange -= GrabbingGrabHandleDidChange;

        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                currentModel.grabbingGrabHandle = false;
            }

            // Update data to match the new model
            UpdateHandMesh();

            //Register for events so we'll know if data changes later
            currentModel.grabbingGrabHandleDidChange += GrabbingGrabHandleDidChange;

        }
    }

    public bool GrabbingGrabHandle { set => model.grabbingGrabHandle = value; get => model.grabbingGrabHandle; }

    private void GrabbingGrabHandleDidChange(HandSync_Model mode, bool grabbing)
    {
        UpdateHandMesh();
    }

    private void UpdateHandMesh()
    {
        mesh.material = (model.grabbingGrabHandle) ? grabbingGrabHandleMaterial : defaultMaterial;
    }

}
