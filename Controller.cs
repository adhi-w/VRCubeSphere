using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using System.IO;
using Varjo.XR;


public class Controller : MonoBehaviour
{   
    public bool SceneA = true;

    private Rect windowRect = new Rect(20, 20, 180, 300);
    public string userID = "Subject ID";
    public string vidID = "Demo"; //UR
    string initID;
    string sceneName;

    public bool isEyeCalibrated = false;
    public bool isSaving= false;
    public bool isVideoStart = false;

    bool isStart = false;
    bool isVideoFinished = false;
    bool isShifted = false;

    private VideoPlayer vidPlayer;

    //------------------old-----------------------
    GameObject ColliderSceneA, ColliderSceneB;

    GameObject[] ColliderMaleScene2 = new GameObject[6];
     GameObject[] ColliderFemaleScene2 = new GameObject[6];
     GameObject[] ColliderMaleScene3 = new GameObject[4];
     GameObject[] ColliderFemaleScene3 = new GameObject[4];
    //-----------------------old------------------------

    [Header("ColliderEvents Setting")]
    [SerializeField]
    GameObject colliderEvents_anchor;

    [SerializeField]
    GameObject CollCeilingFloorN, CollCeilingFloorS, ColliderAN, ColliderAS, ColliderBN, ColliderBS;

    [SerializeField]
    GameObject[]
        ColliderMaleBodyAN = new GameObject[4],
        ColliderFemaleBodyAN = new GameObject[4];

    [SerializeField]
    GameObject[]
        ColliderMaleBodyAS = new GameObject[4],
        ColliderFemaleBodyAS = new GameObject[4];

    [SerializeField]
    GameObject[]
        ColliderMaleBodyBN = new GameObject[2],
        ColliderFemaleBodyBN = new GameObject[2];

    [SerializeField]
    GameObject[]
        ColliderMaleBodyBS = new GameObject[2],
        ColliderFemaleBodyBS = new GameObject[2];
   
    VarjoDepthOcclusion DepthOcclusion;


    [Header("Video Info")]
    public float videoFramerate;
    public float frame;
    public float video_duration;
    public float video_index_time;

    public float addCm = 5;
    float sc = 1;    
    float px, py, pz;

    bool viewpoint_flag = false;

    public GameObject camera_anchor;
    public Vector3 calibrateCamera;
    public Vector3 addHeight, camera;
    Vector3 calibrateCameraRot;

    public float tall;

    float setCM = 193.75f; //170; 
    float initCM;

    private void Awake()
    {     

       // Show Video-See-Through 
        DepthOcclusion = gameObject.GetComponent<VarjoDepthOcclusion>();
        DepthOcclusion.isDisable = true; //Activate VidSeeThru
    }

    // Start is called before the first frame update
    void Start()
    {            
        vidPlayer = GetComponent<VideoPlayer>();
        //video_duration = (float) vidPlayer.clip.length; //shows Video length / duration

        ////select video source
        SelectVideo();

        sc = transform.localScale.x;
        viewpoint_flag = true;
        
        initCM = (setCM - 155 )/ 155 * 6;

    }

   void SelectVideo()
    {
        if (SceneA)
        {           
            vidPlayer.url = "file://D:/Videos (D)/InstaPro2/Scene 2_v3_185011.mp4";
            initID = "A";
            sceneName = "Scene-A";
            //  ColliderSceneA.SetActive(true); //old
            //  ColliderSceneB.SetActive(false);  //old


            video_duration = 141.4740f;
            //141.5414
        }
        else 
        {
            vidPlayer.url = "file://D:/Videos (D)/InstaPro2/Scene 3_v3_191713.mp4";
            initID = "B";
            sceneName = "Scene-B";
            //   ColliderSceneA.SetActive(false);      //old
            //  ColliderSceneB.SetActive(true);     //old

            video_duration = 136.3650f;
            //136.4363
        }
       
    }

    // Update is called once per frame

    void Update()
    {
        
        UseKeyboard();      

        video_index_time = (float) vidPlayer.time;  //video current/index time when it is playing
        frame = vidPlayer.frame;
        videoFramerate = frame / (float)video_index_time;        

        CalibrateViewpoint(viewpoint_flag);
        viewpoint_flag = false;

        //transform.position = calibrateCamera + new Vector3(px, py - initCM, pz);   //py - initCM -->this will add height 
        transform.position = calibrateCamera + new Vector3(px, py, pz);   // default
        transform.localScale = new Vector3(sc, transform.localScale.y, -sc);

        camera = camera_anchor.transform.localPosition; // this acts as label
        addHeight = new Vector3(px, -py, pz);

        tall = py * 155 / -6 + 155;

        onPause();
        isVideoStart = isStart;

        SetColliderEvents(frame);


        // SwitchColliderScene2(frame); //old
        // SwitchColliderScene3(frame); //old

        //Stop Video & switch to vidSeeThru --> this is very messy
        if (video_index_time >= video_duration)
        {
            vidPlayer.Stop();
            DepthOcclusion.isDisable = false;
        }
       
    }

    private void SetColliderEvents(float iframe)
    {
        // Enable/Disable Collider Events based on the selected scene and condition
        if (SceneA)
        {
            // disable Scene B colliders
            ColliderBN.SetActive(false);
            ColliderBS.SetActive(false);

            if (isShifted)
            {
                CollCeilingFloorN.SetActive(false);
                CollCeilingFloorS.SetActive(true);
                ColliderAN.SetActive(false);
                ColliderAS.SetActive(true);                
            }
            else
            {
                CollCeilingFloorN.SetActive(true);
                CollCeilingFloorS.SetActive(false);
                ColliderAN.SetActive(true);
                ColliderAS.SetActive(false);
            }

            SwitchColliderSceneA(iframe);
        }
        else //Scene B
        {
            // disable Scene A colliders
            ColliderAN.SetActive(false);
            ColliderAS.SetActive(false);

            if (isShifted)
            {
                CollCeilingFloorN.SetActive(false);
                CollCeilingFloorS.SetActive(true);
                ColliderBN.SetActive(false);
                ColliderBS.SetActive(true);
            }
            else
            {
                CollCeilingFloorN.SetActive(true);
                CollCeilingFloorS.SetActive(false);
                ColliderBN.SetActive(true);
                ColliderBS.SetActive(false);
            }
            SwitchColliderSceneB(iframe);
        }
    }
    
    private void SwitchColliderSceneA(float iframe)
    {
        bool M1=false, M2=false, M3 = false;        //Male position , M0 is the default position
        bool F1 = false, F2 = false, F3 = false;        //Male position, F0 is the default position

        //checking male pos in M1
        if (iframe >= 295 && iframe <= 635) M1 = true;
        else if (iframe >= 1986 && iframe <= 2026) M1 = true;
        else if (iframe >= 3208 && iframe <= 3923) M1 = true;
        else M1 = false;

        //checking male pos in M2
        if (iframe >= 1131 && iframe <= 1644) M2 = true;
        else if (iframe >= 2441 && iframe <= 2657) M2 = true;
        else M2 = false;

        //checking male pos in M3
        if (iframe >= 636 && iframe <= 850) M3 = true;
        else if (iframe >= 1645 && iframe <= 1985) M3 = true;
        else if (iframe >= 2026 && iframe <= 2428) M3 = true;
        else if (iframe >= 2657 && iframe <= 3208) M3 = true;
        else M3 = false;

        //////*******//////
        //checking female pos in F1
        if (iframe >= 910 && iframe <= 926) F1 = true;
        else if (iframe >= 1008 && iframe <= 2333) F1 = true;
        else F1 = false;

        //checking female pos in F2
        if (iframe >= 2333 && iframe <= 2958) F2 = true;
        else if (iframe >= 3650 && iframe <= 3885) F2 = true;        
        else F2 = false;

        //checking female pos in F3
        if (iframe >= 926 && iframe <= 1008) F3 = true;
        else if (iframe >= 2958 && iframe <= 3650) F3 = true;
        else F3 = false;

        // Check the selective condition (normal / shifted)
        if (isShifted)
        {
            //Switch collider position for Male Body& Face for Scene A Shifted
            if (M1 == true) 
            {
                ColliderMaleBodyAS[0].SetActive(false);
                ColliderMaleBodyAS[1].SetActive(true);
                ColliderMaleBodyAS[2].SetActive(false);
                ColliderMaleBodyAS[3].SetActive(false);
            }
            else if (M2 == true)
            {
                ColliderMaleBodyAS[0].SetActive(false);
                ColliderMaleBodyAS[1].SetActive(false);
                ColliderMaleBodyAS[2].SetActive(true);
                ColliderMaleBodyAS[3].SetActive(false);
            }
            else if (M3 == true)
            {
                ColliderMaleBodyAS[0].SetActive(false);
                ColliderMaleBodyAS[1].SetActive(false);
                ColliderMaleBodyAS[2].SetActive(false);
                ColliderMaleBodyAS[3].SetActive(true);
            }
            else // default pos -- M0
            {
                ColliderMaleBodyAS[0].SetActive(true);
                ColliderMaleBodyAS[1].SetActive(false);
                ColliderMaleBodyAS[2].SetActive(false);
                ColliderMaleBodyAS[3].SetActive(false);
            }

            //Switch collider position for Female Body& Face for Scene A Shifted
            if (F1 == true)
            {
                ColliderFemaleBodyAS[0].SetActive(false);
                ColliderFemaleBodyAS[1].SetActive(true);
                ColliderFemaleBodyAS[2].SetActive(false);
                ColliderFemaleBodyAS[3].SetActive(false);
            }
            else if (F2 == true)
            {
                ColliderFemaleBodyAS[0].SetActive(false);
                ColliderFemaleBodyAS[1].SetActive(false);
                ColliderFemaleBodyAS[2].SetActive(true);
                ColliderFemaleBodyAS[3].SetActive(false);
            }
            else if (F3 == true)
            {
                ColliderFemaleBodyAS[0].SetActive(false);
                ColliderFemaleBodyAS[1].SetActive(false);
                ColliderFemaleBodyAS[2].SetActive(false);
                ColliderFemaleBodyAS[3].SetActive(true);
            }
            else // default pos -- F0
            {
                ColliderFemaleBodyAS[0].SetActive(true);
                ColliderFemaleBodyAS[1].SetActive(false);
                ColliderFemaleBodyAS[2].SetActive(false);
                ColliderFemaleBodyAS[3].SetActive(false);
            }
        }
        else  // Normal
        {
            //Switch collider position for Male Body& Face for Scene A Normal
            if (M1 == true)
            {
                ColliderMaleBodyAN[0].SetActive(false);
                ColliderMaleBodyAN[1].SetActive(true);
                ColliderMaleBodyAN[2].SetActive(false);
                ColliderMaleBodyAN[3].SetActive(false);
            }
            else if (M2 == true)
            {
                ColliderMaleBodyAN[0].SetActive(false);
                ColliderMaleBodyAN[1].SetActive(false);
                ColliderMaleBodyAN[2].SetActive(true);
                ColliderMaleBodyAN[3].SetActive(false);
            }
            else if (M3 == true)
            {
                ColliderMaleBodyAN[0].SetActive(false);
                ColliderMaleBodyAN[1].SetActive(false);
                ColliderMaleBodyAN[2].SetActive(false);
                ColliderMaleBodyAN[3].SetActive(true);
            }
            else // default pos -- M0
            {
                ColliderMaleBodyAN[0].SetActive(true);
                ColliderMaleBodyAN[1].SetActive(false);
                ColliderMaleBodyAN[2].SetActive(false);
                ColliderMaleBodyAN[3].SetActive(false);
            }

            //Switch collider position for Female Body& Face for Scene A Normal
            if (F1 == true)
            {
                ColliderFemaleBodyAN[0].SetActive(false);
                ColliderFemaleBodyAN[1].SetActive(true);
                ColliderFemaleBodyAN[2].SetActive(false);
                ColliderFemaleBodyAN[3].SetActive(false);
            }
            else if (F2 == true)
            {
                ColliderFemaleBodyAN[0].SetActive(false);
                ColliderFemaleBodyAN[1].SetActive(false);
                ColliderFemaleBodyAN[2].SetActive(true);
                ColliderFemaleBodyAN[3].SetActive(false);
            }
            else if (F3 == true)
            {
                ColliderFemaleBodyAN[0].SetActive(false);
                ColliderFemaleBodyAN[1].SetActive(false);
                ColliderFemaleBodyAN[2].SetActive(false);
                ColliderFemaleBodyAN[3].SetActive(true);
            }
            else // default pos -- F0
            {
                ColliderFemaleBodyAN[0].SetActive(true);
                ColliderFemaleBodyAN[1].SetActive(false);
                ColliderFemaleBodyAN[2].SetActive(false);
                ColliderFemaleBodyAN[3].SetActive(false);
            }

        }

    }

    private void SwitchColliderSceneB(float iframe)
    {
        bool Mb1 = false, Fb1 = false;

        //checking Male position in Mb1
        if (iframe >= 1962 && iframe <= 2409) Mb1 = true;
        else if (iframe >= 2574 && iframe <= 2733) Mb1 = true;
        else if (iframe >= 2909 && iframe <= 3573) Mb1 = true;
        else Mb1 = false;

        //checking Female position in Fb1
        if ((iframe >= 1740 && iframe <= 1980)) Fb1 = true;
        else if (iframe >= 2884 && iframe <= 2980) Fb1 = true;
        else if (iframe >= 4000 && iframe <= 4086) Fb1 = true;
        else Fb1 = false;

        // Check the selective condition (normal / shifted)
        if (isShifted)
        {
            //Switch collider position for Male Body & Face for Scene B shifted
            if (Mb1 == true)
            {
                ColliderMaleBodyBS[0].SetActive(false);
                ColliderMaleBodyBS[1].SetActive(true);
            }
            else // default pos -- F0            
            {
                ColliderMaleBodyBS[0].SetActive(true);
                ColliderMaleBodyBS[1].SetActive(false);
            }

            //Switch collider position for Female Body & Face   for Scene B shifted
            if (Fb1 == true)
            {
                ColliderFemaleBodyBS[0].SetActive(false);
                ColliderFemaleBodyBS[1].SetActive(true);
            }
            else // default pos -- Fb0
            {
                ColliderFemaleBodyBS[0].SetActive(true);
                ColliderFemaleBodyBS[1].SetActive(false);
            }
        }
        else // Normal
        {
            //Switch collider position for Male Body & Face for Scene B shifted
            if (Mb1 == true)
            {
                ColliderMaleBodyBN[0].SetActive(false);
                ColliderMaleBodyBN[1].SetActive(true);
            }
            else // default pos -- F0            
            {
                ColliderMaleBodyBN[0].SetActive(true);
                ColliderMaleBodyBN[1].SetActive(false);
            }

            //Switch collider position for Female Body & Face   for Scene B shifted
            if (Fb1 == true)
            {
                ColliderFemaleBodyBN[0].SetActive(false);
                ColliderFemaleBodyBN[1].SetActive(true);
            }
            else // default pos -- Fb0
            {
                ColliderFemaleBodyBN[0].SetActive(true);
                ColliderFemaleBodyBN[1].SetActive(false);
            }
        }
    }


    private void SwitchColliderScene2(float iframe)
    {
        bool A2 = false, A3 = false;        // Male position
        bool B2 = false, B3 = false;        // Female position

        //checking male pos in A2
        if (iframe >= 307 && iframe <= 646) A2 = true;       
        else if(iframe >= 1653 && iframe <= 2180) A2 = true;
        else if (iframe >= 3204 && iframe <= 3930) A2 = true;
        else A2 = false;

        //checking male pos in A3
        if (iframe >= 1140 && iframe <= 1652) A3 = true;
        else if (iframe >= 2441 && iframe <= 2663) A3 = true;
        else A3 = false;       

        //Switch collider position for Male Body& Face
        if (A2 == true)
        {
            ColliderMaleScene2[0].SetActive(false);
            ColliderMaleScene2[1].SetActive(false);
            ColliderMaleScene2[2].SetActive(true);
            ColliderMaleScene2[3].SetActive(true);
            ColliderMaleScene2[4].SetActive(false);
            ColliderMaleScene2[5].SetActive(false);
        }
        else if(A3 == true)
        {
            ColliderMaleScene2[0].SetActive(false);
            ColliderMaleScene2[1].SetActive(false);
            ColliderMaleScene2[2].SetActive(false);
            ColliderMaleScene2[3].SetActive(false);
            ColliderMaleScene2[4].SetActive(true);
            ColliderMaleScene2[5].SetActive(true);
        }
        else
        {
            ColliderMaleScene2[0].SetActive(true);
            ColliderMaleScene2[1].SetActive(true);
            ColliderMaleScene2[2].SetActive(false);
            ColliderMaleScene2[3].SetActive(false);
            ColliderMaleScene2[4].SetActive(false);
            ColliderMaleScene2[5].SetActive(false);
        }

        /**/
        //checking female pos in B2
        if (iframe >= 912 && iframe <= 2273) B2 = true;
        else if (iframe >= 2962 && iframe <= 3642) B2 = true;
        else B2 = false;

        //checking female pos in B3
        if (iframe >= 2314 && iframe <= 2961) B3 = true;
        else if (iframe >= 3641 && iframe <= 3861) B3 = true;
        else if (iframe >= 4225 && iframe <= 4240) B3 = true;
        else B3 = false;

        //Switch collider position for Female Body& Face
        if (B2 == true)
        {
            ColliderFemaleScene2[0].SetActive(false);
            ColliderFemaleScene2[1].SetActive(false);
            ColliderFemaleScene2[2].SetActive(true);
            ColliderFemaleScene2[3].SetActive(true);
            ColliderFemaleScene2[4].SetActive(false);
            ColliderFemaleScene2[5].SetActive(false);
        }
        else if (B3 == true)
        {
            ColliderFemaleScene2[0].SetActive(false);
            ColliderFemaleScene2[1].SetActive(false);
            ColliderFemaleScene2[2].SetActive(false);
            ColliderFemaleScene2[3].SetActive(false);
            ColliderFemaleScene2[4].SetActive(true);
            ColliderFemaleScene2[5].SetActive(true);
        }
        else
        {
            ColliderFemaleScene2[0].SetActive(true);
            ColliderFemaleScene2[1].SetActive(true);
            ColliderFemaleScene2[2].SetActive(false);
            ColliderFemaleScene2[3].SetActive(false);
            ColliderFemaleScene2[4].SetActive(false);
            ColliderFemaleScene2[5].SetActive(false);
        }
       
    }

    private void SwitchColliderScene3(float iframe)
    {
        bool C = false, D=false;

        //checking Male position in C2
        if (iframe >= 1962 && iframe <= 2409) C = true;
        else if (iframe >= 2574 && iframe <= 2733) C = true;
        else if (iframe >= 2909 && iframe <= 3573) C = true;
        else C = false;

        //checking Female position in D2
        if ((iframe >= 1740 && iframe <= 1980)) D = true;
        else if (iframe >= 2884 && iframe <= 2980) D = true;
        else if (iframe >= 4000 && iframe <= 4086) D = true;
        else D = false;

        //Switch collider position for Male Body & Face
        if(C==true)
        {
            ColliderMaleScene3[0].SetActive(false);
            ColliderMaleScene3[1].SetActive(false);
            ColliderMaleScene3[2].SetActive(true);
            ColliderMaleScene3[3].SetActive(true);
        }
        else
        {
            ColliderMaleScene3[0].SetActive(true);
            ColliderMaleScene3[1].SetActive(true);
            ColliderMaleScene3[2].SetActive(false);
            ColliderMaleScene3[3].SetActive(false);
        }

        //Switch collider position for Female Body & Face
        if (D == true)
        {
            ColliderFemaleScene3[0].SetActive(false);
            ColliderFemaleScene3[1].SetActive(false);
            ColliderFemaleScene3[2].SetActive(true);
            ColliderFemaleScene3[3].SetActive(true);
        }
        else
        {
            ColliderFemaleScene3[0].SetActive(true);
            ColliderFemaleScene3[1].SetActive(true);
            ColliderFemaleScene3[2].SetActive(false);
            ColliderFemaleScene3[3].SetActive(false);
        }
    }

    private void CalibrateViewpoint(bool flag)
    {
        if (flag || Input.GetKeyDown(KeyCode.C))
        {
            calibrateCamera = camera_anchor.transform.localPosition;
            calibrateCameraRot = camera_anchor.transform.localRotation.eulerAngles;

            // Set the cubeSphere y-axis rotation relative to the camera
            // this is used for a reference
            transform.localRotation = Quaternion.Euler(0, calibrateCameraRot.y, 0);

            //Set colliderEvents anchor
            //the initial position for both camera & colider events are similar
            //the initial rotation of colider events is relative to the cubesphere, means both have similar rotation
            colliderEvents_anchor.transform.localPosition = camera_anchor.transform.localPosition;
            colliderEvents_anchor.transform.localRotation = this.transform.localRotation;
        }               
    }

    void OnGUI()
    {      
        //GUI.color = Color.green;
        windowRect = GUI.Window(0, windowRect, DoMyWindow, sceneName + " Controls");
    }

   public void DoMyWindow(int windowID)
    {
        GUI.enabled = !isStart;
        userID = GUI.TextField(new Rect(15, 20, 140, 30), userID);
        isShifted = GUI.Toggle(new Rect(15, 60, 140, 30), isShifted, "Shifted " + sceneName);

        if (GUI.Button(new Rect(15, 100, 140, 30), "Calibrate User Gaze"))
        {
            isEyeCalibrated = true;
            VarjoEyeTracking.RequestGazeCalibration(VarjoEyeTracking.GazeCalibrationMode.Fast);
        }

        if (GUI.Button(new Rect(15, 140, 140, 30), "Calibrate User View"))
        {
            viewpoint_flag = true;           
        }        

        if (GUI.Button(new Rect(15, 180, 140, 30), "Start"))
        {
            DepthOcclusion.isDisable = true;

            isStart = true;
            isSaving = true;           

            vidPlayer.Play();
            
            //filePath = System.IO.Path.Combine(Application.persistentDataPath, userID);

            //try
            //{
            //    if (!Directory.Exists(filePath))
            //    {
            //        Directory.CreateDirectory(filePath);
            //    }
            //}
            //catch (IOException ex)
            //{
            //    Debug.LogError(ex.Message);
            //}
        }

        GUI.enabled = isVideoFinished;
       
        if (GUI.Button(new Rect(15, 240, 140, 30), "Exit"))
        {
            isSaving = false;
            OnButtonClose();
        }

        GUI.enabled = true;

        //------------------------------------
        // Notification
        if (isShifted)
        {
            py = - (setCM - 155) / 155 * 6;  //Don't forget to uncomment this if you want to use GUI 
            GUI.Label(new Rect(10, 280, 160, 50), "Note: Shifted " + sceneName);
            vidID = initID + "S";
        }
        else
        {
               py = 0;   //Don't forget to uncomment this if you want to use GUI 
            GUI.Label(new Rect(10, 280, 160, 50), "Note: Normal " + sceneName);
            vidID = initID + "N";
        }

        // Make the windows be draggable.
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }

    private void onPause()
    {
        /* Use this code for play/pause in debugging mode
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (vidPlayer.isPlaying) vidPlayer.Pause();
            else if (vidPlayer.isPaused) vidPlayer.Play();
        }
        */

        float f = vidPlayer.frame;
        float length = vidPlayer.frameCount;
        bool isStopped = false;

        if (f == length - 1)    isStopped = true;           
        else if (isStopped == false)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (isStart) isStart = false;
                else if (!isStart) isStart = true;
            }

            else if (vidPlayer.isPlaying && !isStart) vidPlayer.Pause();
            else if (vidPlayer.isPaused && isStart) vidPlayer.Play();
        }
        else vidPlayer.Pause();

        isVideoFinished = isStopped;
    }
    private void OnButtonClose()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
    }
    void UseKeyboard()
    {

        //----------Change the POSITION of Object (related to user viewpoint)----------
        if (Input.GetKeyUp(KeyCode.DownArrow))         // Y Position    --> Taller / Shorter
        {
            py += addCm / 155 * 6;
            //py += dis;          
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            py -= addCm / 155 * 6;
            //py -= dis;           
        }

        //if (Input.GetKeyUp(KeyCode.A))         // X Position
        //{
        //    px += dis;
        //}
        //if (Input.GetKeyUp(KeyCode.D))
        //{
        //    px -= dis;
        //}

        if (Input.GetKeyUp(KeyCode.S))         // Z Position
        {            
            //pz += cm;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            //pz -= cm;
        }


        ////----------------Change the SCALE of Sphere---------------
        if (Input.GetKeyDown(KeyCode.M))         // Scale
        {
            sc += 1;   
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            sc -= 1;    
        }

        // ------------------------------------------

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (vidPlayer.isPlaying) vidPlayer.Pause();
            else if (vidPlayer.isPaused) vidPlayer.Play();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isSaving = false;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif
        }
    }

}
