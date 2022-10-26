using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]

public class RandObjPos : MonoBehaviour
{
    
    [SerializeField] GameObject obj1;
    [SerializeField] GameObject obj2;
    [SerializeField] GameObject obj3;

    [SerializeField] Transform target;

    [SerializeField] Transform constraint1;
    [SerializeField] Transform constraint2;

    public bool randomizeobjectlocations = false;
    Vector3 GetRandSpot(){
        return new Vector3(Random.Range(constraint1.position.x, constraint2.position.x),
            Random.Range(constraint1.position.y, constraint2.position.y), Random.Range(constraint1.position.z, constraint2.position.z));
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    [ExecuteInEditMode]
    void Update()
    {
        if (randomizeobjectlocations){
            obj1.transform.position = GetRandSpot();
            obj2.transform.position = GetRandSpot();
            obj3.transform.position = GetRandSpot();
            Quaternion randomangle = Quaternion.Euler(0f, Random.Range(0,180f), 0f);

            obj1.transform.rotation = randomangle;

            randomangle = Quaternion.Euler(0f, Random.Range(0,180f), 0f);

            obj2.transform.rotation = randomangle;

            int index = Random.Range(0,3);
            if (index == 0){
                target.position = obj1.transform.position;
            }
            else if (index == 1){
                target.position = obj2.transform.position;
            }
            else if (index == 2){
                target.position = obj3.transform.position;
            }
            

            randomizeobjectlocations = false;
        }
        
    }
}
