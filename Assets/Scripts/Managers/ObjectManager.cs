using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;





public class ObjectManager : MonoBehaviour
{
    public int ChosenObjectIndex;

    public int RefObjectIndex; //usethis to calculate template 3 (relative location)

    [Tooltip("List of Colors that objects might be tinted")] [SerializeField] Color[] Colors;
    [Tooltip("Names of colors (for generated descriptions)")] [SerializeField] string[] ColorNames;
    public List<GameObject> physicalObjects;
    [SerializeField] Transform Spawnbound1;
    [SerializeField] Transform Spawnbound2;
    Vector3 spawnpoint;
    [SerializeField] public Transform pointTarget;

    //[SerializeField] public GameObject SelectedObject; // will be "chosen" object as long as WrongGazeAndGesture isn't being generated.
    bool SelectedObjectDoneSpawning;

    [SerializeField] Attributes SelectedObjectAttributes;
    List<GameObject> objectsToDelete;

    [SerializeField] float rangeScale; // this is arbitrary... 

    [SerializeField] Material GrayscaleMaterial;
    int goodObjects = 0;
    public bool isFinished;


    private void Awake()
    {
        spawnpoint = Spawnbound2.position - Spawnbound1.position;
    }

    void OnDrawGizmos()
    {
        // if (spawnpoint != null){
        //     Gizmos.DrawLine(.gameObject.transform.position, spawnpoint);
        //     Gizmos.DrawLine(GameObject.FindGameObjectWithTag("egocentriccam").gameObject.transform.position, spawnpoint);
        // }

        float width = Spawnbound2.transform.position.x - Spawnbound1.transform.position.x;
        float leftdivider = Spawnbound1.transform.position.x + (width / 3);
        float rightdivider = Spawnbound1.transform.position.x + 2 * ((width) / 3);

        float height = Spawnbound2.transform.position.z - Spawnbound1.transform.position.z;
        float frontdivider = Spawnbound1.transform.position.z + (height / 3);
        float backdivider = Spawnbound1.transform.position.z + 2 * ((height) / 3);

        Gizmos.DrawLine(new Vector3(leftdivider, Spawnbound1.position.y, frontdivider), new Vector3(rightdivider, Spawnbound1.position.y, backdivider));


    }

    public void setTarget(Transform obj)
    {// this controls where target for pointing is set, fix this as it is not working properlu

        var rend = obj.GetComponentInChildren<Renderer>();

        pointTarget.SetPositionAndRotation(rend.bounds.center, Quaternion.identity);
    }
    private void Update()
    {
        //if (pointTarget != null)
        // pointTarget.position = loc;
    }
    private void OnEnable()
    {
        rangeScale = 2f;
        objectsToDelete = new List<GameObject>();
    }
    private void OnDisable()
    {
    }


    public List<string> GetObjectPosition(Transform objectTransform)
    {

        List<string> descriptions = new List<string>();

        string observerdescription = "";
        string participantdescription = "";

        float width = Mathf.Abs(Spawnbound2.transform.position.x - Spawnbound1.transform.position.x);
        float leftdivider = Spawnbound1.transform.position.x + (width / 3);
        float rightdivider = Spawnbound1.transform.position.x + 2 * ((width) / 3);

        //Debug.Log("rightdiv: "+ rightdivider + " left div: " + leftdivider);
        float height = Mathf.Abs(Spawnbound2.transform.position.z - Spawnbound1.transform.position.z);
        float frontdivider = Spawnbound1.transform.position.z + (height / 3);
        float backdivider = Spawnbound1.transform.position.z + 2 * ((height) / 3);

        //Debug.Log("frontdiv: " + frontdivider + " back div: " + backdivider);

        int cornerchecker = 0; // equals two when object in a corner

        int centerchecker = 0;

        if (objectTransform.position.z <= frontdivider)
        {
            participantdescription += "back ";
            cornerchecker++;
            observerdescription += "front ";

        }
        else if (objectTransform.position.z <= backdivider)
        {
            centerchecker++;

        }
        else
        {
            cornerchecker++;
            participantdescription += "front ";
            observerdescription += "back ";
        }

        if (objectTransform.position.x <= leftdivider)
        {
            cornerchecker++;
            participantdescription += "right";
            observerdescription += "left";

        }
        else if (objectTransform.position.x <= rightdivider)
        {
            centerchecker++;
        }
        else
        {
            cornerchecker++;
            participantdescription += "left";
            observerdescription += "right";
        }

        if (cornerchecker > 1)
        {
            participantdescription += " corner";
            observerdescription += " corner";
        }
        if (centerchecker > 1)
        {
            participantdescription = "center";
            observerdescription = "center";
        }

        descriptions.Add(observerdescription);
        descriptions.Add(participantdescription);
        return descriptions;

    }

    // gets relative object position, returns a two element list with the observer perspective, then participant perspective.
    public List<string> GetObjectPosition(Transform objectTransform, Transform refObjectTransform)
    {

        List<string> descriptions = new List<string>();

        string observerdescription = "";
        string participantdescription = "";

        // Spatial relations: above, under, in, on, behind, in front of, next to, to the left of, to the right of.

        float maxRange = rangeScale * Mathf.Max(objectTransform.gameObject.GetComponentInChildren<Collider>().bounds.size.x, objectTransform.gameObject.GetComponentInChildren<Collider>().bounds.size.z);

        // maxRange is arbitrary (sort of). the rangeScale parameter might need to be tuned using Amazon Turk Workers.
        RaycastHit hit;

        if (Vector3.Distance(objectTransform.position, refObjectTransform.position) < maxRange)
        {
            if (Physics.Raycast(objectTransform.position, (refObjectTransform.position - objectTransform.position), out hit, maxRange))
            {
                if (hit.transform == refObjectTransform)
                {
                    // if objects have line of sight with each other and are within a threshold distance, then return 'next to' for both observer and participant.
                    observerdescription = "next to";
                    participantdescription = "next to";
                    descriptions.Add(observerdescription);
                    descriptions.Add(participantdescription);

                    return descriptions;
                }
            }
        }

        // else: set up four quadrants, rotated 45 degrees along cardinal directions. each relative position calculated off angle comparisions. 
        Vector3 basevector = new Vector3(-1, 0, 1);
        Vector3 objectsvector = refObjectTransform.position - objectTransform.position; // relative object angle, origin is ref object
        objectsvector = Vector3.Scale(new Vector3(1, 0, 1), objectsvector); //squash y axis on the rel object angle

        float objectangle = Vector3.SignedAngle(basevector, objectsvector, Vector3.up);

        if (objectangle > 90)
        {
            observerdescription += "right of";
            participantdescription += "left of";

        }
        else if (objectangle > 0)
        {
            observerdescription += "behind";
            participantdescription += "in front of";

        }
        else if (objectangle < -90)
        {
            observerdescription += "in front of";
            participantdescription += "behind";
        }
        else
        {
            observerdescription += "left of";
            participantdescription += "right of";
        }

        descriptions.Add(participantdescription);
        descriptions.Add(observerdescription);
        return descriptions;

    }
    // Color Object by preprossessing through a grayscale shader and then tinting the base color from a selection.
    private void colorObject(List<Renderer> renderers, Color color)
    {

        //Step 1: convert the current texture to grayscale
        foreach (Renderer renderer in renderers)
        {

            Texture textureRGB = renderer.material.mainTexture;
            if (textureRGB != null)
            {

                RenderTexture rt = RenderTexture.GetTemporary(textureRGB.width, textureRGB.height);

                Graphics.Blit(textureRGB, rt, GrayscaleMaterial);

                Texture2D textureGrayscale = new Texture2D(textureRGB.width, textureRGB.height);

                textureGrayscale.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);

                textureGrayscale.Apply();

                renderer.material.mainTexture = textureGrayscale;
                RenderTexture.ReleaseTemporary(rt);
            }


            renderer.material.color = color;
            //Step 2: Discolor the base color texture on the material to the color of choice.
        }

    }

    // Randomizes generation of object (ensures is visible), adds to PhysicalObjects.
    public GameObject SpawnObject(GameObject obj, Camera ego, Camera main, float bufferPeriod, int objIndex)
    {
        if (obj == null)
        {
            Debug.Log("ISSUE: here and shouldnt be: ");
            return null;
            //return null;
        }
        if (obj.name == null)
        {
            obj.name = "default";
        }

        bool obj_visible = false;

        // check if objects are visible from both maincam and egocentric cam.
        int fails = 0;
        while (!obj_visible && fails < 25)
        {
            spawnpoint = new Vector3(Random.Range(Spawnbound1.position.x, Spawnbound2.position.x),
            Random.Range(Spawnbound1.position.y, Spawnbound2.position.y), Random.Range(Spawnbound1.position.z, Spawnbound2.position.z));

            Ray ray = new Ray(main.transform.position, spawnpoint - main.transform.position);
            RaycastHit hit;
            //Debug.Log(Physics.Raycast(ray,out hit));
            if (Physics.Raycast(ray, out hit))
            {

                Transform objectHit = hit.transform;

                //Debug.Log(objectHit.tag);
                if (objectHit.CompareTag("table"))
                {


                    Ray egoray = new Ray(ego.transform.position, spawnpoint - ego.transform.position);
                    RaycastHit egohit;


                    if (Physics.Raycast(egoray, out egohit))
                    {
                        Transform egoobjectHit = egohit.transform;

                        if (egoobjectHit.CompareTag("table"))
                        {//TODO HERE CAN ADD CODE TO MAKE SURE NOT WITHIN CERTAIN DELTA FROM OTHER OBJECTS
                            obj_visible = true;
                        }
                        else
                        {
                            //Debug.Log("not visible from egocam, trying again");
                        }
                        //Debug: object not visible.

                    }
                }
                else
                {
                    //Debug.Log("GO HIT " + objectHit.gameObject + " pos: " + objectHit.transform.position + " at: " + hit.point);
                    //Debug.Log("not visible from maincam, trying again");

                }
            }
            fails++;
        }
        if (fails >= 25)
        {
            Debug.Log("Failed to Succesfully spawn " + obj.name + " Please Investigate");
        }
        //going to completely revamp this system for spawning objects as follows:
        //first do all generational stuff beforehand regarding small/large and all that crap
        //determine longest diagonal of an object by using its renderer or box collider or any other means
        //subtract half this diagonal from all dimensions so no more than half the item can be outside of the bounds now (well actually do a little over to be generous)
        //generate object and wait a bit and then make sure all other objects are 
        //and then check if all corners of objects are visible :(

        string obj_name = obj.name;
        Quaternion randomangle = Quaternion.Euler(0f, Random.Range(0, 180f), 0f);
        GameObject item = GameObject.Instantiate(obj, spawnpoint, randomangle) as GameObject;
        item.name = obj_name; // ensures additional characters like "(1)" aren't added even if multiple copies of objects.
        item.transform.SetParent(this.transform);
        bool firstTime = false;
        if (objIndex == -1)
        {
            physicalObjects.Add(item);
            objIndex = physicalObjects.Count - 1;
            firstTime = true;
        }
        else
            physicalObjects[objIndex] = item;

        item.tag = "Object";

        item.AddComponent<Attributes>();
        Attributes theattributes = item.GetComponent<Attributes>();
        if (item.TryGetComponent(out NonColorable nonColorable))
        {
            //Debug.Log("Here manual coloring worked: " + nonColorable.color_name);
            theattributes.non_colorable = true;
            theattributes.color_name = nonColorable.color_name;
        }
        else
        {
            var itemRenderers = item.GetComponentsInChildren<Renderer>();
            List<Renderer> itemrends = new List<Renderer>();
            foreach (var rend in itemRenderers)
            {
                itemrends.Add(rend);
            }

            int colorid = Random.Range(0, Colors.Count());
            Color pickedColor = Colors[colorid];
            colorObject(itemrends, pickedColor);
            theattributes.color_name = ColorNames[colorid];
            theattributes.non_colorable = false;

        }


        if (Random.Range(0, 2) == 1)
        { // 50-50 chance of 'large' item
            item.transform.localScale *= 1.3f;
            theattributes.size = "large";
        }
        else theattributes.size = "small";


        if (obj == null)
        {
            Debug.Log("here and shouldnt be second time: ");
            //return null;
        }
        if (firstTime)
            StartCoroutine(FreezeAndRecordPosition(item, theattributes, ego, main, bufferPeriod, obj, objIndex));
        return item;
    }
    // guarantee chosen object is spawned, roll for multiple instances. returns physicalObjects list.

    public void GenerateObjects(List<GameObject> object_pool, GameObject chosen_object, int num_objects, int instanceindex, Camera egocentriccam, Camera maincam, float bufferPeriod)
    {

        // Multiple copies? 50-50 chance

        bool multiple_copies = true;
        int chosencount = 1;

        if (Random.Range(0, 2) > 0)
        { // 0 means no copies, 1 means yes
            multiple_copies = true;
        }
        else
        {
            multiple_copies = false;
        }

        GameObject originalchosen = SpawnObject(chosen_object, egocentriccam, maincam, bufferPeriod, -1);

        if (multiple_copies)
        {
            GameObject chosen1 = SpawnObject(chosen_object, egocentriccam, maincam, bufferPeriod, -1);


            /*while (Attributes.Equals(originalchosen.GetComponent<Attributes>(), chosen1.GetComponent<Attributes>())){  // Ensure that there is a discriminator between chosen object copies
                physicalObjects.Remove(chosen1);
                chosen1.SetActive(false);
                objectsToDelete.Add(chosen1);
                //Destroy(chosen1);
                chosen1 = SpawnObject(chosen_object, egocentriccam, maincam, bufferPeriod, -1);
            }*/
            chosencount++;
            if (Random.Range(0, 2) > 0)
            { //Another copy?? 50-50 chance if already multiple copies. (.25 chance 1 copy, .25 2 copies)
                GameObject chosen2 = SpawnObject(chosen_object, egocentriccam, maincam, bufferPeriod, -1);
                //commented out below as it caused bugs
                //also i feel like we shouldnt stop similar objects from spawning together it can and will exist
                /*while (Attributes.Equals(originalchosen.GetComponent<Attributes>(), chosen2.GetComponent<Attributes>())){  // Ensure that there is a discriminator between chosen object copies
                    physicalObjects.Remove(chosen2);
                    chosen2.SetActive(false);
                    objectsToDelete.Add(chosen2);
                    //Destroy(chosen2);
                    chosen2 = SpawnObject(chosen_object, egocentriccam, maincam, bufferPeriod, -1);
                }*/
                chosencount++;

            }

        }
        ChosenObjectIndex = 0;

        List<GameObject> elegible_refobjs = new List<GameObject>(); //keep track of non-chosen objects.

        List<GameObject> copypool = new List<GameObject>();

        foreach (GameObject o in object_pool)
        {
            copypool.Add(o);
        }
        //
        while (physicalObjects.Count < num_objects)
        {
            if (Random.Range(0, 2) > 0)
            { // 0 means no copies, 1 means yes
                multiple_copies = true;
            }
            else
            {
                multiple_copies = false;
            }

            GameObject obj_original = SpawnObject(copypool[Random.Range(0, copypool.Count)], egocentriccam, maincam, bufferPeriod, -1);
            elegible_refobjs.Add(obj_original);

            if (multiple_copies && physicalObjects.Count < num_objects)
            {
                GameObject obj1 = SpawnObject(obj_original, egocentriccam, maincam, bufferPeriod, -1);

                /*while (Attributes.Equals(obj_original.GetComponent<Attributes>(), obj1.GetComponent<Attributes>())){  // Ensure that there is a discriminator between chosen object copies
                    physicalObjects.Remove(obj1);
                    obj1.SetActive(false);
                    objectsToDelete.Add(obj1);
                    //Destroy(chosen1);
                    obj1 = SpawnObject(obj_original, egocentriccam, maincam, bufferPeriod, -1);
                }*/

                elegible_refobjs.Add(obj1);


                if (Random.Range(0, 2) > 0 && physicalObjects.Count < num_objects)
                { //Another copy?? 50-50 chance if already multiple copies. (.25 chance 1 copy, .25 2 copies)
                    GameObject obj2 = SpawnObject(obj_original, egocentriccam, maincam, bufferPeriod, -1);

                    /*while (Attributes.Equals(obj_original.GetComponent<Attributes>(), obj2.GetComponent<Attributes>())){  // Ensure that there is a discriminator between chosen object copies
                        physicalObjects.Remove(obj2);
                        obj2.SetActive(false);
                        objectsToDelete.Add(obj2);
                        //Destroy(chosen1);
                        obj2 = SpawnObject(obj_original, egocentriccam, maincam, bufferPeriod, -1);
                    }*/

                    elegible_refobjs.Add(obj2);
                }
            }
            copypool.Remove(obj_original); // Same alt object can't appear again

        }


        RefObjectIndex = Random.Range(0, elegible_refobjs.Count) + chosencount;

    }
    bool checkForObjectVisible(GameObject go, Camera ego, Camera main)
    {

        if (go == null)
        {
            return false;
        }
        if (go.GetInstanceID() != physicalObjects[0].GetInstanceID() && go.name == physicalObjects[0].name && (!SelectedObjectDoneSpawning || Vector3.Distance(physicalObjects[0].transform.position, go.transform.position) < 0.5f))
        {
            //Debug.Log(Vector3.Distance(physicalObjects[0].transform.position, go.transform.position) + " chosen: " + physicalObjects[0].name + " comparison: " + go.name);
            return false;//duplicate is not too close check
        }
        Renderer rend = new Renderer();
        if (go.GetComponent<Renderer>() != null)
            rend = go.GetComponent<Renderer>();
        else
        {
            if (go.GetComponentInChildren<Renderer>() != null)
            {
                rend = go.GetComponentInChildren<Renderer>();
            }
            else
            {
                foreach (Transform child in go.transform)
                {
                    foreach (Transform child2 in child)
                    {
                        if (child2.GetComponent<Renderer>() != null)
                        {
                            rend = child2.GetComponent<Renderer>();
                        }
                    }
                }
            }
        }

        Ray ray = new Ray(main.transform.position, rend.bounds.center - main.transform.position);
        //Ray ray = main.ScreenPointToRay(go.transform.position);
        RaycastHit hit;
        //Physics.Raycast(ray, out hit);
        //Debug.Log(Physics.Raycast(ray,out hit));
        //Debug.Log("should draw to: " + go.transform.position);
        //Debug.DrawRay(ray.origin, ray.direction * 50, Color.yellow, 3);
        if (Physics.Raycast(ray, out hit))
        {

            Transform objectHit = hit.transform;
            if (go.name == null)
            {
                Debug.Log("issue with name");
            }
            if (objectHit.gameObject != null && objectHit.gameObject.name != null && objectHit.gameObject.name.Equals(go.name) || objectHit.transform.parent != null && objectHit.transform.parent.gameObject != null && objectHit.transform.parent.gameObject.name != null && objectHit.transform.parent.gameObject.name.Equals(go.name) || objectHit.transform.parent.transform.parent != null && objectHit.transform.parent.transform.parent.gameObject != null && objectHit.transform.parent.transform.parent.gameObject.name != null && objectHit.transform.parent.transform.parent.gameObject.name.Equals(go.name))
            {
                Ray ray2 = new Ray(ego.transform.position, rend.bounds.center - ego.transform.position);


                //Debug.DrawRay(ray2.origin, ray2.direction * 50, Color.yellow, 3);
                //Ray ray2 = ego.ScreenPointToRay(go.transform.position);
                RaycastHit hit2;

                //Physics.Raycast(ray2, out hit2);
                //Debug.Log(Physics.Raycast(ray,out hit));
                if (Physics.Raycast(ray2, out hit2))
                {

                    Transform objectHit2 = hit2.transform;

                    //Debug.Log(objectHit.tag);
                    if (objectHit2.gameObject != null && objectHit2.gameObject.name != null && objectHit2.gameObject.name.Equals(go.name) || objectHit2.transform.parent != null && objectHit2.transform.parent.gameObject != null && objectHit2.transform.parent.gameObject.name != null && objectHit2.transform.parent.gameObject.name.Equals(go.name) || objectHit2.transform.parent.transform.parent != null && objectHit2.transform.parent.transform.parent.gameObject != null && objectHit2.transform.parent.transform.parent.gameObject.name != null && objectHit2.transform.parent.transform.parent.gameObject.name.Equals(go.name))
                    {
                        if (hit2.point.y + .2 > spawnpoint.y)//.2 is threshold
                            return true;
                    }
                    else
                    {
                        //Debug.Log("Could not see " + go.name + " in ego view");
                        //Debug.Log("hit " + objectHit2.gameObject.name + " instead of " + go.name + " or obj is on the ground");
                    }
                }
            }
            else
            {
                //Debug.Log("Could not see " + go.name + " in main view");
                //Debug.Log("hit " + objectHit.gameObject.name + " instead of " + go.name);
            }
        }
        return false;
    }


    public IEnumerator FreezeAndRecordPosition(GameObject item, Attributes theattributes, Camera ego, Camera main, float bufferPeriod, GameObject obj, int objIndex)
    {
        //string name = item.name;
        yield return new WaitForSeconds(1);
        GameObject curr = item;
        List<GameObject> toDestroy = new List<GameObject>();
        int refreshObjectsCount = 0;
        if (item == null)
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("ISSUE PLEASE INVESTIGATE");//still got here investigate
            curr = SpawnObject(obj, ego, main, bufferPeriod, objIndex);
        }
        else if (curr.name == null)
        {
            Debug.Log("name issue");
            curr.name = obj.name;
        }
        while (!checkForObjectVisible(curr, ego, main) && refreshObjectsCount < 5)
        {
            if (curr != null)
                curr.SetActive(false);
            toDestroy.Add(curr);
            curr = SpawnObject(obj, ego, main, bufferPeriod, objIndex);
            if (curr == null)
            {
                yield return new WaitForSeconds(0.1f);
                Debug.Log("ISSUE PLEASE INVESTIGATE");//still got here investigate
                curr = SpawnObject(obj, ego, main, bufferPeriod, objIndex);
            }
            else if (curr.name == null)
            {
                Debug.Log("name issue");
                curr.name = obj.name;
            }
            yield return new WaitForSeconds(bufferPeriod / 2);
            refreshObjectsCount++;
        }
        if (refreshObjectsCount >= 8)
        {
            Debug.Log("Failed to properly generate the object: " + item.name);
        }
        if (physicalObjects[0].GetInstanceID() == curr.GetInstanceID())
        {
            SelectedObjectDoneSpawning = true;
        }
        //here check if object can be seen from all views
        List<string> atts = new List<string>();
        if (item != null)
        {

            atts = GetObjectPosition(item.transform);

            theattributes.absolute_location_observer = atts[0];
            theattributes.absolute_location_participant = atts[1];
            //item.GetComponentInChildren<Rigidbody>().isKinematic = true;

        }
        goodObjects++;
        if (goodObjects == physicalObjects.Count)
            isFinished = true;

        yield return new WaitUntil(IsFinished);
        foreach (GameObject go in toDestroy)
        {
            //DestroyImmediate(go);
            objectsToDelete.Add(go);
        }
    }
    bool IsFinished()
    {
        return isFinished;
    }
}
