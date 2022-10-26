using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

// This script handles the logic for generating image data.
public class RecordingManager : MonoBehaviour
{

    [SerializeField] RecorderController main;
    [SerializeField] RecorderController overhead;
    [SerializeField] RecorderController egocentric;
    [SerializeField] RecorderController main_depth;
    [SerializeField] RecorderController overhead_depth;
    [SerializeField] RecorderController egocentric_depth;
    [SerializeField] RecorderController main_skeletal;
    [SerializeField] RecorderController overhead_skeletal;
    [SerializeField] RecorderController egocentric_skeletal;

    [SerializeField] int framerate;


    //ego bounding box stuff
    string egoDir;
    //List<Vector2> egoBB;
    public List<GameObject> physicalObjects;
    //GameObject obj;
    List<List<Rect>> EgoBB;//when serializing need to put bounding boxes: and then each object's uid
    Camera egoCamera;
    bool mainBool;
    bool topLBottomL;
    public bool recordDepthCamera;
    public bool recordSkeletalCamera;



    int keyframes_start;
    int keyframes_end;


    [SerializeField] public bool Recording = true;



    // Start is called before the first frame update
    public void ManualStart(List<GameObject> physicalObjects)
    {
        this.physicalObjects = physicalObjects;

    }


    //Change input to take in gameobject camera. (no need to search for gameobject w tag)
    // this only generates video, not sure how to config for a screenshot at gesture peak.
    public void Configure(string scenedatafolder, string cameraTag, string cameraView, int instanceindex, string template, GameObject go, Camera cam, bool pngOrJpg, bool topLBottomL, string cameraType)
    {


        //setup output file name, format, select correct camera, based on GameObject tag. (MainCamera for main, overheadcam for overhead)
        CameraInputSettings cameraInputSettings = new CameraInputSettings();
        if (cameraTag == "exo_view")
        {
            cameraInputSettings.CameraTag = "MainCamera" + instanceindex;
        }
        else if (cameraTag == "top_view")
        {
            cameraInputSettings.CameraTag = "overheadcam" + instanceindex;
        }
        else if (cameraTag == "ego_view")
        {
            cameraInputSettings.CameraTag = "egocentriccam" + instanceindex;
        }
        else if (cameraTag == "exo_view_skeletal")
        {
            cameraInputSettings.CameraTag = "MainCameraSkeletal" + instanceindex;
        }
        else if (cameraTag == "top_view_skeletal")
        {
            cameraInputSettings.CameraTag = "overheadcamskeletal" + instanceindex;
        }
        else if (cameraTag == "ego_view_skeletal")
        {
            cameraInputSettings.CameraTag = "egocentriccamskeletal" + instanceindex;
        }
        else if (cameraTag == "exo_view_depth")
        {
            cameraInputSettings.CameraTag = "MainCameraDepth" + instanceindex;
        }
        else if (cameraTag == "top_view_depth")
        {
            cameraInputSettings.CameraTag = "overheadcamdepth" + instanceindex;
        }
        else if (cameraTag == "ego_view_depth")
        {
            cameraInputSettings.CameraTag = "egocentriccamdepth" + instanceindex;
        }

        this.topLBottomL = topLBottomL;

        cameraInputSettings.Source = ImageSource.TaggedCamera;

        ImageRecorderSettings imageRecorderSettings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        RecordingSession session = new RecordingSession();

        imageRecorderSettings.Enabled = true;
        if (pngOrJpg)
            imageRecorderSettings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        else
            imageRecorderSettings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.JPEG;

        imageRecorderSettings.FileNameGenerator.FileName = DefaultWildcard.Frame;
        imageRecorderSettings.FileNameGenerator.Root = OutputPath.Root.Absolute;
        imageRecorderSettings.FileNameGenerator.Leaf = scenedatafolder + "/" + template + "/" + cameraView + "/canonical_frame_" + cameraType;
        imageRecorderSettings.FileNameGenerator.CreateDirectory(session);

        //output file naming convention
        //todo add root directory to leaf
        string leaf = scenedatafolder + "/" + template + "/" + cameraView + "/video_" + cameraType;
        //imageRecorderSettings.OutputFile = FileNameGenerator.SanitizePath( leaf + "/vid/");

        imageRecorderSettings.FileNameGenerator.FileName = DefaultWildcard.Frame;
        //TODO HERE SET ROOT 

        imageRecorderSettings.FileNameGenerator.Leaf = leaf;
        //Debug.Log(imageRecorderSettings.FileNameGenerator.BuildAbsolutePath(session));
        imageRecorderSettings.FileNameGenerator.CreateDirectory(session);


        // output file dimensions (Note: in editor, the game view needs have the display set to this resolution.)
        imageRecorderSettings.imageInputSettings.OutputHeight = Screen.height;
        imageRecorderSettings.imageInputSettings.OutputWidth = Screen.width;
        imageRecorderSettings.imageInputSettings.RecordTransparency = true;
        imageRecorderSettings.FrameRatePlayback = FrameRatePlayback.Constant;
        imageRecorderSettings.RecordMode = RecordMode.Manual;

        RecorderControllerSettings settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        settings.AddRecorderSettings(imageRecorderSettings);
        settings.FrameRate = framerate;
        imageRecorderSettings.imageInputSettings = cameraInputSettings;

        //create recorder controller according to params;
        if (Recording)
        {
            if (cameraTag == "exo_view")
            {
                main = new UnityEditor.Recorder.RecorderController(settings);

            }
            else if (cameraTag == "top_view")
            {
                overhead = new UnityEditor.Recorder.RecorderController(settings);

            }
            else if (cameraTag == "ego_view")
            {
                egoDir = leaf;
                egocentric = new UnityEditor.Recorder.RecorderController(settings);
                egoCamera = cam;

            }
            else if (cameraTag == "exo_view_skeletal")
            {
                main_skeletal = new UnityEditor.Recorder.RecorderController(settings);
            }
            else if (cameraTag == "top_view_skeletal")
            {
                overhead_skeletal = new UnityEditor.Recorder.RecorderController(settings);
            }
            else if (cameraTag == "ego_view_skeletal")
            {
                egocentric_skeletal = new UnityEditor.Recorder.RecorderController(settings);
            }
            else if (cameraTag == "exo_view_depth")
            {
                main_depth = new UnityEditor.Recorder.RecorderController(settings);
            }
            else if (cameraTag == "top_view_depth")
            {
                overhead_depth = new UnityEditor.Recorder.RecorderController(settings);
            }
            else if (cameraTag == "ego_view_depth")
            {
                egocentric_depth = new UnityEditor.Recorder.RecorderController(settings);
            }
        }
    }
    public void SetStartKeyframe()//have to call both of these four times (for each modality)
    {
        if (Recording)
            keyframes_start = EgoBB[0].Count;
    }
    public void SetEndKeyframe()
    {
        if (Recording)
            keyframes_end = EgoBB[0].Count;

    }

    public void StartRecording()
    {
        if (Recording)
        {
            main.PrepareRecording();
            main.StartRecording();
            overhead.PrepareRecording();
            overhead.StartRecording();
            egocentric.PrepareRecording();
            egocentric.StartRecording();


            if (recordDepthCamera)
            {
                main_depth.PrepareRecording();
                main_depth.StartRecording();
                overhead_depth.PrepareRecording();
                overhead_depth.StartRecording();
                egocentric_depth.PrepareRecording();
                egocentric_depth.StartRecording();
            }
            if (recordSkeletalCamera)
            {
                main_skeletal.PrepareRecording();
                main_skeletal.StartRecording();
                overhead_skeletal.PrepareRecording();
                overhead_skeletal.StartRecording();
                egocentric_skeletal.PrepareRecording();
                egocentric_skeletal.StartRecording();
            }
            EgoBB = new List<List<Rect>>();
            for (int i = 0; i < physicalObjects.Count; i++)
            {
                EgoBB.Add(new List<Rect>(150));//has never made more than like 120 frames
            }
        }
    }
    private void FixedUpdate()
    {
        if (!Recording)
            return;
        if (main != null && main.IsRecording())
        {

            if (mainBool)
            {
                for (int i = 0; i < physicalObjects.Count; i++)
                {
                    EgoBB[i].Add(GUIRectWithObject(physicalObjects[i], egoCamera));
                }
            }
            mainBool = !mainBool;
            //mainCounter++;
        }
        else
        {
            mainBool = false;
            //mainCounter = 0;
        }
    }


    public void StopRecording()
    {
        // If Recording boolean is checked in the inspector
        if (Recording)
        {
            main.StopRecording();
            overhead.StopRecording();
            egocentric.StopRecording();

            List<BoundingBoxes> bb = new List<BoundingBoxes>();
            for (int i = 0; i < physicalObjects.Count; i++)
            {
                BoundingBoxes boxes = new BoundingBoxes(EgoBB[i], physicalObjects[i].GetInstanceID(), topLBottomL);
                bb.Add(boxes);
            }
            string exported_ego_data = JsonUtility.ToJson(new VideoInformation(keyframes_start, keyframes_end, bb));
            WriteString(exported_ego_data, egoDir + "/bounding_boxes.json");
            //egoBB = new List<Vector2>();

            if (recordDepthCamera)
            {
                main_depth.StopRecording();
                overhead_depth.StopRecording();
                egocentric_depth.StopRecording();
            }
            if (recordSkeletalCamera)
            {
                main_skeletal.StopRecording();
                overhead_skeletal.StopRecording();
                egocentric_skeletal.StopRecording();
            }
        }

    }
    static void WriteString(string s, string path)
    {

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(s);
        writer.Close();


    }
    public Rect GUIRectWithObject(GameObject go, Camera camera)
    {
        //Vector3[] vertices = game_object.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] vertices = new Vector3[0];
        // apply the world transforms (position, rotation, scale) to the mesh points and then get their 2D position
        // relative to the camera


        if (go.transform.GetComponent<MeshFilter>() != null)
        {
            vertices = go.transform.GetComponent<MeshFilter>().mesh.vertices;
        }
        int count = 0;
        foreach (Transform child in go.transform)
        {
            if (child.GetComponent<MeshFilter>() != null)
            {
                count += child.GetComponent<MeshFilter>().mesh.vertices.Length;
            }
            foreach (Transform child2 in child)
            {
                if (child2.GetComponent<MeshFilter>() != null)
                {
                    count += child2.GetComponent<MeshFilter>().mesh.vertices.Length;
                }
            }
        }


        Vector2[] vertices_2d = new Vector2[vertices.Length + count];
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices_2d[i] = camera.WorldToScreenPoint(go.transform.TransformPoint(vertices[i]));
        }
        int j = vertices.Length;
        foreach (Transform child in go.transform)
        {
            if (child.GetComponent<MeshFilter>() != null)
            {
                foreach (Vector3 v in child.GetComponent<MeshFilter>().mesh.vertices)
                {
                    vertices_2d[j] = camera.WorldToScreenPoint(child.TransformPoint(v));
                    j++;
                }
            }
            foreach (Transform child2 in child)
            {
                if (child2.GetComponent<MeshFilter>() != null)
                {
                    foreach (Vector3 v in child2.GetComponent<MeshFilter>().mesh.vertices)
                    {
                        vertices_2d[j] = camera.WorldToScreenPoint(child2.TransformPoint(v));
                        j++;
                    }
                }
            }
        }
        //Debug.Log(vertices_2d.Length);
        if (vertices_2d.Length == 0)
        {
            Debug.Log("Should not be here please investigate");
            return new Rect();
        }

        // find the min max bounds of the 2D points
        Vector2 min = vertices_2d[0];
        Vector2 max = vertices_2d[0];
        foreach (Vector2 vertex in vertices_2d)
        {
            min = Vector2.Min(min, vertex);
            max = Vector2.Max(max, vertex);
        }
        if (min.x < 1)
            min.x = 1;
        if (min.y < 1)
            min.y = 1;
        if (max.x > Screen.width)
            max.x = Screen.width;
        if (max.y > Screen.height)
            max.y = Screen.height;
        if (min.x > Screen.width)
            min.x = Screen.width;
        if (min.y > Screen.height)
            min.y = Screen.height;
        if (max.x < 1)
            max.x = 1;
        if (max.y < 1)
            max.y = 1;
        // thats our perfect bounding box
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    // Save the animation to a new folder, named the current date/time.
    [System.Serializable]
    public struct VideoInformation
    {
        [SerializeField] int keyframes_start;
        [SerializeField] int keyframes_end;
        [SerializeField] List<BoundingBoxes> list_bounding_boxes;
        public VideoInformation(int keyframes_start, int keyframes_end, List<BoundingBoxes> list_bounding_boxes)
        {
            this.keyframes_end = keyframes_end;
            this.keyframes_start = keyframes_start;
            this.list_bounding_boxes = list_bounding_boxes;
        }
    }
    [System.Serializable]
    public struct BoundingBoxes
    {
        [SerializeField] int object_id;
        [SerializeField] List<Vector2> bounding_boxes;

        public BoundingBoxes(List<Rect> bb, int object_id, bool topLBottomL)
        {
            bounding_boxes = new List<Vector2>();
            for (int i = 0; i < bb.Count; i++)
            {
                if (bb[i].min.x == 1 && bb[i].max.x == 1 || bb[i].min.x == Screen.width && bb[i].max.x == Screen.width || bb[i].min.y == 1 && bb[i].max.y == 1 || bb[i].min.y == Screen.height && bb[i].max.y == Screen.height || bb[i].min.x == Screen.width && bb[i].max.x == 1 || bb[i].min.x == 1 && bb[i].max.x == Screen.width || bb[i].min.y == 1 && bb[i].max.y == Screen.height || bb[i].min.y == Screen.height && bb[i].max.y == 1)
                {
                    this.bounding_boxes.Add(new Vector2(-1, -1));
                    this.bounding_boxes.Add(new Vector2(-1, -1));
                }
                else
                {

                    if (!topLBottomL)
                    {
                        this.bounding_boxes.Add(bb[i].min);
                        this.bounding_boxes.Add(bb[i].max);
                    }
                    else
                    {
                        this.bounding_boxes.Add(new Vector2(bb[i].min.x, Screen.height - bb[i].min.y));
                        this.bounding_boxes.Add(new Vector2(bb[i].max.x, Screen.height - bb[i].max.y));
                    }
                }
            }
            this.object_id = object_id;
        }
    }
}


