using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_InfoScreen : MonoBehaviour
{
    [SerializeField]
    Material inactiveMaterial;

    [SerializeField]
    Material activeMaterial;


    MeshRenderer mesh;

    void Awake()
    {
        mesh = GetComponentInChildren<MeshRenderer>();

        InteractiveScreen screenBase = transform.root.GetComponent<InteractiveScreen>();

        if (screenBase) screenBase.OnButtonHighlighted += HandleHighlighting;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void HandleHighlighting(GameObject highlightedButton)
    {
        mesh.material = (highlightedButton == this) ? activeMaterial : inactiveMaterial;
    }
}
