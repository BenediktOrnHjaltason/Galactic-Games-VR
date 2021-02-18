using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureFree : StructureSync
{
    [SerializeField]
    MeshRenderer mesh;

    Material[] materials;

    string graphVariableGlow = "Vector1_A563CEE";

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        materials = mesh.materials;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        //HandleSideGlow();
    }

    //TODO: Fix issue with shadergraph material black on Quest
    void HandleSideGlow()
    {
        /*
        if (!AvailableToManipulate)
        {
            if (rtt.isOwnedLocallySelf)
            {
                //Increment float opacity
                if (sideGlowOpacity < 1) sideGlowOpacity = model.sideGlowOpacity += 0.1f;

                //Set opacity on material
                //else materials[0].SetFloat(graphVariableGlow, sideGlowOpacity);


            }
            else
            {
                sideGlowOpacity = model.sideGlowOpacity;

                //Set opcaity on material
                //else materials[0].SetFloat(graphVariableGlow, sideGlowOpacity);
            }
        }

        else if (rtt.isOwnedLocallySelf && sideGlowOpacity > 0.6f)
        {
            sideGlowOpacity = model.sideGlowOpacity -= 0.01f;

            //set opacity on material
            //else materials[0].SetFloat(graphVariableGlow, sideGlowOpacity);
        }

        else if (rtt.isOwnedRemotelySelf && sideGlowOpacity > 0.6f)
        {
            //set opacity on material
            //else materials[0].SetFloat(graphVariableGlow, sideGlowOpacity);
        }
        */
    }
}
