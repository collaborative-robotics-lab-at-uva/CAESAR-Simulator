using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

/*
SuperManger is high-level manager of multiple scenes run concurrently. It defines the overall size of the data generation, 
as well as ensures that each object is selected and spawned similar number of times.

To Generate a set of data:

1. Set the # of Scenes. 
2. Set the object pool.
3. Set the Human pool.

Conditions:

Humans must be chosen equal number of times (pure random).

Objects must be chosen equal number of times (chose from the pool, then elimante from the pool until all objects have been chosen).

*/

public class SuperManager : MonoBehaviour
{

    [SerializeField] GameObject ScenePrefab;
    [SerializeField] List<SceneManager> sceneManagers;
    [SerializeField] List<GameObject> ChosenObjectPool; // Set of objects elegible for becoming the Chosen object -- every time object is chosen from this list, delete until all objects have been chosen


    [Header("Resources")]

    [Tooltip("Specifies which objects will be used for data generation")]
    [SerializeField] List<GameObject> ObjectPool; // Set of objects we can spawn in general. (use these to populate table)

    [Tooltip("Specifies which humans will be used for data generation")]
    [SerializeField] List<GameObject> Humans; // Set of Humans that may be spawned (chosen at pure random).

    [Tooltip("Specifies which table prefabs will be used for data generation")]
    [SerializeField] List<GameObject> Tables; // Set of tables that may be spawned (chosen at pure random).

    [Tooltip("Specifies which floor materials will be used for data generation")]
    [SerializeField] List<Material> FloorMaterials; // Set of Materials to be set onto the floor;

    [Header("Dataset Generation Parameters")]

    [Tooltip("Specifies how many scenes can be run concurrently")]
    [SerializeField] int parallelScenes; // How many instances of the scene can be run concurrently;

    [Tooltip("Specifies total number of scenes generated")]
    [SerializeField] int totalScenesGenerated; // Total number of datapoints generated.

    [Tooltip("Specifies whether or not objects will be loaded in dynamically or not")]
    [SerializeField] bool dynamicallyLoadObjects; // Total number of datapoints generated.

    [Tooltip("Specifies whether or not video will be recorded")]
    [SerializeField] bool record;

    [Tooltip("Specifies whether or not video will be recorded")]
    bool activateDepthCamera = false;

    [Tooltip("Specifies whether or not the depth camera will also be recorded")]
    [SerializeField] bool activateSkeletalCamera;

    [Tooltip("Specifies whether or not the depth camera will also be recorded")]
    [SerializeField] bool drawBoundingBoxes;

    [Tooltip("Specifies whether or not images/video will be stored in png's vs jpg's; png = true jpg = false")]
    [SerializeField] bool pngOrJpg; // true is png false is jpg

    [Tooltip("Specifies whether or not you are not using the default directory - your unity project recordings folder")]
    [SerializeField] bool useDifferentDirectory;

    [Tooltip("Specifies what directory to record in if using different directory, start with / don't end with /")]
    [SerializeField] string directory;

    [Tooltip("Specifies width of screen you use and height of captured images")]
    [SerializeField] int width;

    [Tooltip("Specifies height of screen you use and height of captured images")]
    [SerializeField] int height;

    [Tooltip("Specifies whether origin is on the top left of images (true) or bottom left of images (false)")]
    [SerializeField] bool originTopLOrBottomL;

    [Tooltip("Specifies whether to dynamically load a scene (false) or used the pre-loaded scene passed in (true), NOTE this is meant for one scene not multiple")]
    [SerializeField] bool usePreloadedScene;




    void InstantiateScenes(int instance_count)
    {

        int basex = -24;
        int x;
        int z;
        for (int i = 0; i < instance_count; i++)
        {
            z = 0; // 3 columns of scenes
            x = basex + (i) * 12;
            Vector3 scenespawnpos = new Vector3(x, ScenePrefab.transform.position.y, z);
            GameObject scene = Instantiate(ScenePrefab, scenespawnpos, ScenePrefab.transform.rotation) as GameObject;
            scene.name = i.ToString();
            SceneManager currmanager = scene.GetComponentInChildren<SceneManager>();
            GameObject table = Tables[Random.Range(0, Tables.Count)];
            Material floormaterial = FloorMaterials[Random.Range(0, FloorMaterials.Count)];
            currmanager.SetupScene(table, floormaterial, Random.Range(1, 4), i, usePreloadedScene); // 1-3 lights;
            scene.GetComponentInChildren<RecordingManager>().Recording = record;
            sceneManagers.Add(currmanager);
        }
    }
    void instantiateScene(int i)
    {
        int basex = -24;
        int x;
        int z;
        z = 0; // 3 columns of scenes
        x = basex + (i) * 12;
        Vector3 scenespawnpos = new Vector3(x, ScenePrefab.transform.position.y, z);
        GameObject scene = Instantiate(ScenePrefab, scenespawnpos, ScenePrefab.transform.rotation) as GameObject;
        scene.name = i.ToString();
        SceneManager currmanager = scene.GetComponentInChildren<SceneManager>();
        GameObject table = Tables[Random.Range(0, Tables.Count)];
        Material floormaterial = FloorMaterials[Random.Range(0, FloorMaterials.Count)];
        currmanager.SetupScene(table, floormaterial, Random.Range(1, 4), i, usePreloadedScene); // 1-3 lights;
        scene.GetComponentInChildren<RecordingManager>().Recording = record;
        sceneManagers[i] = currmanager;
    }

    IEnumerator BeginDataGeneration()
    {
        var objects = Resources.LoadAll("objects", typeof(GameObject));
        foreach (var o in objects)
        {
            if (!dynamicallyLoadObjects)
                break;
            if (o != null)
            {
                GameObject obj = (GameObject)o;
                ObjectPool.Add(obj);
                ChosenObjectPool.Add(obj);
            }
        }


        InstantiateScenes(parallelScenes);

        int absoluteIndex = 0; // how many datapoints total we have generated so far.

        for (int i = 0; i < parallelScenes; i++)
        {
            if (ChosenObjectPool.Count < 1)
            {
                foreach (GameObject o in ObjectPool)
                {
                    ChosenObjectPool.Add(o);
                }
            }
            int chosenindex = Random.Range(0, ChosenObjectPool.Count);
            GameObject chosen = ChosenObjectPool[chosenindex];
            while (chosen == null)
            {
                ChosenObjectPool.Remove(chosen);
                ObjectPool.Remove(chosen);
                chosenindex = Random.Range(0, ChosenObjectPool.Count);
                chosen = ChosenObjectPool[chosenindex];
            }
            ChosenObjectPool.Remove(chosen);
            ObjectPool.Remove(chosen);
            float bufferperiod = 3f;
            string cleandate = System.DateTime.Now.ToString().Replace(" ", ".").Replace("/", ".").Replace(":", ".");
            sceneManagers[i].RUNNING = true;
            StartCoroutine(sceneManagers[i].StartTrial(Humans[Random.Range(0, Humans.Count)], ObjectPool, chosen, Random.Range(4, 11), absoluteIndex, bufferperiod, cleandate, drawBoundingBoxes, pngOrJpg, directory, width, height, originTopLOrBottomL, activateDepthCamera, activateSkeletalCamera, usePreloadedScene));
            yield return new WaitForSeconds(bufferperiod + 1f);
            ObjectPool.Insert(chosenindex, chosen);
            absoluteIndex++;
        }

        int nonbusyscenes = 0; // used to keep track of how many scenes are still chugging along.

        // as long as we don't have the desired number of datapoints, keep checking whether each instance is RUNNING.
        while (nonbusyscenes < parallelScenes)
        {
            yield return new WaitForSeconds(2f); // wait 2 secs before trying to loop thru again.

            for (int i = 0; i < parallelScenes; i++)
            { 

                if (sceneManagers[i].RUNNING == false)
                {

                    // then scene must not be busy. add to the nonbusy scenes (duh)

                    nonbusyscenes++;

                    //now we check if we have met the datapoints. if not, then run it! lose the nonbusyscene :(
                    if (absoluteIndex < totalScenesGenerated)
                    {
                        instantiateScene(i);
                        nonbusyscenes--;
                        if (ChosenObjectPool.Count < 1)
                        {
                            foreach (GameObject o in ObjectPool)
                            {
                                ChosenObjectPool.Add(o);
                            }
                        }
                        int chosenindex = Random.Range(0, ChosenObjectPool.Count);
                        GameObject chosen = ChosenObjectPool[chosenindex];
                        while (chosen == null)
                        {
                            ChosenObjectPool.Remove(chosen);
                            ObjectPool.Remove(chosen);
                            chosenindex = Random.Range(0, ChosenObjectPool.Count);
                            chosen = ChosenObjectPool[chosenindex];
                        }
                        ChosenObjectPool.Remove(chosen);
                        ObjectPool.Remove(chosen);

                        float bufferperiod = 5f;

                        string cleandate = System.DateTime.Now.ToString().Replace(" ", ".").Replace("/", ".").Replace(":", ".");
                        sceneManagers[i].RUNNING = true;
                        Debug.Log("Scene " + absoluteIndex + " starting");
                        StartCoroutine(sceneManagers[i].StartTrial(Humans[Random.Range(0, Humans.Count)], ObjectPool, chosen, Random.Range(4, 11), absoluteIndex, bufferperiod, cleandate, drawBoundingBoxes, pngOrJpg, directory, width, height, originTopLOrBottomL, activateDepthCamera, activateSkeletalCamera, usePreloadedScene));
                        yield return new WaitForSeconds(bufferperiod + 1f);
                        ObjectPool.Insert(chosenindex, chosen);
                        absoluteIndex++;
                    }
                    else
                    {
                        continue;
                        //we've met the datapoints and this scene is free. now we're waiting for all the other scenes to be free. if they are also free, then EXIT.
                    }

                }
                else
                {
                    //that scene must be busy! lets check if its busy later.
                }
            }

        }

        Debug.Log("Simulation Finished! Total runtime is: " + Time.realtimeSinceStartup + " seconds");

#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif
    }

    void Start()
    {

        //Load objects in Resources into the ObjectPool / ChosenObjectPool
        Screen.SetResolution(width, height, false);
        if (Screen.width != width && Screen.height != height)
        {
            Debug.Log("Change Display Resolution to inputs width and height inputs listed on super manager in order to properly run simulation");
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
            return;
        }

        Application.targetFrameRate = 30;
        if (!useDifferentDirectory)
        {
            directory = "";
        }

        StartCoroutine(BeginDataGeneration());

    }
}