using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;




public class UIHandDevice : MonoBehaviour
{
    [SerializeField]
    GameObject eyeAnchor;


    Material material;
    protected Vector3 fullScale;

    MeshRenderer mesh;

    [SerializeField]
    GameObject parent;

    //Scale up/down
    protected bool playerWatching = false;
    protected float scaleMultiplier = 0;

    public virtual void Operate(EHandSide hand)
    {
        if (!material) return;

        switch (hand)
        {
            case EHandSide.LEFT:

                playerWatching = PlayerWatchingLeftHand();
                break;

            case EHandSide.RIGHT:

                playerWatching = PlayerWatchingRightHand();
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

    protected bool PlayerWatchingLeftHand()
    {
        return Vector3.Dot(parent.transform.right, eyeAnchor.transform.forward) < -0.96f &&
                           Vector3.Dot(parent.transform.forward, -eyeAnchor.transform.up) < -0.96f;
    }

    protected bool PlayerWatchingRightHand()
    {
        return Vector3.Dot(-parent.transform.right, eyeAnchor.transform.forward) < -0.96f &&
                           Vector3.Dot(parent.transform.forward, -eyeAnchor.transform.up) < -0.96f;
    }



    void deactivate()
    {
        transform.localScale = new Vector3(0, 0, 0);
        playerWatching = false;
    }
}
