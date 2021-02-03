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
        NONE,
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

    List<GameObject> slides = new List<GameObject>();

    [SerializeField]
    Vector3 slidesScale;

    [SerializeField]
    Vector3 slidesRotation;

    //----**** Operation ****----//

    public event Action<GameObject> OnButtonHighlighted;

    bool operatingAnything = false;

    bool operatingMinMax = false;

    EScreenState openOrClosed = EScreenState.OPEN;

    float minMaxTransitionTime = 0;


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
        }
    }

    private void FixedUpdate()
    {
        if (operatingMinMax)
        {
            if (minMaxTransitionTime < 1) minMaxTransitionTime += 0.1f;

            if (openOrClosed == EScreenState.OPEN)
            {
                mainFramePivotBase.transform.localScale = Vector3.Lerp(mainFramePivotFullScale, Vector3.zero, fastInEaseOut.Evaluate(minMaxTransitionTime));
            }
            else
            {
                mainFramePivotBase.transform.localScale = Vector3.Lerp(Vector3.zero, mainFramePivotFullScale, fastInEaseOut.Evaluate(minMaxTransitionTime));
            }

            if (minMaxTransitionTime >= 1)
            {

                openOrClosed = (openOrClosed == EScreenState.OPEN) ? EScreenState.CLOSED : EScreenState.OPEN;

                minMaxTransitionTime = 0.0f;

                operatingMinMax = false;
                operatingAnything = false;
            }
        }
        

    }

    public void HandleButtonHighLights(GameObject button)
    {
        OnButtonHighlighted?.Invoke(button);
    }

    public void NextFrame()
    {
    }

    public void PreviousFrame()
    {
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
