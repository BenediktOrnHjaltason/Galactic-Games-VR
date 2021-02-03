using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InteractiveScreen : MonoBehaviour
{

    [SerializeField]
    AnimationCurve fastInEaseOut;


    //Frame

    [SerializeField]
    Vector3 mainPivotFullScale;

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



    public event Action<GameObject> OnButtonHighlighted;


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

    // Update is called once per frame
    void Update()
    {
        
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
    }
}
