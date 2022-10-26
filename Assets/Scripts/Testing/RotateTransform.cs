using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTransform : MonoBehaviour
{
    // Start is called before the first frame update

    public Vector3 rotation;
    public Transform changedTransform;
    void Start()
    {
        rotation = new Vector3(0,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate(){
        changedTransform.rotation.eulerAngles.Set(rotation[0],rotation[1], rotation[2]);
    }
}
