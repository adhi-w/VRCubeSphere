using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Video;
using UnityEngine.XR;
using Varjo.XR;

public enum GazeDataSource
{
    InputSubsystem,
    GazeAPI
}

public class VarjoGazeTracking : MonoBehaviour
{    
    float durCeiling, durFloor, durMaleB, durMaleF, durFemaleF, durFemaleB, timercounter, totalHitDuration;
    Controller controller;
    float timercounterSys;

    bool togCeil, togFloor, togMB, togMF, togFB, togFF;
    int fixaHitCeiling, fixaHitFloor, fixaHitMB, fixaHitMF, fixaHitFB, fixaHitFF;

    [Header("Gaze data")]
    public GazeDataSource gazeDataSource = GazeDataSource.InputSubsystem;

    [Header("Gaze calibration settings")]
    public VarjoEyeTracking.GazeCalibrationMode gazeCalibrationMode = VarjoEyeTracking.GazeCalibrationMode.Fast;
    public KeyCode calibrationRequestKey = KeyCode.Space;
 
    [Header("Gaze output filter settings")]
    public VarjoEyeTracking.GazeOutputFilterType gazeOutputFilterType = VarjoEyeTracking.GazeOutputFilterType.Standard;
    public KeyCode setOutputFilterTypeKey = KeyCode.RightShift;

    [Header("Gaze data output frequency")]
    public VarjoEyeTracking.GazeOutputFrequency frequency;

    [Header("Toggle gaze target visibility")]
    public KeyCode toggleGazeTarget = KeyCode.Return;

    [Header("Debug Gaze")]
    public KeyCode checkGazeAllowed = KeyCode.PageUp;
    public KeyCode checkGazeCalibrated = KeyCode.PageDown;

    [Header("Toggle fixation point indicator visibility")]
    public bool showFixationPoint = true;

    [Header("Visualization Transforms")]
    public Transform fixationPointTransform;
    public Transform leftEyeTransform;
    public Transform rightEyeTransform;

    [Header("XR camera")]
    public Camera xrCamera;

    [Header("Gaze point indicator")]
    public GameObject gazeTarget;

    [Header("Gaze ray radius")]
    public float gazeRadius = 0.01f;

    [Header("Gaze point distance if not hit anything")]
    public float floatingGazeTargetDistance = 5f;

    [Header("Gaze target offset towards viewer")]
    public float targetOffset = 0.2f;

    [Header("Amout of force give to freerotating objects at point where user is looking")]
    public float hitForce = 5f;



    //---------------------------------------------------------
    //------------------Saving Data----------------------------------
    [Header("Gaze data logging")]
    public KeyCode loggingToggleKey = KeyCode.RightControl;

    [Header("Default path is Logs under application data path.")]
    public bool useCustomLogPath = false;
    string customLogPath = @"D:\Documents (D)\1-TeleMeeting_GazeTrackingData\";
    string vidID;
    string userID;
    bool isSaving = false;
    bool isEyeCalibrated = false;
    bool isVideoStart = false;

    float vidFrame = 0f, vidTime = 0f;

    //------------------------------------------------------------
    // -------------------------------------------------------

    [Header("Print gaze data framerate while logging.")]
    public bool printFramerate = false;
    
    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;
    private Eyes eyes;
    private VarjoEyeTracking.GazeData gazeData;
    private List<VarjoEyeTracking.GazeData> dataSinceLastUpdate;
    private Vector3 leftEyePosition;
    private Vector3 rightEyePosition;
    private Quaternion leftEyeRotation;
    private Quaternion rightEyeRotation;
    private Vector3 fixationPoint;
    private Vector3 direction;
    private Vector3 rayOrigin;
    private RaycastHit hit;
    private float distance;
    private StreamWriter writer = null;
    private bool logging = false;

    private static readonly string[] ColumnNames = { "Frame", "CaptureTime", "LogTime", "HMDPosition", "HMDRotation", "GazeStatus", "CombinedGazeForward", "CombinedGazePosition", "LeftEyeStatus", "LeftEyeForward", "LeftEyePosition", "LeftEyePupilSize", "RightEyeStatus", "RightEyeForward", "RightEyePosition", "RightEyePupilSize", "FocusDistance", "FocusStability" };
    private const string ValidString = "VALID";
    private const string InvalidString = "INVALID";

    int gazeDataCount = 0;
    float gazeTimer = 0f;
    
    private Rect windowRect1 = new Rect(20, 340, 220, 240);

    [Header("For Testing only")]
    public GameObject LoadGaze, LoadFixa;
    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(XRNode.CenterEye, devices);
        device = devices.FirstOrDefault();
    }

    void OnEnable()
    {
        if (!device.isValid)
        {
            GetDevice();
        }
    }

    // Start is called before the first frame update
    private void Awake()
    {
        //Load GameObject function from Controller.cs
        controller = gameObject.GetComponent<Controller>();

    }

    private void Start()
    {

       

        VarjoEyeTracking.SetGazeOutputFrequency(frequency);
        //Hiding the gazetarget if gaze is not available or if the gaze calibration is not done
        if (VarjoEyeTracking.IsGazeAllowed() && VarjoEyeTracking.IsGazeCalibrated())
        {
            gazeTarget.SetActive(true);
        }
        else
        {
            gazeTarget.SetActive(false);
        }

        if (showFixationPoint)
        {
            fixationPointTransform.gameObject.SetActive(true);
        }
        else
        {
            fixationPointTransform.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()  // Change to fixedUpdate
    {
        //Load Controller.cs Input
        vidID = controller.vidID;
        userID = controller.userID;
        isSaving = controller.isSaving;
        isEyeCalibrated = controller.isEyeCalibrated;
        isVideoStart = controller.isVideoStart;
        vidFrame = controller.frame;
        vidTime = controller.video_index_time;
        //-------------------------------------------
        //------------------------------------

       
        if (logging && printFramerate)
        {
            gazeTimer += Time.deltaTime;
            if (gazeTimer >= 1.0f)
            {
                Debug.Log("Gaze data rows per second: " + gazeDataCount);
                gazeDataCount = 0;
                gazeTimer = 0f;
            }
        }

        // Request gaze calibration
        if (Input.GetKeyDown(calibrationRequestKey)) //|| isEyeCalibrated
        {
            VarjoEyeTracking.RequestGazeCalibration(gazeCalibrationMode);           
            
        }
        

        // Set output filter type
        if (Input.GetKeyDown(setOutputFilterTypeKey))
        {
            VarjoEyeTracking.SetGazeOutputFilterType(gazeOutputFilterType);
            Debug.Log("Gaze output filter type is now: " + VarjoEyeTracking.GetGazeOutputFilterType());
        }

        // Check if gaze is allowed
        if (Input.GetKeyDown(checkGazeAllowed))
        {
            Debug.Log("Gaze allowed: " + VarjoEyeTracking.IsGazeAllowed());
        }

        // Check if gaze is calibrated
        if (Input.GetKeyDown(checkGazeCalibrated))
        {
            Debug.Log("Gaze calibrated: " + VarjoEyeTracking.IsGazeCalibrated());
        }

        // Toggle gaze target visibility
        if (Input.GetKeyDown(toggleGazeTarget))
        {
            gazeTarget.GetComponentInChildren<MeshRenderer>().enabled = !gazeTarget.GetComponentInChildren<MeshRenderer>().enabled;
        }

        // Get gaze data if gaze is allowed and calibrated
        if (VarjoEyeTracking.IsGazeAllowed() && VarjoEyeTracking.IsGazeCalibrated())
        {
            //Get device if not valid
            if (!device.isValid)
            {
                GetDevice();
            }

            // Show gaze target
            gazeTarget.SetActive(true);    

            if (gazeDataSource == GazeDataSource.InputSubsystem)
            {
                // Get data for eye positions, rotations and the fixation point
                if (device.TryGetFeatureValue(CommonUsages.eyesData, out eyes))
                {
                    if (eyes.TryGetLeftEyePosition(out leftEyePosition))
                    {
                        leftEyeTransform.localPosition = leftEyePosition;
                    }

                    if (eyes.TryGetLeftEyeRotation(out leftEyeRotation))
                    {
                        leftEyeTransform.localRotation = leftEyeRotation;
                    }

                    if (eyes.TryGetRightEyePosition(out rightEyePosition))
                    {
                        rightEyeTransform.localPosition = rightEyePosition;
                    }

                    if (eyes.TryGetRightEyeRotation(out rightEyeRotation))
                    {
                        rightEyeTransform.localRotation = rightEyeRotation;
                    }

                    if (eyes.TryGetFixationPoint(out fixationPoint))
                    {
                        fixationPointTransform.localPosition = fixationPoint;
                    }
                }

                // Set raycast origin point to VR camera position
                rayOrigin = xrCamera.transform.position;

                // Direction from VR camera towards fixation point
                direction = (fixationPointTransform.position - xrCamera.transform.position).normalized;
                
            }
            else
            {
                gazeData = VarjoEyeTracking.GetGaze();

                if (gazeData.status != VarjoEyeTracking.GazeStatus.Invalid)
                {
                    // GazeRay vectors are relative to the HMD pose so they need to be transformed to world space
                    if (gazeData.leftStatus != VarjoEyeTracking.GazeEyeStatus.Invalid)
                    {
                        leftEyeTransform.position = xrCamera.transform.TransformPoint(gazeData.left.origin);
                        leftEyeTransform.rotation = Quaternion.LookRotation(xrCamera.transform.TransformDirection(gazeData.left.forward));
                    }

                    if (gazeData.rightStatus != VarjoEyeTracking.GazeEyeStatus.Invalid)
                    {
                        rightEyeTransform.position = xrCamera.transform.TransformPoint(gazeData.right.origin);
                        rightEyeTransform.rotation = Quaternion.LookRotation(xrCamera.transform.TransformDirection(gazeData.right.forward));
                    }

                    // Set gaze origin as raycast origin
                    rayOrigin = xrCamera.transform.TransformPoint(gazeData.gaze.origin);

                    // Set gaze direction as raycast direction
                    direction = xrCamera.transform.TransformDirection(gazeData.gaze.forward);

                    // Fixation point can be calculated using ray origin, direction and focus distance
                    fixationPointTransform.position = rayOrigin + direction * gazeData.focusDistance;
                }
            }
        }



        // Raycast to world from VR Camera position towards fixation point
        if (Physics.SphereCast(rayOrigin, gazeRadius, direction, out hit) && isVideoStart)  // && isVideoStart isSaving isActive when the video starts
        {

            //if (Physics.Raycast(rayOrigin, direction, out hit) && isVideoStart)
            //{

            //rayOrigin = xrCamera.transform.TransformPoint(LoadGaze.transform.position);
            //direction = xrCamera.transform.TransformDirection(LoadGaze.transform.forward);
            //if (Physics.Raycast(rayOrigin, -direction, out hit) && isVideoStart)
            //{

            // Put target on gaze raycast position with offset towards user
            gazeTarget.transform.position = hit.point- direction * targetOffset;

            // Make gaze target point towards user
            gazeTarget.transform.LookAt(rayOrigin, Vector3.up);

            // Scale gazetarget with distance so it apperas to be always same size
            distance = hit.distance;
            gazeTarget.transform.localScale = Vector3.one * distance;

            // Prefer layers or tags to identify looked objects in your application
            // This is done here using GetComponent for the sake of clarity as an example
            //RotateWithGaze rotateWithGaze = hit.collider.gameObject.GetComponent<RotateWithGaze>();
            //if (rotateWithGaze != null)
            //{
            //    rotateWithGaze.RayHit();
            //}

            // Alternative way to check if you hit object with tag
            //if (hit.transform.CompareTag("FreeRotating"))
            //{
            //    AddForceAtHitPosition();

            //}


            //if (hit.transform.CompareTag("ColliderCeiling"))
            //{
            //    //durCeiling += Time.deltaTime;
            //}
                       

            // Raycast Hit Collider Ceiling
            if (hit.collider.tag == "ColliderCeiling")
            {
                durCeiling += Time.deltaTime;                
                togFloor = false; 
                togMB=false; togMF = false; 
                togFB = false; togFF=false;

                if (togCeil == false) { fixaHitCeiling++; togCeil = true; }

            }
            /// Raycast Hit Collider Floor
            else if (hit.collider.tag == "ColliderFloor")
            {
                durFloor += Time.deltaTime;
                togCeil = false; 
                togMB = false; togMF = false; 
                togFB = false; togFF = false;

                if (togFloor == false) { fixaHitFloor++; togFloor = true; }

            }
            /// Raycast Hit Collider Male
            else if (hit.collider.tag == "ColliderMaleFace")
            {
                durMaleF += Time.deltaTime;
                togCeil = false; togFloor = false; 
                togMB = false; 
                togFB = false; togFF = false;

                if (togMF==false) { fixaHitMF++; togMF = true; }

            }
            else if (hit.collider.tag == "ColliderMaleBody")
            {
                durMaleB += Time.deltaTime;togMF = false;
                togCeil = false; togFloor = false; 
                togMF = false; 
                togFB = false; togFF = false;

                if (togMB == false) { fixaHitMB++; togMB = true; }
            }
            /// Raycast Hit Collider Female
            else if (hit.collider.tag == "ColliderFemaleFace")
            {
                durFemaleF += Time.deltaTime; togMF = false;
                togCeil = false; togFloor = false; 
                togMB = false; togMF = false; 
                togFB = false; 

                if (togFF == false) { fixaHitFF++; togFF = true; }
            }
            else if (hit.collider.tag == "ColliderFemaleBody")
            {
                durFemaleB += Time.deltaTime;togMF = false;
                togCeil = false; togFloor = false; 
                togMB = false; togMF = false; 
                togFF = false;

                if (togFB == false) { fixaHitFB++; togFB = true; }
            }

           
            Vector3 incomingVec = hit.point - rayOrigin;
            Vector3 reflectVec = Vector3.Reflect(incomingVec, hit.normal);

          // Debug.DrawLine(rayOrigin, hit.point, Color.red);
            //Debug.DrawRay(hit.point, reflectVec, Color.green);
           
        }
        else
        {
            // If gaze ray didn't hit anything, the gaze target is shown at fixed distance
            gazeTarget.transform.position = rayOrigin + direction * floatingGazeTargetDistance;
            gazeTarget.transform.LookAt(rayOrigin, Vector3.up);
            gazeTarget.transform.localScale = Vector3.one * floatingGazeTargetDistance;

            togCeil = false; togFloor = false; 
            togMB = false; togMF = false; 
            togFB = false; togFF = false;
        }
        

        float last_timercounter = timercounter;
        timercounter = vidTime;
        if (timercounter <= 0) timercounter = last_timercounter;



        /* //native
            // Press button for saving thelog data
            if (Input.GetKeyDown(loggingToggleKey))
            {
                if (!logging)
                {
                    StartLogging();               
                }
                else
                {
                    StopLogging();
                }

                return;
            }

            if (logging)
            {
                int dataCount = VarjoEyeTracking.GetGazeList(out dataSinceLastUpdate);
                foreach (var data in dataSinceLastUpdate)
                {
                    LogGazeData(data); 
                }
            }
        */

        if (isSaving)
        {
            totalHitDuration = durCeiling + durFloor + durMaleF + durMaleB + durFemaleF + durFemaleB;

            //timercounterSys += Time.deltaTime; // for debuging

            //StartLogging();
            //int dataCount = VarjoEyeTracking.GetGazeList(out dataSinceLastUpdate);
            //foreach (var data in dataSinceLastUpdate)
            //{
            //    LogGazeData(data);
            //}
        }
        else if(!isSaving) StopLogging();
        
    }

    bool isHitStable(Vector3 newpos, Vector3 oldpos)
    {
        bool state = true;
        bool isEyeBlinked1 = false, isEyeBlinked2 = false;

        if (newpos.x == 0 && newpos.y == 0 && newpos.z == 0)
        {
            isEyeBlinked1 = true;
        }
        else isEyeBlinked1 = false;

        float dy = newpos.y - oldpos.y;
        float dz = newpos.z - oldpos.z;

        if (dy <= 2 && dy >= 2 && dz <= 2 && dz >= 2)
        {
            isEyeBlinked2 = true;
        }
        else isEyeBlinked2 = false;

        if (isEyeBlinked1 && isEyeBlinked2) state = false;
        else state = true;

        return state;        
    }
    //--------------------------------------------------
    private void OnGUI()
    {
        GUI.color = Color.red;
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

        //GUILayout.Label("Ceiling : "  + durCeiling.ToString("f2"));
        //GUILayout.Label("Floor \t :" + durFloor.ToString("f2"));
        //GUILayout.Label("Male Face :" + durMaleF.ToString("f2"));
        //GUILayout.Label("Male Body :" + durMaleB.ToString("f2"));
        //GUILayout.Label("Female Face :" + durFemaleF.ToString("f2"));
        //GUILayout.Label("Female Body :" + durFemaleB.ToString("f2"));

        GUILayout.Label("Total Gaze Hit " + totalHitDuration.ToString("f2"));
        GUILayout.Label("Timer " + timercounter.ToString("f2"));

        GUILayout.Label("TimerSys " + timercounterSys.ToString("f2"));

        // Make the windows be draggable.
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }
    //------------------------------------------------------


    void AddForceAtHitPosition()
    {
        //Get Rigidbody form hit object and add force on hit position
        Rigidbody rb = hit.rigidbody;
        if (rb != null)
        {
            rb.AddForceAtPosition(direction * hitForce, hit.point, ForceMode.Force);
        }
    }

    void LogGazeData(VarjoEyeTracking.GazeData data)
    {
        string[] logData = new string[18];

        // Gaze data frame number
        //logData[0] = data.frameNumber.ToString();
        logData[0] = vidFrame.ToString();

        // Gaze data capture time (nanoseconds)
        logData[1] = data.captureTime.ToString();

        // Log time (milliseconds)
        logData[2] = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();

        // HMD
        logData[3] = xrCamera.transform.localPosition.ToString("F3");
        logData[4] = xrCamera.transform.localRotation.ToString("F3");

        // Combined gaze
        bool invalid = data.status == VarjoEyeTracking.GazeStatus.Invalid;
        logData[5] = invalid ? InvalidString : ValidString;
        logData[6] = invalid ? "" : data.gaze.forward.ToString("F3");
        logData[7] = invalid ? "" : data.gaze.origin.ToString("F3");

        // Left eye
        bool leftInvalid = data.leftStatus == VarjoEyeTracking.GazeEyeStatus.Invalid;
        logData[8] = leftInvalid ? InvalidString : ValidString;
        logData[9] = leftInvalid ? "" : data.left.forward.ToString("F3");
        logData[10] = leftInvalid ? "" : data.left.origin.ToString("F3");
        logData[11] = leftInvalid ? "" : data.leftPupilSize.ToString();

        // Right eye
        bool rightInvalid = data.rightStatus == VarjoEyeTracking.GazeEyeStatus.Invalid;
        logData[12] = rightInvalid ? InvalidString : ValidString;
        logData[13] = rightInvalid ? "" : data.right.forward.ToString("F3");
        logData[14] = rightInvalid ? "" : data.right.origin.ToString("F3");
        logData[15] = rightInvalid ? "" : data.rightPupilSize.ToString();

        // Focus
        logData[16] = invalid ? "" : data.focusDistance.ToString();
        logData[17] = invalid ? "" : data.focusStability.ToString();

        Log(logData);
    }


    // Write given values in the log file
    void Log(string[] values)
    {
        if (!logging || writer == null)
            return;

        string line = "";
        for (int i = 0; i < values.Length; ++i)
        {
            values[i] = values[i].Replace("\r", "").Replace("\n", ""); // Remove new lines so they don't break csv
            line += values[i] + (i == (values.Length - 1) ? "" : ";"); // Do not add semicolon to last data string
        }
        writer.WriteLine(line);
    }

    // Save Gaze-PointofInterest Data separately from log
    void SaveGazeHitCollider()
    {
        DateTime now = DateTime.Now;
        string fileTime = string.Format("{0}-{1:00}-{2:00}-{3:00}-{4:00} _", now.Year, now.Month, now.Day, now.Hour, now.Minute);

        string filePath = customLogPath + fileTime + vidID + "-" + userID +".txt";

        /*
        using (StreamWriter colwriter = new StreamWriter(filePath, true))
        {           
            colwriter.WriteLine("Ceiling:\t  " + durCeiling.ToString("f2"));
            colwriter.WriteLine("Floor:\t " + durFloor.ToString("f2"));
            colwriter.WriteLine("Male Face:\t " + durMaleF.ToString("f2"));
            colwriter.WriteLine("Male Body:\t " + durMaleB.ToString("f2"));
            colwriter.WriteLine("Female Face:\t " + durFemaleF.ToString("f2"));
            colwriter.WriteLine("Female Body:\t " + durFemaleB.ToString("f2"));
            colwriter.WriteLine("Total Gaze Hit:\t " + totalHitDuration.ToString("f2"));
            colwriter.WriteLine("Timer:\t " + timercounter.ToString("f2"));
        }
        */
        using (StreamWriter colwriter = new StreamWriter(filePath, true))
        {
            totalHitDuration = durCeiling + durFloor + durMaleF + durMaleB + durFemaleF + durFemaleB;

            colwriter.WriteLine("Ceiling; Floor;  Male Face; Male Body; Female Face; Female Body; Total Gaze Hit; Timer");
            colwriter.Write(durCeiling.ToString("f2") + "; ");
            colwriter.Write(durFloor.ToString("f2") + "; ");
            colwriter.Write(durMaleF.ToString("f2") + "; ");
            colwriter.Write(durMaleB.ToString("f2") + "; ");
            colwriter.Write(durFemaleF.ToString("f2") + "; ");
            colwriter.Write(durFemaleB.ToString("f2") + "; ");
            colwriter.Write(totalHitDuration.ToString("f2") + "; ");
            colwriter.WriteLine(timercounter.ToString("f2"));
        }
     }

    void SaveAppendingGazeHitCol()
    {
        string filePath = customLogPath + "ColliderHitData.csv";

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

    public void StartLogging()
    {
        if (logging)
        {
            Debug.LogWarning("Logging was on when StartLogging was called. No new log was started.");
            return;
        }

        logging = true;

        string logPath = useCustomLogPath ? customLogPath : Application.dataPath + "/Logs/";
        Directory.CreateDirectory(logPath);

        DateTime now = DateTime.Now;
        string fileName = string.Format("{0}-{1:00}-{2:00}-{3:00}-{4:00} _", now.Year, now.Month, now.Day, now.Hour, now.Minute);

        //Use predetermined folder/path
        string path = customLogPath + fileName + vidID + "-" + userID +".csv";   //logPath +
        writer = new StreamWriter(path);

        Log(ColumnNames);
        Debug.Log("Log file started at: " + path); 
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

    void OnApplicationQuit()
    {
        if (isVideoStart) SaveAppendingGazeHitCol(); //SaveGazeHitCollider();  //save total amount of gazing towards point of interests (collider)
        StopLogging();
    }
}
