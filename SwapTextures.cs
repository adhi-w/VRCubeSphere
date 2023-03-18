using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapTextures : MonoBehaviour
{
    public Texture[] textures;
    public int currentTextures;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown (KeyCode.LeftShift))
        {
            currentTextures++;
            currentTextures %= textures.Length;
            GetComponent<Renderer>().material.mainTexture = textures[currentTextures];
        }
        
    }
}
