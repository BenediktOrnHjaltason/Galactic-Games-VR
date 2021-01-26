using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIOmniDeviceMenu : UIHandDevice
{
    [SerializeField]
    List<Vector3> menuPositions;

    GameObject menuIndicator;

    int numberOfDevices; //Including the NONE option

    public int NumberOfDevices { set => numberOfDevices = value; }

    //Must never go to 0
    int activeDeviceIndex = 1; 


    public event Action<int> OnMenuChange;

    private void Awake()
    {
        menuIndicator = transform.Find("MenuIndicator").gameObject;
        SetMenuIndicatorLocation(0);
    }

    void SetMenuIndicatorLocation(int index)
    {
        menuIndicator.transform.localPosition = menuPositions[index];
    }

    public override void Operate(EHandSide hand)
    {
        playerWatching = PlayerWatchingRightHand();

        if (playerWatching)
        {
            if (scaleMultiplier < 1) scaleMultiplier += 0.1f;

            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                //Ned i meny

                activeDeviceIndex++;

                if (activeDeviceIndex == numberOfDevices) activeDeviceIndex = 1; 
                

                SetMenuIndicatorLocation(activeDeviceIndex - 1);
                OnMenuChange?.Invoke(activeDeviceIndex);
            }
                        

            else if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                //Opp i meny

                activeDeviceIndex--;

                if (activeDeviceIndex == 0) activeDeviceIndex = numberOfDevices - 1;

                SetMenuIndicatorLocation(activeDeviceIndex-1);
                OnMenuChange?.Invoke(activeDeviceIndex);
            }
        }

        else if (!playerWatching && scaleMultiplier > 0) scaleMultiplier -= 0.1f;

        //Set scale of UI
        transform.localScale = fullScale * scaleMultiplier;
    }
}
