/*
 * this program is spesifically used for loading Head Tracking data from Varjo data logging in CubeSpehere
 * for loading gaze tracking PilotTeleTheater
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadCSV2 : MonoBehaviour
{
    public string filename;
    string filePath = @"D:\Documents (D)\Gaze Tracking _PilotTeleTheater\"; //Gaze Tracking _PilotTeleTheater
    int frameLength = 0;
    public float frame;

    public Vector3 HmdRot;


    List<int> DataFrame = new List<int>();
     List<Quaternion> HMDrot = new List<Quaternion>();
     List<Vector3> HMDpos = new List<Vector3>();
     List<Vector3> EyeRot = new List<Vector3>();
     List<String> GazeStatus = new List<String>();
     List<DateTime> dateTime = new List<DateTime>();





    private VideoPlayer vidPlayer;
    public GameObject camera_anchor, gaze_anchor;

    // Start is called before the first frame update
    void Start()
    {
        vidPlayer = GetComponent<VideoPlayer>();
        frameLength = (int)vidPlayer.frameCount;

        LoadData();        


    }

    void Update()
    {
        frame = vidPlayer.frame;

        if (frame >= 0)
        {
            // Normalize Heading angle
            HmdRot = Q2E(HMDrot[(int)frame]);
            Vector3 hn = Q2E(HMDrot[0]);            
            HmdRot.y = HmdRot.y - hn.y;

            camera_anchor.transform.localRotation = Quaternion.Euler(HmdRot);

            //camera_anchor.transform.position = new Vector3(0, HMDpos[(int)frame].y, 0);
            //camera_anchor.transform.localRotation = HMDrot[(int)frame];
            gaze_anchor.transform.localRotation = Quaternion.Euler(EyeRot[(int)frame]);

            //  HmdRot = Q2E(HMDrot[(int)frame]);



        }
    }

    Vector3 Q2E(Quaternion a)
    {
        Vector3 e = Quaternion.ToEulerAngles(a);        
        return e * Mathf.Rad2Deg;
    }

    void LoadData()
    {
        // Read the content of text file as individual lines
        string[] lines = System.IO.File.ReadAllLines(@filePath + filename);
        // Create lists


        // Split each row into column data
        for (int i = 1; i <= lines.Length - 1; i++)       //i <= lines.Length-1
        {
            // Splitting is based on semicolon delimeter
            string[] data = lines[i].Split(';');


            //// Convert string to integer and store data to the list
            int seq = Convert.ToInt16(data[0]);

            Vector3 hmdP = StringToVector3(data[3]);
            Quaternion hmdR = StringToQuaternion(data[4]);
            Vector3 eyR = StringToVector3(data[6]);

            DataFrame.Add(seq);
            HMDpos.Add(hmdP);
            HMDrot.Add(hmdR);
            EyeRot.Add(eyR);
            GazeStatus.Add(data[5]);


            // Debug.Log(seq);
            // Debug.Log(data[0] + " : " + hmdP.x + " : " + hmdP.y + " : " + hmdP.z + " : " + hmdP);
            // Debug.Log(data[4] +" : " + hmdR.x + " : " + hmdR.y + " : " + hmdR.z + " : " + hmdR.w + " : " + hmdR);
        }
        //Debug.Log(GazeStatus[850]);
    }

    public static Quaternion StringToQuaternion(string sVector)
    {
        if (string.IsNullOrEmpty(sVector)) sVector = "(0, 0, 0, 0)";

        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector4
        Quaternion result = new Quaternion(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));


        return result;
    }

    public static Vector3 StringToVector3(string sVector)
    {
       
        if (string.IsNullOrEmpty(sVector)) sVector = "(0, 0, 0)";
        

        
        // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")"))
            {
                sVector = sVector.Substring(1, sVector.Length - 2);
            }
       
        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    
    // Update is called once per frame

}
