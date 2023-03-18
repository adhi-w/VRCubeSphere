/*
 * Current Result is inaccurate
 * */
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CollisionDetection : MonoBehaviour
{

    float durCeiling, durFloor, durMaleB, durMaleF, durFemaleF, durFemaleB, timercounter, totalHitDuration;
    Controller controller;

    bool togCeil, togFloor, togMB, togMF, togFB, togFF;
    int fixaHitCeiling, fixaHitFloor, fixaHitMB, fixaHitMF, fixaHitFB, fixaHitFF;
    private Rect windowRect1 = new Rect(20, 340, 220, 240);

    [Header("Default path is Logs under application data path.")]
    public bool useCustomLogPath = false;
    string customLogPath = @"D:\Documents (D)\1-TeleMeeting_GazeTrackingData\";
    string vidID;
    string userID;
    bool isSaving = false;
    bool isEyeCalibrated = false;
    bool isVideoStart = false;

    float vidFrame = 0f, vidTime = 0f;

    private void Awake()
    {
        //Load GameObject function from Controller.cs
        controller = gameObject.GetComponent<Controller>();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Load Controller.cs Input
        //vidID = controller.vidID;
        //userID = controller.userID;
        //isSaving = controller.isSaving;
        //isEyeCalibrated = controller.isEyeCalibrated;
        //isVideoStart = controller.isVideoStart;
        //vidFrame = controller.frame;
        //vidTime = controller.video_index_time;
        //-------------------------------------------
        //------------------------------------

        float last_timercounter = timercounter;
        timercounter = vidTime;
        if (timercounter <= 0) timercounter = last_timercounter;

        totalHitDuration = durCeiling + durFloor + durMaleF + durMaleB + durFemaleF + durFemaleB;

    }

    private void OnTriggerEnter(Collider other)
    {
        //Check to see if the Collider's name is ...
        if (other.tag == "ColliderCeiling")
        {
            durCeiling += Time.deltaTime;
            togFloor = false;
            togMB = false; togMF = false;
            togFB = false; togFF = false;

            if (togCeil == false) { fixaHitCeiling++; togCeil = true; }

            //Output the message
            Debug.Log("ColliderCeiling!");
        }
        else if (other.tag == "ColliderFloor")
        {
            durFloor += Time.deltaTime;
            togCeil = false;
            togMB = false; togMF = false;
            togFB = false; togFF = false;

            if (togFloor == false) { fixaHitFloor++; togFloor = true; }

            Debug.Log("ColliderFloor!");
        }
        else if (other.tag == "ColliderMaleFace")
        {
            durMaleF += Time.deltaTime;
            togCeil = false; togFloor = false;
            togMB = false;
            togFB = false; togFF = false;

            if (togMF == false) { fixaHitMF++; togMF = true; }
            Debug.Log("ColliderMaleFace!");
        }
        else if (other.tag == "ColliderMaleBody")
        {
            durMaleB += Time.deltaTime; togMF = false;
            togCeil = false; togFloor = false;
            togMF = false;
            togFB = false; togFF = false;

            if (togMB == false) { fixaHitMB++; togMB = true; }
            Debug.Log("ColliderMaleBody!");
        }
        else if (other.tag == "ColliderFemaleFace")
        {
            durFemaleF += Time.deltaTime; togMF = false;
            togCeil = false; togFloor = false;
            togMB = false; togMF = false;
            togFB = false;

            if (togFF == false) { fixaHitFF++; togFF = true; }
            Debug.Log("ColliderFemaleFace!");
        }
        else if (other.tag == "ColliderFemaleBody")
        {
            durFemaleB += Time.deltaTime; togMF = false;
            togCeil = false; togFloor = false;
            togMB = false; togMF = false;
            togFF = false;

            if (togFB == false) { fixaHitFB++; togFB = true; }
            Debug.Log("ColliderFemaleBody!");
        }
        else
        {
            togCeil = false; togFloor = false;
            togMB = false; togMF = false;
            togFB = false; togFF = false;
            Debug.Log("Undetected!");
        }
    }

    private void OnGUI()
    {
        GUI.color = Color.blue;
        windowRect1 = GUI.Window(1, windowRect1, DoMyWindow, "Gaze Count");

    }

    void DoMyWindow(int windowID)
    {
        //totalHitDuration = durCeiling + durFloor + durMaleF + durMaleB + durFemaleF + durFemaleB;

        GUILayout.Label("Ceiling : " + fixaHitCeiling.ToString("f2") + "\t timer:" + durCeiling.ToString("f2"));
        GUILayout.Label("Floor \t :" + fixaHitFloor.ToString("f2") + "\t timer:" + durFloor.ToString("f2"));
        GUILayout.Label("Male Face :" + fixaHitMF.ToString("f2") + "\t timer:" + durMaleF.ToString("f2"));
        GUILayout.Label("Male Body :" + fixaHitMB.ToString("f2") + "\t timer:" + durMaleB.ToString("f2"));
        GUILayout.Label("Female Face :" + fixaHitFF.ToString("f2") + "\t timer:" + durFemaleF.ToString("f2"));
        GUILayout.Label("Female Body :" + fixaHitFB.ToString("f2") + "\t timer:" + durFemaleB.ToString("f2"));
        GUILayout.Label("Total Gaze Hit " + totalHitDuration.ToString("f2"));
        GUILayout.Label("Timer " + timercounter.ToString("f2"));

        // Make the windows be draggable.
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }

    void SaveAppendingGazeHitCol()
    {
        string filePath = customLogPath + "RefineCollHitData.csv";

        // This text is added only once to the file.
        if (!File.Exists(filePath))
        {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine("UserID, VidID, Ceil FixaHit, Floor FixaHit, MF FixaHit, MB FixaHit, FF FixaHit, FB FixaHit, Ceil Dur, Floor Dur, MF Dur, MB Dur, FF Dur, FB Dur, Total Hit Dur, Timer Counter");
            }
        }

        // This text is always added, making the file longer over time
        // if it is not deleted.
        using (StreamWriter colwriter = File.AppendText(filePath))
        {
            colwriter.Write(userID.ToString() + ", ");
            colwriter.Write(vidID.ToString() + ", ");
            colwriter.Write(fixaHitCeiling.ToString() + ", ");
            colwriter.Write(fixaHitFloor.ToString() + ", ");
            colwriter.Write(fixaHitMF.ToString() + ", ");
            colwriter.Write(fixaHitMB.ToString() + ", ");
            colwriter.Write(fixaHitFF.ToString() + ", ");
            colwriter.Write(fixaHitFB.ToString() + ", ");

            colwriter.Write(durCeiling.ToString("f2") + ", ");
            colwriter.Write(durFloor.ToString("f2") + ", ");
            colwriter.Write(durMaleF.ToString("f2") + ", ");
            colwriter.Write(durMaleB.ToString("f2") + ", ");
            colwriter.Write(durFemaleF.ToString("f2") + ", ");
            colwriter.Write(durFemaleB.ToString("f2") + ", ");
            colwriter.Write(totalHitDuration.ToString("f2") + ", ");
            colwriter.WriteLine(timercounter.ToString("f2"));
        }

        // Open the file to read from.
        using (StreamReader sr = File.OpenText(filePath))
        {
            string s = "";
            while ((s = sr.ReadLine()) != null)
            {
                Console.WriteLine(s);
            }
        }

    }

    void OnApplicationQuit()
    {
        if (isVideoStart) SaveAppendingGazeHitCol();  //save total amount of gazing towards point of interests (collider)
        
    }

}
