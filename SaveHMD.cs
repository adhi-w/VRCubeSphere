using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using System;
using System.IO;


public class SaveHMD : MonoBehaviour
{
    Controller controller;

    [SerializeField]
    GameObject camera_, fixaPoint_, gazeTarget_;

    string Path = @"D:\Documents (D)\1-TeleMeeting_GazeTrackingData\GazeTrackingData\";
    string vidID;
    string userID;
    bool isSaving = false;
    bool isVideoStart = false;
    private bool logging = false;

    private StreamWriter writer = null;

    float frame;
    int seq;

    private static readonly string[] ColumnNames = { "UserID", "VidID", "Seq","Frame", "HMDPosition", "HMDRotation", "GazePosition", "GazeRotation", "FixationPoint" };

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
        isSaving = controller.isSaving;
        isVideoStart = controller.isVideoStart;
        userID = controller.userID;
        vidID = controller.vidID;
        frame = controller.frame;

        if (isSaving)
        {
            seq++;
            StartLogging();
            LogGazeData(seq);
            
        }
        else if (!isSaving) StopLogging();

    }

    void LogGazeData(int i)
    {
        string[] logData = new string[9];

        // ID
        logData[0] = userID.ToString();
        logData[1] = vidID.ToString();

        // Sequence
        logData[2] = i.ToString();

        // Gaze data frame number
        logData[3] = frame.ToString();

        // HMD 
        logData[4] = camera_.transform.localPosition.ToString("F3");
        logData[5] = camera_.transform.localRotation.eulerAngles.ToString("F3");

        // Gaze
        logData[6] = gazeTarget_.transform.localPosition.ToString("F3");
        logData[7] = gazeTarget_.transform.localRotation.eulerAngles.ToString("F3");

        // Fixation
        logData[8] = fixaPoint_.transform.localPosition.ToString("F3");

        Log(logData);
    }
    public void StartLogging()
    {
        if (logging)
        {
            Debug.LogWarning("Logging was on when StartLogging was called. No new log was started.");
            return;
        }

        logging = true;

        DateTime now = DateTime.Now;
        string fileName = string.Format("{0:00}-{1:00}-{2:00} _", now.Day, now.Hour, now.Minute);

        //Use predetermined folder/path
        string path = Path + fileName + userID + "-" + vidID + ".txt";   //logPath +
        writer = new StreamWriter(path);

        Log(ColumnNames);
        Debug.Log("Log file started at: " + path);
    }

    // Write given values in the log file
    void Log(string[] values)
    {
        if (!logging || writer == null)
            return;

        string line = "";
        for (int i = 0; i < values.Length; ++i)
        {
          //  values[i] = values[i].Replace("\r", "").Replace("\n", ""); // Remove new lines so they don't break csv
            line += values[i] + (i == (values.Length - 1) ? "" : ";"); // Do not add semicolon to last data string
        }
        writer.WriteLine(line);
    }

    void StopLogging()
    {

        if (!logging)
            return;

        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }
        logging = false;
        Debug.Log("Logging ended");
    }

    void OnApplicationQuit()    {
       
        StopLogging();
    }
}
