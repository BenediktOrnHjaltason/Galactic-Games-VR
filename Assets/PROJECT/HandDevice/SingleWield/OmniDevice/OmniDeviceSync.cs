using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

public class OmniDeviceSync : HandDeviceSync
{
    MeshRenderer modeScreen;
    
    //Ring of animated objects on the Omnidevice
    List<MeshRenderer> floaties = new List<MeshRenderer>();

    Vector3 floatiesLocalPositionStart = new Vector3(-0.0002236087f, 0.09533767f, -0.04992739f);

    List<Vector3> scalesLocalPositionsEnd = new List<Vector3>();

    float operationEffectMultiplier;
    bool floatiesReset = false;
    float timeOffsett = 0;

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < 8; i++)
            floaties.Add(transform.GetChild(i).gameObject.GetComponent<MeshRenderer>());

        scalesLocalPositionsEnd.Add(new Vector3(-0.0002236087f, 0.201f, -0.04992739f));
        scalesLocalPositionsEnd.Add(new Vector3(0.073f, 0.172f, -0.05f));
        scalesLocalPositionsEnd.Add(new Vector3(0.106f, 0.09533767f, -0.05f));
        scalesLocalPositionsEnd.Add(new Vector3(0.075f, 0.022f, -0.05f));
        scalesLocalPositionsEnd.Add(new Vector3(-0.0002236087f, -0.009f, -0.04992739f));
        scalesLocalPositionsEnd.Add(new Vector3(-0.074f, 0.019f, -0.05f));
        scalesLocalPositionsEnd.Add(new Vector3(-0.107f, 0.09533767f, -0.05f));
        scalesLocalPositionsEnd.Add(new Vector3(-0.073f, 0.172f, -0.05f));

        modeScreen = transform.GetChild(8).GetComponent<MeshRenderer>();

        //Makes individual floaties animate in a smooth wave
        timeOffsett = (Mathf.PI * 2) / scalesLocalPositionsEnd.Count;

        baseClassExtended = true;
    }


    public override void FixedUpdate()
    {
        if (visible) AnimateFloaties();
    }

    void AnimateFloaties()
    {
        if (operationEffectMultiplier < 0.0f && !floatiesReset)
        {
            for (int i = 0; i < floaties.Count; i++) floaties[i].transform.localPosition = floatiesLocalPositionStart;
            floatiesReset = true;
        }

        if (OperationState == EHandDeviceState.IDLE && operationEffectMultiplier > 0)
        {
            operationEffectMultiplier -= 0.2f;

            if (floatiesReset) floatiesReset = false;
        }

        else if (OperationState == EHandDeviceState.SCANNING)
        {
            if (operationEffectMultiplier < 1) operationEffectMultiplier += 0.2f;

            for (int i = 0; i < floaties.Count; i++)
            {
                floaties[i].transform.localPosition =

                    Vector3.Lerp(floatiesLocalPositionStart, scalesLocalPositionsEnd[i], (Mathf.Abs(Mathf.Sin(Time.time * 5))) * operationEffectMultiplier);
            }
        }


        else if (OperationState == EHandDeviceState.CONTROLLING)
        {
            if (operationEffectMultiplier < 1) operationEffectMultiplier += 0.2f;

            for (int i = 0; i < floaties.Count; i++)
            {
                floaties[i].transform.localPosition =

                    Vector3.Lerp(floatiesLocalPositionStart, scalesLocalPositionsEnd[i], (((Mathf.Sin(Time.time * 3 + (timeOffsett * (i + 1))) + 1) / 2)) * operationEffectMultiplier);
            }
        }
    }

    protected override void UpdateVisible()
    {
        visible = model.visible;

        base.UpdateVisible();

        if (visible)
        {
            foreach (MeshRenderer floatie in floaties) floatie.enabled = true;
            modeScreen.enabled = true;
        }

        else
        {
            foreach (MeshRenderer floatie in floaties) floatie.enabled = false;
            modeScreen.enabled = false;
        }
    }
}
