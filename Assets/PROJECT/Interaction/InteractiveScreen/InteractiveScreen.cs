using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Normal.Realtime;

enum EPopUpDirection
{
    UP,
    RIGHT_UPPER,
    RIGHT_CENTER,
    RIGHT_LOWER,
    DOWN,
    LEFT_UPPER,
    LEFT_CENTER,
    LEFT_LOWER,
}

[Serializable]
public class FramePopUpPositions
{
    public Vector3 up;
    public Vector3 rightUpper;
    public Vector3 rightCenter;
    public Vector3 rightLower;
    public Vector3 down;
    public Vector3 leftUpper;
    public Vector3 leftCenter;
    public Vector3 leftLower;
}

public class InteractiveScreen : MonoBehaviour
{

    [SerializeField]
    EPopUpDirection popUpDirection;

    [SerializeField]
    AnimationCurve fastInEaseOut;


    //Frame

    [SerializeField]
    GameObject framePivotBase;

    [SerializeField]
    RealtimeTransform framePivotRtt;

    [SerializeField]
    GameObject frame;

    //Buttons

    [SerializeField]
    InteractButton buttonMinMax;

    [SerializeField]
    InteractButton buttonForward;

    [SerializeField]
    InteractButton buttonBackwards;

    [SerializeField]
    TextMeshPro MinMaxIcon;

    //Slides
    [SerializeField]
    List<Material> slidesGraphics;

    [SerializeField]
    GameObject slidesPivotBase;

    [SerializeField]
    Vector3 slidesPivotBaseScale;

    List<GameObject> slides = new List<GameObject>();

    List<MeshRenderer> slidesMeshRenderers = new List<MeshRenderer>();

    [SerializeField]
    Vector3 slidesScale;

    [SerializeField]
    Vector3 slidesRotation;

    [SerializeField]
    FramePopUpPositions popUpFramePositions;

    //----Progress Bar

    [SerializeField]
    GameObject progressBarPivot;

    float progressBarIncrement;

    [SerializeField]
    MeshRenderer sizesReference;


    //----**** Operation ****----//

    public event Action<GameObject> OnButtonHighlighted;

    float transitionTime = 0;

    //-- MinMax

    

    //-- ChangeSlide

    ESlidesOperationPhase slideChangePhase;

    bool pingPong = false;


    InteractiveScreenSync screenSync;
    Realtime realtime;


    void Awake()
    {
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();

        screenSync = GetComponent<InteractiveScreenSync>();
        screenSync.OnSlidesChanged += SetScreenVisuals;
        screenSync.OnMinMaxChanged += SetIcons;
        screenSync.OnEnterRoom += Initialize;

        //Make slides
        for (int i = 0; i < slidesGraphics.Count; i++)
        {
            slides.Add(GameObject.CreatePrimitive(PrimitiveType.Plane));
            slides[i].name = "slide " + i.ToString();
            slides[i].transform.SetParent(slidesPivotBase.transform);
            slides[i].transform.localPosition = Vector3.zero;
            slides[i].transform.localScale = slidesScale;
            slides[i].transform.rotation = Quaternion.Euler(slidesRotation);
            slides[i].layer = 9;

            MeshRenderer mr = slides[i].GetComponent<MeshRenderer>();
            mr.material = slidesGraphics[i];
            if (i != 0) mr.enabled = false;

            slidesMeshRenderers.Add(mr);
        }

        //ProgressBar
        progressBarIncrement = 1.0f / (float)slides.Count;
        progressBarPivot.transform.localScale = new Vector3(progressBarIncrement, 1, 1);

        //Initialize
        MinMaxIcon.text = "?";
        sizesReference.enabled = false;
        SetFrameLocalPosition();


        //Setup buttons

        buttonMinMax.OnExecute += ToggleMinMax;

        if (slides.Count > 1)
        {
            buttonForward.OnExecute += NextFrame;
            buttonBackwards.OnExecute += PreviousFrame;
        }

        else
        {
            //Disable buttons and progress bar
            buttonForward.gameObject.SetActive(false);
            buttonBackwards.gameObject.SetActive(false);
            progressBarPivot.transform.parent.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (!framePivotRtt.isOwnedLocallySelf) return;

        //Operate screen
        if (screenSync.ExecutingMinMax)
        {
            if (transitionTime < 1) transitionTime += 0.1f;

            //-----

            if (screenSync.OpenOrClosed == EScreenState.OPEN)
            {
                framePivotBase.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, fastInEaseOut.Evaluate(transitionTime));
            }
            else
            {
                framePivotBase.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, fastInEaseOut.Evaluate(transitionTime));
            }

            //-----

            if (transitionTime >= 1)
            {
                //Sync open or closed!
                screenSync.OpenOrClosed = (screenSync.OpenOrClosed == EScreenState.OPEN) ? EScreenState.CLOSED : EScreenState.OPEN;

                MinMaxIcon.text = (screenSync.OpenOrClosed == EScreenState.OPEN) ? "--" : "?";

                transitionTime = 0.0f;

                screenSync.ExecutingMinMax = false;
                screenSync.ExecutingAnything = false;
            }
        }

        else if (screenSync.ExecutingSlideChange)
        {
            if (slideChangePhase == ESlidesOperationPhase.RETRACTING)
            {
                if (transitionTime < 1) transitionTime += 0.2f;
                //----

                slidesPivotBase.transform.localScale = Vector3.Lerp(slidesPivotBaseScale, Vector3.zero, fastInEaseOut.Evaluate(transitionTime));

                //----
                if (transitionTime >= 1)
                {
                    pingPong = !pingPong;

                    screenSync.SlidesChanged = pingPong;

                    transitionTime = 0.0f;
                    slideChangePhase = ESlidesOperationPhase.EXPANDING;
                }
            }

            else if (slideChangePhase == ESlidesOperationPhase.EXPANDING)
            {
                if (transitionTime < 1) transitionTime += 0.2f;
                //----

                slidesPivotBase.transform.localScale = Vector3.Lerp(Vector3.zero, slidesPivotBaseScale, fastInEaseOut.Evaluate(transitionTime));

                //----
                if (transitionTime >= 1)
                {
                    transitionTime = 0.0f;
                    screenSync.ExecutingSlideChange = false;
                    screenSync.ExecutingAnything = false;
                }
            }
        }
    }

    public void HandleButtonHighLights(GameObject button)
    {
        OnButtonHighlighted?.Invoke(button);
    }

    public void NextFrame()
    {
        if (!framePivotRtt.isOwnedLocallySelf) framePivotRtt.RequestOwnership();

        if (!screenSync.ExecutingAnything)
        {
            screenSync.ExecutingAnything = true;


            screenSync.PreviousSlideIndex = screenSync.ActiveSlideIndex;

            screenSync.ActiveSlideIndex++;

            if (screenSync.ActiveSlideIndex > slides.Count - 1) screenSync.ActiveSlideIndex = 0;

            


            slideChangePhase = ESlidesOperationPhase.RETRACTING;
            screenSync.ExecutingSlideChange = true;
        }
    }

    public void PreviousFrame()
    {
        if (!framePivotRtt.isOwnedLocallySelf) framePivotRtt.RequestOwnership();

        if (!screenSync.ExecutingAnything)
        {
            screenSync.ExecutingAnything = true;


            screenSync.PreviousSlideIndex = screenSync.ActiveSlideIndex;

            screenSync.ActiveSlideIndex--;

            if (screenSync.ActiveSlideIndex < 0) screenSync.ActiveSlideIndex = slides.Count - 1;


            slideChangePhase = ESlidesOperationPhase.RETRACTING;
            screenSync.ExecutingSlideChange = true;
        }
    }

    public void ToggleMinMax()
    {
        if (!framePivotRtt.isOwnedLocallySelf) framePivotRtt.RequestOwnership();

        if (!screenSync.ExecutingAnything)
        {
            screenSync.ExecutingAnything = true;

            screenSync.ExecutingMinMax = true;
        }
    }

    void SetFrameLocalPosition()
    {
        switch (popUpDirection)
        {
            case EPopUpDirection.UP:
                frame.transform.localPosition = popUpFramePositions.up;
                break;
            case EPopUpDirection.RIGHT_UPPER:
                frame.transform.localPosition = popUpFramePositions.rightUpper;
                break;
            case EPopUpDirection.RIGHT_CENTER:
                frame.transform.localPosition = popUpFramePositions.rightCenter;
                break;
            case EPopUpDirection.RIGHT_LOWER:
                frame.transform.localPosition = popUpFramePositions.rightLower;
                break;
            case EPopUpDirection.DOWN:
                frame.transform.localPosition = popUpFramePositions.down;
                break;
            case EPopUpDirection.LEFT_LOWER:
                frame.transform.localPosition = popUpFramePositions.leftLower;
                break;
            case EPopUpDirection.LEFT_CENTER:
                frame.transform.localPosition = popUpFramePositions.leftCenter;
                break;
            case EPopUpDirection.LEFT_UPPER:
                frame.transform.localPosition = popUpFramePositions.leftUpper;
                break;
        }
    }

    void SetScreenVisuals()
    {
        slidesMeshRenderers[screenSync.ActiveSlideIndex].enabled = true;

        for (int i = 0; i < slidesMeshRenderers.Count; ++i)
            if (i != screenSync.ActiveSlideIndex && slidesMeshRenderers[i].enabled) slidesMeshRenderers[i].enabled = false;

        progressBarPivot.transform.localScale = new Vector3(progressBarIncrement * (screenSync.ActiveSlideIndex + 1), 1, 1);
    }

    void Initialize()
    {
        SetScreenVisuals();

        framePivotBase.transform.localScale = (screenSync.OpenOrClosed == EScreenState.OPEN) ?  Vector3.one : Vector3.zero;
        MinMaxIcon.text = (screenSync.OpenOrClosed == EScreenState.OPEN) ? "--" : "?";
    }

    void SetIcons()
    {
        MinMaxIcon.text = (screenSync.OpenOrClosed == EScreenState.OPEN) ? "--" : "?";
    }
}
