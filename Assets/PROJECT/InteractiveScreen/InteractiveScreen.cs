using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    AnimationCurve fastInEaseOut;


    //Frame

    [SerializeField]
    GameObject mainFramePivotBase;

    [SerializeField]
    Vector3 mainFramePivotFullScale;

    //Buttons

    [SerializeField]
    Button_InteractiveScreen buttonMinMax;

    [SerializeField]
    Button_InteractiveScreen buttonForward;

    [SerializeField]
    Button_InteractiveScreen buttonBackwards;

    //Slides
    [SerializeField]
    List<Material> slidesGraphics;

    [SerializeField]
    GameObject slidesPivotBase;

    Vector3 slidesPivotBaseScale = new Vector3(1,1,1);

    List<GameObject> slides = new List<GameObject>();

    List<MeshRenderer> slidesMeshRenderers = new List<MeshRenderer>();

    [SerializeField]
    Vector3 slidesScale;

    [SerializeField]
    Vector3 slidesRotation;

    //----**** Operation ****----//

    public event Action<GameObject> OnButtonHighlighted;

    bool operatingAnything = false;

    float transitionTime = 0;

    //-- MinMax

    bool operatingMinMax = false;

    EScreenState openOrClosed = EScreenState.OPEN;

    //-- ChangeSlide

    bool operatingSlideChange = false;

    int activeSlideIndex = 0;
    int previousSlideIndex = 0;

    ESlidesOperationPhase slideChangePhase;

    




    void Awake()
    {
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
    }

    private void FixedUpdate()
    {
        if (operatingMinMax)
        {
            if (transitionTime < 1) transitionTime += 0.1f;

            //-----

            if (openOrClosed == EScreenState.OPEN)
            {
                mainFramePivotBase.transform.localScale = Vector3.Lerp(mainFramePivotFullScale, Vector3.zero, fastInEaseOut.Evaluate(transitionTime));
            }
            else
            {
                mainFramePivotBase.transform.localScale = Vector3.Lerp(Vector3.zero, mainFramePivotFullScale, fastInEaseOut.Evaluate(transitionTime));
            }

            //-----

            if (transitionTime >= 1)
            {
                openOrClosed = (openOrClosed == EScreenState.OPEN) ? EScreenState.CLOSED : EScreenState.OPEN;

                transitionTime = 0.0f;

                operatingMinMax = false;
                operatingAnything = false;
            }
        }

        if (operatingSlideChange)
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
                    operatingSlideChange = false;
                    operatingAnything = false;
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
        if (!operatingAnything)
        {
            operatingAnything = true;


            previousSlideIndex = activeSlideIndex;

            activeSlideIndex++;

            if (activeSlideIndex > slides.Count - 1) activeSlideIndex = 0;

            slideChangePhase = ESlidesOperationPhase.RETRACTING;
            operatingSlideChange = true;
        }
    }

    public void PreviousFrame()
    {
        if (!operatingAnything)
        {
            operatingAnything = true;

            
            previousSlideIndex = activeSlideIndex;

            activeSlideIndex--;

            if (activeSlideIndex < 0) activeSlideIndex = slides.Count - 1;

            slideChangePhase = ESlidesOperationPhase.RETRACTING;
            operatingSlideChange = true;
        }
    }

    public void ToggleMinMax()
    {
        if (!operatingAnything)
        {
            operatingAnything = true;

            operatingMinMax = true;
        }
    }
}
