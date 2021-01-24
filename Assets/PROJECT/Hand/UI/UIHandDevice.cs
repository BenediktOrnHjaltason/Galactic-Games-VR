using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using Normal.Realtime;



public class UIHandDevice : MonoBehaviour
{
    [SerializeField]
    GameObject eyeAnchor;


    Material material;
    Vector3 fullScale;

    MeshRenderer mesh;

    //Scale up/down
    bool playerWatching = false;
    float scaleMultiplier = 0;

    Realtime realtime;


    private void Awake()
    {
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
    }

    public void Operate(EHandSide hand)
    {
        if (!material) return;

        switch (hand)
        {
            case EHandSide.LEFT:

                    playerWatching = Vector3.Dot(transform.right, eyeAnchor.transform.forward) < -0.96f &&
                                      Vector3.Dot(transform.forward, -eyeAnchor.transform.up) < -0.96f;
                break;

            case EHandSide.RIGHT:

                playerWatching = (Vector3.Dot(-transform.right, eyeAnchor.transform.forward) < -0.96f &&
                                      Vector3.Dot(transform.forward, -eyeAnchor.transform.up) < -0.96f);
                break;
        }

        if (playerWatching && scaleMultiplier < 1) scaleMultiplier += 0.1f;

        else if (!playerWatching && scaleMultiplier > 0) scaleMultiplier -= 0.1f;

        //Set scale of UI
        transform.localScale = fullScale * scaleMultiplier;
    }

    public void Initialize()
    {
        mesh = GetComponent<MeshRenderer>();
    }

    public void Set(HandDeviceUIData data)
    {
        mesh.material = this.material = data.material;
        this.fullScale = data.fullScale;
    }

    void deactivate()
    {
        transform.localScale = new Vector3(0, 0, 0);
        playerWatching = false;
    }
}
