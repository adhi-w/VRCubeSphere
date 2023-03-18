using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadHMDLog : MonoBehaviour
{
    public string filename;
    string filePath = @"D:\Documents (D)\1-TeleMeeting_GazeTrackingData\GazeTrackingData\"; 
    int frameLength = 0;
    public float frame;


    List<string> UserID = new List<string>();
    List<string> VidID = new List<string>();
    List<int> Sequence = new List<int>();
    List<int> Frame = new List<int>();
    List<Vector3> HMDpos = new List<Vector3>();
    List<Vector3> HMDrot = new List<Vector3>();
    List<Vector3> GazePos = new List<Vector3>();
    List<Vector3> GazeRot = new List<Vector3>();
    List<Vector3> FixationPos = new List<Vector3>();


    private VideoPlayer vidPlayer;
    public GameObject mainsphere_anchor, camera_anchor, gaze_anchor, fixation_anchor;

    // Start is called before the first frame update
    void Start()
    {
        vidPlayer = GetComponent<VideoPlayer>();
        frameLength = (int)vidPlayer.frameCount;

        LoadData();
    }

    // Update is called once per frame
    void Update()
    {
        frame = vidPlayer.frame;

        if (frame >= 0)
        {
            // Normalize Heading angle
            float y = HMDrot[0].y;

          //  mainsphere_anchor.transform.localRotation = Quaternion.Euler(0,y,0); //inaccurate
            camera_anchor.transform.localRotation = Quaternion.Euler(HMDrot[(int)frame]);
            //camera_anchor.transform.position = new Vector3(0, HMDpos[(int)frame].y, 0);

          //  gaze_anchor.transform.localPosition = GazePos[(int)frame];
            gaze_anchor.transform.localRotation = Quaternion.Euler(GazeRot[(int)frame]);

            //fixation_anchor.transform.localPosition = FixationPos[(int)frame];
            fixation_anchor.transform.localPosition = new Vector3(FixationPos[(int)frame].x, FixationPos[(int)frame].y - 1.5f, FixationPos[(int)frame].z);



        }
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
            string userID = data[0];
            string vidID = data[1];
            int seq = Convert.ToInt16(data[2]);
            int frame = Convert.ToInt16(data[3]);
            Vector3 hmdP = StringToVector3(data[4]);
            Vector3 hmdR = StringToVector3(data[5]);
            Vector3 gazeP = StringToVector3(data[6]);
            Vector3 gazeR = StringToVector3(data[7]);
            Vector3 fixaP = StringToVector3(data[8]);

            UserID.Add(userID);
            VidID.Add(vidID);
            Sequence.Add(seq);
            Frame.Add(frame);
            HMDpos.Add(hmdP);
            HMDrot.Add(hmdR);
            GazePos.Add(gazeP);
            GazeRot.Add(gazeR);
            FixationPos.Add(fixaP);

            // Debug.Log(seq);
        }
        //Debug.Log(HMDpos[850]);
    }


    Vector3 Q2E(Quaternion a)
    {
        Vector3 e = Quaternion.ToEulerAngles(a);
        return e * Mathf.Rad2Deg;
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

}
