using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

// This script handles the logic for generating image data.
public class ProceduralRecordingManager : MonoBehaviour
{
    [SerializeField] RecorderController main;

    [SerializeField] Gesture gesture;

    [SerializeField] Animator animator;

    [SerializeField] public BezierSolution.GeneratePointingBezier gpb;

    [SerializeField]  int framerate;

    [SerializeField] public bool Recording = true;



    // Start is called before the first frame update
    void Start()
    {
        Configure("procedural", "bezier");

         
        // Configure("overheadcam");
        // Configure("egocentriccam");
    }

    public void makelinear(){
        GameObject obj = gpb.GetActiveBezier();
       BezierSolution.BezierSpline spline=  obj.GetComponent<BezierSolution.BezierSpline>();
       spline.ConstructLinearPath();
    }

    public void makespline(){
        GameObject obj = gpb.GetActiveBezier();
        BezierSolution.BezierSpline spline=  obj.GetComponent<BezierSolution.BezierSpline>();
        spline.AutoConstructSpline2();
    }
    

    //Change input to take in gameobject camera. (no need to search for gameobject w tag)
    // this only generates video, not sure how to config for a screenshot at gesture peak.
    public void Configure(string scenedatafolder, string generatortype){

       
        //setup output file name, format, select correct camera, based on GameObject tag. (MainCamera for main, overheadcam for overhead)
        
        CameraInputSettings cameraInputSettings = new CameraInputSettings();
        
        
        cameraInputSettings.Source = ImageSource.MainCamera;

        RecorderSettings recorderSettings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        RecordingSession session = new RecordingSession();
        
        recorderSettings.Enabled = true;
        recorderSettings.OutputFile = 
        
        recorderSettings.FileNameGenerator.FileName = generatortype + "_" +DefaultWildcard.Frame;
        recorderSettings.FileNameGenerator.Leaf =scenedatafolder+ "/";
        recorderSettings.FileNameGenerator.CreateDirectory(session);
        
        //output file naming convention
        string leaf = scenedatafolder+ "/" ;
        //imageRecorderSettings.OutputFile = FileNameGenerator.SanitizePath( leaf + "/vid/");
    
        recorderSettings.FileNameGenerator.FileName = DefaultWildcard.Frame;
        recorderSettings.FileNameGenerator.Leaf = leaf;
        //Debug.Log(imageRecorderSettings.FileNameGenerator.BuildAbsolutePath(session));
        recorderSettings.FileNameGenerator.CreateDirectory(session);
        
        
        // output file dimensions (Note: in editor, the game view needs have the display set to this resolution.)
    
        recorderSettings.FrameRatePlayback = FrameRatePlayback.Constant;
        recorderSettings.RecordMode = RecordMode.Manual;

        RecorderControllerSettings settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        settings.AddRecorderSettings(recorderSettings);
        settings.FrameRate = framerate;
        recorderSettings.RecordMode = RecordMode.Manual;
        
        //create recorder controller according to params;

        main = new UnityEditor.Recorder.RecorderController(settings);
        
    }

    public void PlayAnimation(){

    }
    public void StartRecording()
    {
        if (Recording){
            main.PrepareRecording();
            main.StartRecording();
            //overhead.PrepareRecording();
            //overhead.StartRecording();
            //egocentric.PrepareRecording();
            //egocentric.StartRecording();
        }

    }


    public void StopRecording()
    {
        // If Recording boolean is checked in the inspector
        if(Recording){
            main.StopRecording();
            //overhead.StopRecording();
            //egocentric.StopRecording();

        }
        
    }


    // Save the animation to a new folder, named the current date/time.
    
   
}


