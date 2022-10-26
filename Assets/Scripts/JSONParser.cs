using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[System.Serializable]
public class JSONParser
{

    public static Color_json ConvertColorToJson(Color color, string name = "N/A")
    {
        float r = color.r;
        float g = color.g;
        float b = color.b;
        float a = color.a;

        Color_json result = new Color_json(r, g, b, a, name);

        return result;

    }
    public static List<Light_json> ConvertLightsToJson(List<GameObject> lights)
    {

        int num_lights = 0;
        List<Light_json> result = new List<Light_json>();

        foreach (GameObject light in lights)
        {
            num_lights++;
            Vector3 pos = light.transform.position;
            Light unitylight = light.GetComponent<Light>();
            float i = unitylight.intensity;
            float r = unitylight.color.r;
            float b = unitylight.color.b;
            float g = unitylight.color.g;
            float a = unitylight.color.a;
            float colortemp = unitylight.colorTemperature;

            Color_json l_color = new Color_json(r, g, b, a, "N/A");

            Light_json l_json = new Light_json(pos, i, l_color, colortemp);
            result.Add(l_json);
        }

        return result;

    }

    [System.Serializable]
    public struct Scene_json
    {
        [SerializeField] Color_json background_color;
        [SerializeField] string table_name;
        [SerializeField] int num_lights;
        [SerializeField] List<Light_json> lights;

        [SerializeField] int num_objects;
        [SerializeField] List<Object_json> objects;
        [SerializeField] string human_name;
        [SerializeField] public InstructionSet_json instructions;

        [SerializeField] ImageData_json only_objects;
        [SerializeField] ImageData_json only_gaze;
        [SerializeField] ImageData_json only_gesture;
        [SerializeField] ImageData_json both_gaze_gesture;
        [SerializeField] ImageData_jsonWrong wrong_gaze_gesture;
        [SerializeField] ImageData_jsonWrong wrong_gaze;
        [SerializeField] ImageData_jsonWrong wrong_gesture;


        public Scene_json(string folderpath, List<GameObject> lightobjects, Color unity_backgroundcolor, string tablename, List<GameObject> physicalobjects, string humanname,
         int chosenobj_index, int refobj_index, string relation, string inverserelation, Camera overhead, Camera front, Camera ego, int wrongIndex, List<List<Rect>> EgoBB, bool topLBottomL, string non_existence_obj)
        {

            this.background_color = ConvertColorToJson(unity_backgroundcolor);
            this.table_name = tablename;
            this.num_lights = lightobjects.Count;
            this.lights = ConvertLightsToJson(lightobjects);
            this.num_objects = physicalobjects.Count;

            List<Object_json> temp_objects = new List<Object_json>();

            for (int i = 0; i < physicalobjects.Count; i++)
            {

                bool isChosen = (i == chosenobj_index);
                bool isRef = (i == refobj_index);

                Object_json obj = new Object_json(physicalobjects[i], isChosen, isRef, overhead, front, ego, topLBottomL, EgoBB[i]);
                temp_objects.Add(obj);
            }
            this.objects = temp_objects;
            this.human_name = humanname;
            this.instructions = new InstructionSet_json(temp_objects[chosenobj_index], temp_objects[refobj_index], relation, inverserelation, non_existence_obj);


            string chosenObjName = physicalobjects[chosenobj_index].name;
            bool ambiguous = false;
            if (physicalobjects.Count != temp_objects.Count)
            {
                Debug.Log("Issue with json generation");
            }
            for (int i = 0; i < physicalobjects.Count; i++)
            {
                if (i != chosenobj_index && chosenObjName.Equals(physicalobjects[i].name))
                {
                    if (!relation.Equals("next to") || Vector3.Distance(physicalobjects[chosenobj_index].transform.position, physicalobjects[refobj_index].transform.position) * 2 <= Vector3.Distance(physicalobjects[i].transform.position, physicalobjects[refobj_index].transform.position))
                    {// for next to has to be twice as close as copy
                        if (temp_objects[i] != null && temp_objects[chosenobj_index] != null && temp_objects[i].absolute_location_observer != null && temp_objects[chosenobj_index].absolute_location_observer != null && temp_objects[chosenobj_index].absolute_location_observer.Equals(temp_objects[i].absolute_location_observer))
                        {
                            ambiguous = true;
                            break;
                        }
                    }
                }
            }
            this.only_objects = new ImageData_json(folderpath, "onlyObjects", ambiguous, topLBottomL);
            this.only_gaze = new ImageData_json(folderpath, "onlyGaze", false, topLBottomL);
            this.only_gesture = new ImageData_json(folderpath, "onlyGesture", false, topLBottomL);
            this.both_gaze_gesture = new ImageData_json(folderpath, "bothGazeGesture", false, topLBottomL);
            this.wrong_gaze_gesture = new ImageData_jsonWrong(folderpath, "wrongGazeGesture", wrongIndex, topLBottomL);
            this.wrong_gaze = new ImageData_jsonWrong(folderpath, "wrongGaze", wrongIndex, topLBottomL);
            this.wrong_gesture = new ImageData_jsonWrong(folderpath, "wrongGesture", wrongIndex, topLBottomL);
            //TODO EGO BB, add it to imageData_json
        }


    }
    [System.Serializable]
    public struct InstructionSet_json
    {
        // onlyObjects, onlyGaze, onlyGesture, BothGazeAndGesture, WrongGazeAndGesture all share this same instruction set.

        [SerializeField] TemplateNULL_json template_null;

        [SerializeField] Template1_1_json template_1_1;
        [SerializeField] Template1_2_json template_1_2;
        [SerializeField] Template2_1_json exo_template_2_1; //spatial absolute
        [SerializeField] Template2_2_json exo_template_2_2; //spatial absolute    
        [SerializeField] Template3_1_json exo_template_3_1; //spatial relative
        [SerializeField] Template3_2_json exo_template_3_2; //spatial relative


        [SerializeField] Template2_1_json ego_template_2_1; //inverse spatial absolute (egoview)
        [SerializeField] Template2_2_json ego_template_2_2; //inverse spatial absolute (egoview)

        [SerializeField] Template3_1_json ego_template_3_1; //inverse spatial relative (egoview)
        [SerializeField] Template3_2_json ego_template_3_2; //inverse spatial relative (egoview)


        // fetches the chosenObject and populates template1, 2, 3s fields with correct info.

        public InstructionSet_json(Object_json cobject, Object_json robject, string relation, string inverserelation, string non_existence_obj)
        {
            template_null = new TemplateNULL_json(Random.Range(0f, 1f));
            template_1_1 = new Template1_1_json(cobject, non_existence_obj);
            template_1_2 = new Template1_2_json(cobject, non_existence_obj);

            exo_template_2_1 = new Template2_1_json(cobject, false, non_existence_obj);
            exo_template_2_2 = new Template2_2_json(cobject, false, non_existence_obj);
            ego_template_2_1 = new Template2_1_json(cobject, true, non_existence_obj);
            ego_template_2_2 = new Template2_2_json(cobject, true, non_existence_obj);

            exo_template_3_1 = new Template3_1_json(cobject, robject, relation, false, non_existence_obj);
            exo_template_3_2 = new Template3_2_json(cobject, robject, relation, false, non_existence_obj);
            ego_template_3_1 = new Template3_1_json(cobject, robject, inverserelation, true, non_existence_obj);
            ego_template_3_2 = new Template3_2_json(cobject, robject, inverserelation, true, non_existence_obj);

        }
    }
    [System.Serializable]
    public struct ImageData_json
    {
        [SerializeField] Camera_json exo_cam;
        [SerializeField] Camera_json top_cam;
        [SerializeField] Camera_json ego_cam;
        [SerializeField] bool is_ambiguous;

        public ImageData_json(string scenedatafolder, string template, bool ambiguous, bool topLBottomL)
        {
            //sceneDataFolder is unique, hardcode frontcam overheadcam and egocam to have their own sub_folders for images+videos.

            this.exo_cam = new Camera_json(scenedatafolder + "/" + template + "/exo_view/canonical_frame/", scenedatafolder + "/" + template + "/exo_view/video/");
            this.ego_cam = new Camera_json(scenedatafolder + "/" + template + "/ego_view/canonical_frame/", scenedatafolder + "/" + template + "/ego_view/video/");
            this.top_cam = new Camera_json(scenedatafolder + "/" + template + "/top_view/canonical_frame/", scenedatafolder + "/" + template + "/top_view/video/");
            this.is_ambiguous = ambiguous;
            /*if (rect.xMin == 1 && rect.xMax == 1 || rect.xMin == Screen.width && rect.xMax == Screen.width || rect.yMin == 1 && rect.yMax == 1 || rect.yMin == Screen.height && rect.yMax == Screen.height)
            {
                this.ego_view_object_start_point = new Vector2(-1, -1);
                this.ego_view_object_end_point = new Vector2(-1, -1);
            }
            else
            {
                if (topLBottomL)
                {
                    this.ego_view_object_start_point = new Vector2(rect.min.x, Screen.height - rect.min.y);
                    this.ego_view_object_end_point = new Vector2(rect.max.x, Screen.height - rect.max.y);
                }
                else
                {
                    this.ego_view_object_start_point = rect.min;
                    this.ego_view_object_end_point = rect.max;
                }

            }*/
        }


    }
    [System.Serializable]
    public struct ImageData_jsonWrong
    {
        [SerializeField] Camera_json exo_cam;
        [SerializeField] Camera_json top_cam;
        [SerializeField] Camera_json ego_cam;
        [SerializeField] int wrong_object_id;
        [SerializeField] bool is_ambiguous;

        public ImageData_jsonWrong(string scenedatafolder, string template, int wrongIndex, bool topLBottomL)
        {
            //sceneDataFolder is unique, hardcode frontcam overheadcam and egocam to have their own sub_folders for images+videos.

            this.exo_cam = new Camera_json(scenedatafolder + "/" + template + "/exo_view/canonical_frame/", scenedatafolder + "/" + template + "/exo_view/video/");
            this.ego_cam = new Camera_json(scenedatafolder + "/" + template + "/ego_view/canonical_frame/", scenedatafolder + "/" + template + "/ego_view/video/");
            this.top_cam = new Camera_json(scenedatafolder + "/" + template + "/top_view/canonical_frame/", scenedatafolder + "/" + template + "/top_view/video/");
            this.wrong_object_id = wrongIndex;
            bool ambiguous = true;
            this.is_ambiguous = ambiguous;
            /*if (rect.xMin == 1 && rect.xMax == 1 || rect.xMin == Screen.width && rect.xMax == Screen.width || rect.yMin == 1 && rect.yMax == 1 || rect.yMin == Screen.height && rect.yMax == Screen.height || rect.xMin == 1 && rect.xMax == Screen.width || rect.xMin == Screen.width && rect.xMax == 1 || rect.yMin == 1 && rect.yMax == Screen.height || rect.yMin == Screen.height && rect.yMax == 1)
            {
                this.ego_view_object_start_point = new Vector2(-1, -1);
                this.ego_view_object_end_point = new Vector2(-1, -1);
            }
            else
            {
                if (topLBottomL)
                {
                    this.ego_view_object_start_point = new Vector2(rect.min.x, Screen.height - rect.min.y);
                    this.ego_view_object_end_point = new Vector2(rect.max.x, Screen.height - rect.max.y);
                }
                else
                {
                    this.ego_view_object_start_point = rect.min;
                    this.ego_view_object_end_point = rect.max;
                }

            }*/
        }


    }

    [System.Serializable]
    public class Object_json
    {
        [SerializeField] public int object_id; //instance id.
        [SerializeField] public string object_name; //GameObject Name    
        [SerializeField] public string object_type; //chosen || ref || other
        [SerializeField] public Color_json object_color; //Color

        [SerializeField] public string absolute_location_observer; // default
        [SerializeField] public string absolute_location_participant; //inverted instructions.

        [SerializeField] public string object_size; //Large || Normal

        [SerializeField] private Vector2 top_view_object_start_point;
        [SerializeField] private Vector2 top_view_object_end_point;

        [SerializeField] private Vector2 exo_view_object_start_point;
        [SerializeField] private Vector2 exo_view_object_end_point;

        [SerializeField] private Vector2 only_objects_ego_view_object_start_point;
        [SerializeField] private Vector2 only_objects_ego_view_object_end_point;

        [SerializeField] private Vector2 only_gaze_ego_view_object_start_point;
        [SerializeField] private Vector2 only_gaze_ego_view_object_end_point;

        [SerializeField] private Vector2 only_gesture_ego_view_object_start_point;
        [SerializeField] private Vector2 only_gesture_ego_view_object_end_point;

        [SerializeField] private Vector2 both_gaze_gesture_ego_view_object_start_point;
        [SerializeField] private Vector2 both_gaze_gesture_ego_view_object_end_point;

        [SerializeField] private Vector2 wrong_gaze_gesture_ego_view_object_start_point;
        [SerializeField] private Vector2 wrong_gaze_gesture_ego_view_object_end_point;


        public Object_json(GameObject obj, bool isChosen, bool isRef, Camera overhead, Camera front, Camera ego, bool topLBottomL, List<Rect> egoBB)
        {
            if (obj.TryGetComponent(out Attributes attributes))
            {
                string colorname = attributes.color_name;
                Color_json c_json;
                if (attributes.non_colorable)
                {
                    Color c = Color.clear;
                    ColorUtility.TryParseHtmlString(colorname, out c);
                    c_json = ConvertColorToJson(c, colorname);
                }
                else
                {
                    Color c = obj.GetComponentInChildren<Renderer>().material.color;
                    c_json = ConvertColorToJson(c, colorname);
                }



                this.object_id = obj.GetInstanceID();
                this.object_name = obj.name;
                if (isChosen)
                {
                    this.object_type = "chosen";
                }
                else if (isRef)
                {
                    this.object_type = "ref";
                }
                else
                {
                    this.object_type = "other";
                }
                this.object_color = c_json;
                this.absolute_location_observer = attributes.absolute_location_observer;
                this.absolute_location_participant = attributes.absolute_location_participant;
                this.object_size = attributes.size;

                List<Vector2> minMax = GUIRectWithObject(obj, overhead, topLBottomL);
                this.top_view_object_start_point = minMax[0];
                this.top_view_object_end_point = minMax[1];

                minMax = GUIRectWithObject(obj, front, topLBottomL);
                this.exo_view_object_start_point = minMax[0];
                this.exo_view_object_end_point = minMax[1];

                //ego bounding boxes ////////////////////////////////
                minMax = GetRegularizedBB(egoBB[0], topLBottomL);
                this.only_objects_ego_view_object_start_point = minMax[0];
                this.only_objects_ego_view_object_end_point = minMax[1];

                minMax = GetRegularizedBB(egoBB[1], topLBottomL);
                this.only_gaze_ego_view_object_start_point = minMax[0];
                this.only_gaze_ego_view_object_end_point = minMax[1];

                minMax = GetRegularizedBB(egoBB[2], topLBottomL);
                this.only_gesture_ego_view_object_start_point = minMax[0];
                this.only_gesture_ego_view_object_end_point = minMax[1];

                minMax = GetRegularizedBB(egoBB[3], topLBottomL);
                this.both_gaze_gesture_ego_view_object_start_point = minMax[0];
                this.both_gaze_gesture_ego_view_object_end_point = minMax[1];

                minMax = GetRegularizedBB(egoBB[4], topLBottomL);
                this.wrong_gaze_gesture_ego_view_object_start_point = minMax[0];
                this.wrong_gaze_gesture_ego_view_object_end_point = minMax[1];
            }
            else
            {
                Debug.LogError("Gameobject could not be parsed into JSON.");
            }

        }
        List<Vector2> GetRegularizedBB(Rect rect, bool topLBottomL)
        {
            bool invalidBoundingBox = (rect.xMin == 1 && rect.xMax == 1 || rect.xMin == Screen.width && rect.xMax == Screen.width || rect.yMin == 1 && rect.yMax == 1 || rect.yMin == Screen.height && rect.yMax == Screen.height || rect.xMin == 1 && rect.xMax == Screen.width || rect.xMin == Screen.width && rect.xMax == 1 || rect.yMin == 1 && rect.yMax == Screen.height || rect.yMin == Screen.height && rect.yMax == 1);
            if (invalidBoundingBox)
            {
                return new List<Vector2> { new Vector2(-1, -1), new Vector2(-1, -1) };
            }
            else
            {
                if (topLBottomL)
                {
                    return new List<Vector2> { new Vector2(rect.min.x, Screen.height - rect.min.y), new Vector2(rect.max.x, Screen.height - rect.max.y) };
                }
                else
                {
                    return new List<Vector2> { rect.min, rect.max };
                }

            }
        }
        public List<Vector2> GUIRectWithObject(GameObject go, Camera camera, bool topLBottomL)
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
                Debug.Log("ISSUE, Should not be here please investigate");
                return new List<Vector2> { new Vector2(-1, -1), new Vector2(-1, -1) };
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
            Rect rect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
            return GetRegularizedBB(rect, topLBottomL);

        }
    }


    [System.Serializable]
    public struct Light_json
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private float intensity;
        [SerializeField] private Color_json color;
        [SerializeField] private float light_color; // in kelvin

        public Light_json(Vector3 pos, float intensity, Color_json color, float lightcolor)
        {
            this.position = pos;
            this.intensity = intensity;
            this.color = color;
            this.light_color = lightcolor;
        }

    }
    [System.Serializable]
    public struct TemplateNULL_json
    {
        [SerializeField] string instruction_string; //this or that.

        public TemplateNULL_json(float prob)
        {
            string istring;
            if (prob > .5f)
                istring = "this";
            else
                istring = "that";
            instruction_string = istring;


        }

    }
    [System.Serializable]
    public struct Template1_1_json
    {
        [SerializeField] private string instruction_string_1; // the [chosenObjectName]
        [SerializeField] private string instruction_string_2; //  the [size/color] [chosenObjectName]
        [SerializeField] private int chosen_object_id;
        [SerializeField] private string chosen_object_name;
        [SerializeField] private string non_existence_string;

        public Template1_1_json(Object_json chosenObject, string non_existence_obj)
        {
            /*
            [Randomly turn off object size or color property, but use at least one]
            [If the scene contains multiple instances of the same object but in a different size then use the object size property]
            */

            string csize = chosenObject.object_size;

            string cname = chosenObject.object_name;

            int id = chosenObject.object_id;

            Color_json ccolor;

            ccolor = chosenObject.object_color;

            // template 1.1: chosenObject name
            // template 1.2: chosenObject size, color, name

            string instruction1 = "";
            string instruction2 = "the " + chosenObject.object_name;
            string instruction3 = "the " + non_existence_obj;

            ccolor = null;
            csize = null;
            instruction1 = "the " + csize + " " + cname;


            instruction_string_1 = instruction1;
            instruction_string_2 = instruction2;
            chosen_object_name = cname;
            chosen_object_id = id;
            non_existence_string = instruction3;
        }
    }
    [System.Serializable]
    public struct Template1_2_json
    {
        [SerializeField] private string instruction_string_1; // the [chosenObjectName]
        [SerializeField] private string instruction_string_2; //  the [size/color] [chosenObjectName]
        [SerializeField] private string chosen_object_size;
        [SerializeField] private int chosen_object_id;
        [SerializeField] private Color_json chosen_object_color;
        [SerializeField] private string chosen_object_name;
        [SerializeField] private string non_existence_string;
        public Template1_2_json(Object_json chosenObject, string non_existence_obj)
        {
            /*
            [Randomly turn off object size or color property, but use at least one]
            [If the scene contains multiple instances of the same object but in a different size then use the object size property]
            */

            string csize = chosenObject.object_size;

            string cname = chosenObject.object_name;

            int id = chosenObject.object_id;

            Color_json ccolor;

            ccolor = chosenObject.object_color;

            // template 1.1: chosenObject name
            // template 1.2: chosenObject size, color, name

            string instruction1 = "";
            string instruction2 = "the " + chosenObject.object_name;
            string instruction3 = "the " + non_existence_obj;


            if (ccolor != null)
                instruction1 = "the " + ccolor.getName() + " " + cname;


            instruction_string_1 = instruction1;
            instruction_string_2 = instruction2;
            chosen_object_size = csize;
            chosen_object_color = ccolor;
            chosen_object_name = cname;
            chosen_object_id = id;
            non_existence_string = instruction3;
        }
    }
    [System.Serializable]
    public struct Template2_1_json
    {
        [SerializeField] private string instruction_string_1; //  [Absolute location]-[Target-Object name]
        [SerializeField] private string instruction_string_2; //   [Absolute location]-[Target Object size / Target Object color, Target Object name] 
        //[SerializeField] private Object_json chosenObject; 
        [SerializeField] private string instruction_perspective;

        [SerializeField] private string chosen_object_name;

        [SerializeField] private int chosen_object_id;
        [SerializeField] private string non_existence_string;
        public Template2_1_json(Object_json chosenObject, bool inverse, string non_existence_obj)
        {
            string ip;
            if (inverse)
            {
                ip = "ego";
            }
            else
            {
                ip = "exo";
            }
            instruction_perspective = ip;
            //[Randomly turn off object size or color properties, but use at least one]
            //[If the scene contains multiple instances of the same object but in a different size then use the object size property]

            // template 2.1: absolute location chosenObject name
            // template 2.2: absolute location chosenObject size, color, name

            string csize = chosenObject.object_size;
            string cname = chosenObject.object_name;
            Color_json ccolor = chosenObject.object_color;

            string cabslocation;
            if (inverse)
            {
                cabslocation = chosenObject.absolute_location_participant; // observer template2, participant for inverse template2
            }
            else
            {
                cabslocation = chosenObject.absolute_location_observer;
            }




            string cinstruction1 = "the " + cname;
            string cinstruction2;


            cinstruction2 = "the " + cabslocation + " " + csize + " " + cname;
            string cinstruction3 = "the " + cabslocation + " " + csize + " " + non_existence_obj;
            ccolor = null;
            csize = null;

            this.instruction_string_1 = cinstruction1;
            this.instruction_string_2 = cinstruction2;
            //this.chosenObject = chosenObject;
            this.chosen_object_name = chosenObject.object_name;
            this.chosen_object_id = chosenObject.object_id;
            this.non_existence_string = cinstruction3;
        }
    }
    [System.Serializable]
    public struct Template2_2_json
    {
        [SerializeField] private string instruction_string_1; //  [Absolute location]-[Target-Object name]
        [SerializeField] private string instruction_string_2; //   [Absolute location]-[Target Object size / Target Object color, Target Object name] 
        //[SerializeField] private Object_json chosenObject;
        [SerializeField] private string instruction_perspective;


        [SerializeField] private string chosen_object_name;

        [SerializeField] private string chosen_object_size;
        [SerializeField] private int chosen_object_id;
        [SerializeField] private Color_json chosen_object_color;
        [SerializeField] private string non_existence_string;
        public Template2_2_json(Object_json chosenObject, bool inverse, string non_existence_obj)
        {
            string ip;
            if (inverse)
            {
                ip = "ego";
            }
            else
            {
                ip = "exo";
            }
            instruction_perspective = ip;
            //[Randomly turn off object size or color properties, but use at least one]
            //[If the scene contains multiple instances of the same object but in a different size then use the object size property]

            // template 2.1: absolute location chosenObject name
            // template 2.2: absolute location chosenObject size, color, name

            string csize = chosenObject.object_size;
            string cname = chosenObject.object_name;
            Color_json ccolor = chosenObject.object_color;

            string cabslocation;
            if (inverse)
            {
                cabslocation = chosenObject.absolute_location_participant; // observer template2, participant for inverse template2
            }
            else
            {
                cabslocation = chosenObject.absolute_location_observer;
            }




            string cinstruction1 = "the " + cname;
            string cinstruction2;

            cinstruction2 = "the " + cabslocation + " " + ccolor.name + " " + cname;
            string cinstruction3 = "the " + cabslocation + " " + ccolor.name + " " + non_existence_obj;

            this.instruction_string_1 = cinstruction1;
            this.instruction_string_2 = cinstruction2;
            //this.chosenObject = chosenObject;
            this.chosen_object_name = chosenObject.object_name;
            this.chosen_object_color = chosenObject.object_color;
            this.chosen_object_size = chosenObject.object_size;
            this.chosen_object_id = chosenObject.object_id;
            this.non_existence_string = cinstruction3;
        }
    }
    [System.Serializable]
    public struct Template3_1_json
    {
        [SerializeField] private string instruction_string_1; //  [Target Object name]-[Spatial relation]-[Relative Object name]
        [SerializeField] private string instruction_string_2; //   [Target Object size, Target Object color, Target Object name] - [Spatial relation] - [Spatial Object size, Spatial Object color, Spatial Object name]
        [SerializeField] private string instruction_perspective;

        [SerializeField] private int chosen_object_id;
        [SerializeField] private string chosen_object_name;


        [SerializeField] private int ref_object_id;
        [SerializeField] private string ref_object_name;
        [SerializeField] private string ref_object_size;
        [SerializeField] private Color_json ref_object_color;

        [SerializeField] private string spatial_relation;
        [SerializeField] private string non_existence_string;
        public Template3_1_json(Object_json chosenObject, Object_json refObject, string relation, bool inverse, string non_existence_obj)
        {

            string ip;
            if (inverse)
            {
                ip = "ego";
            }
            else
            {
                ip = "exo";
            }
            instruction_perspective = ip;
            //Sub-Template 3.1: [Target Object name]-[Spatial relation]-[Relative Object name]
            //Sub-Template 3.2: [Target Object size, Target Object color, Target Object name] - [Spatial relation] - [Spatial Object size, Spatial Object color, Spatial Object name]
            //[For target and relative objects: Randomly turn off target/relative object size or color properties, but use at least one property]
            //[For target and relative objects: If the scene contains multiple instances of the same object but in a different size then use the object size property]


            string instruction1 = "the " + chosenObject.object_name + " " + relation + " the " + refObject.object_name;
            string instruction2 = "";

            Color_json ccolor = chosenObject.object_color;
            string csize = chosenObject.object_size;
            string cname = chosenObject.object_name;

            string rsize = refObject.object_size;
            string rname = refObject.object_name;
            Color_json rcolor = refObject.object_color;

            instruction2 += "the " + csize + " " + cname + " ";
            string instruction3 = "the " + csize + " " + non_existence_obj + " " + relation + " ";

            ccolor = null;
            csize = null;

            instruction2 += relation + " ";

            if (UnityEngine.Random.Range(0f, 1f) > .5f)
            {
                //50-50 chance for size or color recorded; rolled ONCE per scene.
                //templateNumber = "Template 3.1";
                //ccolor = null;
                instruction2 += "the " + rsize + " " + rname;
                instruction3 += "the " + rsize + " " + rname;
            }
            else
            {
                //templateNumber = "Template 3.2";
                //csize = null;
                instruction2 += "the " + rcolor.getName() + " " + rname;
                instruction3 += "the " + rcolor.getName() + " " + rname;

            }
            instruction_string_1 = instruction1;
            instruction_string_2 = instruction2;

            chosen_object_name = chosenObject.object_name;
            chosen_object_id = chosenObject.object_id;

            ref_object_name = refObject.object_name;
            ref_object_size = refObject.object_size;
            ref_object_color = refObject.object_color;
            ref_object_id = refObject.object_id;

            spatial_relation = relation;
            non_existence_string = instruction3;
        }
    }
    [System.Serializable]
    public struct Template3_2_json
    {
        [SerializeField] private string instruction_string_1; //  [Target Object name]-[Spatial relation]-[Relative Object name]
        [SerializeField] private string instruction_string_2; //   [Target Object size, Target Object color, Target Object name] - [Spatial relation] - [Spatial Object size, Spatial Object color, Spatial Object name]
        [SerializeField] private string instruction_perspective;

        [SerializeField] private int chosen_object_id;
        [SerializeField] private string chosen_object_name;
        [SerializeField] private string chosen_object_size;
        [SerializeField] private Color_json chosen_object_color;

        [SerializeField] private int ref_object_id;
        [SerializeField] private string ref_object_name;
        [SerializeField] private string ref_object_size;
        [SerializeField] private Color_json ref_object_color;

        [SerializeField] private string spatialRelation;
        [SerializeField] private string non_existence_string;
        public Template3_2_json(Object_json chosenObject, Object_json refObject, string relation, bool inverse, string non_existence_obj)
        {

            string ip;
            if (inverse)
            {
                ip = "ego";
            }
            else
            {
                ip = "exo";
            }
            instruction_perspective = ip;
            //Sub-Template 3.1: [Target Object name]-[Spatial relation]-[Relative Object name]
            //Sub-Template 3.2: [Target Object size, Target Object color, Target Object name] - [Spatial relation] - [Spatial Object size, Spatial Object color, Spatial Object name]
            //[For target and relative objects: Randomly turn off target/relative object size or color properties, but use at least one property]
            //[For target and relative objects: If the scene contains multiple instances of the same object but in a different size then use the object size property]


            string instruction1 = "the " + chosenObject.object_name + " " + relation + " the " + refObject.object_name;
            string instruction2 = "";

            Color_json ccolor = chosenObject.object_color;
            string csize = chosenObject.object_size;
            string cname = chosenObject.object_name;

            string rsize = refObject.object_size;
            string rname = refObject.object_name;
            Color_json rcolor = refObject.object_color;


            if (ccolor != null)
                instruction2 += "the " + ccolor.getName() + " " + cname + " ";


            instruction2 += relation + " ";
            string instruction3 = "the " + ccolor.getName() + " " + non_existence_obj + " " + relation + " ";

            if (UnityEngine.Random.Range(0f, 1f) > .5f)
            {
                //50-50 chance for size or color recorded; rolled ONCE per scene.
                //templateNumber = "Template 3.1";
                //ccolor = null;
                instruction2 += "the " + rsize + " " + rname;
                instruction3 += "the " + rsize + " " + rname;
            }
            else
            {
                //templateNumber = "Template 3.2";
                //csize = null;
                instruction2 += "the " + rcolor.getName() + " " + rname;
                instruction3 += "the " + rcolor.getName() + " " + rname;

            }
            instruction_string_1 = instruction1;
            instruction_string_2 = instruction2;

            chosen_object_name = chosenObject.object_name;
            chosen_object_size = chosenObject.object_size;
            chosen_object_color = chosenObject.object_color;
            chosen_object_id = chosenObject.object_id;

            ref_object_name = refObject.object_name;
            ref_object_size = refObject.object_size;
            ref_object_color = refObject.object_color;
            ref_object_id = refObject.object_id;

            spatialRelation = relation;
            non_existence_string = instruction3;
        }
    }
    [System.Serializable]
    public struct Camera_json
    {
        [SerializeField] private string image_path;
        [SerializeField] private string video_folder_path;
        public Camera_json(string imgpath, string videopath)
        {
            image_path = imgpath;
            video_folder_path = videopath;
        }
    }
    [System.Serializable]
    public class Color_json
    {
        [SerializeField] private float r;
        [SerializeField] private float g;
        [SerializeField] private float b;
        [SerializeField] private float a;

        [SerializeField] public string name;

        public Color_json(float red, float green, float blue, float alpha, string nameparam)
        {
            r = red;
            g = green;
            b = blue;
            a = alpha;
            name = nameparam;
        }
        public string getName()
        {
            return name;
        }
    }
}