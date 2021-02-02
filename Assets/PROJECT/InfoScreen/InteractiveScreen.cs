using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InteractiveScreen : MonoBehaviour
{

    [SerializeField]
    AnimationCurve fastInEaseOut;

    [SerializeField]
    List<Material> slidesGraphics;

    [SerializeField]
    GameObject slidesPivotBase;


    List<GameObject> slides = new List<GameObject>();

    [SerializeField]
    Vector3 slidesLocalPosition;


    [SerializeField]
    Vector3 slidesScale;

    [SerializeField]
    Vector3 slidesRotation;



    public event Action<GameObject> OnButtonHighlighted;


    void Awake()
    {
        for (int i = 0; i < slidesGraphics.Count; i++)
        {
            slides.Add(GameObject.CreatePrimitive(PrimitiveType.Plane));
            slides[i].name = "slide " + i.ToString();
            slides[i].transform.SetParent(slidesPivotBase.transform);
            slides[i].transform.localPosition = slidesLocalPosition;
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
}
