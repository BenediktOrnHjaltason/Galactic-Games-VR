using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class UIDeviceInfo : MonoBehaviour
{

    [SerializeField]
    GameObject handAnchor;

    [SerializeField]
    GameObject eyeAnchor;

    public Material material;
    public Vector3 fullScale;

    //Scale up/down
    bool playerWatching = false;
    float scaleMultiplier = 0;


    public void Operate(EHandSide hand)
    {
        if (!material) return;

        switch (hand)
        {
            case EHandSide.LEFT:

                    playerWatching = (Vector3.Dot(handAnchor.transform.right, eyeAnchor.transform.forward) < -0.96f &&
                                      Vector3.Dot(handAnchor.transform.forward, -eyeAnchor.transform.up) < -0.96f);
                break;

            case EHandSide.RIGHT:
                
                    playerWatching = (Vector3.Dot(-handAnchor.transform.right, eyeAnchor.transform.forward) < -0.96f &&
                                      Vector3.Dot(handAnchor.transform.forward, -eyeAnchor.transform.up) < -0.96f);
                break;
        }

        if (playerWatching && scaleMultiplier < 1) scaleMultiplier += 0.1f;

        else if (!playerWatching && scaleMultiplier > 0) scaleMultiplier -= 0.1f;

        //Set scale of UI
        transform.localScale = fullScale * scaleMultiplier;
    }

    void deactivate()
    {
        transform.localScale = new Vector3(0, 0, 0);
        playerWatching = false;
    }
}
