using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    Animatic footRise;

    [SerializeField]
    Animatic screenScaleOut;

    [SerializeField]
    Animatic keyboardScaleOut;

    [SerializeField]
    Animatic logoAnimatic;

    bool animaticHasRun = false;

    // Start is called before the first frame update
    void Start()
    {
        footRise.OnAnimaticEnds += screenScaleOut.Run;
        screenScaleOut.OnAnimaticEnds += keyboardScaleOut.Run;

        logoAnimatic.Run();
    }

    private void FixedUpdate()
    {
        if (!animaticHasRun && Time.time > 2)
        {
            footRise.Run();
            animaticHasRun = true;
        }
    }
}
