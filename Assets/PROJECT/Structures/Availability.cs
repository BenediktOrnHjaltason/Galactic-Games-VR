using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class Availability : RealtimeComponent<Availability_Model>
{
    //To make sure only one player can manipulate this object at any given time
    bool available = true;


    protected override void OnRealtimeModelReplaced(Availability_Model previousModel, Availability_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.availableDidChange -= AvailableDidChange;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current availability value.
            if (currentModel.isFreshModel)
                currentModel.available = available;

            // Update availability to match the new model
            UpdateAvailability();

            // Register for events so we'll know if availability changes later
            currentModel.availableDidChange += AvailableDidChange;
        }
    }

    private void AvailableDidChange(Availability_Model model, bool available)
    {
        UpdateAvailability();

        Debug.Log("Availability on server changed to " + model.available.ToString());
    }

    private void UpdateAvailability()
    {
        available = model.available;
    }

    /// <summary>
    /// Available to being moved (Depends on someone is standing on it or not)
    /// </summary>

    public bool Available
    {
        set
        {
            model.available = value;
        }

        get
        {
            return available;
        }
    }
}
