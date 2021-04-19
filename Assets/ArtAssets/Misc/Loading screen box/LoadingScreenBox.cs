using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class LoadingScreenBox : MonoBehaviour
{    
    public GameObject logoGraphicsPivot;

    public Animatic LogoTextScaleDown;
    
    public Animatic LogoGraphicScaleDown;

    public Animatic leftDoorOpen;
    
    public Animatic rightDoorOpen;

    public Animatic floorScaleDown;

    public Animatic leftWallOpen;

    public Animatic topOpen;

    public Animatic rightWallOpen;

    public Animatic backScaleDown;

    bool rotateGraphics = true;

    float graphicsRotationIncrement = 0.3f;
    float graphicsRotationX = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        LogoTextScaleDown.OnAnimaticEnds += LogoGraphicScaleDown.Run;

        LogoGraphicScaleDown.OnAnimaticEnds += leftDoorOpen.Run;
        LogoGraphicScaleDown.OnAnimaticEnds += rightDoorOpen.Run;

        leftDoorOpen.OnAnimaticEnds += floorScaleDown.Run;
        leftDoorOpen.OnAnimaticEnds += leftWallOpen.Run;
        leftDoorOpen.OnAnimaticEnds += topOpen.Run;
        leftDoorOpen.OnAnimaticEnds += rightWallOpen.Run;

        rightWallOpen.OnAnimaticEnds += backScaleDown.Run;
    }

    public void StartAnimatics()
    {
        LogoTextScaleDown.Run();
    }

    private void FixedUpdate()
    {
        graphicsRotationX -= graphicsRotationIncrement;

        logoGraphicsPivot.transform.localRotation = Quaternion.Euler(graphicsRotationX, 90.0f, -90.0f);
    }

    public void DestroySelf()
    {
        Debug.Log("LSB: DestroySelf() called");
        GameObject.Destroy(this.gameObject);
    }
}
