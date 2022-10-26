using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

public class SceneManager : MonoBehaviour
{
    public bool RUNNING;
    bool pngOrJpg;
    string directory;
    int width;
    int height;
    bool topLBottomL;
    bool recordDepthCamera;
    bool recordSkeletalCamera;
    [SerializeField] GameObject Scene;
    [SerializeField] int instanceIndex;
    [SerializeField] private int trialCount;
    bool drawBoundingBoxes;
    int currentModality;

    [SerializeField] GameObject lightPrefab;  // contains prefab for light. spawn up to three per scene.
    [SerializeField] List<GameObject> Lights; // contains all lights. used for exporting to JSON.
    [SerializeField] GameObject Table; // contains the table. used for exporting to JSON.

    [SerializeField] Renderer FloorRenderer; // the floor renderer, used to set material.
    [SerializeField] List<GameObject> Walls; // the walls, used to set colors.
    [SerializeField] List<List<Rect>> EgoBB; //first list dimensions is same as physical objects, second list dimension is 5, so is n by 5 matrix

    [SerializeField] GameObject ActivePerson; //set by SpawnPerson
    [SerializeField] GameObject currentObject;

    [SerializeField] string scenepath; // relative!
    [SerializeField] string absscenepath; // absolute!

    [SerializeField] string currentTemplate; //used for saving screenshots to correct folder. update in start trial()

    [Header("Markers and SpawnBounds")]

    [SerializeField] Transform personMarker; // contains pos / rotation / scale of where person should spawn.

    [SerializeField] Transform tableMarker; // contains pos / rot / scale of where table should spawn.

    [SerializeField] Transform lightSpawnBound1;
    [SerializeField] Transform lightSpawnBound2;

    [SerializeField] public static int trialBuffer { get; } = 2;

    [Header("Managers")]

    [SerializeField] ObjectManager objectManager;

    [SerializeField] public RecordingManager recordingManager;

    [SerializeField] GestureManager gestureManager;

    [Header("MainRecordingCameras")]

    [SerializeField] Camera mainscreenshot;
    [SerializeField] Camera overheadscreenshot;
    [SerializeField] Camera egocentricscreenshot;
    [SerializeField] GameObject egocentriccam; // default cameras
    [SerializeField] GameObject overheadcam; // default cameras
    [SerializeField] GameObject MainCamera; // default cameras

    [Header("AdditionalCamerafeatures")]
    [SerializeField] Camera exoSkeletalCamera;
    [SerializeField] Camera topSkeletalCamera;
    [SerializeField] Camera egoSkeletalCamera;
    [SerializeField] Camera exoSkeletalCameraVid;
    [SerializeField] Camera topSkeletalCameraVid;
    [SerializeField] Camera egoSkeletalCameraVid;

    [SerializeField] Camera exoDepthCamera;
    [SerializeField] Camera topDepthCamera;
    [SerializeField] Camera egoDepthCamera;
    [SerializeField] Camera exoDepthCameraVid;
    [SerializeField] Camera topDepthCameraVid;
    [SerializeField] Camera egoDepthCameraVid;


    [Header("Debug")]
    [SerializeField] GameObject testobj1;
    [SerializeField] GameObject testobj2;

    // this triggers mid animation. save a pic!
    public void TakeScreenshots()
    {
        Rect egoRect = GUIRectWithObject(currentObject, egocentricscreenshot);
        List<Rect> tempEgoBB = new List<Rect>(objectManager.physicalObjects.Count);
        for (int i = 0; i < objectManager.physicalObjects.Count; i++)
        {
            EgoBB[i].Add(GUIRectWithObject(objectManager.physicalObjects[i], egocentricscreenshot));
            tempEgoBB.Add(EgoBB[i][currentModality]);
        }
        Rect mainRect = GUIRectWithObject(currentObject, mainscreenshot);
        Rect overheadRect = GUIRectWithObject(currentObject, overheadscreenshot);
        //take screenshot for main camera, and only do for other cameras if recording those modalities
        StartCoroutine(MyScreenCapture.TakeScreenShot(mainscreenshot, absscenepath + "/" + currentTemplate + "/exo_view/canonical_frame_rgb/", drawBoundingBoxes, mainRect, pngOrJpg, width, height));
        StartCoroutine(MyScreenCapture.TakeScreenShot(overheadscreenshot, absscenepath + "/" + currentTemplate + "/top_view/canonical_frame_rgb/", drawBoundingBoxes, overheadRect, pngOrJpg, width, height));
        StartCoroutine(MyScreenCapture.TakeScreenShot(egocentricscreenshot, absscenepath + "/" + currentTemplate + "/ego_view/canonical_frame_rgb/", drawBoundingBoxes, egoRect, pngOrJpg, width, height, tempEgoBB));
        if (recordDepthCamera)
        {
            StartCoroutine(MyScreenCapture.TakeScreenShot(exoDepthCamera, absscenepath + "/" + currentTemplate + "/exo_view/canonical_frame_depth/", false, mainRect, pngOrJpg, width, height));
            StartCoroutine(MyScreenCapture.TakeScreenShot(topDepthCamera, absscenepath + "/" + currentTemplate + "/top_view/canonical_frame_depth/", false, overheadRect, pngOrJpg, width, height));
            StartCoroutine(MyScreenCapture.TakeScreenShot(egoDepthCamera, absscenepath + "/" + currentTemplate + "/ego_view/canonical_frame_depth/", false, egoRect, pngOrJpg, width, height, tempEgoBB));
        }
        if (recordSkeletalCamera)
        {
            StartCoroutine(MyScreenCapture.TakeScreenShot(exoSkeletalCamera, absscenepath + "/" + currentTemplate + "/exo_view/canonical_frame_skeletal/", false, mainRect, pngOrJpg, width, height));
            StartCoroutine(MyScreenCapture.TakeScreenShot(topSkeletalCamera, absscenepath + "/" + currentTemplate + "/top_view/canonical_frame_skeletal/", false, overheadRect, pngOrJpg, width, height));
            StartCoroutine(MyScreenCapture.TakeScreenShot(egoSkeletalCamera, absscenepath + "/" + currentTemplate + "/ego_view/canonical_frame_skeletal/", false, egoRect, pngOrJpg, width, height, tempEgoBB));
        }
        currentModality++;
    }

    public void SetupTrial()
    {

        recordingManager.StartRecording();
    }

    public IEnumerator StartTrial(GameObject personprefab, List<GameObject> object_pool, GameObject chosen_object, int num_objects, int absoluteindex, float bufferperiod, string cleandate, bool drawBoundingBoxes, bool pngOrJpg, string directory, int width, int height, bool topLBottomL, bool recordDepthCamera, bool recordSkeletalCamera, bool usePreloadedScene)
    {
        /**
        (1) At this stage the scene will not have a human
         (2) Randomly choose whether the target object will have multiple instances in the scene.
          But if the target object has multiple instances then randomly decide whether we will create
           multiple instances of other objects so that the scene is not biased. 
           (3) every object in the object library will appear the same number of times in the scene as the target object.
            Thus I will suggest choosing an object as target object from the object library before generating the scene
             (4) Ranmodly choose the number of objects 
             (5) vary the color (wall and table color) and light to create 15 presets

        **/

        // Spawn human, disable mesh so that egocentric camera exists (Object manager needs this to Generate Objects, check for visibility)

        ActivePerson = SpawnPerson(personprefab, usePreloadedScene);


        this.drawBoundingBoxes = drawBoundingBoxes;
        this.pngOrJpg = pngOrJpg;
        this.directory = directory;
        this.width = width;
        this.height = height;
        this.topLBottomL = topLBottomL;
        this.recordDepthCamera = recordDepthCamera;
        this.recordSkeletalCamera = recordSkeletalCamera;
        recordingManager.recordDepthCamera = recordDepthCamera;
        recordingManager.recordSkeletalCamera = recordSkeletalCamera;
        if (!recordDepthCamera)
        {
            topDepthCamera.gameObject.SetActive(false);
            exoDepthCamera.gameObject.SetActive(false);
            egoDepthCamera.gameObject.SetActive(false);
            topDepthCameraVid.gameObject.SetActive(false);
            exoDepthCameraVid.gameObject.SetActive(false);
            egoDepthCameraVid.gameObject.SetActive(false);
        }
        if (!recordSkeletalCamera)
        {
            topSkeletalCameraVid.gameObject.SetActive(false);
            exoSkeletalCameraVid.gameObject.SetActive(false);
            egoSkeletalCameraVid.gameObject.SetActive(false);
            topSkeletalCamera.gameObject.SetActive(false);
            exoSkeletalCamera.gameObject.SetActive(false);
            egoSkeletalCamera.gameObject.SetActive(false);
        }


        List<Renderer> humanrends = new List<Renderer>();
        foreach (var rend in ActivePerson.GetComponentsInChildren<Renderer>())
        {
            humanrends.Add(rend);
            rend.enabled = false;
        }

        // Start Generating Objects
        List<GameObject> physicalobjects;
        if (!usePreloadedScene)
        {
            objectManager.GenerateObjects(object_pool, chosen_object, num_objects, instanceIndex, egocentricscreenshot, mainscreenshot, bufferperiod);

            yield return new WaitForSeconds(bufferperiod); // wait for objects to settle.
            while (!objectManager.isFinished)
            {
                yield return new WaitForSeconds(bufferperiod / 4);
            }
            physicalobjects = objectManager.physicalObjects;
        }
        else
        {
            yield return new WaitForSeconds(bufferperiod);
            physicalobjects = new List<GameObject>();
            foreach (Transform trans in objectManager.transform)
            {
                physicalobjects.Add(trans.gameObject);
            }
        }


        for (int i = 0; i < physicalobjects.Count; i++)
        {
            if (physicalobjects[i] == null)
            {
                Debug.Log("null ISSUE please invesigate");
                yield return new WaitForSeconds(0.1f);
                physicalobjects[i] = objectManager.SpawnObject(object_pool[0], egocentricscreenshot, mainscreenshot, bufferperiod, i);
                yield return new WaitForSeconds(0.1f);

            }
            physicalobjects[i].GetComponentInChildren<Rigidbody>().isKinematic = true;
        }
        recordingManager.ManualStart(physicalobjects);
        //finished objection generation

        int chosen_index;

        int ref_index;
        //just changes the chosen and reference objects based on if a preloaded scene is being used
        if (!usePreloadedScene)
        {
            chosen_index = objectManager.ChosenObjectIndex;
            currentObject = physicalobjects[chosen_index];
            ref_index = objectManager.RefObjectIndex;
        }
        else
        {
            chosen_index = 0;
            currentObject = physicalobjects[chosen_index];
            ref_index = 2;
        }


        EgoBB = new List<List<Rect>>();
        for (int i = 0; i < physicalobjects.Count; i++)
        {
            EgoBB.Add(new List<Rect>(5));
        }

        // Generate Instructions (Relative only, Absolute available through the PhysicalObjects <Attribute> component).
        List<string> rel_instructions = objectManager.GetObjectPosition(physicalobjects[chosen_index].transform, physicalobjects[ref_index].transform); // rel_instructions[0] is from maincam, [1] is inverted.

        currentTemplate = "only_objects";
        string path = ConfigureRecordingManger(absoluteindex, currentTemplate, cleandate, physicalobjects[chosen_index], directory); // configure camera for OnlyObjects. (we're not actually going to record anything, but this sets up the correct folders.)

        scenepath = path;
        absscenepath = path;
        TakeScreenshots();

        yield return new WaitForSeconds(.5f);

        //generate wrongGazeAndGesture object, make sure it is far enough from the chosen object 
        int wrongobjindex = Random.Range(0, physicalobjects.Count);
        float thresh = 0.5f;
        int counter = 0;
        while (counter < 20 && (physicalobjects[wrongobjindex].name == physicalobjects[chosen_index].name || Vector3.Distance(physicalobjects[wrongobjindex].transform.position, physicalobjects[chosen_index].transform.position) < thresh))
        {
            counter++;
            wrongobjindex = Random.Range(0, physicalobjects.Count);
        }
        int wrongObjId = physicalobjects[wrongobjindex].GetInstanceID();
        if (usePreloadedScene)
        {
            wrongobjindex = 1;
            wrongObjId = physicalobjects[wrongobjindex].GetInstanceID();
        }


        // make the human visible
        foreach (var rend in ActivePerson.GetComponentsInChildren<Renderer>())
        {
            rend.enabled = true;
        }

        // Set Target object position to ChosenObject in obj manager;
        objectManager.setTarget(physicalobjects[chosen_index].transform);

        // Begin Recording -- Only Gaze
        currentTemplate = "only_gaze";
        path = ConfigureRecordingManger(absoluteindex, currentTemplate, cleandate, physicalobjects[chosen_index], directory); // configure camera for onlyGaze. 
        StartCoroutine(RecordGesture(true, false));
        while (gestureManager.AnimationPlaying)
        { //wait for animation to finish

            yield return null;
        }

        // Begin Recording -- Only Gesture
        currentTemplate = "only_gesture";
        path = ConfigureRecordingManger(absoluteindex, currentTemplate, cleandate, physicalobjects[chosen_index], directory); // configure camera for onlyGesture. 
        StartCoroutine(RecordGesture(false, true));
        while (gestureManager.AnimationPlaying)
        { //wait for animation to finish
            yield return null;
        }


        // Begin Recording -- Both Gaze and Gesture
        currentTemplate = "both_gaze_gesture";
        path = ConfigureRecordingManger(absoluteindex, currentTemplate, cleandate, physicalobjects[chosen_index], directory); // configure camera for bothGazeAndGesture.
        StartCoroutine(RecordGesture(true, true));
        while (gestureManager.AnimationPlaying)
        { //wait for animation to finish
            yield return null;
        }


        // Begin Recording -- Wrong Gaze and Gesture
        currentTemplate = "wrong_gaze_gesture";
        currentObject = physicalobjects[wrongobjindex];
        objectManager.setTarget(physicalobjects[wrongobjindex].transform);
        path = ConfigureRecordingManger(absoluteindex, currentTemplate, cleandate, physicalobjects[wrongobjindex], directory); // configure camera for wrongGazeAndGesture.
        StartCoroutine(RecordGesture(true, true));
        while (gestureManager.AnimationPlaying)
        { //wait for animation to finish
            yield return null;
        }

        currentTemplate = "wrong_gesture";
        path = ConfigureRecordingManger(absoluteindex, currentTemplate, cleandate, physicalobjects[wrongobjindex], directory); // configure camera for onlyGesture. 
        StartCoroutine(RecordGesture(false, true));
        while (gestureManager.AnimationPlaying)
        { //wait for animation to finish
            yield return null;
        }

        currentTemplate = "wrong_gaze";
        path = ConfigureRecordingManger(absoluteindex, currentTemplate, cleandate, physicalobjects[wrongobjindex], directory); // configure camera for onlyGaze. 
        StartCoroutine(RecordGesture(true, false));
        while (gestureManager.AnimationPlaying)
        { //wait for animation to finish 
            yield return null;
        }
        

        //find object not in physical objects but in object pool
        int uniqueObjCounter = 0;
        List<string> physicalobjectsNames = new List<string>();
        foreach (GameObject go in physicalobjects)
        {
            if (!physicalobjectsNames.Contains(go.name))
            {
                physicalobjectsNames.Add(go.name);
            }
        }
        int chosenindex = Random.Range(0, object_pool.Count);
        while (uniqueObjCounter < 50 && physicalobjectsNames.Contains(object_pool[chosenindex].name))
        {
            chosenindex = Random.Range(0, object_pool.Count);
        }

        //Json stuff
        if (!usePreloadedScene)
        {
            // Parse Scene + bundle Instructions & objects into JSON.

            JSONParser.Scene_json scene_Json = new JSONParser.Scene_json(path, Lights, Walls[0].GetComponentInChildren<Renderer>().material.color, Table.name,
            physicalobjects, ActivePerson.name, chosen_index, ref_index, rel_instructions[0], rel_instructions[1],
            overheadscreenshot, mainscreenshot, egocentricscreenshot, wrongObjId, EgoBB, topLBottomL, object_pool[chosenindex].name); // It's huge!!! oh well.

            string exported_data = JsonUtility.ToJson(scene_Json);

            WriteString(exported_data, path + "/data.json");
        }

        //get gameobject as parent of parent, and delete all child game objects as well as run the garbage collector manually
        GameObject sceneInHierarchy = gameObject.transform.parent.gameObject.transform.parent.gameObject;
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
        RUNNING = false;
        Destroy(sceneInHierarchy);
    }

    // Configure Recording manager, return the relative scene path.
    string ConfigureRecordingManger(int absoluteindex, string template, string cleandate, GameObject go, string directory)
    {
        string path;
        DirectoryInfo folder;
        if (string.IsNullOrEmpty(directory))
        {
            path = System.IO.Directory.GetCurrentDirectory() + "/Recordings/" + cleandate + "_" + absoluteindex;
        }
        else
        {
            path = directory + "/Recordings/" + cleandate + "_" + absoluteindex;
        }
        folder = Directory.CreateDirectory(path);

        recordingManager.Configure(path, "exo_view", "exo_view", instanceIndex, template, go, mainscreenshot, pngOrJpg, topLBottomL, "rgb");
        recordingManager.Configure(path, "top_view", "top_view", instanceIndex, template, go, overheadscreenshot, pngOrJpg, topLBottomL, "rgb");
        recordingManager.Configure(path, "ego_view", "ego_view", instanceIndex, template, go, egocentricscreenshot, pngOrJpg, topLBottomL, "rgb");
        if (recordDepthCamera)
        {
            recordingManager.Configure(path, "exo_view_depth", "exo_view", instanceIndex, template, go, exoDepthCamera, pngOrJpg, topLBottomL, "depth");
            recordingManager.Configure(path, "top_view_depth", "top_view", instanceIndex, template, go, topDepthCamera, pngOrJpg, topLBottomL, "depth");
            recordingManager.Configure(path, "ego_view_depth", "ego_view", instanceIndex, template, go, egoDepthCamera, pngOrJpg, topLBottomL, "depth");
        }
        if (recordSkeletalCamera)
        {
            recordingManager.Configure(path, "exo_view_skeletal", "exo_view", instanceIndex, template, go, exoSkeletalCamera, pngOrJpg, topLBottomL, "skeletal");
            recordingManager.Configure(path, "top_view_skeletal", "top_view", instanceIndex, template, go, topSkeletalCamera, pngOrJpg, topLBottomL, "skeletal");
            recordingManager.Configure(path, "ego_view_skeletal", "ego_view", instanceIndex, template, go, egoSkeletalCamera, pngOrJpg, topLBottomL, "skeletal");
        }
        return path;
    }

    public void CleanupTrial()
    {

        recordingManager.StopRecording();
    }

    public void SetupScene(GameObject tableprefab, Material floormaterial, int numlights, int instance_index, bool usePreloadedScene)
    {

        // assign instanceIndex
        if (!usePreloadedScene)
        {
            // spawn table
            GameObject table = Instantiate(tableprefab, tableMarker.position, tableprefab.transform.rotation) as GameObject;
            table.name = tableprefab.name;
            table.transform.SetParent(Scene.transform);
            Table = table;


            //spawn random lights

            for (int i = 0; i < numlights; i++)
            {
                Vector3 lightpos = new Vector3(Random.Range(lightSpawnBound1.position.x, lightSpawnBound2.position.x),
                   Random.Range(lightSpawnBound1.position.y, lightSpawnBound2.position.y), Random.Range(lightSpawnBound1.position.z, lightSpawnBound2.position.z)); // chose random location within the light constraints.

                GameObject light = Instantiate(lightPrefab, lightpos, Quaternion.identity) as GameObject;

                RandomizeLight(light);
                light.name = lightPrefab.name;
                light.transform.SetParent(Scene.transform);
                Lights.Add(light);
            }

            // assign random material to floor

            FloorRenderer.material = floormaterial;

            // assign random color to walls
            Color randomcolor = new Color(
                (float)Random.Range(.4f, 1f),
                (float)Random.Range(.4f, 1f),
                (float)Random.Range(.4f, 1f)
            );

            foreach (GameObject wall in Walls)
            {
                SetColor(wall, randomcolor);
            }
        }
        instanceIndex = instance_index;



        //assign cameras correct tags

        TagHelper.AddTag("MainCamera" + instance_index);
        MainCamera.tag = "MainCamera" + instance_index;

        TagHelper.AddTag("overheadcam" + instance_index);
        overheadcam.tag = "overheadcam" + instance_index;

        TagHelper.AddTag("MainCameraDepth" + instance_index);
        exoDepthCameraVid.tag = "MainCameraDepth" + instance_index;

        TagHelper.AddTag("overheadcamdepth" + instance_index);
        topDepthCameraVid.tag = "overheadcamdepth" + instance_index;

        TagHelper.AddTag("MainCameraSkeletal" + instance_index);
        exoSkeletalCameraVid.tag = "MainCameraSkeletal" + instance_index;

        TagHelper.AddTag("overheadcamskeletal" + instance_index);
        topSkeletalCameraVid.tag = "overheadcamskeletal" + instance_index;

        currentModality = 0;
    }

    public void RandomizeLight(GameObject light)
    {


        var l = light.GetComponent<Light>();
        if (l != null)
        {
            l.colorTemperature = (float)Random.Range(3000, 10000); // Light temperature in Kelvin (according to blackbody radiation) 6500 is pure white.
            l.intensity = (float)Random.Range(300, 2000); //Light intensity a value between 300 - 3000 (min inclusive max exclusive) Lumens
        }

    }

    public void SetColor(GameObject obj, Color color)
    {
        var r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = color;
        }

    }

    GameObject SpawnPerson(GameObject personprefab, bool usePreloadedScene)
    {
        GameObject personcontainer;
        GameObject person;
        if (!usePreloadedScene)
        {
            personcontainer = Instantiate(personprefab, new Vector3(personMarker.transform.position.x, personMarker.transform.position.y, personMarker.transform.position.z), personprefab.transform.rotation);
            person = personcontainer.transform.GetChild(0).gameObject;
            ActivePerson = person;
            person.name = personprefab.name;
            // set attributes to correct items.

        }
        else
        {
            personcontainer = ActivePerson;
            person = personcontainer.transform.GetChild(0).gameObject;
            ActivePerson = person;
        }
        BezierSolution.GeneratePointingBezier gen = person.GetComponent<BezierSolution.GeneratePointingBezier>();
        this.objectManager.pointTarget = gen.targetTransform; //should this be here?




        egocentriccam = person.GetComponentInChildren<Camera>().gameObject;
        // create and assign unique tag for egocentric cam.
        TagHelper.AddTag("egocentriccam" + instanceIndex.ToString());
        egocentriccam.tag = "egocentriccam" + instanceIndex.ToString();
        egocentricscreenshot = person.transform.GetComponentsInChildren<Camera>()[1];
        egoDepthCamera = person.transform.GetComponentsInChildren<Camera>()[2];
        egoSkeletalCamera = person.transform.GetComponentsInChildren<Camera>()[3];
        egoDepthCameraVid = person.transform.GetComponentsInChildren<Camera>()[4];
        egoSkeletalCameraVid = person.transform.GetComponentsInChildren<Camera>()[5];

        TagHelper.AddTag("egocentriccamdepth" + instanceIndex.ToString());
        egoDepthCameraVid.tag = "egocentriccamdepth" + instanceIndex.ToString();

        TagHelper.AddTag("egocentriccamskeletal" + instanceIndex.ToString());
        egoSkeletalCameraVid.tag = "egocentriccamskeletal" + instanceIndex.ToString();

        person.transform.SetParent(Scene.transform);

        //create another gameobject, set target transform component as its child
        Transform pointtarget = person.GetComponent<BezierSolution.GeneratePointingBezier>().targetTransform;
        GameObject targetparent = new GameObject("target_parent");
        targetparent.transform.SetParent(person.transform.parent);
        targetparent.transform.SetPositionAndRotation(personMarker.position, personMarker.rotation);

        pointtarget.gameObject.transform.SetParent(targetparent.transform);

        //move other objects used in Stochastic gesture generation as person's siblings

        personcontainer.transform.GetChild(0).SetParent(targetparent.transform);

        DestroyImmediate(personcontainer);

        gestureManager.AssignFields(targetparent.transform.GetChild(1), person);

        //set localgesturemanager for both left and right IKTargets containing BezierTargetDriver.
        BezierTargetDriver[] drivers = targetparent.transform.GetChild(1).GetComponentsInChildren<BezierTargetDriver>();
        drivers[0].SetLocalGestureManager(gestureManager);
        drivers[1].SetLocalGestureManager(gestureManager);
        return person;
    }


    IEnumerator RecordGesture(bool gaze, bool gesture)
    {


        recordingManager.StartRecording();
        gestureManager.BeginGesture(gaze, gesture);
        while (gestureManager.AnimationPlaying)
        {
            yield return new WaitForFixedUpdate();
        }
        recordingManager.StopRecording();
        yield return new WaitForSeconds(trialBuffer);

    }

    static void WriteString(string s, string path)
    {

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(s);
        writer.Close();


    }

    // Update is called once per frame
    void Update()
    {

        if (testobj1 != null)
        {
            Debug.Log(objectManager.GetObjectPosition(testobj1.transform)[0]);
        }
    }

    public Rect GUIRectWithObject(GameObject go, Camera camera)//use camera to determine 2d bounding box of object
    {
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
}

public static class TagHelper
{
    public static void AddTag(string tag)
    {
        UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if ((asset != null) && (asset.Length > 0))
        {
            SerializedObject so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; ++i)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return;     // Tag already present, nothing to do.
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedProperties();
            so.Update();
        }
    }
}
