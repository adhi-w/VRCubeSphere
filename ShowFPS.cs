using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShowFPS : MonoBehaviour
{
    //public Text fpsText;
    public float fps, deltaTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime)*0.1f;
        fps = 1.0f / deltaTime;
        //fpsText.text = Mathf.Ceil(fps).ToString();
       
    }
}
