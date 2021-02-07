using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using System;

public class InteractiveScreenSync : RealtimeComponent<InteractiveScreenSync_Model>
{
    protected override void OnRealtimeModelReplaced(InteractiveScreenSync_Model previousModel, InteractiveScreenSync_Model currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.executingAnythingDidChange -= ExecutingAnythingDidChange;
            previousModel.executingMinMaxDidChange -= ExecutingMinMaxDidChange;
            previousModel.executingSlideChangeDidChange -= ExecutingSlideChangeDidChange;
            previousModel.activeSlideIndexDidChange -= ActiveSlideIndexDidChange;
            previousModel.previousSlideIndexDidChange -= PreviousSlideIndexDidChange;
            previousModel.slideChangedDidChange -= SlideChanged;
        }

        if (currentModel != null)
        {
            // If this is a model that has no data set on it, populate it with the current value.
            if (currentModel.isFreshModel)
            {
                currentModel.executingAnything = false;
                currentModel.executingMinMax = false;
                currentModel.executingSlideChange = false;
                currentModel.activeSlideIndex = activeSlideIndex;
                currentModel.previousSlideIndex = previousSlideIndex;
                currentModel.slideChanged = false;
            }

            // Update data to match the new model
            UpdateExecutingAnything();
            UpdateExecutingMinMax();
            UpdateExecutingSlideChange();
            UpdateActiveSlideIndex();
            UpdatePreviousSlideIndex();


            //Register for events so we'll know if data changes later
            currentModel.executingAnythingDidChange += ExecutingAnythingDidChange;
            currentModel.executingMinMaxDidChange += ExecutingMinMaxDidChange;
            currentModel.executingSlideChangeDidChange += ExecutingSlideChangeDidChange;
            currentModel.activeSlideIndexDidChange += ActiveSlideIndexDidChange;
            currentModel.previousSlideIndexDidChange += PreviousSlideIndexDidChange;
            currentModel.slideChangedDidChange += SlideChanged;

        }
    }

    bool executingAnything = false;
    public bool ExecutingAnything { set => model.executingAnything = value; get => executingAnything; }

    void ExecutingAnythingDidChange(InteractiveScreenSync_Model model, bool state)
    {
        UpdateExecutingAnything();
    }

    void UpdateExecutingAnything()
    {
        executingAnything = model.executingAnything;
    }

    //----

    bool executingMinMax = false;
    public bool ExecutingMinMax { set => model.executingMinMax = value;  get => executingMinMax; }

    void ExecutingMinMaxDidChange(InteractiveScreenSync_Model model, bool state)
    {
        UpdateExecutingMinMax();
    }

    void UpdateExecutingMinMax()
    {
        executingMinMax = model.executingMinMax;
    }

    //----

    bool executingSlideChange = false;
    public bool ExecutingSlideChange { set => model.executingSlideChange = value; get => executingSlideChange; }

    void ExecutingSlideChangeDidChange(InteractiveScreenSync_Model model, bool state)
    {
        UpdateExecutingSlideChange();
    }

    void UpdateExecutingSlideChange()
    {
        executingSlideChange = model.executingSlideChange;
    }

    //----

    int activeSlideIndex = 0;
    public int ActiveSlideIndex { set => model.activeSlideIndex = value; get => activeSlideIndex; }

    void ActiveSlideIndexDidChange(InteractiveScreenSync_Model model, int index)
    {
        UpdateActiveSlideIndex();
    }

    void UpdateActiveSlideIndex()
    {
        activeSlideIndex = model.activeSlideIndex;
    }

    //----

    int previousSlideIndex = 0;
    public int PreviousSlideIndex { set => model.previousSlideIndex = value; get => previousSlideIndex; }

    void PreviousSlideIndexDidChange(InteractiveScreenSync_Model model, int index)
    {
        UpdatePreviousSlideIndex();
    }

    void UpdatePreviousSlideIndex()
    {
        previousSlideIndex = model.previousSlideIndex;
    }

    //------ (Just to trigger slide change in everyone)
    public bool SlidesChanged { set => model.slideChanged = value; }

    public event Action OnSlidesChanged;

    void SlideChanged(InteractiveScreenSync_Model model, bool dummy)
    {
        OnSlidesChanged?.Invoke();
    }

    private void OnConnectedToServer()
    {
        
    }
}
