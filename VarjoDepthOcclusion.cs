
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Varjo.XR;

public class VarjoDepthOcclusion : MonoBehaviour
{
    [HideInInspector]
    Controller controller;

    public bool isDisable = true;
    public float range = 0.55f;

    public KeyCode VidSeeThru = KeyCode.LeftControl;

    
    // Start is called before the first frame update
    void Start()
    {
        controller = gameObject.GetComponent<Controller>();
    }

    // Update is called once per frame
    void Update()
    {

        //OnSeeThrough();
        //SetDepth();

    }

    public void SetDepth()
    {
        VarjoRendering.SetDepthTestNearZ(0);
        VarjoRendering.SetDepthTestFarZ(range);

    }

    public void OnSeeThrough()
    {

        if (Input.GetKeyDown(VidSeeThru)) isDisable = !isDisable;

        if (!isDisable) 
        {
            // Start rendering the video see-through image
            VarjoMixedReality.StartRender();
            OnEnable();
        }

        else
        {
            // Stop rendering the video see-through image
            VarjoMixedReality.StopRender();
            OnDisable();
        }
    }

    private void OnEnable()
    {
        // Enable Depth Estimation.
        VarjoMixedReality.EnableDepthEstimation();
    }

    private void OnDisable()
    {
        // Disable Depth Estimation.
        VarjoMixedReality.DisableDepthEstimation();
    }

}
