using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

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
    

    enum EScreenState
    {
        OPEN,
        CLOSED
    }

    enum ESlidesOperationPhase
    {
        RETRACTING,
        EXPANDING
    }

    [SerializeField]
    EPopUpDirection popUpDirection;

    [SerializeField]
    AnimationCurve fastInEaseOut;


    //Frame

    [SerializeField]
    GameObject framePivotBase;

    [SerializeField]
    GameObject frame;

    //Buttons

    [SerializeField]
    Button_InteractiveScreen buttonMinMax;

    [SerializeField]
    Button_InteractiveScreen buttonForward;

    [SerializeField]
    Button_InteractiveScreen buttonBackwards;

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

    EScreenState openOrClosed = EScreenState.CLOSED;

    //-- ChangeSlide

    int activeSlideIndex = 0;
    int previousSlideIndex = 0;

    ESlidesOperationPhase slideChangePhase;


    InteractiveScreenSync screenSync;



    void Awake()
    {
        screenSync = GetComponent<InteractiveScreenSync>();

        //Setup buttons

        buttonForward.OnExecute += NextFrame;
        OnButtonHighlighted += buttonForward.SetMaterial;

        buttonBackwards.OnExecute += PreviousFrame;
        OnButtonHighlighted += buttonBackwards.SetMaterial;

        buttonMinMax.OnExecute += ToggleMinMax;
        OnButtonHighlighted += buttonMinMax.SetMaterial;

        //Make slides
        for (int i = 0; i < slidesGraphics.Count; i++)
        {
            slides.Add(GameObject.CreatePrimitive(PrimitiveType.Plane));
            slides[i].name = "slide " + i.ToString();
            slides[i].transform.SetParent(slidesPivotBase.transform);
            slides[i].transform.localPosition = Vector3.zero;
            slides[i].transform.localScale = slidesScale;
            slides[i].transform.rotation = Quaternion.Euler(slidesRotation);

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
    }

    private void FixedUpdate()
    {

        //Operate Buttons

        if (screenSync.ExecutingMinMax)
        {
            if (transitionTime < 1) transitionTime += 0.1f;

            //-----

            if (openOrClosed == EScreenState.OPEN)
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


                openOrClosed = (openOrClosed == EScreenState.OPEN) ? EScreenState.CLOSED : EScreenState.OPEN;

                MinMaxIcon.text = (openOrClosed == EScreenState.OPEN) ? "--" : "?";

                transitionTime = 0.0f;

                screenSync.ExecutingMinMax = false;
                screenSync.ExecutingAnything = false;
            }
        }

        if (screenSync.ExecutingSlideChange)
        {
            if (slideChangePhase == ESlidesOperationPhase.RETRACTING)
            {
                if (transitionTime < 1) transitionTime += 0.2f;
                //----

                slidesPivotBase.transform.localScale = Vector3.Lerp(slidesPivotBaseScale, Vector3.zero, fastInEaseOut.Evaluate(transitionTime));

                //----
                if (transitionTime >= 1)
                {
                    transitionTime = 0.0f;
                    slideChangePhase = ESlidesOperationPhase.EXPANDING;

                    slidesMeshRenderers[previousSlideIndex].enabled = false;
                    slidesMeshRenderers[activeSlideIndex].enabled = true;
                }
            }

            if (slideChangePhase == ESlidesOperationPhase.EXPANDING)
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
        if (!screenSync.ExecutingAnything)
        {
            screenSync.ExecutingAnything = true;


            previousSlideIndex = activeSlideIndex;

            activeSlideIndex++;

            if (activeSlideIndex > slides.Count - 1) activeSlideIndex = 0;

            progressBarPivot.transform.localScale = new Vector3(progressBarIncrement * (activeSlideIndex + 1), 1, 1);


            slideChangePhase = ESlidesOperationPhase.RETRACTING;
            screenSync.ExecutingSlideChange = true;
        }
    }

    public void PreviousFrame()
    {
        if (!screenSync.ExecutingAnything)
        {
            screenSync.ExecutingAnything = true;

            
            previousSlideIndex = activeSlideIndex;

            activeSlideIndex--;

            if (activeSlideIndex < 0) activeSlideIndex = slides.Count - 1;

            progressBarPivot.transform.localScale = new Vector3(progressBarIncrement * (activeSlideIndex + 1), 1, 1);

            slideChangePhase = ESlidesOperationPhase.RETRACTING;
            screenSync.ExecutingSlideChange = true;
        }
    }

    public void ToggleMinMax()
    {
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
}
