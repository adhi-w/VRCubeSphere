using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHeading : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion rota = Quaternion.Euler(0.05f, 0.15f, 0.94f);
        Quaternion rotb = new Quaternion (0.08f, 0.27f, -0.03f, 0.80f);
        transform.rotation = new Quaternion(rota.x + rotb.x, rota.y + rotb.y, rota.z + rotb.z, rota.w + rotb.w);
    }
}

